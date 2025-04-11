using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

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
    
    public DateTime? GetDateFromExif(string filename)
    {
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var exifSubDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var originalDate = exifSubDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
        var dateFormat = new DateTimeFormatInfo {DateSeparator = ":", TimeSeparator = ":"};
        return originalDate != null ? DateTime.Parse(originalDate, dateFormat) : null;
    }
    
    public string GetCameraMake(string filename)
    {   
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var exifSubDirectory = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
        return exifSubDirectory?.GetDescription(ExifDirectoryBase.TagMake) ?? string.Empty;
    }
    
    public IList<string> RetrieveExifTags(string filename)
    {
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var exifSubDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        if (exifSubDirectory == null) return new List<string>();
        
        var tags = new List<string>();
        foreach (var tag in exifSubDirectory.Tags)
        {
            tags.Add($"{tag.Name}: {tag.Description}");
        }

        return tags;
    }
}