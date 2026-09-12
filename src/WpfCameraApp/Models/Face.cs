namespace WpfCameraApp.Models
{
    // Simple value object representing a detected face bounding box.
    public sealed record Face(int X, int Y, int Width, int Height);
}
