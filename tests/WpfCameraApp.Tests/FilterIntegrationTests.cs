using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfCameraApp.ViewModels;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class FilterIntegrationTests
    {
        [TestMethod]
        public void SelectedFilter_Property_Updates_Service()
        {
            var fakeCamera = new FakeCameraService();
            var filters = new FakeImageFilterService();

            var vm = new MainViewModel(fakeCamera, filters);

            vm.SelectedFilter = "Grayscale";

            Assert.AreEqual("Grayscale", filters.SelectedFilter);
        }

        [TestMethod]
        public void Reset_Restores_Defaults_And_Calls_Service()
        {
            var fakeCamera = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fakeCamera, filters);

            vm.SelectedFilter = "Brightness";
            vm.Brightness = 42.0;

            vm.ResetCommand.Execute(null);

            Assert.IsTrue(filters.ResetCalled);
            Assert.AreEqual("Original", vm.SelectedFilter);
            Assert.AreEqual(0.0, vm.Brightness);
            Assert.AreEqual(1.0, vm.Contrast);
            Assert.AreEqual(0.0, vm.Tint);
        }

        [TestMethod]
        public async Task FrameArrival_Invokes_FilterService()
        {
            var fakeCamera = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var faceSvc = new FakeFaceDetectionService();
            var vm = new MainViewModel(fakeCamera, filters, faceSvc);

            byte[] sample = Encoding.ASCII.GetBytes("TEST");
            fakeCamera.SimulateFrame(sample, 2, 2);

            // Wait for the background processing to observe LastApplied
            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (filters.LastApplied == null && sw.ElapsedMilliseconds < 2000)
            {
                await Task.Delay(50);
            }

            Assert.IsNotNull(filters.LastApplied);
            CollectionAssert.AreEqual(sample, filters.LastApplied);
            // No faces returned by fake face service default, so FaceRects should be empty
            Assert.AreEqual(0, vm.FaceRects.Count);
        }
    }
}
