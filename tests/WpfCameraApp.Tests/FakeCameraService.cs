using System;
using System;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Models;
using WpfCameraApp.Services;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Tests
{
    // Test-friendly fake camera service: synchronous Start/Stop and helpers to simulate events.
    public class FakeCameraService : ICameraService
    {
        public event EventHandler<CameraFrameEventArgs>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning { get; private set; }

        public bool Disposed { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            IsRunning = true;
            return Task.CompletedTask;
        }

        public Task StopAsync()
        {
            IsRunning = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            Disposed = true;
        }

        public void SimulateFrame(byte[] data, int width, int height)
        {
            FrameArrived?.Invoke(this, new CameraFrameEventArgs(new CameraFrame(data, width, height)));
        }

        public void SimulateError(string message)
        {
            ErrorOccurred?.Invoke(this, message);
        }
    }
}
