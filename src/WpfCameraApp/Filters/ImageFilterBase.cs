namespace WpfCameraApp.Filters
{
    public abstract class ImageFilterBase
    {
        public abstract byte[] Process(byte[] frameData);
    }
}
