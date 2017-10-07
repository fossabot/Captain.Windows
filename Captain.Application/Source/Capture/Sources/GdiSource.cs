using System;
using System.Drawing;
using Captain.Application.Native;

namespace Captain.Application {
  internal class GdiSource : CaptureSource {
    /// <summary>
    ///   Attached window handle
    /// </summary>
    private readonly IntPtr windowHandle;

    /// <summary>
    ///   Window drawing context
    /// </summary>
    private IntPtr drawCtx;

    /// <summary>
    ///   Destination drawing context
    /// </summary>
    private IntPtr destCtx;

    /// <summary>
    ///   Destination bitmap handle
    /// </summary>
    private IntPtr bitmapHandle;

    /// <summary>
    ///   Instantiates this class
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="initialArea">Initial screen region</param>
    public GdiSource(IntPtr handle, Rectangle initialArea) : base(initialArea) {
      this.windowHandle = handle == IntPtr.Zero ? User32.GetDesktopWindow() : handle;
      this.drawCtx = User32.GetWindowDC(this.windowHandle);
      this.destCtx = Gdi32.CreateCompatibleDC(this.drawCtx);
      this.bitmapHandle = Gdi32.CreateCompatibleBitmap(this.drawCtx, initialArea.Width, initialArea.Height);
    }

    /// <summary>
    ///   Acquires a single video frame
    /// </summary>
    /// <returns>The frame Bitmap</returns>
    internal override Bitmap AcquireVideoFrame() {
      Gdi32.SelectObject(this.destCtx, this.bitmapHandle);
      Gdi32.BitBlt(this.destCtx, 0, 0, Area.Width, Area.Height, this.drawCtx,
        this.windowHandle == IntPtr.Zero ? Area.X : 0,
        this.windowHandle == IntPtr.Zero ? Area.Y : 0,
        Gdi32.TernaryRasterOperations.SRCCOPY);

      Bitmap bmp = Image.FromHbitmap(this.bitmapHandle);

      User32.ReleaseDC(this.windowHandle, this.drawCtx);
      this.drawCtx = IntPtr.Zero;

      User32.ReleaseDC(this.windowHandle, this.destCtx);
      this.destCtx = IntPtr.Zero;

      Gdi32.DeleteObject(this.bitmapHandle);
      this.bitmapHandle = IntPtr.Zero;

      return bmp;
    }

    /// <summary>
    ///   Releases resources
    /// </summary>
    public override void Dispose() {
      if (this.drawCtx != IntPtr.Zero) { User32.ReleaseDC(this.windowHandle, this.drawCtx); }
      if (this.destCtx != IntPtr.Zero) { User32.ReleaseDC(this.windowHandle, this.destCtx); }
      if (this.bitmapHandle != IntPtr.Zero) { Gdi32.DeleteObject(this.bitmapHandle); }
    }
  }
}