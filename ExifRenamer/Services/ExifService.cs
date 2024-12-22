using System.Collections.Generic;
using MetadataExtractor;

namespace ExifRenamer.Services;

public class ExifService
{
    public IReadOnlyList<Directory> ExtractExifData(string filepath)
    {
        return ImageMetadataReader.ReadMetadata(filepath);
    }

    public string? GetExifValue(string path, int tag)
    {
        var directories = ImageMetadataReader.ReadMetadata(path);
        foreach (var directory in directories)
        {
            var tagValue = directory.GetDescription(tag);
            if (tagValue != null) return tagValue;
        }

        return null;
    }
}