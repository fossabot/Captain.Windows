using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
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
    ///   HACK
    /// </summary>
    private static EncoderInfo encoderInfo;

    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    public EncoderInfo EncoderInfo {
      // HACK
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
                   DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss.") + extension);

    /// <summary>
    ///   Dummy parameterless constructor
    ///   HACK
    /// </summary>
    // ReSharper disable once UnusedMember.Global
    public FileOutputStream() : this(GetFileName(encoderInfo.Extension)) { }

    /// <summary>
    ///   Actual class constructor
    ///   NOTE: Always allow IOutputStreams to be read from if you want them to be selected as master streams
    /// </summary>
    private FileOutputStream(string fileName) : base(fileName, FileMode.CreateNew, FileAccess.ReadWrite) =>
      this.fileName = fileName;

    /// <summary>
    ///   Called when the data has been successfully copied to this output stream
    /// </summary>
    /// <returns>A <see cref="CaptureResult"/> instance containing result information</returns>
    public CaptureResult Commit() {
      Flush();

      var result = new CaptureResult {
        ToastTitle = "Capture saved!",
        ToastContent = "The file has been saved to your Captures folder.",
        ToastUri = new Uri(this.fileName),
        ToastPreview = EncoderInfo.PreviewBitmap
      };

      return result;
    }
  }
}
