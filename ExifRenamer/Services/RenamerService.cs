using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ExifRenamer.Models;
using MetadataExtractor;
using MetadataExtractor.Formats.Exif;

namespace ExifRenamer.Services;

public class RenamerService
{
    public List<RenamerPatternModel> GetBuiltInRenamerPatterns()
    {
        return new List<RenamerPatternModel>
        {
new() { Name = "Date (YY-MM-DD)", Description = "Year-Month-Day (2-digit year)" },
new() { Name = "Date (YYYY-MM-DD)", Description = "Year-Month-Day (4-digit year)" },
new() { Name = "Date (YY-Monthname-DD)", Description = "Year-Monthname-Day (2-digit year)" },
new() { Name = "Date (YYYY-Monthname-DD)", Description = "Year-Monthname-Day (4-digit year)" },
new() { Name = "Date Time (YY-MM-DD_HHMMSS)", Description = "Year-Month-Day HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (YYYY-MM-DD_HHMMSS)", Description = "Year-Month-Day HourMinuteSecond (4-digit year)" },
new() { Name = "Date Time (YY-Monthname-DD_HHMMSS)", Description = "Year-Monthname-Day HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (YYYY-Monthname-DD_HHMMSS)", Description = "Year-Monthname-Day HourMinuteSecond (4-digit year)" },
new() { Name = "Date (DD-MM-YY)", Description = "Day-Month-Year (2-digit year)" },
new() { Name = "Date (DD-MM-YYYY)", Description = "Day-Month-Year (4-digit year)" },
new() { Name = "Date (DD-Monthname-YY)", Description = "Day-Monthname-Year (2-digit year)" },
new() { Name = "Date (DD-Monthname-YYYY)", Description = "Day-Monthname-Year (4-digit year)" },
new() { Name = "Date Time (DD-MM-YY_HHMMSS)", Description = "Day-Month-Year HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (DD-MM-YYYY_HHMMSS)", Description = "Day-Month-Year HourMinuteSecond (4-digit year)" },
new() { Name = "Date Time (DD-Monthname-YY_HHMMSS)", Description = "Day-Monthname-Year HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (DD-Monthname-YYYY_HHMMSS)", Description = "Day-Monthname-Year HourMinuteSecond (4-digit year)" },
new() { Name = "Date (MM-DD-YY)", Description = "Month-Day-Year (2-digit year)" },
new() { Name = "Date (MM-DD-YYYY)", Description = "Month-Day-Year (4-digit year)" },
new() { Name = "Date (Monthname-DD-YY)", Description = "Monthname-Day-Year (2-digit year)" },
new() { Name = "Date (Monthname-DD-YYYY)", Description = "Monthname-Day-Year (4-digit year)" },
new() { Name = "Date Time (MM-DD-YY_HHMMSS)", Description = "Month-Day-Year HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (MM-DD-YYYY_HHMMSS)", Description = "Month-Day-Year HourMinuteSecond (4-digit year)" },
new() { Name = "Date Time (Monthname-DD-YY_HHMMSS)", Description = "Monthname-Day-Year HourMinuteSecond (2-digit year)" },
new() { Name = "Date Time (Monthname-DD-YYYY_HHMMSS)", Description = "Monthname-Day-Year HourMinuteSecond (4-digit year)" },
            new() { Name = "Custom", Description = "Custom" }
        };
    }

    public bool RenameFile(string filename, string newFilename)
    {
        var file = new FileInfo(filename);
        var newFile = new FileInfo(newFilename);
        if (!file.Exists || newFile.Exists) return false;
        file.MoveTo(newFilename);
        return true;
    }

    public void BuildFileName(string filename, RenamerPatternModel pattern)
    {
        if (pattern.Name == "Date (YY-MM-DD)")
        {
            var file = new FileInfo(filename);
            var date = file.CreationTime;
            RenameFile(filename, date.ToLongDateString());
        }
    }

    public PreviewModel[] GetRenamePreviews(string[] filenames, RenamerPatternModel pattern)
    {
        var previews = new PreviewModel[filenames.Length];
        for (var i = 0; i < filenames.Length; i++) previews[i] = GetRenamePreview(filenames[i], pattern);
         previews = MakeUniqueFilenames(previews);
        return previews;
    }

