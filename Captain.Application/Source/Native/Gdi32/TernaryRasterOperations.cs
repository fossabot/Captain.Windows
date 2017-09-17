// ReSharper disable All

namespace Captain.Application.Native {
  /// <summary>
  ///   Ternary raster operations used by GDI bit block transfer functions
  /// </summary>
  internal enum TernaryRasterOperations {
    /// <summary>
    ///   dest = source
    /// </summary>
    SRCCOPY = 0x00CC0020
  }
}
