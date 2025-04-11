using System.Threading.Tasks;
using Avalonia.Controls;
using ExifRenamer.ViewModels;
using ExifRenamer.Views;

namespace ExifRenamer.Services;

public class DialogService(MainWindow owner) : IDialogService
{
    public async Task<string?> ShowFolderBrowserDialogAsync()
    {
        var dialog = new OpenFolderDialog();
        return await dialog.ShowAsync(owner);
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