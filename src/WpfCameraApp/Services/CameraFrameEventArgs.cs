using System;
using WpfCameraApp.Models;

namespace WpfCameraApp.Services
{
    public sealed class CameraFrameEventArgs : EventArgs
    {
        public CameraFrame Frame { get; }

        public CameraFrameEventArgs(CameraFrame frame)
        {
            Frame = frame;
        }
    }
}
