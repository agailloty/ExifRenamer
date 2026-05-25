using System.IO;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExifRenamer.Models;
using ExifRenamer.Services;

namespace ExifRenamer.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private readonly SettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly AppSettings _appSettings;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsFfmpegConfigured))]
    private string _ffmpegPath = string.Empty;

    [ObservableProperty]
    private string _outputSubfolderName = "Final";

    public bool IsFfmpegConfigured =>
        !string.IsNullOrWhiteSpace(FfmpegPath) && File.Exists(FfmpegPath);

    public SettingsViewModel(SettingsService settingsService, IDialogService dialogService)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _appSettings = settingsService.Load();
        // Assign backing fields directly to avoid triggering Save() during init
        _ffmpegPath = _appSettings.FfmpegPath;
        _outputSubfolderName = _appSettings.OutputSubfolderName;
    }

    partial void OnFfmpegPathChanged(string value) => PersistSettings();

    partial void OnOutputSubfolderNameChanged(string value) => PersistSettings();

    [RelayCommand]
    private async Task BrowseFfmpegAsync()
    {
        var path = await _dialogService.ShowFilePickerAsync(
            "Sélectionner ffmpeg",
            new[] { "ffmpeg.exe", "ffmpeg" });
        if (!string.IsNullOrEmpty(path))
            FfmpegPath = path;
    }

    private void PersistSettings()
    {
        _appSettings.FfmpegPath = FfmpegPath;
        _appSettings.OutputSubfolderName = OutputSubfolderName;
        _settingsService.Save(_appSettings);
    }
}
