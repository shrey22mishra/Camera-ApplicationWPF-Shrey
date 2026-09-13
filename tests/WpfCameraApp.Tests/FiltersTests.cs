using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenCvSharp;
using WpfCameraApp.Filters;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class FiltersTests
    {
        private Mat CreateSolidBgr(byte b, byte g, byte r)
        {
            var m = new Mat(new Size(2,2), MatType.CV_8UC3, new Scalar(b, g, r));
            return m;
        }

        [TestMethod]
        public void OriginalFilter_Returns_Clone()
        {
            using var src = CreateSolidBgr(10, 20, 30);
            var f = new OriginalFilter();
            using var outMat = f.Apply(src);

            Assert.AreEqual(src.Rows, outMat.Rows);
            Assert.AreEqual(src.Cols, outMat.Cols);
            // ensure pixels equal
            var sBytes = src.ToBytes();
            var oBytes = outMat.ToBytes();
            CollectionAssert.AreEqual(sBytes, oBytes);
        }

        [TestMethod]
        public void GrayscaleFilter_Makes_Channels_Equal()
        {
            using var src = CreateSolidBgr(100, 50, 25);
            var f = new GrayscaleFilter();
            using var outMat = f.Apply(src);

            // check one pixel channels equal
            var v = outMat.Get<Vec3b>(0,0);
            Assert.AreEqual(v.Item0, v.Item1);
            Assert.AreEqual(v.Item1, v.Item2);
        }

        [TestMethod]
        public void SepiaFilter_Changes_Pixels()
        {
            using var src = CreateSolidBgr(10, 20, 30);
            var f = new SepiaFilter();
            using var outMat = f.Apply(src);

            var s = src.Get<Vec3b>(0,0);
            var o = outMat.Get<Vec3b>(0,0);
            // should not be identical
            CollectionAssert.AreNotEqual(new byte[] { s.Item0, s.Item1, s.Item2 }, new byte[] { o.Item0, o.Item1, o.Item2 });
        }

        [TestMethod]
        public void BrightnessFilter_Increases_Values()
        {
            using var src = CreateSolidBgr(10, 10, 10);
            var f = new BrightnessFilter(0.5); // +50
            using var outMat = f.Apply(src);

            var o = outMat.Get<Vec3b>(0,0);
            Assert.IsTrue(o.Item0 > 10 || o.Item1 > 10 || o.Item2 > 10);
        }

        [TestMethod]
        public void ContrastFilter_Scales_Values()
        {
            using var src = CreateSolidBgr(50, 60, 70);
            var f = new ContrastFilter(0.5); // reduce contrast
            using var outMat = f.Apply(src);

            var o = outMat.Get<Vec3b>(0,0);
            Assert.IsTrue(o.Item0 < 50 || o.Item1 < 60 || o.Item2 < 70);
        }

        [TestMethod]
        public void TintFilter_Adds_Red_When_Positive()
        {
            using var src = CreateSolidBgr(10, 10, 10);
            var f = new TintFilter(1.0);
            using var outMat = f.Apply(src);

            var o = outMat.Get<Vec3b>(0,0);
            Assert.IsTrue(o.Item2 > 10);
        }
    }
}
