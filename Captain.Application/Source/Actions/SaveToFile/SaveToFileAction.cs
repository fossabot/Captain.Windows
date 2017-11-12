using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using Action = Captain.Common.Action;

namespace Captain.Application {
  /// <inheritdoc cref="Action" />
  /// <summary>
  ///   Action for saving captures to the file system
  /// </summary>
  [DisplayName("en", "Save to file")]
  internal sealed class SaveToFileAction : Action, IYieldsUri, IHasImage, IHasOptions<SaveToFileOptions> {
    /// <summary>
    ///   Underlying file stream
    /// </summary>
    private readonly FileStream fileStream;

    /// <summary>
    ///   Path for the file
    /// </summary>
    private readonly string path;

    /// <summary>
    ///   Class constructor - opens underlying file stream
    /// </summary>
    public SaveToFileAction() {
      // TODO: add template variables for extension, etc.
      this.path = TemplateHelper.GetString(TemplateHelper.Normalize(Options.PathTemplate)) + ".jpeg";

      // ensure the path exists
      Directory.CreateDirectory(Path.GetDirectoryName(this.path) ?? throw new InvalidOperationException("Invalid path."));

      // create underlying file stream
      this.fileStream = new FileStream(this.path, FileMode.OpenOrCreate);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Retrieves a custom image to be displayed alongside this plugin
    /// </summary>
    /// <returns>An <see cref="T:System.Drawing.Image" /> instance</returns>
    public Image GetImage() => Resources.SaveToFile;

    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public SaveToFileOptions Options { get; set; } = new SaveToFileOptions();

    /// <inheritdoc />
    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    public void DisplayOptionsInterface(IWin32Window ownerWindow) => throw new NotImplementedException();

    /// <inheritdoc />
    /// <summary>
    ///   Writes a sequence of bytes to the current stream and advances the current position within this stream by the
    ///   number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes to be copied.</param>
    /// <param name="offset">
    ///   The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current
    ///   stream. This value is ignored and must be equal to <see cref="P:Captain.Common.Action.Position" />.
    /// </param>
    /// <param name="count">The number of bytes to be written to the current stream.</param>
    public override void Write(byte[] buffer, int offset, int count) {
      this.fileStream.Write(buffer, offset, count);
      if (this.fileStream.Length == Length) {
        // finished copying data
        SetStatus(ActionStatus.Success);
      }

      base.Write(buffer, offset, count);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the unmanaged resources used by the <see cref="T:System.IO.Stream" /> and optionally releases the
    ///   managed resources.
    /// </summary>
    /// <param name="disposing">
    ///   true to release both managed and unmanaged resources; false to release only unmanaged
    ///   resources.
    /// </param>
    protected override void Dispose(bool disposing) {
      this.fileStream?.Dispose();
      base.Dispose(disposing);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Retrieves the Universal Resource Identifier (URI) for the processed capture.
    /// </summary>
    /// <remarks>
    ///   This method is called by Captain if and only if the <see cref="P:Captain.Common.Action.Status" /> property has
    ///   been set to <see cref="F:Captain.Common.ActionStatus.Success" />.
    /// </remarks>
    /// <returns>An instance of <see cref="T:System.Uri" />, representing the capture.</returns>
    public Uri GetUri() => new Uri(this.path);
  }
}