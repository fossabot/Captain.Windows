using System;
using System.Drawing;
using Captain.Application.Native;
using Captain.Common;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides video capture capabilities using GDI, for its use in platforms that do not support desktop duplication
  /// </summary>
  internal sealed class GdiVideoProvider : VideoProvider {
    /// <summary>
    ///   Attached window handle
    /// </summary>
    private readonly IntPtr windowHandle;

    /// <summary>
    ///   Destination bitmap handle
    /// </summary>
    private IntPtr bitmapHandle;

    /// <summary>
    ///   Destination drawing context
    /// </summary>
    private IntPtr destCtx;

    /// <summary>
    ///   Window drawing context
    /// </summary>
    private IntPtr drawCtx;

    /// <inheritdoc />
    /// <summary>
    ///   Creates a new provider instance
    /// </summary>
    /// <param name="captureBounds">Capture region</param>
    /// <param name="handle">Attached window handle</param>
    public GdiVideoProvider(Rectangle captureBounds, IntPtr? handle = null) : base(captureBounds, handle) {
      this.windowHandle = handle ?? User32.GetDesktopWindow();
      this.drawCtx = User32.GetWindowDC(this.windowHandle);
      this.destCtx = Gdi32.CreateCompatibleDC(this.drawCtx);
      this.bitmapHandle = Gdi32.CreateCompatibleBitmap(this.drawCtx, captureBounds.Width, captureBounds.Height);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Acquires a single frame from this provider
    /// </summary>
    public override void AcquireFrame() {
      Gdi32.SelectObject(this.destCtx, this.bitmapHandle);
      Gdi32.BitBlt(this.destCtx,
                   0,
                   0,
                   CaptureBounds.Width,
                   CaptureBounds.Height,
                   this.drawCtx,
                   this.windowHandle == IntPtr.Zero ? CaptureBounds.X : 0,
                   this.windowHandle == IntPtr.Zero ? CaptureBounds.Y : 0,
                   Gdi32.TernaryRasterOperations.SRCCOPY);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases last captured frame resources
    /// </summary>
    public override void ReleaseFrame() {
      if (this.drawCtx != IntPtr.Zero) { User32.ReleaseDC(this.windowHandle, this.drawCtx); }
      if (this.destCtx != IntPtr.Zero) { User32.ReleaseDC(this.windowHandle, this.destCtx); }
      if (this.bitmapHandle != IntPtr.Zero) { Gdi32.DeleteObject(this.bitmapHandle); }

      this.drawCtx = this.destCtx = this.bitmapHandle = IntPtr.Zero;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a <see cref="T:System.Drawing.Bitmap" /> instance from the last captured frame
    /// </summary>
    /// <returns>A <see cref="T:System.Drawing.Bitmap" /> instance</returns>
    public override Bitmap CreateFrameBitmap() => Image.FromHbitmap(this.bitmapHandle);

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() => ReleaseFrame();
  }
}