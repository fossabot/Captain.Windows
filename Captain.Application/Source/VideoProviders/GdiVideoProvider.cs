using System;
using System.Drawing;
using System.Drawing.Imaging;
using Captain.Application.Native;
using Captain.Common;
using BitmapData = Captain.Common.BitmapData;
using static Captain.Application.Application;
using Guid = System.Guid;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides video capture capabilities using GDI, for its use in platforms that do not support desktop duplication
  /// </summary>
  /// <remarks>
  ///   TODO: Use PrintWindow API when a window handle is provided
  /// </remarks>
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
      Log.WriteLine(LogLevel.Debug, "creating GDI video provider");
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
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="T:Captain.Common.BitmapData" /> containing raw bitmap information</returns>
    public override BitmapData LockFrameBitmap() {
      Bitmap bmp = Image.FromHbitmap(this.bitmapHandle);
      System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(Point.Empty, bmp.Size),
        ImageLockMode.ReadWrite,
        bmp.PixelFormat);

      return new BitmapData {
        Height = bmp.Height,
        Scan0 = data.Scan0,
        Width = bmp.Width,
        PixelFormat = Guid.Empty,
        Stride = data.Stride,
        PixelFormatId = (int) bmp.PixelFormat,
        LockPointer = new IntPtr(data.Reserved) // HACK: we're taking advantage of this unused field here - NON-RELATED!
      };
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the bitmap created for this frame
    /// </summary>
    /// <param name="data">Bitmap data returned by the <see cref="M:Captain.Common.VideoProvider.LockFrameBitmap" /> method</param>
    public override void UnlockFrameBitmap(BitmapData data) =>
      Image.FromHbitmap(this.bitmapHandle)
        .UnlockBits(new System.Drawing.Imaging.BitmapData {
          Height = data.Height,
          Scan0 = data.Scan0,
          PixelFormat = (PixelFormat) data.PixelFormatId,
          Width = data.Width,
          Stride = data.Stride,
          Reserved = data.LockPointer.ToInt32()
        });

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() => ReleaseFrame();
  }
}