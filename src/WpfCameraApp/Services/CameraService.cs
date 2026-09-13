using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using WpfCameraApp.Models;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Services
{
    public class CameraService : ICameraService
    {
        private readonly IVideoCaptureFactory _factory;
        private VideoCapture? capture;
        private Task? _captureTask;
        private CancellationTokenSource? _internalCts;
        private readonly object _sync = new();
        private bool _disposed;

        public event EventHandler<CameraFrameEventArgs>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning { get; private set; }

        public CameraService()
            : this(null)
        {
        }

        public CameraService(IVideoCaptureFactory? factory)
        {
            _factory = factory ?? new DefaultVideoCaptureFactory();
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            lock (_sync)
            {
                if (_disposed)
                    throw new ObjectDisposedException(nameof(CameraService));

                if (IsRunning)
                    return;

                _internalCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                try
                {
                    capture = _factory.Create();
                    if (!capture.IsOpened())
                        capture.Open(0);

                    if (!capture.IsOpened())
                        throw new InvalidOperationException("The camera could not be opened.");
                }
                catch (Exception ex)
                {
                    OnError("Unable to start camera: " + ex.Message);
                    CleanupCapture();
                    throw new InvalidOperationException("Unable to start camera.", ex);
                }

                var token = _internalCts.Token;
                _captureTask = Task.Run(() => CaptureLoopAsync(token), CancellationToken.None);
                IsRunning = true;
            }

            await Task.CompletedTask;
        }

        private async Task CaptureLoopAsync(CancellationToken token)
        {
            int consecutiveFailures = 0;
            const int tries = 30;
            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (capture == null)
                        break;

                    using var mat = new Mat();
                    try
                    {
                        if (!capture.Read(mat) || mat.Empty())
                        {
                            consecutiveFailures++;
                            if (consecutiveFailures >= tries)
                            {
                                OnError("Camera appears to be unavailable. Stopping capture.");
                                try { _internalCts?.Cancel(); } catch { }
                                break;
                            }

                            await Task.Delay(50, token).ConfigureAwait(false);
                            continue;
                        }

                        consecutiveFailures = 0;

                        byte[]? buf = null;
                        try
                        {
                            buf = mat.ImEncode(".png");
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

                try
                {
                    _internalCts?.Cancel();
                }
                catch { }
            }

            try
            {
                var t = _captureTask;
                if (t != null)
                {
                    await t.ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                OnError("Error while stopping camera: " + ex.Message);
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
                capture?.Release();
            }
            catch { }

            try
            {
                capture?.Dispose();
            }
            catch { }

            capture = null;

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
            if (_disposed)
                return;

            _disposed = true;

            try { _internalCts?.Cancel(); } catch { }

            try
            {
                _captureTask?.Wait(500);
            }
            catch { }

            CleanupCapture();
        }
    }
}
