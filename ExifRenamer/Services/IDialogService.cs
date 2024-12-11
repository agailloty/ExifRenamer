using System.Threading.Tasks;

namespace ExifRenamer.Services
{
    public interface IDialogService
    {
        Task<string?> ShowFolderBrowserDialogAsync();
    }
}