using Avalonia.Controls;
using ExifRenamer.ViewModels;

namespace ExifRenamer.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Get screen information
            var screen = Screens.Primary;
            // Set width and height as percentages of the screen
            Width = screen.WorkingArea.Width * 0.5; 
            Height = screen.WorkingArea.Height * 0.6;
        }
    }
}