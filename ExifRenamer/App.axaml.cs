using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using ExifRenamer.Services;
using ExifRenamer.ViewModels;
using ExifRenamer.Views;

namespace ExifRenamer;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // Line below is needed to remove Avalonia data validation.
            // Without this line you will get duplicate validations from both Avalonia and CT
            BindingPlugins.DataValidators.RemoveAt(0);
            desktop.MainWindow = new MainWindow();
            desktop.MainWindow.DataContext =
                new MainWindowViewModel(new DialogService(desktop.MainWindow as MainWindow));
        }

        base.OnFrameworkInitializationCompleted();
    }
}