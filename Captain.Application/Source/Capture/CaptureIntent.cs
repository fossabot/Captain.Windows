using System;
using System.ComponentModel;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Holds capture start parameters passed to actions
  /// </summary>
  internal class CaptureIntent {
    /// <summary>
    ///   Action type associated with this intent. It is never <c>ActionType.None</c> or has different flags
    /// </summary>
    internal ActionType ActionType { get; }

    /// <summary>
    ///   Contains the area of the virtual desktop that has been selected by the user
    /// </summary>
    internal Rectangle VirtualArea { get; set; }

    /// <summary>
    ///   When capturing an application running in fullscreen mode (so <c>Monitor</c> property is set) or capturing a
    ///   window the grabber is attached to, this will contain the handle.
    /// </summary>
    internal IntPtr WindowHandle { get; set; } = IntPtr.Zero;

    /// <summary>
    ///   Creates a new capture intent
    /// </summary>
    /// <param name="type"></param>
    internal CaptureIntent(ActionType type) {
      if (type != ActionType.Screenshot && type != ActionType.Record) {
        throw new InvalidEnumArgumentException(@"The ActionType must be Screenshot or Record");
      }

      ActionType = type;
    }
  }
}
