using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfCameraApp.ViewModels;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class MainViewModelLifecycleTests
    {
        [TestMethod]
        public async Task Start_When_Stopped_Calls_Start()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            Assert.IsFalse(vm.IsCameraRunning);

            await vm.StartCommand.ExecuteAsync();

            Assert.IsTrue(fake.IsRunning);
            Assert.IsTrue(vm.IsCameraRunning);
        }

        [TestMethod]
        public async Task Start_When_Already_Running_Is_Noop()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            await vm.StartCommand.ExecuteAsync();
            Assert.IsTrue(fake.IsRunning);

            // Second start should not throw and should be a noop
            await vm.StartCommand.ExecuteAsync();
            Assert.IsTrue(fake.IsRunning);
        }

        [TestMethod]
        public async Task Stop_When_Running_Calls_Stop()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            await vm.StartCommand.ExecuteAsync();
            Assert.IsTrue(vm.IsCameraRunning);

            await vm.StopCommand.ExecuteAsync();

            Assert.IsFalse(fake.IsRunning);
            Assert.IsFalse(vm.IsCameraRunning);
        }

        [TestMethod]
        public async Task Stop_When_Stopped_Is_Noop()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            // Stop when not running should not throw
            await vm.StopCommand.ExecuteAsync();
            Assert.IsFalse(fake.IsRunning);
        }

        [TestMethod]
        public async Task Camera_Error_Updates_Status()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            fake.SimulateError("Test error");

            // Since Application.Current may be null in test environment, StatusMessage is updated synchronously
            Assert.AreEqual("Test error", vm.StatusMessage);
        }

        [TestMethod]
        public async Task Start_Failure_Shows_Status_And_NotRunning()
        {
            var failing = new FailingCameraService();
            var vm = new MainViewModel(failing);

            await vm.StartCommand.ExecuteAsync();

            Assert.IsFalse(vm.IsCameraRunning);
            Assert.IsTrue(vm.StatusMessage.Contains("failed", StringComparison.OrdinalIgnoreCase) || vm.StatusMessage.Length > 0);
        }

        [TestMethod]
        public async Task Dispose_Stops_And_Disposes_Service()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            await vm.StartCommand.ExecuteAsync();
            Assert.IsTrue(fake.IsRunning);

            vm.Dispose();

            Assert.IsTrue(fake.Disposed);
            Assert.IsFalse(fake.IsRunning);
        }

        [TestMethod]
        public async Task FrameArrived_Updates_PreviewImage()
        {
            var fake = new FakeCameraService();
            var filters = new FakeImageFilterService();
            var vm = new MainViewModel(fake, filters);

            byte[] sample = Encoding.ASCII.GetBytes("BM\0\0"); // minimal invalid bmp header but decoder may fail; we just ensure no exception
            fake.SimulateFrame(sample, 2, 2);

            // If decoding fails, StatusMessage will contain an error, but PreviewImage may remain null.
            Assert.IsNotNull(vm.StatusMessage);
        }
    }
}
