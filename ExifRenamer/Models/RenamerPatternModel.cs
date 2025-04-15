using System.Text;
using System.Text.RegularExpressions;

namespace ExifRenamer.Models;

public class RenamerPatternModel
{
    public string RawPattern { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsCustomDateFormat { get; set; }
}