using System.Threading;
using System.Threading.Tasks;

namespace WpfCameraApp.Services.Interfaces
{
    public interface IImageFilterService
    {
        string SelectedFilter { get; set; }
        double Brightness { get; set; }
        double Contrast { get; set; }
        double Tint { get; set; }

        byte[]? Reset();

        Task<byte[]> ApplyFilterAsync(byte[] encodedFrame, CancellationToken cancellationToken = default);
    }
}
