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
    ///   Gets or sets the monitor index for full-screen captures of software running in an exclusive cooperation level.
    ///   If this is not such capture kind, this property is set to -1
    /// </summary>
    internal int Monitor { get; set; } = -1;

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
