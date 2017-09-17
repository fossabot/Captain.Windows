using System;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Helper class which abstracts screen capture
  /// </summary>
  internal class CaptureHelper : IDisposable {
    /// <summary>
    ///   Underlying capture source
    /// </summary>
    private CaptureSource source;

    /// <summary>
    ///   Attach a window
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="bounds">Window bounds</param>
    internal void AttachWindow(IntPtr handle, Rectangle bounds) {
      if (this.source is null) {
        // try to initialize DXGI
        try {
          // TODO: implement DXGI support
          throw new NotImplementedException();
        } catch {
          // fall back to GDI
          this.source = new GdiSource(handle, bounds);
        }
      } else if (this.source is GdiSource) {
        this.source.Dispose();
        this.source = new GdiSource(handle, bounds);
      } else if (this.source is DesktopDuplicationSource) {
        // TODO
        throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Detaches a window and binds to an screen area instead
    /// </summary>
    /// <param name="area">Screen area</param>
    internal void DetachWindow(Rectangle area) {
      if (this.source is GdiSource) {
        this.source.Dispose();
        this.source = new GdiSource(IntPtr.Zero, area);
      } else if (this.source is DesktopDuplicationSource) {
        // TODO
        throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Captures a bitmap from screen
    /// </summary>
    /// <param name="area">Virtual desktop area</param>
    /// <returns>A Bitmap containing the selected screen region</returns>
    internal Bitmap CaptureFromScreen(Rectangle area) {
      if (this.source is null) {
        try {
          // TODO
          throw new NotImplementedException();
#if false
          // use DXGI desktop duplication when supported
          (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] outputInfo =
            DisplayHelper.GetOutputInfoFromRect(area);

          foreach (var output in outputInfo) {
            var source = new DesktopDuplicationSource(output.Bounds, output.AdapterIndex,
              output.OutputIndex);

            var bitmap = new Bitmap(output.Bounds.Width, output.Bounds.Height);
            var bitmapData = bitmap.LockBits(output.Bounds, ImageLockMode.WriteOnly, PixelFormat.Format32bppRgb);

            // capture actual frame
            this.source.AcquireVideoFrame(bitmapData.Scan0);

            // TODO: merge bitmaps
            return bitmap;
        }
#endif
        } catch {
          // fall back to GDI
          this.source = new GdiSource(IntPtr.Zero, area);
        }
      } else if (!this.source.Area.Equals(area)) {
        this.source.Area = area;
      }

      return this.source.AcquireVideoFrame();
    }

    /// <summary>
    ///   Class destructor
    /// </summary>
    ~CaptureHelper() => ReleaseUnmanagedResources();

    /// <summary>
    ///   Releases unmanaged resources
    /// </summary>
    private void ReleaseUnmanagedResources() => this.source?.Dispose();

    /// <summary>
    ///   Releases resources
    /// </summary>
    public void Dispose() {
      ReleaseUnmanagedResources();
      GC.SuppressFinalize(this);
    }
  }
}
