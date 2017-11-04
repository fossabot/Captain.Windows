using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Captain.Common;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Encodes still captures as PNG images
  /// </summary>
  [DisplayName("PNG")]
  internal sealed class PngCaptureEncoder : IStaticEncoder {
    /// <inheritdoc />
    /// <summary>
    ///   Encoder information
    /// </summary>
    public EncoderInfo EncoderInfo => new EncoderInfo {
      Extension = "png",
      MediaType = "image/png"
    };

    /// <inheritdoc />
    /// <summary>
    ///   Performs the encoding
    /// </summary>
    /// <param name="bitmap">Capture</param>
    /// <param name="outputStream">Output stream</param>
    public void Encode(Bitmap bitmap, Stream outputStream) => bitmap.Save(outputStream, ImageFormat.Png);
  }
}
