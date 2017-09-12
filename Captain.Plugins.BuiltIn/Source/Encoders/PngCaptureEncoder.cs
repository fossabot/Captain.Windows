using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Captain.Common;

namespace Captain.Plugins.BuiltIn {
  /// <summary>
  ///   Encodes still captures as PNG images
  /// </summary>
  [DisplayName("PNG")]
  public class PngCaptureEncoder : IStaticEncoder {
    /// <summary>
    ///   Encoder information
    /// </summary>
    public EncoderInfo EncoderInfo => new EncoderInfo() {
      Extension = "png",
      MediaType = "image/png"
    };

    /// <summary>
    ///   Performs the encoding
    /// </summary>
    /// <param name="bitmap">Capture</param>
    /// <param name="outputStream">Output stream</param>
    public void Encode(Bitmap bitmap, Stream outputStream) {
      bitmap.Save(outputStream, ImageFormat.Png);
    }
  }
}
