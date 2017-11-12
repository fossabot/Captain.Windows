using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using SharpDX.DXGI;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Contains diverse utility methods for working with displays and output devices
  /// </summary>
  internal static class DisplayHelper {
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

      if (factory.GetAdapterCount1() == 0) {
        // no usable adapters - retrieve virtual desktop information
        int outputIndex = 0;
        foreach (Screen screen in Screen.AllScreens) {
          triples.Add((adapterIndex, outputIndex, screen.Bounds));
          Log.WriteLine(LogLevel.Debug, $"screen #{outputIndex} " + screen.Bounds.ToString().Trim('{', '}'));
          outputIndex++;
        }

        return triples.ToArray();
      }

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
          Log.WriteLine(LogLevel.Debug,
            $"{adapter.Description.Description} // {output.Description.DeviceName} " +
            outputRect.ToString().Trim('{', '}'));
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

      if (factory.GetAdapterCount1() == 0) {
        // no usable adapters - retrieve virtual desktop information
        int outputIndex = 0;
        foreach (Screen screen in Screen.AllScreens) {
          // calculate intersection
          var intersection = Rectangle.Intersect(rect, screen.Bounds);

          // make sure the rectangles intersect
          if (intersection != Rectangle.Empty) {
            triples.Add((adapterIndex, outputIndex, intersection));
            Log.WriteLine(LogLevel.Debug, $"screen #{outputIndex} " + screen.Bounds.ToString().Trim('{', '}'));
          }

          outputIndex++;
        }

        return triples.ToArray();
      }

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

          // make sure the rectangles intersect
          if (intersection != Rectangle.Empty) {
            triples.Add((adapterIndex, outputIndex, intersection));
            Log.WriteLine(LogLevel.Debug,
              $"{adapter.Description.Description} // {output.Description.DeviceName} " +
              outputRect.ToString().Trim('{', '}'));
          }

          outputIndex++;
        }

        adapterIndex++;
      }

      return triples.ToArray();
    }

    /// <summary>
    ///   Gets the DPI value for the default display or the screen containing the specified window
    /// </summary>
    /// <param name="hwnd">Optionally specify a window handle</param>
    /// <returns>A float value containing the screen DPI</returns>
    internal static float GetScreenDpi(IntPtr? hwnd = null) {
      try {
        // return system DPI/DPI for the specified window
        return hwnd.HasValue ? User32.GetDpiForWindow(hwnd.Value) : User32.GetDpiForSystem();
      } catch (EntryPointNotFoundException) {
        // unsupported platform
        using (var graphics = Graphics.FromHwnd(hwnd ?? User32.GetDesktopWindow())) { return graphics.DpiX; }
      }
    }
  }
}