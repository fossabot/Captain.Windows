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
    public Rectangle CaptureBounds { get; protected set; } // ReSharper disable UnusedParameter.Local

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
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="BitmapData"/> containing raw bitmap information</returns>
    public abstract BitmapData LockFrameBitmap();

    /// <summary>
    ///   Releases the bitmap created for this frame
    /// </summary>
    /// <param name="data">Bitmap data returned by the <see cref="LockFrameBitmap"/> method</param>
    public abstract void UnlockFrameBitmap(BitmapData data);

    /// <summary>
    ///   Updates the video provider capture position
    /// </summary>
    /// <remarks>
    ///   This is possible because updating the position does not affect allocated buffers, textures, etc.
    ///   As long as <paramref name="x" /> and <paramref name="y" /> are within the screen bounds, updating the position
    ///   should not affect capture.
    /// </remarks>
    /// <param name="x">New X axis value</param>
    /// <param name="y">New Y axis value</param>
    public virtual void UpdatePosition(int x, int y) =>
      CaptureBounds = new Rectangle(new Point(x, y), CaptureBounds.Size);

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose();
  }
}