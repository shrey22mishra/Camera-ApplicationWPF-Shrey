using OpenCvSharp;

namespace WpfCameraApp.Filters
{
    public class TintFilter : IImageFilter
    {
        private readonly double _tintAmount;

        public TintFilter(double tintAmount)
        {
            _tintAmount = tintAmount;
        }

        public Mat Apply(Mat source)
        {
            var dst = source.Clone();

            double add = _tintAmount * 80.0;

            double bAdd = 0, gAdd = 0, rAdd = 0;
            if (add >= 0)
            {
                rAdd = add;
            }
            else
            {
                bAdd = -add;
            }

            var scalar = new Scalar(bAdd, gAdd, rAdd);
            Cv2.Add(dst, scalar, dst);
            return dst;
        }
    }
}
