using System;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Models;

namespace WpfCameraApp.Services.Interfaces
{
    public interface ICameraService : IDisposable
    {
        Task StartAsync(CancellationToken cancellationToken = default);

        Task StopAsync();

        bool IsRunning { get; }

        event EventHandler<CameraFrameEventArgs>? FrameArrived;

        event EventHandler<string>? ErrorOccurred;
    }
}
