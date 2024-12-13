using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ExifRenamer.Views;

public partial class ExifMetadataDialog : Window
{
    public ExifMetadataDialog()
    {
        var screen = Screens.Primary;
        InitializeComponent();
        Width = screen.WorkingArea.Width * 0.3;
        Height = screen.WorkingArea.Height * 0.4;
    }
}