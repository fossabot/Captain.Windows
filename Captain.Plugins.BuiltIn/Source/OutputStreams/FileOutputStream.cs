using System;
using System.IO;
using Captain.Common;

namespace Captain.Plugins.BuiltIn {
  [DisplayName("Save to file")]
  public class FileOutputStream : FileStream, IOutputStream {
    /// <summary>
    ///   Generated file name
    /// </summary>
    private readonly string fileName;

    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    private static EncoderInfo encoderInfo;

    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    public EncoderInfo EncoderInfo {
      get => encoderInfo;
      set => encoderInfo = value;
    }

    /// <summary>
    ///   Returns a file name for this capture
    /// </summary>
    /// <param name="extension">The file extension</param>
    /// <returns>A file name</returns>
    private static string GetFileName(string extension) =>
      Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                   DateTime.Now.ToString("dd-MM-yyyy HH.mm.ss.") + extension);

    /// <summary>
    ///   Dummy parameterless constructor
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public FileOutputStream() : this(GetFileName(encoderInfo.Extension)) { }

    /// <summary>
    ///   Actual class constructor
    /// </summary>
    /// <remarks>
    ///   Always allow streams implementing <see cref="IOutputStream"/> to be read from so they can be selected as
    ///   master stream
    /// </remarks>
    private FileOutputStream(string fileName) : base(fileName, FileMode.CreateNew, FileAccess.ReadWrite) =>
      this.fileName = fileName;

    /// <summary>
    ///   Called when the data has been successfully copied to this output stream
    /// </summary>
    /// <returns>A <see cref="CaptureResult"/> instance containing result information</returns>
    public CaptureResult Commit() {
      var result = new CaptureResult {
        ToastTitle = "Capture saved!",
        ToastContent = "The file has been saved to your Captures folder.",
        ToastUri = new Uri(this.fileName)
      };

      return result;
    }
  }
}
