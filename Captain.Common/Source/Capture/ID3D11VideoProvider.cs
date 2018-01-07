using System;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides graphical output from display devices in form of bitmaps and
  ///   Direct3D 11 surfaces, allowing for fully accelerated video encoding.
  /// </summary>
  public interface ID3D11VideoProvider : IBitmapVideoProvider {
    /// <summary>
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="BitmapData" /> containing raw bitmap information</returns>
    IntPtr SurfacePointer { get; }
  }
}