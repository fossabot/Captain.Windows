using System;

namespace Captain.Common {
  /// <summary>
  ///   Implemented by an <see cref="Action" /> that reports progress.
  /// </summary>
  public interface IReportsProgress {
    /// <summary>
    ///   Action progress value.
    /// </summary>
    /// <value>
    ///   The value ranges from [0, 1]
    /// </value>
    double Progress { get; }

    /// <summary>
    ///   Triggered when the progress changes.
    /// </summary>
    /// <value>
    ///   The value of the <see cref="double" /> event argument ranges from [0, 1]
    /// </value>
    /// <remarks>
    ///   If an <see cref="Action" /> reports a progress outside the acceptable range, Captain will treat the action as
    ///   reporting indeterminate progress.
    /// </remarks>
    event EventHandler<double> OnProgressChanged;
  }
}