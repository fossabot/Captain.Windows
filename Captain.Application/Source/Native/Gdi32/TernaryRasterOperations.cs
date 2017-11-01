using System;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  internal static partial class Gdi32 {
    /// <summary>
    ///   Ternary raster operations used by GDI bit block transfer functions
    /// </summary>
    [Flags]
    internal enum TernaryRasterOperations {
      /// <summary>
      ///   dest = source
      /// </summary>
      SRCCOPY = 0x00CC0020
    }
  }
}