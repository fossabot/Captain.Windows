using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;

namespace Captain.Application {
  /// <summary>
  ///   Encodes still captures as PNG images
  /// </summary>
  [DisplayName("JPEG"), ThreadApartmentState(ApartmentState.STA)]
  internal sealed class JpegCaptureEncoder : IStaticEncoder, IConfigurableObject {
    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public IDictionary<object, object> UserConfiguration { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Encoder information
    /// </summary>
    public EncoderInfo EncoderInfo => new EncoderInfo {
      Extension = "jpg",
      MediaType = "image/jpeg"
    };

    /// <summary>
    ///   Performs the encoding
    /// </summary>
    /// <param name="bitmap">Capture</param>
    /// <param name="outputStream">Output stream</param>
    public void Encode(Bitmap bitmap, Stream outputStream) => bitmap.Save(outputStream, ImageFormat.Jpeg);

    /// <inheritdoc />
    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    public void DisplayConfigurationInterface() {
      MessageBox.Show(@"Hello, world!");
    }
  }
}
