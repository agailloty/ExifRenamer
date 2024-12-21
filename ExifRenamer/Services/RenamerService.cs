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
            new RenamerPatternModel { Name = "Date (YY-MM-DD)", Description = "" },
            new RenamerPatternModel { Name = "Date (YYYY-MM-DD)", Description = "" },
            new RenamerPatternModel { Name = "Date (YY-Monthname-DD)", Description = "" },
            new RenamerPatternModel { Name = "Date (YYYY-Monthname-DD)", Description = "" },
            new RenamerPatternModel { Name = "Date Time (YY-MM-DD_HHMMSS)", Description = "" },
            new RenamerPatternModel { Name = "Date Time (YYYY-MM-DD_HHMMSS)", Description = "" },
            new RenamerPatternModel { Name = "Date Time (YY-Monthname-DD_HHMMSS)", Description = "" },
            new RenamerPatternModel { Name = "Custom", Description = "Custom" }
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
        for (var i = 0; i < filenames.Length; i++)
        {
            previews[i] = GetRenamePreview(filenames[i], pattern);
        }
        return previews;
    }

    private PreviewModel GetRenamePreview(string filename, RenamerPatternModel pattern)
    {
        var file = new FileInfo(filename);
        var date = file.CreationTime;
        return new PreviewModel {OldFilename = file.Name, NewFilename = date.ToLongDateString()};
    }
}

