using System;
using System.Drawing;
using System.IO;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   A action handles captures.
  /// </summary>
  public class Action : Stream {
    #region Fields

    /// <summary>
    ///   Length, in bytes, of the stream.
    /// </summary>
    private long length;

    /// <summary>
    ///   Current action status.
    /// </summary>
    private ActionStatus status = ActionStatus.Ongoing;

    /// <summary>
    ///   Optional status message.
    /// </summary>
    private string statusMessage;

    #endregion

    #region Properties

    /// <summary>
    ///   Codec instance.
    /// </summary>
    public ICodecBase Codec { get; private set; }

    /// <summary>
    ///   Current action status.
    /// </summary>
    public ActionStatus Status {
      get => this.status;
      protected set {
        this.status = value;
        OnStatusChanged?.Invoke(this, value);
      }
    }

    /// <summary>
    ///   Optional status message.
    /// </summary>
    public string StatusMessage {
      get => this.statusMessage;
      set {
        this.statusMessage = value;
        OnStatusMessageChanged?.Invoke(this, value);
      }
    }

    /// <summary>
    ///   Thumbnail bitmap to be displayed in toast notifications and in the action list dialog.
    /// </summary>
    public Bitmap Thumbnail { get; set; }

    /// <summary>
    ///   When the task fails, exception that caused this task to have the <see cref="ActionStatus.Failed"/> status.
    /// </summary>
    public Exception InnerException { get; protected set; }

    /// <summary>
    ///   Buffer size.
    /// </summary>
    public int BufferSize { get; protected set; } = 8192;

    #endregion

    #region Events

    /// <summary>
    ///   Triggered when the action status changes.
    /// </summary>
    public event EventHandler<ActionStatus> OnStatusChanged;

    /// <summary>
    ///   Triggered when the action status message changes.
    /// </summary>
    public event EventHandler<string> OnStatusMessageChanged;

    #endregion

    #region Overriden Stream properties

    /// <inheritdoc />
    /// <summary>
    ///   Indicates whether the current stream supports being read.
    /// </summary>
    public sealed override bool CanRead => false;

    /// <inheritdoc />
    /// <summary>
    ///   Indicates whether the current stream supports seeking.
    /// </summary>
    public sealed override bool CanSeek => false;

    /// <inheritdoc />
    /// <summary>
    ///   Indicates whether the current stream supports being written to.
    /// </summary>
    public sealed override bool CanWrite => true;

    /// <inheritdoc />
    /// <summary>
    ///   Length, in bytes, of the stream.
    /// </summary>
    public override long Length => this.length;

    /// <inheritdoc />
    /// <summary>Gets or sets the position within the current stream.</summary>
    /// <returns>The current position within the stream.</returns>
    /// <exception cref="T:System.IO.IOException">An I/O error occurs.</exception>
    /// <exception cref="T:System.NotSupportedException">The stream does not support seeking.</exception>
    /// <exception cref="T:System.ObjectDisposedException">Methods were called after the stream was closed.</exception>
    public override long Position { get; set; }

    #endregion

    /// <inheritdoc />
    /// <summary>
    ///   Creates a new instance of this class.
    /// </summary>
    /// <param name="codec">Codec instance.</param>
    public Action(ICodecBase codec) => Codec = codec;

    /// <summary>
    ///   Sets task status.
    /// </summary>
    /// <param name="newStatus">New task status</param>
    /// <param name="innerException">Optional exception for <see cref="ActionStatus.Failed"/></param>
    public void SetStatus(ActionStatus newStatus, Exception innerException = null) {
      Status = newStatus;
      InnerException = innerException;
      Flush();
    }

    #region Overriden Stream methods

    /// <inheritdoc />
    /// <summary>
    ///   Clears all buffers for this stream and causes any buffered data to be written to the underlying device.
    /// </summary>
    public override void Flush() { }

    /// <inheritdoc />
    /// <summary>
    ///   Reads a sequence of bytes from the current stream and advances the position within the stream by the number
    ///   of bytes read.
    /// </summary>
    /// <remarks>
    ///   Actions do not support to be read from.
    /// </remarks>
    /// <param name="buffer">An array of bytes.</param>
    /// <param name="offset">
    ///   The zero-based byte offset in <paramref name="buffer" /> at which to begin storing the data read from the
    ///   current stream.
    /// </param>
    /// <param name="count">The maximum number of bytes to be read from the current stream.</param>
    /// <returns>The number of bytes read into the buffer.</returns>
    /// <exception cref="T:System.NotSupportedException">Thrown when this method is called.</exception>
    public sealed override int Read(byte[] buffer, int offset, int count) =>
      throw new NotSupportedException("Action streams are write-only");

    /// <inheritdoc />
    /// <summary>
    ///   Sets the position within the current stream.
    /// </summary>
    /// <param name="offset">A byte offset relative to the <paramref name="origin" /> parameter.</param>
    /// <param name="origin">A value indicating the reference point used to obtain the new position.</param>
    /// <returns>The new position within the current stream.</returns>
    public override long Seek(long offset, SeekOrigin origin) {
      switch (origin) {
        case SeekOrigin.End: throw new InvalidOperationException("Can't seek from the end of the stream.");
        case SeekOrigin.Begin:
          Position = offset;
          break;
        case SeekOrigin.Current:
          Position -= offset;
          break;
      }

      return Position;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Sets the length of the current stream.
    /// </summary>
    /// <param name="value">The desired length of the current stream in bytes.</param>
    public sealed override void SetLength(long value) => this.length = value;

    /// <inheritdoc />
    /// <summary>
    ///   Writes a sequence of bytes to the current stream and advances the current position within this stream by the
    ///   number of bytes written.
    /// </summary>
    /// <param name="buffer">An array of bytes to be copied.</param>
    /// <param name="offset">
    ///   The zero-based byte offset in <paramref name="buffer" /> at which to begin copying bytes to the current
    ///   stream.
    /// </param>
    /// <param name="count">The number of bytes to be written to the current stream.</param>
    public override void Write(byte[] buffer, int offset, int count) {}

    #endregion
  }
}