using System.Threading.Tasks;
using Avalonia.Controls;

namespace ExifRenamer.Services
{
    public class DialogService(Window owner) : IDialogService
    {
        public async Task<string?> ShowFolderBrowserDialogAsync()
        {
            var dialog = new OpenFolderDialog();
            return await dialog.ShowAsync(owner);
        }
    }
}