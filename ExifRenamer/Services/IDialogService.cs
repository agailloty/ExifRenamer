using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace ExifRenamer.Services;

public interface IDialogService
{
    Task<string?> ShowFolderBrowserDialogAsync();
    Task ShowExifMetadataDialogAsync();
}