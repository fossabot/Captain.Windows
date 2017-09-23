using Captain.Application.Native;
using Captain.Common;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.Drawing;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Contains diverse utility methods for working with displays and output devices
  /// </summary>
  internal static class DisplayHelper {
#if false
    /// <summary>
    ///   Determines the display bounds that can be captured using normal methods
    /// </summary>
    /// <returns>A <see cref="Region"/> containing the acceptable bounds</returns>
    internal static Region GetAcceptableRegion() {
      var factory = new Factory1();
      var region = new Region();

      // enumerate outputs
      foreach (Adapter1 adapter in factory.Adapters1) {
        foreach (Output output in adapter.Outputs) {
          Log.WriteLine(LogLevel.Debug,
                        $"found output device: {output.Description.DeviceName} (attached: " +
                        output.Description.IsAttachedToDesktop +
                        "): ({0}, {1}, {2}, {3})",
                        output.Description.DesktopBounds.Left,
                        output.Description.DesktopBounds.Top,
                        output.Description.DesktopBounds.Right,
                        output.Description.DesktopBounds.Bottom);

          if (!output.Description.IsAttachedToDesktop) {
            // not attached to desktop - exclude output device
            continue;
          }

          // construct the POINT that's on the top-left edge of the output device area
          POINT outputTopLeftEdge;
          outputTopLeftEdge.x = output.Description.DesktopBounds.Left;
          outputTopLeftEdge.y = output.Description.DesktopBounds.Top;

          // try to find a window on that position
          IntPtr outputFullScreenHwnd;
          if ((outputFullScreenHwnd = User32.WindowFromPoint(outputTopLeftEdge)) != IntPtr.Zero) {
            // found window on top-left edge of output device
            // get window bounds
            if (User32.GetWindowRect(outputFullScreenHwnd, out RECT outputWindowRect)) {
              // compare the window bounds to those of the output device area - if they match then the window is on
              // full-screen mode. If this is the case, we may not be able to capture that screen region with normal
              // methods
              if (outputWindowRect.Equals(output.Description.DesktopBounds) &&
                  ((User32.WindowStylesEx)User32.GetWindowLongPtr(outputFullScreenHwnd,
                                                                  (int)User32.WindowLongParam.GWL_EXSTYLE))
                  .HasFlag(User32.WindowStylesEx.WS_EX_TOPMOST)) {
                // this window is a full-screen one - we may need to hook into some DX functions in order to capture it
                continue;
              }
            }
          }

          region.Union(new Rectangle(output.Description.DesktopBounds.Left, output.Description.DesktopBounds.Top,
            output.Description.DesktopBounds.Right - output.Description.DesktopBounds.Left,
            output.Description.DesktopBounds.Bottom - output.Description.DesktopBounds.Left));
        }
      }

      return region;
    }
#endif

    /// <summary>
    ///   Gets adapter/output information
    /// </summary>
    /// <returns>
    ///   A triplet containing the adapter and output indices alongside their bounds
    /// </returns>
    internal static (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] GetOutputInfo() {
      var factory = new Factory1();
      var triples = new List<(int, int, Rectangle)>();
      int adapterIndex = 0;

      // enumerate outputs
      foreach (Adapter1 adapter in factory.Adapters1) {
        int outputIndex = 0;

        foreach (Output output in adapter.Outputs) {
          // convert to Rectangle
          var outputRect = new Rectangle(output.Description.DesktopBounds.Left,
                                         output.Description.DesktopBounds.Top,
                                         output.Description.DesktopBounds.Right -
                                         output.Description.DesktopBounds.Left,
                                         output.Description.DesktopBounds.Bottom -
                                         output.Description.DesktopBounds.Top);
          triples.Add((adapterIndex, outputIndex, outputRect));
          outputIndex++;
        }

        adapterIndex++;
      }

      return triples.ToArray();
    }

    /// <summary>
    ///   Gets adapter/output indices from a given virtual desktop rectangle
    /// </summary>
    /// <param name="rect">The rectangle</param>
    /// <returns>
    ///   A triplet containing the adapter and output indices and the bounds that intersect with their regions
    /// </returns>
    internal static (int AdapterIndex, int OutputIndex, Rectangle Bounds)[] GetOutputInfoFromRect(Rectangle rect) {
      var factory = new Factory1();
      var triples = new List<(int, int, Rectangle)>();
      int adapterIndex = 0;

      // enumerate outputs
      foreach (Adapter1 adapter in factory.Adapters1) {
        int outputIndex = 0;

        foreach (Output output in adapter.Outputs) {
          // convert to Rectangle
          var outputRect = new Rectangle(output.Description.DesktopBounds.Left,
                                         output.Description.DesktopBounds.Top,
                                         output.Description.DesktopBounds.Right -
                                         output.Description.DesktopBounds.Left,
                                         output.Description.DesktopBounds.Bottom -
                                         output.Description.DesktopBounds.Top);

          // calculate intersection
          var intersection = Rectangle.Intersect(rect, outputRect);

          if (intersection != Rectangle.Empty) {
            // make sure the rectangles intersect
            triples.Add((adapterIndex, outputIndex, intersection));
          }

          outputIndex++;
        }

        adapterIndex++;
      }

      return triples.ToArray();
    }
  }
}
