using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Models;
using ExifRenamer.Services;

namespace ExifRenamer.ViewModels;

public partial class VideoCompressorViewModel : ViewModelBase
{
    private readonly VideoCompressorService _compressorService;
    private readonly IDialogService _dialogService;
    private readonly SettingsViewModel _settings;
    private CancellationTokenSource? _cts;

    // ── Observable properties ────────────────────────────────────────────────

    [ObservableProperty]
    private VideoCompressionPreset _selectedPreset = null!;

    [ObservableProperty]
    private bool _usePostfix = true;

    [ObservableProperty]
    private string _postfix = "V";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanStart))]
    private bool _isRunning;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    [ObservableProperty]
    private int _processedCount;

    [ObservableProperty]
    private int _totalCount;

    // ── Collections ──────────────────────────────────────────────────────────

    public ObservableCollection<DirectoryInfo> Folders { get; } = new();
    public ObservableCollection<VideoCompressionJobViewModel> Jobs { get; } = new();
    public IReadOnlyList<VideoCompressionPreset> Presets { get; }

    // ── Commands ─────────────────────────────────────────────────────────────

    public ICommand AddFolderCommand { get; }
    public ICommand RemoveFolderCommand { get; }
    public ICommand StartCommand { get; }
    public ICommand CancelCommand { get; }

    // ── Computed ─────────────────────────────────────────────────────────────

    public bool CanStart => Folders.Count > 0 && !IsRunning;

    // ── Constructor ──────────────────────────────────────────────────────────

    public VideoCompressorViewModel(
        VideoCompressorService compressorService,
        IDialogService dialogService,
        SettingsViewModel settings)
    {
        _compressorService = compressorService;
        _dialogService = dialogService;
        _settings = settings;

        Presets = BuildPresets();
        _selectedPreset = Presets[2]; // Balanced by default

        AddFolderCommand = new AsyncRelayCommand(AddFolderAsync);
        RemoveFolderCommand = new RelayCommand<DirectoryInfo>(RemoveFolder);
        StartCommand = new AsyncRelayCommand(StartCompressionAsync, () => CanStart);
        CancelCommand = new RelayCommand(CancelCompression, () => IsRunning);

        Folders.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(CanStart));
            ((AsyncRelayCommand)StartCommand).NotifyCanExecuteChanged();
        };
    }

    // ── Command implementations ───────────────────────────────────────────────

    private async Task AddFolderAsync()
    {
        var path = await _dialogService.ShowFolderBrowserDialogAsync();
        if (!string.IsNullOrEmpty(path))
        {
            var dir = new DirectoryInfo(path);
            if (Folders.All(f => f.FullName != dir.FullName))
                Folders.Add(dir);
        }
    }

    private void RemoveFolder(DirectoryInfo? folder)
    {
        if (folder is null) return;
        Folders.Remove(folder);
        var prefix = folder.FullName.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                     + Path.DirectorySeparatorChar;
        var toRemove = Jobs
            .Where(j => j.InputPath.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            .ToList();
        foreach (var job in toRemove)
            Jobs.Remove(job);
    }

    private async Task StartCompressionAsync()
    {
        if (string.IsNullOrEmpty(_settings.FfmpegPath)) return;

        _cts = new CancellationTokenSource();
        IsRunning = true;
        Jobs.Clear();

        var outputSubfolder = string.IsNullOrWhiteSpace(_settings.OutputSubfolderName)
            ? "Final"
            : _settings.OutputSubfolderName;

        var videoFiles = _compressorService.GetVideoFiles(Folders.Select(f => f.FullName));

        TotalCount = videoFiles.Length;
        ProcessedCount = 0;

        if (TotalCount == 0)
        {
            StatusMessage = "No videos found in the selected folders.";
            IsRunning = false;
            return;
        }

        StatusMessage = $"0 / {TotalCount} files processed";

        // Build job list upfront
        foreach (var file in videoFiles)
        {
            var info = new FileInfo(file);
            var outputFolder = Path.Combine(info.DirectoryName ?? string.Empty, outputSubfolder);
            var baseName = Path.GetFileNameWithoutExtension(file);
            var ext = Path.GetExtension(file);
            var outName = UsePostfix ? $"{baseName}{Postfix}{ext}" : $"{baseName}{ext}";
            var outPath = Path.Combine(outputFolder, outName);

            Jobs.Add(new VideoCompressionJobViewModel
            {
                InputFilename = info.Name,
                OutputFilename = outName,
                InputPath = file,
                OutputPath = outPath,
                Status = VideoCompressionJobStatus.Queued
            });
        }

        // Process each job sequentially
        for (var i = 0; i < Jobs.Count; i++)
        {
            if (_cts.IsCancellationRequested) break;

            var job = Jobs[i];
            job.Status = VideoCompressionJobStatus.Processing;
            StatusMessage = $"Compressing {job.InputFilename}...";

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(job.OutputPath)!);

                var progressReporter = new Progress<string>(line =>
                    StatusMessage = $"[{job.InputFilename}] {line}");

                var result = await _compressorService.CompressAsync(
                    job.InputPath,
                    job.OutputPath,
                    SelectedPreset,
                    _settings.FfmpegPath,
                    progressReporter,
                    _cts.Token);

                if (result.Success)
                {
                    job.InputSize = result.InputSize;
                    job.OutputSize = result.OutputSize;
                    job.Status = VideoCompressionJobStatus.Done;
                }
                else
                {
                    job.ErrorMessage = result.ErrorMessage;
                    job.Status = VideoCompressionJobStatus.Failed;
                }
            }
            catch (OperationCanceledException)
            {
                job.Status = VideoCompressionJobStatus.Cancelled;
                for (var j = i + 1; j < Jobs.Count; j++)
                    Jobs[j].Status = VideoCompressionJobStatus.Cancelled;
                break;
            }

            ProcessedCount++;
            StatusMessage = $"{ProcessedCount} / {TotalCount} files processed";
        }

        var doneCount = Jobs.Count(j => j.Status == VideoCompressionJobStatus.Done);
        var failedCount = Jobs.Count(j => j.Status == VideoCompressionJobStatus.Failed);
        StatusMessage = failedCount > 0
            ? $"Done — {doneCount} compressed, {failedCount} error(s) out of {TotalCount}"
            : $"Done — {doneCount} file(s) compressed out of {TotalCount}";

        IsRunning = false;
        _cts.Dispose();
        _cts = null;
    }

    private void CancelCompression() => _cts?.Cancel();

    // ── Partial hooks ─────────────────────────────────────────────────────────

    partial void OnIsRunningChanged(bool value)
    {
        OnPropertyChanged(nameof(CanStart));
        ((AsyncRelayCommand)StartCommand).NotifyCanExecuteChanged();
        ((RelayCommand)CancelCommand).NotifyCanExecuteChanged();
    }

    // ── Preset definitions ────────────────────────────────────────────────────

    private static IReadOnlyList<VideoCompressionPreset> BuildPresets() =>
        new List<VideoCompressionPreset>
        {
            new() { Name = "Very high quality", Description = "CRF 18 · slow — near-original quality, large files",                Crf = 18, FfmpegPreset = "slow"      },
            new() { Name = "High quality",         Description = "CRF 22 · medium — excellent quality, good compression",               Crf = 22, FfmpegPreset = "medium"    },
            new() { Name = "Balanced",             Description = "CRF 27 · veryfast — good quality/size trade-off (recommended)",       Crf = 27, FfmpegPreset = "veryfast"  },
            new() { Name = "Reduced size",         Description = "CRF 30 · veryfast — lighter files, slight quality loss",              Crf = 30, FfmpegPreset = "veryfast"  },
            new() { Name = "Minimum size",         Description = "CRF 36 · ultrafast — maximum compression, reduced quality",           Crf = 36, FfmpegPreset = "ultrafast" },
            new() { Name = "Full HD (1080p)",      Description = "CRF 23 · veryfast — scales to 1920×… (source ≥ 1080p)",              Crf = 23, FfmpegPreset = "veryfast",  ScaleFilter = "1920:-2" },
            new() { Name = "HD (720p)",            Description = "CRF 23 · veryfast — scales to 1280×… (source ≥ 720p)",               Crf = 23, FfmpegPreset = "veryfast",  ScaleFilter = "1280:-2" },
            new() { Name = "Social media",         Description = "CRF 28 · veryfast — 720p optimised for online sharing",               Crf = 28, FfmpegPreset = "veryfast",  ScaleFilter = "1280:-2" },
        }.AsReadOnly();
}
