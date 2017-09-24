using System;
using Captain.Common;

namespace Captain.Application {
  /// <summary>
  ///   Represents a failed capture result
  /// </summary>
  internal class UnsuccessfulCaptureResult : CaptureResult {
    /// <summary>
    ///   Contains an instance of the underlying exception
    /// </summary>
    internal Exception Exception { get; }


    /// <summary>
    ///   Creates an instance of this class
    /// </summary>
    /// <param name="exception">The underlying exception instance</param>
    internal UnsuccessfulCaptureResult(Exception exception) {
      Exception = exception;

      ToastTitle = Resources.Toast_OutputStreamFailedCaption;
      ToastContent = Resources.Toast_OutputStreamFailedContent;
    }
  }
}
