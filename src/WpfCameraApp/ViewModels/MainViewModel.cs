using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfCameraApp.Commands;
using WpfCameraApp.Services.Interfaces;


namespace WpfCameraApp.ViewModels
{
    // Main ViewModel for the application. Keeps minimal logic for UI state only.
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly ICameraService _cameraService;
        private bool _isCameraRunning;
        private string? _selectedFilter;
        private double _brightness = 0.0;
        private double _contrast = 1.0;
        private double _tint = 0.0;
        private bool _detectFaces;
        private string? _statusMessage;
        private ImageSource? _previewImage;

        public string Title { get; } = "WPF Camera App";

        public IEnumerable<string> Filters { get; } = new List<string>
        {
            "None",
            "Grayscale",
            "Sepia",
            "Invert"
        };

        public string? SelectedFilter
        {
            get => _selectedFilter;
            set => SetProperty(ref _selectedFilter, value);
        }

        public double Brightness
        {
            get => _brightness;
            set => SetProperty(ref _brightness, value);
        }

        public double Contrast
        {
            get => _contrast;
            set => SetProperty(ref _contrast, value);
        }

        public double Tint
        {
            get => _tint;
            set => SetProperty(ref _tint, value);
        }

        public bool DetectFaces
        {
            get => _detectFaces;
            set => SetProperty(ref _detectFaces, value);
        }

        public bool IsCameraRunning
        {
            get => _isCameraRunning;
            private set
            {
                if (SetProperty(ref _isCameraRunning, value))
                {
                    // Update command states
                    StartCommand.RaiseCanExecuteChanged();
                    StopCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string? StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        // Commands
        public AsyncCommand StartCommand { get; }
        public AsyncCommand StopCommand { get; }
        public RelayCommand ResetCommand { get; }

        // Bound preview image
        public ImageSource? PreviewImage
        {
            get => _previewImage;
            private set => SetProperty(ref _previewImage, value);
        }

        public MainViewModel() : this(null) { }

        public MainViewModel(ICameraService? cameraService)
        {
            _cameraService = cameraService ?? new CameraService();

            SelectedFilter = "None";

            StartCommand = new AsyncCommand(StartAsync, () => !IsCameraRunning);
            StopCommand = new AsyncCommand(StopAsync, () => IsCameraRunning);
            ResetCommand = new RelayCommand(Reset);

            StatusMessage = "Ready";

            // Subscribe to frames and errors
            _cameraService.FrameArrived += OnFrameArrived;
            _cameraService.ErrorOccurred += OnErrorOccurred;
        }

        private async Task StartAsync()
        {
            try
            {
                StatusMessage = "Starting camera...";
                await _cameraService.StartAsync();
                IsCameraRunning = _cameraService.IsRunning;
                StatusMessage = IsCameraRunning ? "Camera running" : "Camera failed to start";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Start failed: {ex.Message}";
            }
        }

        private async Task StopAsync()
        {
            try
            {
                StatusMessage = "Stopping camera...";
                await _cameraService.StopAsync();
                IsCameraRunning = _cameraService.IsRunning;
                PreviewImage = null;
                StatusMessage = "Camera stopped";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Stop failed: {ex.Message}";
            }
        }

        private void Reset()
        {
            SelectedFilter = "None";
            Brightness = 0.0;
            Contrast = 1.0;
            Tint = 0.0;
            DetectFaces = false;
            StatusMessage = "Settings reset";
        }

        private void OnErrorOccurred(object? sender, string message)
        {
            // Marshal to UI thread
            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                StatusMessage = message;
            }));
        }

        private void OnFrameArrived(object? sender, Services.CameraFrameEventArgs e)
        {
            // Convert on a threadpool thread (already background), but marshal UI update to dispatcher
            var bytes = e.Frame.PixelData;

            Application.Current?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    using var ms = new MemoryStream(bytes);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.StreamSource = ms;
                    bmp.EndInit();
                    bmp.Freeze();
                    PreviewImage = bmp;
                    StatusMessage = "Preview updated";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Frame processing error: {ex.Message}";
                }
            }));
        }

        public void Dispose()
        {
            try
            {
                _cameraService.FrameArrived -= OnFrameArrived;
                _cameraService.ErrorOccurred -= OnErrorOccurred;
            }
            catch { }

            try
            {
                _cameraService.StopAsync().GetAwaiter().GetResult();
            }
            catch { }

            try
            {
                _cameraService.Dispose();
            }
            catch { }
        }
    }
}
