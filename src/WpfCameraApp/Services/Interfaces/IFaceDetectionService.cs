using System.Collections.Generic;
using WpfCameraApp.Models;

namespace WpfCameraApp.Services.Interfaces
{
    // Architectural contract for face detection responsibilities.
    public interface IFaceDetectionService
    {
        // Accepts raw frame data and returns detected face bounding boxes.
        IEnumerable<Face> DetectFaces(byte[] frameData);
    }
}
