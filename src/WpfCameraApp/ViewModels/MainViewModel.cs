using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfCameraApp.Commands;
using WpfCameraApp.Models;
using WpfCameraApp.Services;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.ViewModels
{
    public class MainViewModel : ViewModelBase, IDisposable
    {
        private readonly IImageFilterService _imageFilterService;
        private readonly IFaceDetectionService? _faceDetectionService;
        private readonly bool _ownsFilterService;
        private readonly bool _ownsFaceService;
        private CancellationTokenSource? _processingCts;
        private int _isProcessing;
        private int _frameVersion;
        private Task? _currentTask;
        private bool _isCameraRunning;
        private string? _selectedFilter;
        private double _brightness = 0.0;
        private double _contrast = 1.0;
        private double _tint = 0.0;
        private bool _detectFaces;
        private string? _statusMessage;

        public string Title { get; } = "WPF Camera App";

        public IEnumerable<string> Filters { get; } = new List<string>
        {
            "Original",
            "Grayscale",
            "Sepia"
        };

        public string? SelectedFilter
        {
            get => _selectedFilter;
            set
            {
                if (SetProperty(ref _selectedFilter, value))
                {
                    if (_imageFilterService != null && !string.IsNullOrWhiteSpace(value))
                        _imageFilterService.SelectedFilter = value!;
                }
            }
        }

        private void UpdateDisplayedFaces(IEnumerable<WpfCameraApp.Models.Face> detections, int frameWidth, int frameHeight, int frameVersion)
        {
            var controlW = PreviewImageActualWidth;
            var controlH = PreviewImageActualHeight;

            if (frameWidth <= 0 || frameHeight <= 0 || controlW <= 0 || controlH <= 0)
            {
                FaceRects.Clear();
                return;
            }

            double ratio = Math.Min(controlW / frameWidth, controlH / frameHeight);
            double renderedW = frameWidth * ratio;
            double renderedH = frameHeight * ratio;
            double offsetX = (controlW - renderedW) / 2.0;
            double offsetY = (controlH - renderedH) / 2.0;

            var displayed = new List<WpfCameraApp.Models.DisplayFace>();
            foreach (var f in detections)
            {
                double x = offsetX + f.X * ratio;
                double y = offsetY + f.Y * ratio;
                double w = f.Width * ratio;
                double h = f.Height * ratio;
                displayed.Add(new WpfCameraApp.Models.DisplayFace(x, y, w, h));
            }

            if (Application.Current?.Dispatcher != null)
            {
                Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    if (frameVersion != Volatile.Read(ref _frameVersion))
                        return;

                    FaceRects.Clear();
                    foreach (var df in displayed) FaceRects.Add(df);
                }));
            }
            else
            {
                if (frameVersion != Volatile.Read(ref _frameVersion))
                    return;

                FaceRects.Clear();
                foreach (var df in displayed) FaceRects.Add(df);
            }
        }

        public double Brightness
        {
            get => _brightness;
            set
            {
                if (SetProperty(ref _brightness, value))
                {
                    if (_imageFilterService != null)
                        _imageFilterService.Brightness = value;
                }
            }
        }

        public double Contrast
        {
            get => _contrast;
            set
            {
                if (SetProperty(ref _contrast, value))
                {
                    if (_imageFilterService != null)
                        _imageFilterService.Contrast = value;
                }
            }
        }

        public double Tint
        {
            get => _tint;
            set
            {
                if (SetProperty(ref _tint, value))
                {
                    if (_imageFilterService != null)
                        _imageFilterService.Tint = value;
                }
            }
        }

        public bool DetectFaces
        {
            get => _detectFaces;
            set
            {
                if (SetProperty(ref _detectFaces, value) && !value)
                    UpdateUi(() => FaceRects.Clear());
            }
        }

        private double _previewImageActualWidth;
        public double PreviewImageActualWidth
        {
            get => _previewImageActualWidth;
            set => SetProperty(ref _previewImageActualWidth, value);
        }

        private double _previewImageActualHeight;
        public double PreviewImageActualHeight
        {
            get => _previewImageActualHeight;
            set => SetProperty(ref _previewImageActualHeight, value);
        }

        public System.Collections.ObjectModel.ObservableCollection<WpfCameraApp.Models.DisplayFace> FaceRects { get; } = new System.Collections.ObjectModel.ObservableCollection<WpfCameraApp.Models.DisplayFace>();

        public bool IsCameraRunning
        {
            get => _isCameraRunning;
            private set
            {
                if (SetProperty(ref _isCameraRunning, value))
                {
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

        public AsyncCommand StartCommand { get; }
        public AsyncCommand StopCommand { get; }
        public RelayCommand ResetCommand { get; }

        private ImageSource? _previewImage;
        public ImageSource? PreviewImage
        {
            get => _previewImage;
            private set => SetProperty(ref _previewImage, value);
        }

        private readonly ICameraService _cameraService;

        private void UpdateUi(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
                dispatcher.BeginInvoke(action);
            else
                action();
        }

        public MainViewModel()
            : this(null, null, null)
        {
        }

        public MainViewModel(ICameraService? cameraService, IImageFilterService? imageFilterService = null, Services.Interfaces.IFaceDetectionService? faceDetectionService = null)
        {
            SelectedFilter = "Original";
            if (imageFilterService != null)
            {
                _imageFilterService = imageFilterService;
                _ownsFilterService = false;
            }
            else
            {
                _imageFilterService = ResolveFilterServiceFromResources();
                _ownsFilterService = true;
            }

            if (faceDetectionService != null)
            {
                _faceDetectionService = faceDetectionService;
                _ownsFaceService = false;
            }
            else
            {
                _faceDetectionService = ResolveFaceDetectionServiceFromResources();
                _ownsFaceService = true;
            }
            if (_imageFilterService != null)
            {
                _imageFilterService.SelectedFilter = SelectedFilter!;
                _imageFilterService.Brightness = Brightness;
                _imageFilterService.Contrast = Contrast;
                _imageFilterService.Tint = Tint;
            }

            if (cameraService != null)
            {
                _cameraService = cameraService;
            }
            else
            {
                ICameraService? resolved = null;
                try
                {
                    if (Application.Current?.Resources.Contains("CameraService") == true)
                    {
                        resolved = Application.Current.Resources["CameraService"] as ICameraService;
                    }
                }
                catch { }

                _cameraService = resolved ?? new StubCameraService();
            }

            _cameraService.FrameArrived += Camera_FrameArrived;
            _cameraService.ErrorOccurred += Camera_ErrorOccurred;

            StartCommand = new AsyncCommand(StartAsync, () => !IsCameraRunning);
            StopCommand = new AsyncCommand(StopAsync, () => IsCameraRunning);
            ResetCommand = new RelayCommand(Reset);

            StatusMessage = "Ready";
        }

        private IImageFilterService? ResolveFilterServiceFromResources()
        {
            try
            {
                if (Application.Current?.Resources.Contains("ImageFilterService") == true)
                {
                    return Application.Current.Resources["ImageFilterService"] as IImageFilterService;
                }
            }
            catch { }

            return new ImageFilterService();
        }

        private Services.Interfaces.IFaceDetectionService? ResolveFaceDetectionServiceFromResources()
        {
            try
            {
                if (Application.Current?.Resources.Contains("FaceDetectionService") == true)
                {
                    return Application.Current.Resources["FaceDetectionService"] as Services.Interfaces.IFaceDetectionService;
                }
            }
            catch { }

            return new Services.FaceDetectionService();
        }

        private async Task StartAsync()
        {
            try
            {
                StatusMessage = "Starting camera...";
                await _cameraService.StartAsync().ConfigureAwait(false);
                System.Diagnostics.Debug.WriteLine("Camera started");

                _processingCts?.Dispose();
                _processingCts = new CancellationTokenSource();

                UpdateUi(() =>
                {
                    IsCameraRunning = _cameraService.IsRunning;
                    StatusMessage = "Camera started";
                });
            }
            catch (Exception ex)
            {
                UpdateUi(() =>
                {
                    IsCameraRunning = false;
                    StatusMessage = $"Start failed: {ex.Message}";
                });
            }
        }

        private async Task StopAsync()
        {
            try
            {
                StatusMessage = "Stopping camera...";
                Interlocked.Increment(ref _frameVersion);
                await _cameraService.StopAsync().ConfigureAwait(false);

                _processingCts?.Cancel();

                try
                {
                    if (_currentTask != null)
                    {
                        await Task.WhenAny(_currentTask, Task.Delay(500)).ConfigureAwait(false);
                    }
                }
                catch { }

                try
                {
                    _processingCts?.Dispose();
                }
                catch { }
                _processingCts = null;

                UpdateUi(() =>
                {
                    IsCameraRunning = false;
                    PreviewImage = null;
                    FaceRects.Clear();
                    StatusMessage = "Camera stopped";
                });
            }
            catch (Exception ex)
            {
                UpdateUi(() => StatusMessage = $"Stop failed: {ex.Message}");
            }
        }

        private void Reset()
        {
            SelectedFilter = "Original";
            Brightness = 0.0;
            Contrast = 1.0;
            Tint = 0.0;
            DetectFaces = false;

            try
            {
                _imageFilterService?.Reset();
            }
            catch { }

            StatusMessage = "Settings reset";
        }

        private void Camera_ErrorOccurred(object? sender, string e)
        {
            void SetError()
            {
                StatusMessage = e;
                IsCameraRunning = false;
                PreviewImage = null;
                FaceRects.Clear();
            }

            Interlocked.Increment(ref _frameVersion);
            _processingCts?.Cancel();
            UpdateUi(SetError);
        }

        private void Camera_FrameArrived(object? sender, Services.CameraFrameEventArgs e)
        {
            if (Interlocked.Exchange(ref _isProcessing, 1) == 1)
                return;

            var bytes = e.Frame.PixelData;
            var token = _processingCts?.Token ?? CancellationToken.None;
            var version = Volatile.Read(ref _frameVersion);

            var task = Task.Run(async () =>
            {
                try
                {
                    byte[] processed = bytes;
                    if (_imageFilterService != null)
                    {
                        try
                        {
                            processed = await _imageFilterService.ApplyFilterAsync(bytes, token).ConfigureAwait(false);
                        }
                        catch (OperationCanceledException) when (token.IsCancellationRequested)
                        {
                            return;
                        }
                        catch
                        {
                            processed = bytes;
                        }
                    }

                    WpfCameraApp.Models.Face[] detections = Array.Empty<WpfCameraApp.Models.Face>();
                    if (DetectFaces && _faceDetectionService != null)
                    {
                        try
                        {
                            var det = _faceDetectionService.DetectFaces(processed);
                            if (det != null)
                                detections = System.Linq.Enumerable.ToArray(det);
                        }
                        catch
                        {
                            detections = Array.Empty<WpfCameraApp.Models.Face>();
                        }
                    }

                    var dispatcher = Application.Current?.Dispatcher;
                    if (dispatcher != null)
                    {
                        await dispatcher.InvokeAsync(() =>
                        {
                            try
                            {
                                PreviewImage = DecodePreviewImage(processed);
                                StatusMessage = $"Frame: {e.Frame.Width}x{e.Frame.Height}";
                                try
                                {
                                    UpdateDisplayedFaces(detections, e.Frame.Width, e.Frame.Height, version);
                                }
                                catch { }
                            }
                            catch (Exception ex)
                            {
                                StatusMessage = $"Frame processing error: {ex.Message}";
                            }
                        }).Task.ConfigureAwait(false);
                    }
                    else
                    {
                        UpdateDisplayedFaces(detections, e.Frame.Width, e.Frame.Height, version);

                        try
                        {
                            PreviewImage = DecodePreviewImage(processed);
                            StatusMessage = $"Frame: {e.Frame.Width}x{e.Frame.Height}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Frame processing error: {ex.Message}";
                        }
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _isProcessing, 0);
                }
            }, token);

            _currentTask = task;
        }

        private static ImageSource DecodePreviewImage(byte[] encodedFrame)
        {
            using var stream = new MemoryStream(encodedFrame, writable: false);
            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            if (decoder.Frames.Count == 0)
                throw new InvalidOperationException("The camera returned an empty image frame.");

            var frame = decoder.Frames[0];
            frame.Freeze();
            return frame;
        }

        public void Dispose()
        {
            try
            {
                _cameraService.FrameArrived -= Camera_FrameArrived;
                _cameraService.ErrorOccurred -= Camera_ErrorOccurred;
            }
            catch { }

            try
            {
                try { _processingCts?.Cancel(); } catch { }
                try
                {
                    _currentTask?.Wait(500);
                }
                catch { }

                try { _cameraService.StopAsync().GetAwaiter().GetResult(); } catch { }
                _cameraService.Dispose();
            }
            catch { }

            try
            {
                if (_ownsFaceService && _faceDetectionService is IDisposable d)
                {
                    try { d.Dispose(); } catch { }
                }
            }
            catch { }
        }
    }
}
