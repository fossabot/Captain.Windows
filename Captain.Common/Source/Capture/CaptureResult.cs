using System;
using System.Drawing;

// ReSharper disable once CheckNamespace
namespace Captain.Common {
  /// <summary>
  ///   Contains information about a successful capture after it has been handled appropiately
  /// </summary>
  // ReSharper disable once ClassNeverInstantiated.Global
  public class CaptureResult {
    /// <summary>
    /// Title of the notification (optional)
    /// </summary>
    public string ToastTitle { get; set; } = null;

    /// <summary>
    /// Content of the notification (optional)
    /// </summary>
    public string ToastContent { get; set; } = null;

    /// <summary>
    /// Thumbnail to be shown on the toast notification (may be unavailable on older platforms)
    /// </summary>
    public Bitmap ToastPreview { get; set; } = null;

    /// <summary>
    /// URL to be open upon toast activation
    /// </summary>
    public Uri ToastUri { get; set; } = null;
  }
}
