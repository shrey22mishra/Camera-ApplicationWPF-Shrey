namespace WpfCameraApp.Models
{
    // Minimal model representing a captured camera frame.
    public sealed record CameraFrame(byte[] PixelData, int Width, int Height);
}
