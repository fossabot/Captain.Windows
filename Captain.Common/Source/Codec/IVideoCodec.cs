using System.Drawing;
using System.IO;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides the logic for encoding video captures
  /// </summary>
  public interface IVideoCodec : ICodecBase {
    /// <summary>
    ///   Frame rate, in frames per second.
    /// </summary>
    int FrameRate { get; }

    /// <summary>
    ///   Bit rate, in bits per second.
    /// </summary>
    int BitRate { get; }

    /// <summary>
    ///   Begins encoding the video.
    /// </summary>
    /// <param name="frameSize">Frame size.</param>
    /// <param name="stream">Output stream.</param>
    void Initialize(Size frameSize, Stream stream);

    /// <summary>
    ///   Encodes a video frame.
    /// </summary>
    /// <param name="data">Bitmap data.</param>
    /// <param name="time">Time in ticks for this frame.</param>
    /// <param name="stream">Output stream.</param>
    void Encode(BitmapData data, long time, Stream stream);

    /// <summary>
    ///   Finalizes video encoding.
    /// </summary>
    /// <param name="stream">Output stream.</param>
    void Finalize(Stream stream);
  }
}