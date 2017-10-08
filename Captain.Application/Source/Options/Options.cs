using System;
using System.IO;
using System.Xml.Serialization;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  [Serializable]
  public class Options {
    /// <summary>
    ///   Default options file name
    /// </summary>
    private const string OptionsFileName = "Options.xml";

    /// <summary>
    ///   Saved position for windows
    /// </summary>
    public WindowPositionMap WindowPositions { get; } = new WindowPositionMap();

    /// <summary>
    ///   Current Options dialog tab index
    /// </summary>
    public uint OptionsDialogTab { get; set; }

    /// <summary>
    ///   Loads an <see cref="Options"/> instance from file
    /// </summary>
    /// <returns>An instance of the <see cref="Options"/> class</returns>
    internal static Options Load() {
      using (var fileStream = new FileStream(Path.Combine(Application.FsManager.GetSafePath(), OptionsFileName),
                                             FileMode.OpenOrCreate)) {
        if (fileStream.Length == 0) {
          Log.WriteLine(LogLevel.Warning, "stream is empty");
          return null;
        }

        try {
          if (new XmlSerializer(typeof(Options)).Deserialize(fileStream) is Options opts) {
            return opts;
          }
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"could not deserialize Options: {exception}");
        }

        return null;
      }
    }

    /// <summary>
    ///   Saves these options to the stream they were loaded from
    /// </summary>
    internal void Save() {
      using (var fileStream = new FileStream(Path.Combine(Application.FsManager.GetSafePath(), OptionsFileName),
                                             FileMode.OpenOrCreate)) {
        fileStream.SetLength(0);
        new XmlSerializer(GetType()).Serialize(fileStream, this);
      }
    }
  }
}
