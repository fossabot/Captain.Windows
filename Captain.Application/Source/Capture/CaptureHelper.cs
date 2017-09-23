using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using Captain.Common;
using SharpDX;
using static Captain.Application.Application;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <summary>
  ///   Helper class which abstracts screen capture
  /// </summary>
  internal class CaptureHelper : IDisposable {

    private (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] outputInfo = DisplayHelper.GetOutputInfo();

    /// <summary>
    ///   Whether we are using DXGI desktop duplication or not
    /// </summary>
    private bool usingDesktopDuplication = false;

    /// <summary>
    ///   Contains the underlying capture sources. If <c>usingDesktopDuplication</c> is false, this array only contains
    ///   a single instance of <see cref="GdiSource" />
    /// </summary>
    private CaptureSource[] sources;

    /// <summary>
    ///   Contains the previously captured area. For we have to perform some checks in case the captured area changes,
    ///   we want to keep track of area changes so we don't check anything that does not need to be checked
    /// </summary>
    private Rectangle previousArea = Rectangle.Empty;

    /// <summary>
    ///   Attached window handle
    /// </summary>
    internal IntPtr WindowHandle { get; set; }

    /// <summary>
    ///   Captures a bitmap from screen
    /// </summary>
    /// <param name="area">Virtual desktop area</param>
    /// <returns>A Bitmap containing the selected screen region</returns>
    internal Bitmap CaptureFromScreen(Rectangle area) {
      if (this.previousArea.Equals(Rectangle.Empty) || !this.previousArea.Equals(area)) {
        (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] info = DisplayHelper.GetOutputInfoFromRect(area);
        Log.WriteLine(LogLevel.Debug, $"found {info.Length} intersecting output devices");

        // dispose previous sources
        ReleaseUnmanagedResources();

        try {
          // use DXGI
          this.sources = info
            .Select(i => new DxgiSource(WindowHandle, i.Bounds, i.AdapterIndex, i.OutputIndex) as CaptureSource)
            .ToArray();
        } catch (SharpDXException exception) {
          // fall back to GDI
          Log.WriteLine(LogLevel.Error, $"{exception}");
          Log.WriteLine(LogLevel.Warning, "could not initialize DXGI source - falling back to GDI");
          this.sources = new CaptureSource[] { new GdiSource(WindowHandle, area) };
        }
      }

      this.previousArea = area;

      if (this.sources.Length == 1) {
        // single source (i.e. GDI)
        Log.WriteLine(LogLevel.Debug, "acquiring single video frame");
        return this.sources.First().AcquireVideoFrame();
      }

      // capture the intersecting bitmaps and merge them in a single image
      (Point Location, Bitmap Bitmap)[] bitmapInfo =
        this.sources.Select(s => (new Point(s.Area.X - area.X, s.Area.Y - area.Y), s.AcquireVideoFrame())).ToArray();
      Log.WriteLine(LogLevel.Debug, $"acquired {bitmapInfo.Length} video frames");

      // TODO: find a more elegant, clean and FAST way to accomplish this
      var finalBitmap = new Bitmap(area.Width, area.Height);
      using (var graphics = Graphics.FromImage(finalBitmap)) {
        foreach ((Point Location, Bitmap Bitmap) partialBitmapInfo in bitmapInfo) {
          graphics.DrawImage(partialBitmapInfo.Bitmap, partialBitmapInfo.Location);
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
        Array.ForEach(this.sources, s => s?.Dispose());
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
