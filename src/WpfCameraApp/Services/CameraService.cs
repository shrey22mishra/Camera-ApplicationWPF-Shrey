using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using WpfCameraApp.Models;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Services
{
    /// <summary>
    /// Camera capture service using OpenCvSharp. Responsible only for capture and frame delivery.
    /// </summary>
    public class CameraService : ICameraService
    {
        private readonly IVideoCaptureFactory _factory;
        private VideoCapture? _capture;
        private Task? _captureTask;
        private CancellationTokenSource? _internalCts;
        private readonly object _sync = new();

        public event EventHandler<CameraFrameEventArgs>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning { get; private set; }

        public CameraService(IVideoCaptureFactory? factory = null)
        {
            _factory = factory ?? new DefaultVideoCaptureFactory();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (IsRunning)
                    return;

                _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }

            try
            {
                _capture = _factory.Create();
                // Try to open the default camera device if not already opened via ctor
                if (!_capture.IsOpened())
                {
                    // Attempt to open index 0 explicitly
                    _capture.Open(0);
                }

                if (!_capture.IsOpened())
                {
                    OnError("Failed to open camera device.");
                    CleanupCapture();
                    return;
                }

                IsRunning = true;

                var token = _internalCts.Token;
                _captureTask = Task.Run(() => CaptureLoopAsync(token), CancellationToken.None);

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                OnError($"Camera start error: {ex.Message}");
                CleanupCapture();
                IsRunning = false;
            }
        }

        private async Task CaptureLoopAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (_capture == null)
                        break;

                    using var mat = new Mat();
                    try
                    {
                        // Read frame (blocking for a frame). Read returns false on failure.
                        if (!_capture.Read(mat) || mat.Empty())
                        {
                            // brief delay before retrying to avoid tight loop on failure
                            await Task.Delay(15, token).ConfigureAwait(false);
                            continue;
                        }

                        // Encode the frame as BMP in-memory. Consumers can decode or convert as needed.
                        // This keeps ownership clear: service provides a copy of encoded bytes.
                        byte[]? buf = null;
                        try
                        {
                            buf = mat.ImEncode(".bmp");
                        }
                        catch (Exception encodeEx)
                        {
                            OnError($"Frame encode error: {encodeEx.Message}");
                        }

                        if (buf != null)
                        {
                            var frame = new CameraFrame(buf, mat.Width, mat.Height);
                            FrameArrived?.Invoke(this, new CameraFrameEventArgs(frame));
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        OnError($"Capture loop error: {ex.Message}");
                        await Task.Delay(100, token).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                // Ensure running state updated and resources cleaned
                IsRunning = false;
                CleanupCapture();
            }
        }

        public async Task StopAsync()
        {
            lock (_sync)
            {
                if (!IsRunning)
                    return;

                _internalCts?.Cancel();
            }

            try
            {
                if (_captureTask != null)
                {
                    await _captureTask.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
                // expected on cancellation
            }
            catch (Exception ex)
            {
                OnError($"Error while stopping camera: {ex.Message}");
            }
            finally
            {
                IsRunning = false;
                CleanupCapture();
            }
        }

        private void CleanupCapture()
        {
            try
            {
                _capture?.Release();
            }
            catch { }

            try
            {
                _capture?.Dispose();
            }
            catch { }

            _capture = null;

            try
            {
                _internalCts?.Dispose();
            }
            catch { }

            _internalCts = null;
            _captureTask = null;
        }

        private void OnError(string message)
        {
            try
            {
                ErrorOccurred?.Invoke(this, message);
            }
            catch { }
        }

        public void Dispose()
        {
            try
            {
                _internalCts?.Cancel();
            }
            catch { }

            try
            {
                _captureTask?.Wait(500);
            }
            catch { }

            CleanupCapture();
        }
    }
}
