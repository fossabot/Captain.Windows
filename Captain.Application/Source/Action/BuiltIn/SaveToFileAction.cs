using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Captain.Common;
using Action = Captain.Common.Action;

namespace Captain.Application {
  /// <inheritdoc cref="Common.Action" />
  /// <summary>
  ///   Action for saving captures to the file system
  /// </summary>
  [DisplayName("en", "Save to file")]
  internal sealed class SaveToFileAction : Action, IYieldsUri, IHasImage, IHasOptions {
    /// <summary>
    ///   Underlying file stream
    /// </summary>
    private readonly FileStream fileStream;

    /// <summary>
    ///   Path for the file
    /// </summary>
    private readonly string path;

    /// <summary>
    ///   Custom substitution templates.
    /// </summary>
    private readonly Dictionary<CommonVariable, Func<object>> templates = TemplateHelper.Templates;

    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object> {
      {"PathTemplate", Resources.SaveToFile_DefaultNameTemplate}
    };

    /// <inheritdoc />
    /// <summary>
    ///   Creates a new instance of this action.
    /// </summary>
    /// <param name="codec">Codec instance.</param>
    public SaveToFileAction(ICodecBase codec) : base(codec) {
      // add substitution templates
      this.templates[CommonVariable.Extension] = () => codec.FileExtension;
      this.templates[CommonVariable.Type] = () => codec is IStillImageCodec
        ? Resources.TemplateHelper_Type_Screenshot
        : Resources.TemplateHelper_Type_Recording;

      // get file path
      this.path = TemplateHelper.GetString(
        TemplateHelper.Normalize((string) Options["PathTemplate"]),
        this.templates);

      // ensure the path exists
      Directory.CreateDirectory(
        Path.GetDirectoryName(this.path) ?? throw new InvalidOperationException("Invalid path."));

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
    ///   Sets the position within the current stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
    /// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    public override long Seek(long offset, SeekOrigin origin) =>
      Position = this.fileStream.Seek(offset, origin);

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
      this.fileStream.Flush();
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
      SetStatus(ActionStatus.Success);
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

    /// <inheritdoc />
    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    public DialogResult DisplayOptionsInterface(IWin32Window ownerWindow) => throw new NotImplementedException();
  }
}