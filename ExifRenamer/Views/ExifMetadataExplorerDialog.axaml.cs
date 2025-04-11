using Avalonia.Controls;

namespace ExifRenamer.Views;

public partial class ExifMetadataExplorerDialog : Window
{
    public ExifMetadataExplorerDialog()
    {
        var screen = Screens.Primary;
        InitializeComponent();
        Width = screen.WorkingArea.Width * 0.3;
        Height = screen.WorkingArea.Height * 0.4;
    }
}