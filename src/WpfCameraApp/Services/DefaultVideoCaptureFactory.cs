using OpenCvSharp;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Services
{
    public class DefaultVideoCaptureFactory : IVideoCaptureFactory
    {
        private readonly int _index;
        public DefaultVideoCaptureFactory(int index = 0) => _index = index;
        public VideoCapture Create(int index = 0) => new VideoCapture(_index);
    }
}
