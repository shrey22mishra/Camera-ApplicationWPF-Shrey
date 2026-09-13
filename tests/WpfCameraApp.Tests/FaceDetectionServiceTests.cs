using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WpfCameraApp.Services;

namespace WpfCameraApp.Tests
{
    [TestClass]
    public class FaceDetectionServiceTests
    {
        [TestMethod]
        public void DetectFaces_When_Classifier_Missing_Returns_Empty()
        {
            var service = new FaceDetectionService(classifierPath: "non-existent-file.xml");

            var results = service.DetectFaces(new byte[] { 1, 2, 3 });

            Assert.IsNotNull(results);
            Assert.AreEqual(0, System.Linq.Enumerable.Count(results));
        }

        [TestMethod]
        public void DetectFaces_With_Invalid_Classifier_File_Is_Graceful()
        {
            var tmp = Path.GetTempFileName();
            File.WriteAllText(tmp, "not-xml-or-classifier");

            try
            {
                var service = new FaceDetectionService(classifierPath: tmp);
                var results = service.DetectFaces(new byte[] { 1, 2, 3 });

                Assert.IsNotNull(results);
                Assert.AreEqual(0, System.Linq.Enumerable.Count(results));
            }
            finally
            {
                File.Delete(tmp);
            }
        }
    }
}
