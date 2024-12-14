using System.Collections.Generic;
using ExifRenamer.Models;

namespace ExifRenamer.Services;

public class RenamerService
{
    public List<RenamerPatternModel> GetBuiltInRenamerPatterns()
    {
        return new List<RenamerPatternModel>
        {
            new RenamerPatternModel { Name = "Date", Description = "Date" },
            new RenamerPatternModel { Name = "Date and Time", Description = "Date and Time" },
            new RenamerPatternModel { Name = "Date and Time with Counter", Description = "Date and Time with Counter" },
            new RenamerPatternModel { Name = "Counter", Description = "Counter" },
            new RenamerPatternModel { Name = "Custom", Description = "Custom" }
        };
    }
}

