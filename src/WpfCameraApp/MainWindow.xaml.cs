using System.Windows;

using System.Windows;

namespace WpfCameraApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Closed += MainWindow_Closed;
        }

        private void PreviewImageControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (DataContext is ViewModels.MainViewModel vm && sender is System.Windows.FrameworkElement fe)
            {
                vm.PreviewImageActualWidth = fe.ActualWidth;
                vm.PreviewImageActualHeight = fe.ActualHeight;
            }
        }

        private void MainWindow_Closed(object? sender, System.EventArgs e)
        {
            if (DataContext is IDisposable d)
            {
                try { d.Dispose(); } catch { }
            }
        }
    }
}
