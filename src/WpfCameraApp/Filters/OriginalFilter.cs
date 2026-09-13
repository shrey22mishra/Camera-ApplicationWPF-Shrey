using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class OriginalFilter : IImageFilter
    {
        public Mat Apply(Mat source)
        {
            return source.Clone();
        }
    }
}
