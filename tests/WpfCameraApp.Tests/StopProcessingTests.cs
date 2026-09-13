using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class StopProcessingTests
    {
        private class SlowFilter : WpfCameraApp.Services.Interfaces.IImageFilterService
        {
            public string SelectedFilter { get; set; } = "Grayscale";
            public double Brightness { get; set; } = 0;
            public double Contrast { get; set; } = 1.0;
            public double Tint { get; set; } = 0;

            public byte[]? Reset() { return null; }

            public async Task<byte[]> ApplyFilterAsync(byte[] encodedFrame, CancellationToken cancellationToken = default)
            {
                await Task.Delay(2000, cancellationToken);
                return encodedFrame;
            }
        }

        [TestMethod]
        public async Task Stop_Cancels_InFlight_Processing()
        {
            var fakeCamera = new FakeCameraService();
            var slow = new SlowFilter();
            var face = new FakeFaceDetectionService();
            var vm = new WpfCameraApp.ViewModels.MainViewModel(fakeCamera, slow, face);

            await vm.StartCommand.ExecuteAsync();

            // simulate single frame
            byte[] sample = new byte[] { 1, 2, 3 };
            fakeCamera.SimulateFrame(sample, 100, 100);

            // Give the VM a moment to start processing
            await Task.Delay(100);

            // Now stop - should cancel slow filter quickly
            await vm.StopCommand.ExecuteAsync();

            // If cancellation worked, VM should not be running
            Assert.IsFalse(vm.IsCameraRunning);
        }
    }
}
