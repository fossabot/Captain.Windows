using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Captain.Common;
using SharpDX;
using Action = Captain.Common.Action;
using BitmapData = Captain.Common.BitmapData;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc cref="Common.Action" />
  /// <summary>
  ///   Copies the captured still image to the clipboard
  /// </summary>
  [DisplayName("en", "Copy to clipboard")]
  internal sealed class CopyToClipboardAction : Action, IFiltered, IPreBitmapEncodingAction, IHasImage {
    /// <inheritdoc />
    /// <summary>
    ///   Creates a new instance of this action.
    /// </summary>
    /// <param name="codec">Codec instance.</param>
    public CopyToClipboardAction(ICodecBase codec) : base(codec) { }

    /// <inheritdoc />
    /// <summary>
    ///   Determines whether the specified input media parameters are acceptable for this action.
    /// </summary>
    /// <param name="type">Task type</param>
    /// <param name="codecInstance">Codec instance</param>
    /// <param name="codecParams">Codec parameters</param>
    /// <returns>A boolean value indicating whether the specified media is supported by this action.</returns>
    public bool GetMediaAcceptance(TaskType type, ICodecBase codecInstance, ICodecParameters codecParams) =>
      type == TaskType.StillImage;

    /// <inheritdoc />
    /// <summary>
    ///   Retrieves a custom image to be displayed alongside this plugin
    /// </summary>
    /// <returns>An <see cref="T:System.Drawing.Image" /> instance</returns>
    public Image GetImage() => Resources.CopyToClipboard;

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
    /// <exception cref="System.NotSupportedException">
    ///   Always thrown, as this action is meant to work only with still image captures which will be received via the
    ///   <see cref="IPreBitmapEncodingAction.SetBitmapData(Common.BitmapData)"/> method.
    /// </exception>
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    /// <inheritdoc />
    /// <summary>
    ///   Sets this action's bitmap data.
    /// </summary>
    /// <param name="orgData">An instance of <see cref="T:Captain.Common.BitmapData" /> containing capture information.</param>
    public void SetBitmapData(BitmapData orgData) {
      using (var bmp = new Bitmap(orgData.Width, orgData.Height, PixelFormat.Format32bppRgb)) {
        // lock target bitmap
        System.Drawing.Imaging.BitmapData data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height),
          ImageLockMode.WriteOnly,
          bmp.PixelFormat);

        // copy data and unlock target bitmap
        Utilities.CopyMemory(data.Scan0, orgData.Scan0, orgData.Height * orgData.Stride);
        bmp.UnlockBits(data);

        // copy to clipboard
        Clipboard.SetImage(bmp);
        SetStatus(ActionStatus.Success);
      }
    }
  }
}