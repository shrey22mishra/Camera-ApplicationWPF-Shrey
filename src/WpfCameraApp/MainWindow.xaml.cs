using System.Windows;

using System.Windows;

namespace WpfCameraApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            // Use LoadComponent to initialize XAML to avoid ambiguous generated InitializeComponent in some build environments.
            var uri = new System.Uri("/WpfCameraApp;component/MainWindow.xaml", System.UriKind.Relative);
            System.Windows.Application.LoadComponent(this, uri);

            // Ensure camera service is stopped when window closes
            this.Closed += (s, e) =>
            {
                if (this.DataContext is IDisposable d)
                {
                    try { d.Dispose(); } catch { }
                }
            };
        }
    }
}
