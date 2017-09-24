using SharpDX.DXGI;
using System.Collections.Generic;
using System.Drawing;

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
