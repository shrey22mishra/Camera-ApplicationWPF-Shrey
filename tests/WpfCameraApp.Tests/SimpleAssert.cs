using System;

namespace WpfCameraApp.Tests
{
    // Minimal assertion helper used to keep tests simple in this environment.
    public static class SimpleAssert
    {
        public static void IsTrue(bool condition)
        {
            if (!condition) throw new Exception("Assert.IsTrue failed");
        }

        public static void IsFalse(bool condition)
        {
            if (condition) throw new Exception("Assert.IsFalse failed");
        }

        public static void AreEqual<T>(T expected, T actual)
        {
            if (!Equals(expected, actual))
                throw new Exception($"Assert.AreEqual failed. Expected: {expected}, Actual: {actual}");
        }
    }
}
