using System.Collections.Generic;
using System.IO;
using ExifRenamer.Models;

namespace ExifRenamer.Services;

public class RenamerService
{
    public List<RenamerPatternModel> GetBuiltInRenamerPatterns()
    {
        return new List<RenamerPatternModel>
        {
            new() { Name = "Date (YY-MM-DD)", Description = "" },
            new() { Name = "Date (YYYY-MM-DD)", Description = "" },
            new() { Name = "Date (YY-Monthname-DD)", Description = "" },
            new() { Name = "Date (YYYY-Monthname-DD)", Description = "" },
            new() { Name = "Date Time (YY-MM-DD_HHMMSS)", Description = "" },
            new() { Name = "Date Time (YYYY-MM-DD_HHMMSS)", Description = "" },
            new() { Name = "Date Time (YY-Monthname-DD_HHMMSS)", Description = "" },
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
        return previews;
    }

    private PreviewModel GetRenamePreview(string filename, RenamerPatternModel pattern)
    {
        var file = new FileInfo(filename);
        var date = file.CreationTime;
        var newFilename = GetFormattedDate(file, pattern);
        return new PreviewModel { OldFilename = file.Name, NewFilename = newFilename };
    }

    private string GetFormattedDate(FileInfo file, RenamerPatternModel pattern)
    {
        var date = file.CreationTime;
        return pattern.Name switch
        {
            "Date (YY-MM-DD)" => date.ToString("yy-MM-dd"),
            "Date (YYYY-MM-DD)" => date.ToString("yyyy-MM-dd"),
            "Date (YY-Monthname-DD)" => date.ToString("yy-MMMM-dd"),
            "Date (YYYY-Monthname-DD)" => date.ToString("yyyy-MMMM-dd"),
            "Date Time (YY-MM-DD_HHMMSS)" => date.ToString("yy-MM-dd_HHmmss"),
            "Date Time (YYYY-MM-DD_HHMMSS)" => date.ToString("yyyy-MM-dd_HHmmss"),
            "Date Time (YY-Monthname-DD_HHMMSS)" => date.ToString("yy-MMMM-dd_HHmmss"),
            _ => date.ToString("yyyy-MM-dd")
        };
    }
}