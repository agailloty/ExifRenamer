using Avalonia.Controls;
using ExifRenamer.ViewModels;

namespace ExifRenamer.Views;

public partial class ExifMetadataExplorerDialog : Window
{
    public ExifMetadataExplorerDialog(ExifInput parameter)
    {
        var screen = Screens.Primary;
        InitializeComponent();
        Width = screen.WorkingArea.Width * 0.4;
        Height = screen.WorkingArea.Height * 0.5;
        
        var vm = new ExifMetadataExplorerDialogViewModel(parameter);
        vm.RequestClose += (_, result) => Close(result);
        DataContext = vm;
    }
}