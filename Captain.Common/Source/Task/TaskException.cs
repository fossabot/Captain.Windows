using System;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Represents an <see cref="T:System.Exception" /> thrown by a <see cref="T:Captain.Common.Task" /> invocation.
  /// </summary>
  public sealed class TaskException : Exception {
    /// <summary>
    ///   Gets or sets the short message for this exception.
    /// </summary>
    public string ShortMessage { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Exception constructor.
    /// </summary>
    /// <param name="shortMessage">Short message for this exception instance.</param>
    /// <param name="message">Long description for this exception.</param>
    /// <param name="innerException">Inner exception that is the cause of this exception.</param>
    public TaskException(string shortMessage, string message, Exception innerException) :
      base(message, innerException) {
      ShortMessage = shortMessage;
    }
  }
}