﻿using System;
using System.Collections.Generic;
using System.Windows.Forms;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
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
    public List<(string, SerializableDictionary<object, object>)> OutputStreams { get; set; } =
      new List<(string, SerializableDictionary<object, object>)>();

    /// <summary>
    ///   Keyboard shortcut assigned to this task
    /// </summary>
    public Keys HotKey { get; set; } = Keys.None;

    /// <summary>
    ///   Custom notification options
    /// </summary>
    public NotificationDisplayOptions NotificationOptions { get; set; } = NotificationDisplayOptions.Inherit;

    /// <summary>
    ///   Creates a default screenshot task using predefined actions (i.e. for first application startup)
    /// </summary>
    /// <returns>A <see cref="Task"/> instance</returns>
    internal static Task CreateDefaultScreenshotTask() => new Task {
      Name = "Default screenshot task",
      Type = TaskType.Screenshot,
      Parameters = new TaskParameters {
        Encoder = typeof(PngCaptureEncoder).FullName,
        RegionType = TaskRegionType.Grab
      },
      OutputStreams = new List<(string, SerializableDictionary<object, object>)> {
        (typeof(ClipboardOutputStream).FullName, new SerializableDictionary<object, object>()),
        (typeof(FileOutputStream).FullName, new SerializableDictionary<object, object>())
      },
      NotificationOptions = NotificationDisplayOptions.Inherit
    };

    /// <summary>
    ///   Creates a default screenshot task using predefined actions (i.e. for first application startup)
    /// </summary>
    /// <returns>A <see cref="Task"/> instance</returns>
    internal static Task CreateDefaultRecordingTask() => new Task {
      Name = "Default recording task",
      Type = TaskType.Recording,
      Parameters = new TaskParameters {
        Encoder = typeof(H264CaptureEncoder).FullName,
        RegionType = TaskRegionType.Grab
      },
      OutputStreams = new List<(string, SerializableDictionary<object, object>)> {
        (typeof(FileOutputStream).FullName, new SerializableDictionary<object, object>())
      },
      NotificationOptions = NotificationDisplayOptions.Inherit
    };
  }
}