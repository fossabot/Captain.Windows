using System;
using System.Drawing;

namespace Captain.Common {
  /// <summary>
  ///   Contains information about a successful capture after it has been handled appropiately
  /// </summary>
  public sealed class CaptureResult {
    /// <summary>
    ///   Optional status message
    /// </summary>
    public string Status { get; set; }

    /// <summary>
    ///   Optional result URI
    /// </summary>
    public Uri Uri { get; set; }

    /// <summary>
    ///   Optional preview bitmap
    /// </summary>
    public Bitmap PreviewBitmap { get; set; }
  }
}
