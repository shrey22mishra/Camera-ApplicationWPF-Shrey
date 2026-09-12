namespace WpfCameraApp.Filters
{
    // Base class for image filters; concrete filters will inherit from this.
    public abstract class ImageFilterBase
    {
        public abstract byte[] Process(byte[] frameData);
    }
}
