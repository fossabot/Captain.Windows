using System.Collections.Generic;
using System.Drawing;

namespace Captain.Common {
  /// <summary>
  ///   Contains information about an encoded capture.
  ///   It is passed to the constructor of all output streams.
  /// </summary>
  public struct EncoderInfo {
    /// <summary>
    ///   Encoded capture file extension
    /// </summary>
    public string Extension { get; set; }

    /// <summary>
    ///   MIME type for the encoded capture
    /// </summary>
    public string MediaType { get; set; }

    /// <summary>
    ///   Extra data associated with the encoded capture
    /// </summary>
    public IDictionary<object, object> UserData { get; set; }

    /// <summary>
    ///   Capture preview bitmap
    /// </summary>
    public Bitmap PreviewBitmap { get; set; }
  }
}
