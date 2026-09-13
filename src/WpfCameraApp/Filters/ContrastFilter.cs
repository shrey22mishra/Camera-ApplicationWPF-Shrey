using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class ContrastFilter : IImageFilter
    {
        private readonly double _contrastFactor;

        public ContrastFilter(double contrastFactor)
        {
            _contrastFactor = contrastFactor;
        }

        public Mat Apply(Mat source)
        {
            var dst = new Mat();
            source.ConvertTo(dst, MatType.CV_8UC3, _contrastFactor, 0);
            return dst;
        }
    }
}
