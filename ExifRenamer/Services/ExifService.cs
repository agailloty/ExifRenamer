using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ExifRenamer.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ExifRenamer.Services;

public class ExifService
{
    private readonly IList<string> _existingExifs;

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
    
    public string GetExifValue(Tag exifTag, string filename)
    {   
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var allTags = directories.SelectMany(d => d.Tags).ToList();
        var tagValue = allTags.FirstOrDefault(t => t.Name == exifTag.Name);
        string result = string.Empty;
        if (tagValue != null)
        {
            result = tagValue.Description ?? string.Empty;
        }
        return result;
    }
    
    public List<string> RetrieveExifTags(string filename)
    {
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var allTags = directories.SelectMany(d => d.Tags).ToList();
        
        var tags = new List<string>();
        foreach (var tag in allTags)
        {
            tags.Add(tag.Name);
        }

        return tags;
    }
    
    public string GetExifTags(string customFormat, string filename)
    {
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var allTags = directories.SelectMany(d => d.Tags).ToList();
        string result = string.Empty;
        if (!string.IsNullOrEmpty(customFormat))
        {
            string pattern = @"%([^%]+)%";

            MatchCollection matches = Regex.Matches(customFormat, pattern);
            var sb = new StringBuilder();
            foreach (Match match in matches)
            {
                var tag = match.Value.Replace("%", "");
                var foundTag = allTags.FirstOrDefault(t => t.Name == tag);
                if (foundTag != null)
                {
                    sb.Append(foundTag.Description ?? string.Empty);
                }
                else
                {
                    sb.Append(match.Value);
                }
            }
        
            result = sb.ToString();
        }
        
        return result;
    }
}