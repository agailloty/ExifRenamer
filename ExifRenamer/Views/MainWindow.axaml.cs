using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ExifRenamer.Services;
using ExifRenamer.ViewModels;

namespace ExifRenamer.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        var dialogService = new DialogService(this);
        DataContext = new MainWindowViewModel(dialogService);
        var screen = Screens.Primary;
        // Set width and height as percentages of the screen
        Width = screen.WorkingArea.Width * 0.5;
    }

    private Grid OverlayGrid => this.FindControl<Grid>("MainWindowOverlay");
    public MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext;

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public void ShowOverlay()
    {
        OverlayGrid.IsVisible = true;
        OverlayGrid.ZIndex = 1000;
    }

    public void HideOverlay()
    {
        OverlayGrid.IsVisible = false;
        OverlayGrid.ZIndex = -1;
    }
}