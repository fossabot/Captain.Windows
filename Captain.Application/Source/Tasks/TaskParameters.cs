using System;
using System.Collections.Generic;
using System.Drawing;

// ReSharper disable MemberCanBeInternal
namespace Captain.Application {
  /// <summary>
  ///   Holds common task parameters
  /// </summary>
  [Serializable]
  public class TaskParameters {
    /// <summary>
    ///   Region capture type
    /// </summary>
    public TaskRegionType RegionType { get; set; } = TaskRegionType.Grab;

    /// <summary>
    ///   When RegionType is RegionType.Fixed, the bounds on the virtual desktop to be captured
    /// </summary>
    public Rectangle FixedRegion { get; set; } = Rectangle.Empty;

    /// <summary>
    ///   When RegionType is RegionType.FullScreen, the indices of the displays to be captured
    /// </summary>
    public List<int> FullScreenMonitors { get; set; } = new List<int>();

    /// <summary>
    ///   Type name for the encoder to be used with this task
    /// </summary>
    public string Encoder { get; set; }

    /// <summary>
    ///   User options for the encoder
    /// </summary>
    public SerializableDictionary<object, object> EncoderOptions { get; set; } = new SerializableDictionary<object, object>();
  }
}