    private PreviewModel GetRenamePreview(string filename, RenamerPatternModel pattern)
    {
        var file = new FileInfo(filename);
        var extension = file.Extension;
        var creationTime = GetDateFromExif(filename) ?? file.CreationTime;
        var newFilename = GetFormattedDate(creationTime, pattern);
        var folderPath = file.Directory.FullName;
        return new PreviewModel { OldFilename = file.Name, NewFilename = newFilename, FolderPath = folderPath, Extension = extension };
    }

    private string GetFormattedDate(DateTime date, RenamerPatternModel pattern)
    {
        return pattern.Name switch
        {

            "Date (YY-MM-DD)" => date.ToString("yy-MM-dd"),
            "Date (YYYY-MM-DD)" => date.ToString("yyyy-MM-dd"),
            "Date (YY-Monthname-DD)" => date.ToString("yy-MMMM-dd"),
            "Date (YYYY-Monthname-DD)" => date.ToString("yyyy-MMMM-dd"),
            "Date Time (YY-MM-DD_HHMMSS)" => date.ToString("yy-MM-dd_HHmmss"),
            "Date Time (YYYY-MM-DD_HHMMSS)" => date.ToString("yyyy-MM-dd_HHmmss"),
            "Date Time (YY-Monthname-DD_HHMMSS)" => date.ToString("yy-MMMM-dd_HHmmss"),
            "Date Time (YYYY-Monthname-DD_HHMMSS)" => date.ToString("yyyy-MMMM-dd_HHmmss"),
            "Date (DD-MM-YY)" => date.ToString("dd-MM-yy"),
            "Date (DD-MM-YYYY)" => date.ToString("dd-MM-yyyy"),
            "Date (DD-Monthname-YY)" => date.ToString("dd-MMMM-yy"),
            "Date (DD-Monthname-YYYY)" => date.ToString("dd-MMMM-yyyy"),
            "Date Time (DD-MM-YY_HHMMSS)" => date.ToString("dd-MM-yy_HHmmss"),
            "Date Time (DD-MM-YYYY_HHMMSS)" => date.ToString("dd-MM-yyyy_HHmmss"),
            "Date Time (DD-Monthname-YY_HHMMSS)" => date.ToString("dd-MMMM-yy_HHmmss"),
            "Date Time (DD-Monthname-YYYY_HHMMSS)" => date.ToString("dd-MMMM-yyyy_HHmmss"),
            "Date (MM-DD-YY)" => date.ToString("MM-dd-yy"),
            "Date (MM-DD-YYYY)" => date.ToString("MM-dd-yyyy"),
            "Date (Monthname-DD-YY)" => date.ToString("MMMM-dd-yy"),
            "Date (Monthname-DD-YYYY)" => date.ToString("MMMM-dd-yyyy"),
            "Date Time (MM-DD-YY_HHMMSS)" => date.ToString("MM-dd-yy_HHmmss"),
            "Date Time (MM-DD-YYYY_HHMMSS)" => date.ToString("MM-dd-yyyy_HHmmss"),
            "Date Time (Monthname-DD-YY_HHMMSS)" => date.ToString("MMMM-dd-yy_HHmmss"),
            "Date Time (Monthname-DD-YYYY_HHMMSS)" => date.ToString("MMMM-dd-yyyy_HHmmss"),
            _ => date.ToString("yyyy-MM-dd")
        };
    }
    
    private PreviewModel[] MakeUniqueFilenames(PreviewModel[] previews)
    {
        var uniqueFilenames = new List<string>();
        foreach (var preview in previews)
        {
            var newName = preview.NewFilename;
            var i = 1;
            while (uniqueFilenames.Contains(newName))
            {
                newName = $"{preview.NewFilename}_{i}";
                i++;
            }
            uniqueFilenames.Add(newName);
            preview.NewFilename = newName;
        }
        return previews;
    }
    
    private DateTime? GetDateFromExif(string filename)
    {
        var directories = ImageMetadataReader.ReadMetadata(filename);
        var exifSubDirectory = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
        var originalDate = exifSubDirectory?.GetDescription(ExifDirectoryBase.TagDateTimeOriginal);
        var dateFormat = new DateTimeFormatInfo {DateSeparator = ":", TimeSeparator = ":"};
        return originalDate != null ? DateTime.Parse(originalDate, dateFormat) : null;
    }
}