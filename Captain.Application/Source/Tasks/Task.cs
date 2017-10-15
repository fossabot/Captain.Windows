using System;
using System.Windows.Forms;

namespace Captain.Application {
  /// <summary>
  ///   Tasks are the main functional unit of the application
  /// </summary>
  [Serializable]
  public class Task {
    /// <summary>
    ///   Task type
    /// </summary>
    public TaskType Type { get; set; }

    /// <summary>
    ///   Task parameters
    /// </summary>
    public TaskParameters Parameters { get; set; }

    /// <summary>
    ///   Task name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Output stream types
    /// </summary>
    public string[] OutputStreams { get; set; }

    /// <summary>
    ///   Keyboard shortcut assigned to this task
    /// </summary>
    public Keys HotKey { get; set; }
  }
}
