using System.Text;
using System.Threading.Tasks;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfCameraApp.ViewModels;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class FaceIntegrationViewModelTests
    {
        [TestMethod]
        public async Task When_DetectFaces_True_ViewModel_Updates_FaceRects()
        {
            var fakeCamera = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var faceSvc = new FakeFaceDetectionService();
            // Return one face at (10,10) size 20x20
            faceSvc.ToReturn = new[] { new WpfCameraApp.Models.Face(10, 10, 20, 20) };

            var vm = new MainViewModel(fakeCamera, filters, faceSvc);
            vm.PreviewImageActualWidth = 200; // simulate rendering size
            vm.PreviewImageActualHeight = 200;
            vm.DetectFaces = true;

            byte[] sample = Encoding.ASCII.GetBytes("TEST");
            fakeCamera.SimulateFrame(sample, 100, 100);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (vm.FaceRects.Count == 0 && sw.ElapsedMilliseconds < 2000)
            {
                await Task.Delay(50);
            }

            Assert.IsTrue(vm.FaceRects.Count >= 1);
            var df = vm.FaceRects[0];
            // With control 200x200 and frame 100x100, ratio=2, offset 0 -> display face at 20,20 size 40x40
            Assert.AreEqual(20, (int)df.X);
            Assert.AreEqual(20, (int)df.Y);
            Assert.AreEqual(40, (int)df.Width);
            Assert.AreEqual(40, (int)df.Height);
        }

        [TestMethod]
        public async Task When_DetectFaces_True_ViewModel_Shows_All_Faces()
        {
            var camera = new FakeCameraService();
            var faces = new FakeFaceDetectionService
            {
                ToReturn = new[]
                {
                    new WpfCameraApp.Models.Face(10, 10, 20, 20),
                    new WpfCameraApp.Models.Face(50, 30, 15, 15)
                }
            };
            var vm = new MainViewModel(camera, new FakeImageFilterService(), faces)
            {
                PreviewImageActualWidth = 200,
                PreviewImageActualHeight = 200,
                DetectFaces = true
            };

            camera.SimulateFrame(Encoding.ASCII.GetBytes("TEST"), 100, 100);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (vm.FaceRects.Count < 2 && sw.ElapsedMilliseconds < 2000)
                await Task.Delay(50);

            Assert.AreEqual(2, vm.FaceRects.Count);
        }

        [TestMethod]
        public async Task Stop_Clears_Face_Overlays()
        {
            var camera = new FakeCameraService();
            var faces = new FakeFaceDetectionService
            {
                ToReturn = new[] { new WpfCameraApp.Models.Face(10, 10, 20, 20) }
            };
            var vm = new MainViewModel(camera, new FakeImageFilterService(), faces)
            {
                PreviewImageActualWidth = 200,
                PreviewImageActualHeight = 200,
                DetectFaces = true
            };

            await vm.StartCommand.ExecuteAsync();
            camera.SimulateFrame(Encoding.ASCII.GetBytes("TEST"), 100, 100);

            var sw = System.Diagnostics.Stopwatch.StartNew();
            while (vm.FaceRects.Count == 0 && sw.ElapsedMilliseconds < 2000)
                await Task.Delay(50);

            await vm.StopCommand.ExecuteAsync();

            Assert.AreEqual(0, vm.FaceRects.Count);
        }
    }
}
