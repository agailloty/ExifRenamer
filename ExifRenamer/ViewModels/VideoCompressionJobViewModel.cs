using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Models;

namespace ExifRenamer.ViewModels;

public class VideoCompressionJobViewModel : ViewModelBase
{
    private VideoCompressionJobStatus _status;
    private string _statusText = string.Empty;
    private long _inputSize;
    private long _outputSize;
    private string? _errorMessage;
    private VideoCompressionPreset _selectedPreset = null!;
    private bool _isPresetEditable = true;

    public required string InputFilename { get; set; }
    public required string InputPath { get; set; }
    public string OutputFilename { get; set; } = string.Empty;
    public string OutputPath { get; set; } = string.Empty;

    public ICommand OpenCommand { get; }

    public VideoCompressionJobViewModel()
    {
        OpenCommand = new RelayCommand(() =>
        {
            try { Process.Start(new ProcessStartInfo(InputPath) { UseShellExecute = true }); }
            catch { /* ignore launch errors */ }
        });
    }

    /// <summary>Shared preset list — bound to the per-job ComboBox.</summary>
    public IReadOnlyList<VideoCompressionPreset> Presets { get; init; } =
        System.Array.Empty<VideoCompressionPreset>();

    public VideoCompressionPreset SelectedPreset
    {
        get => _selectedPreset;
        set => SetProperty(ref _selectedPreset, value);
    }

    /// <summary>False while compression is running so the ComboBox is locked.</summary>
    public bool IsPresetEditable
    {
        get => _isPresetEditable;
        set => SetProperty(ref _isPresetEditable, value);
    }

    public VideoCompressionJobStatus Status
    {
        get => _status;
        set
        {
            if (!SetProperty(ref _status, value)) return;
            StatusText = value switch
            {
                VideoCompressionJobStatus.Queued      => "⏳ Queued",
                VideoCompressionJobStatus.Processing  => "⟳ Processing...",
                VideoCompressionJobStatus.Done        => $"✓ {ReductionText}",
                VideoCompressionJobStatus.Failed      => "✗ Error",
                VideoCompressionJobStatus.Skipped     => "— Skipped",
                VideoCompressionJobStatus.Cancelled   => "✕ Cancelled",
                _                                     => string.Empty
            };
            OnPropertyChanged(nameof(IsProcessing));
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set => SetProperty(ref _statusText, value);
    }

    public long InputSize
    {
        get => _inputSize;
        set => SetProperty(ref _inputSize, value);
    }

    public long OutputSize
    {
        get => _outputSize;
        set
        {
            SetProperty(ref _outputSize, value);
            OnPropertyChanged(nameof(ReductionText));
        }
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public bool IsProcessing => Status == VideoCompressionJobStatus.Processing;

    public string ReductionText
    {
        get
        {
            if (InputSize <= 0 || OutputSize <= 0) return string.Empty;
            var pct = (1.0 - (double)OutputSize / InputSize) * 100;
            return $"-{pct:F1}% ({FormatBytes(InputSize)} → {FormatBytes(OutputSize)})";
        }
    }

    private static string FormatBytes(long bytes) => bytes switch
    {
        >= 1_000_000_000 => $"{bytes / 1_000_000_000.0:F1} GB",
        >= 1_000_000     => $"{bytes / 1_000_000.0:F1} MB",
        >= 1_000         => $"{bytes / 1_000.0:F1} KB",
        _                => $"{bytes} B"
    };
}
