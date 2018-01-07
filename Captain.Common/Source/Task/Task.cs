using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Captain.Common {
  /// <summary>
  ///   Tasks are the functional unit of the app. A task contains information about how the screen is to be captured
  ///   and all the actions to be done with the result.
  /// </summary>
  [Serializable]
  public sealed class Task {
    /// <summary>
    ///   User-defined name for this task.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   List of post-capture actions to be performed. The list contains tuples of the full type name of the
    ///   <see cref="Action" /> and a serializable object that may or not hold custom options for it.
    /// </summary>
    public List<(string ActionType, Dictionary<string, object> Options)> Actions { get; set; } =
      new List<(string, Dictionary<string, object>)>();

    /// <summary>
    ///   A pair made up of the full type name of the codec to be used and an optional object containing options and
    ///   other user data to be used by the codec.
    /// </summary>
    public (string CodecType, Dictionary<string, object> Options) Codec { get; set; }

    /// <summary>
    ///   Hotkey for the task. When registered, a unique combination of keys will be bound to this task. When pressed,
    ///   the task will be started automatically.
    /// </summary>
    public Keys Hotkey { get; set; }

    /// <summary>
    ///   Gets or sets the capture region type.
    /// </summary>
    public RegionType RegionType { get; set; } = RegionType.UserSelected;

    /// <summary>
    ///   Screen region to be captured.
    /// </summary>
    public Rectangle Region { get; set; }

    /// <summary>
    ///   Task type (recording, screenshot...)
    /// </summary>
    public TaskType TaskType { get; set; } = TaskType.StillImage;
  }
}