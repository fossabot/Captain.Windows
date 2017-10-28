using System;
using System.Collections.Generic;
using System.Windows.Forms;

// ReSharper disable MemberCanBeInternal
namespace Captain.Application {
  /// <summary>
  ///   Tasks are the main functional unit of the application
  /// </summary>
  [Serializable]
  public sealed class Task {
    /// <summary>
    ///   Task type
    /// </summary>
    public TaskType Type { get; set; } = TaskType.Screenshot;

    /// <summary>
    ///   Task parameters
    /// </summary>
    public TaskParameters Parameters { get; set; } = new TaskParameters();

    /// <summary>
    ///   Task name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Output streams
    /// </summary>
    /// <remarks>
    ///   Keys contains the type name for the output stream, while values hold an optional dictionary with the user
    ///   options for such stream
    /// </remarks>
    public List<SerializableDictionary<string, SerializableDictionary<object, object>>> OutputStreams { get; set; } =
      new List<SerializableDictionary<string, SerializableDictionary<object, object>>>();

    /// <summary>
    ///   Keyboard shortcut assigned to this task
    /// </summary>
    public Keys HotKey { get; set; } = Keys.None;

    /// <summary>
    ///   Custom notification options
    /// </summary>
    public NotificationDisplayOptions NotificationOptions { get; set; } = NotificationDisplayOptions.Inherit;
  }
}