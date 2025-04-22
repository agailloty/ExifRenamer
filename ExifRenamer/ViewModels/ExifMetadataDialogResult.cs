using System.Collections.Generic;
using System.Collections.ObjectModel;
using ExifRenamer.Common;
using ExifRenamer.Models;

namespace ExifRenamer.ViewModels;

public class ExifMetadataDialogResult
{
    public ClosingResult ClosingResult { get; set; }
    public List<ExifTokenItemViewModel> ExifTokens { get; set; } = new();
}