using Avalonia.Controls;

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
            Width = screen.WorkingArea.Width * 0.4; 
            Height = screen.WorkingArea.Height * 0.6;
        }
    }
}