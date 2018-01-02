using System;
using System.Drawing;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides graphical output from display devices in form of bitmaps
  /// </summary>
  public interface IBitmapVideoProvider : IDisposable {
    /// <summary>
    ///   Capture bounds rectangle
    /// </summary>
    Rectangle CaptureBounds { get; }

    /// <summary>
    ///   Ticks representing the last frame time.
    /// </summary>
    long LastPresentTime { get; }

    /// <summary>
    ///   Acquires a single frame from this provider
    /// </summary>
    void AcquireFrame();

    /// <summary>
    ///   Releases last captured frame resources
    /// </summary>
    void ReleaseFrame();

    /// <summary>
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="BitmapData"/> containing raw bitmap information</returns>
    BitmapData LockFrameBitmap();

    /// <summary>
    ///   Releases the bitmap created for this frame
    /// </summary>
    /// <param name="data">Bitmap data returned by the <see cref="LockFrameBitmap"/> method</param>
    void UnlockFrameBitmap(BitmapData data);
  }
}