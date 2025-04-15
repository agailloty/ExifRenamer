using System.Threading.Tasks;
using ExifRenamer.ViewModels;

namespace ExifRenamer.Services;

public interface IDialogService
{
    Task<string?> ShowFolderBrowserDialogAsync();
    Task<ExifMetadataDialogResult> ShowExifMetadataDialogAsync(ExifInput exifInput);
}