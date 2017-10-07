// ReSharper disable All

namespace Captain.Application.Native {
  internal static partial class Gdi32 {
    /// <summary>
    ///   Describes the values that may be returned from <see cref="GetDeviceCaps(System.IntPtr, DeviceCaps)"/>.
    /// </summary>
    internal enum DeviceCaps {
      /// <summary>
      ///   Logical pixels inch in X
      /// </summary>
      LOGPIXELSX = 88,

      /// <summary>
      ///   Horizontal width in pixels
      /// </summary>
      HORZRES = 8,

      /// <summary>
      ///   Horizontal width of entire desktop in pixels
      /// </summary>
      DESKTOPHORZRES = 118
    }
  }
}
