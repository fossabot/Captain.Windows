using System;
using System.Drawing;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides graphical output from display devices
  /// </summary>
  public abstract class VideoProvider : IDisposable {
    /// <summary>
    ///   Capture bounds rectangle
    /// </summary>
    public Rectangle CaptureBounds { get; protected set; }

    // ReSharper disable UnusedParameter.Local
    /// <summary>
    ///   Creates a new provider instance
    /// </summary>
    /// <param name="captureBounds">Capture region</param>
    /// <param name="windowHandle">Attached window handle</param>
    protected VideoProvider(Rectangle captureBounds, IntPtr? windowHandle = null) => CaptureBounds = captureBounds;

    /// <summary>
    ///   Acquires a single frame from this provider
    /// </summary>
    public abstract void AcquireFrame();

    /// <summary>
    ///   Releases last captured frame resources
    /// </summary>
    public abstract void ReleaseFrame();

    /// <summary>
    ///   Creates a <see cref="Bitmap"/> instance from the last captured frame
    /// </summary>
    /// <returns>A <see cref="Bitmap"/> instance</returns>
    public abstract Bitmap CreateFrameBitmap();

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose();
  }
}
