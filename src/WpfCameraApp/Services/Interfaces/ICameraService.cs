using System;
using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Models;

namespace WpfCameraApp.Services.Interfaces
{
    // Architectural contract for camera capture responsibilities.
    public interface ICameraService : IDisposable
    {
        /// <summary>
        /// Start camera capture asynchronously. Cancellation token can be used to request stop.
        /// </summary>
        Task StartAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Stop camera capture and wait for shutdown to complete.
        /// </summary>
        Task StopAsync();

        /// <summary>
        /// True when the camera capture loop is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Raised when a new frame is available. Consumers should not update UI from this event directly.
        /// </summary>
        event EventHandler<CameraFrameEventArgs>? FrameArrived;

        /// <summary>
        /// Raised on errors (initialization, capture failures, etc.).
        /// </summary>
        event EventHandler<string>? ErrorOccurred;
    }
}
