using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class SepiaFilter : IImageFilter
    {
        public Mat Apply(Mat source)
        {
            var dst = new Mat();
            var kernel = new float[3, 3]
            {
                { 0.272f, 0.534f, 0.131f },
                { 0.349f, 0.686f, 0.168f },
                { 0.393f, 0.769f, 0.189f }
            };

            using var matKernel = Mat.FromArray(kernel);
            try
            {
                Cv2.Transform(source, dst, matKernel);
                var dst8 = new Mat();
                dst.ConvertTo(dst8, MatType.CV_8UC3);
                dst.Dispose();
                return dst8;
            }
            catch
            {
                dst.Dispose();
                throw;
            }
        }
    }
}
