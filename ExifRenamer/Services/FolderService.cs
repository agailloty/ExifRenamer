using System.IO;
using System.Linq;

namespace ExifRenamer.Services;

public class FolderService
{
    public int GetImageFilesCount(string folderPath)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
        return Directory.GetFiles(folderPath)
            .Count(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()));
    }

    public string[] GetImageFiles(string folderPath)
    {
        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff" };
        return Directory.GetFiles(folderPath)
            .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .ToArray();
    }
}