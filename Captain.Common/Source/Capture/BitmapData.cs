using System;

namespace Captain.Common
{
  /// <summary>
  ///   Contains information about a raw bitmap
  /// </summary>
  public struct BitmapData {
    /// <summary>
    ///   Number of pixels in a scanline.
    /// </summary>
    public int Width;

    /// <summary>
    ///   Number of scanlines.
    /// </summary>
    public int Height;

    /// <summary>
    ///   Pixel format GUID (used by WIC).
    /// </summary>
    public Guid PixelFormat;

    /// <summary>
    ///   Pixel format ID (used by GDI).
    /// </summary>
    public int PixelFormatId;

    /// <summary>
    ///   Address of the first scanline.
    /// </summary>
    public IntPtr Scan0;

    /// <summary>
    ///   Number of bytes for each scanline.
    /// </summary>
    public int Stride;

    /// <summary>
    ///   Pointer to the IWICBitmapLock COM object
    /// </summary>
    public IntPtr LockPointer;
  }
}
