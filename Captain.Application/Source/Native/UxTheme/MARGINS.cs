using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Returned by the GetThemeMargins function to define the margins of windows that have visual styles applied.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct MARGINS {
    /// <summary>
    ///   Width of the left border that retains its size.
    /// </summary>
    internal int leftWidth;

    /// <summary>
    ///   Width of the right border that retains its size.
    /// </summary>
    internal int rightWidth;

    /// <summary>
    ///   Height of the top border that retains its size.
    /// </summary>
    internal int topWidth;

    /// <summary>
    ///   Height of the bottom border that retains its size.
    /// </summary>
    internal int bottomWidth;
  }
}