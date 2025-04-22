using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ExifRenamer.ViewModels;

public class ExifInput
{
    public ObservableCollection<ExifTokenItemViewModel> ExifTags { get; set; }
}