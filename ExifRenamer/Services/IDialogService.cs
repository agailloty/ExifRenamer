using System.Threading.Tasks;
using ExifRenamer.ViewModels;

namespace ExifRenamer.Services;

public interface IDialogService
{
    Task<string?> ShowFolderBrowserDialogAsync();
    Task<string?> ShowFilePickerAsync(string title, string[] patterns);
    Task<ExifMetadataDialogResult> ShowExifMetadataDialogAsync(ExifInput exifInput);
}