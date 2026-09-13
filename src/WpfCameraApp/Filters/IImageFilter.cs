using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public interface IImageFilter
    {
        Mat Apply(Mat source);
    }
}
