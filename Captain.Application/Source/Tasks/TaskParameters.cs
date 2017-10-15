using System;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Holds common task parameters
  /// </summary>
  [Serializable]
  public class TaskParameters {
    /// <summary>
    ///   Region capture type
    /// </summary>
    public TaskRegionType RegionType { get; set; }

    /// <summary>
    ///   When RegionType is RegionType.Fixed, the bounds on the virtual desktop to be captured
    /// </summary>
    public Rectangle FixedRegion { get; set; }

    /// <summary>
    ///   When RegionType is RegionType.FullScreen, the indices of the displays to be captured
    /// </summary>
    public int[] FullScreenMonitors { get; set; }
  }
}
