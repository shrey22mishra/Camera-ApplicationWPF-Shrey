using System.Collections.Generic;
using WpfCameraApp.Models;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Tests
{
    public class FakeFaceDetectionService : IFaceDetectionService
    {
        public byte[]? LastFrame { get; private set; }
        public IEnumerable<Face> ToReturn { get; set; } = new Face[0];

        public IEnumerable<Face> DetectFaces(byte[] frameData)
        {
            LastFrame = frameData;
            return ToReturn;
        }
    }
}
