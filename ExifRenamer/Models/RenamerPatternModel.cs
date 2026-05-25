using System.Text;
using System.Text.RegularExpressions;

namespace ExifRenamer.Models;

public class RenamerPatternModel
{
    public string RawPattern { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsCustomDateFormat { get; set; }
}