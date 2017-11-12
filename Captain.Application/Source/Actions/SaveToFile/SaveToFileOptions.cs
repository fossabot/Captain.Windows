using System;
using System.IO;

namespace Captain.Application {
  /// <summary>
  ///   Options object for the "Save to file" action
  /// </summary>
  [Serializable]
  internal sealed class SaveToFileOptions {
    /// <summary>
    ///   File path template
    /// </summary>
    public string PathTemplate { get; set; }

    /// <summary>
    ///   Class constructor (binds defaults)
    /// </summary>
    public SaveToFileOptions() {
      if (String.IsNullOrWhiteSpace(PathTemplate)) {
        PathTemplate = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures,
            Environment.SpecialFolderOption.Create),
          Resources.SaveToFile_DefaultNameTemplate);
      }
    }
  }
}