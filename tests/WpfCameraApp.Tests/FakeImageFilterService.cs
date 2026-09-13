using System.Threading;
using System.Threading.Tasks;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Tests
{
    public class FakeImageFilterService : IImageFilterService
    {
        public string SelectedFilter { get; set; } = "Original";
        public double Brightness { get; set; } = 0.0;
        public double Contrast { get; set; } = 1.0;
        public double Tint { get; set; } = 0.0;

        public bool ResetCalled { get; private set; }
        public byte[]? LastApplied { get; private set; }

        public byte[]? Reset()
        {
            ResetCalled = true;
            SelectedFilter = "Original";
            Brightness = 0.0;
            Contrast = 1.0;
            Tint = 0.0;
            return null;
        }

        public Task<byte[]> ApplyFilterAsync(byte[] encodedFrame, CancellationToken cancellationToken = default)
        {
            LastApplied = encodedFrame;
            // Return the same frame for test simplicity
            return Task.FromResult(encodedFrame);
        }
    }
}
