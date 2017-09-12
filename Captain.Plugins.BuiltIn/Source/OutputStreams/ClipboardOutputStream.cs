using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Captain.Common;

namespace Captain.Plugins.BuiltIn {
  [DisplayName("Copy to clipboard")]
  public class ClipboardOutputStream : MemoryStream, IOutputStream {
    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    public EncoderInfo EncoderInfo { get; set; }

    /// <summary>
    ///   Class constructor.
    ///   Makes sure the media is supported
    /// </summary>
    public ClipboardOutputStream() {
      if (!EncoderInfo.MediaType.StartsWith("image/")) {
        throw new UnsupportedMediaException();
      }
    }

    /// <summary>
    ///   Called when the data has been successfully copied to this output stream
    /// </summary>
    /// <returns>A <see cref="CaptureResult"/> instance containing result information</returns>
    public CaptureResult Commit() {
      Clipboard.SetImage(Image.FromStream(this));

      var result = new CaptureResult();
      result.ToastTitle = "Screenshot copied!";
      result.ToastContent = "The screenshot has been copied to your clipboard.";
      result.ToastPreview = EncoderInfo.PreviewBitmap;

      return result;
    }
  }
}
