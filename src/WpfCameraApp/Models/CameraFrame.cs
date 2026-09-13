namespace WpfCameraApp.Models
{
    public sealed record CameraFrame(byte[] PixelData, int Width, int Height);
}
