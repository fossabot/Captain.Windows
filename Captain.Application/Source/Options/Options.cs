using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Xml.Serialization;
using Captain.Common;
using static Captain.Application.Application;

// ReSharper disable MemberCanBeInternal
namespace Captain.Application {
  /// <summary>
  ///   Class representing application options.
  /// </summary>
  /// <remarks>
  ///   Instances of this class will be serialized and written to the options file. Make sure to keep this class
  ///   backwards-compatible! A single change may render the application unresponsive when upgrading to a different
  ///   version.
  /// </remarks>
  [Serializable]
  public sealed class Options {
    /// <summary>
    ///   Default options file name
    /// </summary>
    internal const string OptionsFileName = "Options.xml";

    /// <summary>
    ///   Saved position for windows
    /// </summary>
    public SerializableDictionary<string, Point> WindowPositions { get; set; } = new SerializableDictionary<string, Point>();

    /// <summary>
    ///   Current Options dialog tab index
    /// </summary>
    public uint OptionsDialogTab { get; set; }

    /// <summary>
    ///   Notification display options
    /// </summary>
    public NotificationDisplayOptions NotificationOptions { get; set; } = NotificationDisplayOptions.ExceptProgress;

    /// <summary>
    ///   Use legacy notification provider
    /// </summary>
    public bool UseLegacyNotificationProvider { get; set; }

    /// <summary>
    ///   Adjusts the behavior of the update manager
    /// </summary>
    public UpdatePolicy UpdatePolicy { get; set; } = UpdatePolicy.CheckOnly;

    /// <summary>
    ///   Last version of the application that was ran
    /// </summary>
    public string LastVersion { get; set; } = String.Empty;

    /// <summary>
    ///   Contains the user tasks
    /// </summary>
    public List<Task> Tasks { get; set; } = new List<Task>();

    /// <summary>
    ///   Loads an <see cref="Options"/> instance from file
    /// </summary>
    /// <returns>An instance of the <see cref="Options"/> class</returns>
    internal static Options Load() {
      try {
        using (var fileStream = new FileStream(Path.Combine(Application.FsManager.GetSafePath(), OptionsFileName),
                                               FileMode.OpenOrCreate)) {
          if (fileStream.Length == 0) {
            Log.WriteLine(LogLevel.Warning, "stream is empty");
            return null;
          }

          if (new XmlSerializer(typeof(Options)).Deserialize(fileStream) is Options opts) {
            return opts;
          }
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Warning, $"could not load options - {exception}");
      }

      return null;
    }

    /// <summary>
    ///   Saves these options to the stream they were loaded from
    /// </summary>
    internal void Save() {
      Log.WriteLine(LogLevel.Verbose, "saving options");

      try {
        using (var fileStream = new FileStream(Path.Combine(Application.FsManager.GetSafePath(), OptionsFileName),
                                               FileMode.OpenOrCreate)) {
          fileStream.SetLength(0);
          new XmlSerializer(GetType()).Serialize(fileStream, this);
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Warning, $"could not save options - {exception}");
      }
    }
  }
}
