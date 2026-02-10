using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using ExifRenamer.ViewModels;
using ExifRenamer.Views;

namespace ExifRenamer.Services;

public class DialogService(MainWindow owner) : IDialogService
{
    public async Task<string?> ShowFolderBrowserDialogAsync()
    {
        var folders = await owner.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            AllowMultiple = false
        });
        
        return folders.Count > 0 ? folders[0].Path.LocalPath : null;
    }

    public async Task<ExifMetadataDialogResult> ShowExifMetadataDialogAsync(ExifInput exifInput)
    {
        owner.ShowOverlay();
        var dialog = new ExifMetadataExplorerDialog(exifInput);
        var result = await dialog.ShowDialog<ExifMetadataDialogResult>(owner);
        owner.HideOverlay();
        return result;
    }
}