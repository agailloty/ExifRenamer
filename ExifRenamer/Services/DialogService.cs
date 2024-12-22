using System.Threading.Tasks;
using Avalonia.Controls;
using ExifRenamer.Views;

namespace ExifRenamer.Services;

public class DialogService(MainWindow owner) : IDialogService
{
    public async Task<string?> ShowFolderBrowserDialogAsync()
    {
        var dialog = new OpenFolderDialog();
        return await dialog.ShowAsync(owner);
    }

    public async Task ShowExifMetadataDialogAsync()
    {
        owner.ShowOverlay();
        var dialog = new ExifMetadataDialog();
        await dialog.ShowDialog(owner);
        owner.HideOverlay();
    }
}