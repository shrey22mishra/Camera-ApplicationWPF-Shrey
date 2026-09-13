using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class GrayscaleFilter : IImageFilter
    {
        public Mat Apply(Mat source)
        {
            var gray = new Mat();
            try
            {
                Cv2.CvtColor(source, gray, ColorConversionCodes.BGR2GRAY);
                var bgr = new Mat();
                Cv2.CvtColor(gray, bgr, ColorConversionCodes.GRAY2BGR);
                return bgr;
            }
            finally
            {
                gray.Dispose();
            }
        }
    }
}
