using System;
using System;
using System.Threading;
using System.Threading.Tasks;
using OpenCvSharp;
using WpfCameraApp.Services.Interfaces;
using WpfCameraApp.Filters;

namespace WpfCameraApp.Services
{
    public class ImageFilterService : IImageFilterService
    {
        public ImageFilterService()
        {
            Brightness = 0.0;
            Contrast = 1.0;
            Tint = 0.0;

            SelectedFilter = "Original";
        }

        public string SelectedFilter { get; set; }

        public double Brightness { get; set; }

        public double Contrast { get; set; }

        public double Tint { get; set; }

        public byte[]? Reset()
        {
            SelectedFilter = "Original";
            Brightness = 0.0;
            Contrast = 1.0;
            Tint = 0.0;
            return null;
        }

        public Task<byte[]> ApplyFilterAsync(byte[] encodedFrame, CancellationToken cancellationToken = default)
        {
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();

                using var mat = Cv2.ImDecode(encodedFrame, ImreadModes.Color);
                if (mat == null || mat.Empty())
                {
                    return encodedFrame;
                }

                cancellationToken.ThrowIfCancellationRequested();

                using var baseResult = CreateBaseFilter().Apply(mat);
                cancellationToken.ThrowIfCancellationRequested();

                using var brightnessResult = new BrightnessFilter(Brightness).Apply(baseResult);
                cancellationToken.ThrowIfCancellationRequested();

                using var contrastResult = new ContrastFilter(Contrast).Apply(brightnessResult);
                cancellationToken.ThrowIfCancellationRequested();

                using var tintResult = new TintFilter(Tint).Apply(contrastResult);
                return tintResult.ImEncode(".png");
            }, cancellationToken);
        }

        private IImageFilter CreateBaseFilter()
        {
            return SelectedFilter switch
            {
                "Grayscale" => new GrayscaleFilter(),
                "Sepia" => new SepiaFilter(),
                _ => new OriginalFilter()
            };
        }
    }
}
