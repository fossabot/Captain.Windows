using System;

namespace Captain.Common {
  /// <summary>
  ///   Codec that may take Direct3D 11 textures as input, in addition to raw bitmap data.
  /// </summary>
  public interface ID3D11Codec {
    /// <summary>
    ///   Gets or sets the Direct3D surface pointer to be passed to the underlying encoder.
    /// </summary>
    IntPtr SurfacePointer { get; set; }
  }
}