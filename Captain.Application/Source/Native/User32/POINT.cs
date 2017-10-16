// ReSharper disable All

using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  /// The POINT structure defines the x- and y- coordinates of a point.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct POINT {
    /// <summary>
    ///  The x-coordinate of the point.
    /// </summary>
    internal int x;

    /// <summary>
    /// The x-coordinate of the point.
    /// </summary>
    internal int y;
  }
}