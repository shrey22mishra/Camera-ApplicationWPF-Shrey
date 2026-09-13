using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenCvSharp;
using WpfCameraApp.Models;
using WpfCameraApp.Services.Interfaces;

namespace WpfCameraApp.Services
{
    public class FaceDetectionService : IFaceDetectionService, IDisposable
    {
        private readonly CascadeClassifier? _classifier;
        private readonly bool _enabled;

        public FaceDetectionService(string? classifierPath = null)
        {
            try
            {
                var path = classifierPath;
                if (string.IsNullOrWhiteSpace(path))
                {
                    var baseDir = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                    var candidates = new[]
                    {
                        Path.Combine(baseDir, "haarcascade_frontalface_default.xml"),
                        Path.Combine(baseDir, "Resources", "haarcascade_frontalface_default.xml"),
                        Path.Combine(baseDir, "assets", "haarcascade_frontalface_default.xml")
                    };

                    path = candidates.FirstOrDefault(File.Exists);
                }

                if (!string.IsNullOrWhiteSpace(path) && File.Exists(path))
                {
                    _classifier = new CascadeClassifier(path);
                    _enabled = !_classifier.Empty();
                }
            }
            catch
            {
                _enabled = false;
            }
        }

        public IEnumerable<Face> DetectFaces(byte[] frameData)
        {
            if (!_enabled || frameData == null || frameData.Length == 0)
                return Array.Empty<Face>();

            try
            {
                using var mat = Cv2.ImDecode(frameData, ImreadModes.Color);
                if (mat == null || mat.Empty())
                    return Array.Empty<Face>();

                using var gray = new Mat();
                Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
                Cv2.EqualizeHist(gray, gray);

                var rects = _classifier!.DetectMultiScale(
                    gray,
                    1.1,
                    5,
                    HaarDetectionTypes.ScaleImage,
                    new Size(80, 80));
                if (rects == null || rects.Length == 0)
                    return Array.Empty<Face>();

                return rects.Select(r => new Face(r.X, r.Y, r.Width, r.Height)).ToArray();
            }
            catch
            {
                return Array.Empty<Face>();
            }
        }

        public void Dispose()
        {
            try
            {
                _classifier?.Dispose();
            }
            catch { }
        }
    }
}
