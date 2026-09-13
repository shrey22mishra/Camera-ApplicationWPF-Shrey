using System;
using System;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Models;
using WpfCameraApp.Services;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Tests
{
    public class FailingCameraService : ICameraService
    {
        public event EventHandler<CameraFrameEventArgs>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Hardware not available");
        }

        public Task StopAsync()
        {
            IsRunning = false;
            return Task.CompletedTask;
        }

        public void Dispose()
        {
        }
    }
}
