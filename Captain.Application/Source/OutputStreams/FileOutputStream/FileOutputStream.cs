using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;

namespace Captain.Application {
  /// <summary>
  ///   Provides file-system capture saving features
  /// </summary>
  [DisplayName("Save to file"), ThreadApartmentState(ApartmentState.STA)]
  internal sealed class FileOutputStream : FileStream, IOutputStream, IConfigurableObject, IWithCustomImage {
    /// <summary>
    ///   Generated file name
    /// </summary>
    private readonly string fileName;

    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    private static EncoderInfo encoderInfo;

    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public IDictionary<object, object> UserConfiguration { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    public void DisplayConfigurationInterface(IWin32Window ownerWindow) { throw new NotImplementedException(); }

    /// <inheritdoc />
    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    /// <remarks>
    ///   BUG: This property is NOT thread-safe!
    ///   TODO: We can not override FileStream due to its constructor requiring non-static
    ///         variable parameters. We'll have to wrap the actual FileStream as a member and
    ///         write to it each time the underlying MemoryStream gets updated
    /// </remarks>
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

    /// <inheritdoc />
    /// <summary>
    ///   Dummy parameterless constructor
    /// </summary>
    public FileOutputStream() : this(GetFileName(encoderInfo.Extension)) { }

    /// <inheritdoc />
    /// <summary>
    ///   Actual class constructor
    /// </summary>
    /// <remarks>
    ///   Always allow streams implementing <see cref="T:Captain.Common.IOutputStream" /> to be read from so they can be selected as
    ///   master stream
    /// </remarks>
    private FileOutputStream(string fileName) : base(fileName, FileMode.CreateNew, FileAccess.ReadWrite) =>
      this.fileName = fileName;

    /// <inheritdoc />
    /// <summary>
    ///   Called when the data has been successfully copied to this output stream
    /// </summary>
    /// <returns>A <see cref="T:Captain.Common.CaptureResult" /> instance containing result information</returns>
    public CaptureResult Commit() => new CaptureResult { Uri = new Uri(this.fileName) };

    /// <inheritdoc />
    /// <summary>
    ///   Retrieves a custom image to be displayed alongside this plugin
    /// </summary>
    /// <returns>An <see cref="T:System.Drawing.Image" /> instance</returns>
    public Image GetCustomImage() => Resources.SaveToFile;
  }
}
