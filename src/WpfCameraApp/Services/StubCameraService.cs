using System;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Models;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Services
{
    public class StubCameraService : ICameraService
    {
        public event EventHandler<CameraFrameEventArgs>? FrameArrived;
        public event EventHandler<string>? ErrorOccurred;

        public bool IsRunning { get; private set; }

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
        }
    }
}
