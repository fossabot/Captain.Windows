using System;
using System.Drawing;
using System.Linq;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Helper class which abstracts screen capture
  /// </summary>
  internal class CaptureHelper : IDisposable {
    /// <summary>
    ///   Underlying capture source
    /// </summary>
    private CaptureSource[] sources = null;

    /// <summary>
    ///   Attached window handle
    /// </summary>
    internal IntPtr WindowHandle { get; private set; }

    /// <summary>
    ///   Attach a window
    ///   TODO: refactor the whole window attachment logic and make it less abstract
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="bounds">Window bounds</param>
    internal void AttachWindow(IntPtr handle, Rectangle bounds) {
      if (this.sources is null) {
        // try to initialize DXGI
        try {
          (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] info = DisplayHelper.GetOutputInfoFromRect(bounds);
          this.sources = info.Select(i => new DxgiSource(handle, i.Bounds, i.AdapterIndex, i.OutputIndex)).ToArray();
        } catch {
          // fall back to GDI
          this.sources = new CaptureSource[] { new GdiSource(handle, bounds) };
        }
      } else if (this.sources.Length == 1 && this.sources[0] is GdiSource) {
        this.sources[0].Dispose();
        this.sources = new CaptureSource[] { new GdiSource(handle, bounds) };
      } else {
        Array.ForEach(this.sources, s => s?.Dispose());
        (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] info = DisplayHelper.GetOutputInfoFromRect(bounds);
        this.sources = info.Select(i => new DxgiSource(handle, i.Bounds, i.AdapterIndex, i.OutputIndex)).ToArray();
      }

      WindowHandle = handle;
    }

    /// <summary>
    ///   Detaches a window and binds to an screen area instead
    /// </summary>
    /// <param name="area">Screen area</param>
    internal void DetachWindow(Rectangle area) => AttachWindow(IntPtr.Zero, area);

    /// <summary>
    ///   Captures a bitmap from screen
    /// </summary>
    /// <param name="area">Virtual desktop area</param>
    /// <returns>A Bitmap containing the selected screen region</returns>
    internal Bitmap CaptureFromScreen(Rectangle area) {
      if (this.sources is null) {
        try {
          Log.WriteLine(LogLevel.Debug, "attempting to duplicate desktop");
          (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] info = DisplayHelper.GetOutputInfoFromRect(area);
          this.sources = info.Select(i => new DxgiSource(IntPtr.Zero, i.Bounds, i.AdapterIndex, i.OutputIndex))
                             .ToArray();
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Warning, $"could not perform desktop duplication - falling back to GDI: {exception}");

          // fall back to GDI
          this.sources = new CaptureSource[] { new GdiSource(IntPtr.Zero, area) };
        }
      } else if (this.sources.Length == 1 && this.sources[0] is GdiSource && !this.sources[0].Area.Equals(area)) {
        Log.WriteLine(LogLevel.Debug, "modifying GDI source area");
        this.sources[0].Area = area;
      } else {
        Log.WriteLine(LogLevel.Debug, "reinstantiating desktop duplication source");

        (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] info = DisplayHelper.GetOutputInfoFromRect(area);

        Array.ForEach(this.sources, s => s?.Dispose());
        this.sources = info.Select(i => new DxgiSource(IntPtr.Zero, i.Bounds, i.AdapterIndex, i.OutputIndex)).ToArray();
      }

      (Point Location, Bitmap Bitmap)[] bitmaps =
        this.sources.Select(s => (new Point(area.X - s.Area.X, area.Y - s.Area.Y), s.AcquireVideoFrame())).ToArray();
      var finalBitmap = new Bitmap(area.Width, area.Height);

      Log.WriteLine(LogLevel.Debug, $"captured {bitmaps.Length} bitmaps - merging");

      using (var graphics = Graphics.FromImage(finalBitmap)) {
        foreach (var bitmap in bitmaps) {
          graphics.DrawImage(bitmap.Bitmap, bitmap.Location);
        }
      }

      return finalBitmap;
    }

    /// <summary>
    ///   Class destructor
    /// </summary>
    ~CaptureHelper() => ReleaseUnmanagedResources();

    /// <summary>
    ///   Releases unmanaged resources
    /// </summary>
    private void ReleaseUnmanagedResources() {
      if (!(this.sources is null)) {
        Array.ForEach(this.sources, s => s.Dispose());
      }
    }

    /// <summary>
    ///   Releases resources
    /// </summary>
    public void Dispose() {
      ReleaseUnmanagedResources();
      GC.SuppressFinalize(this);
    }
  }
}
