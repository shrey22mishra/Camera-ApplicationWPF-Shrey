using OpenCvSharp;

namespace WpfCameraApp.Services.Interfaces
{
    public interface IVideoCaptureFactory
    {
        VideoCapture Create(int index = 0);
    }
}
