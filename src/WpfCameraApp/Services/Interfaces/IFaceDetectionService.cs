using System.Collections.Generic;
using WpfCameraApp.Models;

namespace WpfCameraApp.Services.Interfaces
{
    public interface IFaceDetectionService
    {
        IEnumerable<Face> DetectFaces(byte[] frameData);
    }
}
