using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class BrightnessFilter : IImageFilter
    {
        private readonly double _brightnessDelta;

        public BrightnessFilter(double brightnessDelta)
        {
            _brightnessDelta = brightnessDelta;
        }

        public Mat Apply(Mat source)
        {
            var dst = new Mat();
            double beta = _brightnessDelta * 100.0;
            source.ConvertTo(dst, MatType.CV_8UC3, 1.0, beta);
            return dst;
        }
    }
}
