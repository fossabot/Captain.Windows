using System.Runtime.InteropServices;

// ReSharper disable All
namespace Captain.Application.Native {
  /// <summary>
  /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
  /// </summary>
  /// <remarks>
  /// By convention, the right and bottom edges of the rectangle are normally considered exclusive.
  /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the rectangle.
  /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including,
  /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
  /// </remarks>
  [StructLayout(LayoutKind.Sequential)]
  internal struct RECT {
    /// <summary>
    /// The x-coordinate of the upper-left corner of the rectangle.
    /// </summary>
    internal int left;

    /// <summary>
    /// The y-coordinate of the upper-left corner of the rectangle.
    /// </summary>
    internal int top;

    /// <summary>
    /// The x-coordinate of the lower-right corner of the rectangle.
    /// </summary>
    internal int right;

    /// <summary>
    /// The y-coordinate of the lower-right corner of the rectangle.
    /// </summary>
    internal int bottom;
  }
}
