namespace WpfCameraApp.Services.Interfaces
{
    // Architectural contract for image filtering responsibilities.
    public interface IImageFilterService
    {
        // Accepts raw frame data and returns processed frame data.
        // Details of the pixel format are intentionally unspecified at this stage.
        byte[] ApplyFilters(byte[] frameData);
    }
}
