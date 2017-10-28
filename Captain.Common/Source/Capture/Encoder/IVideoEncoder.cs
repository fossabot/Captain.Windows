using System.Drawing;
using System.IO;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides the logic for encoding video captures
  /// </summary>
  public interface IVideoEncoder : IEncoderBase {
    /// <summary>
    ///   Begins encoding the video
    /// </summary>
    /// <param name="frameSize">Frame size</param>
    /// <param name="outputStream">Destination stream</param>
    void Start(Size frameSize, Stream outputStream);

    /// <summary>
    ///   Encodes a single frame
    ///   TODO: Implement some sort of (optional) AudioProvider support
    /// </summary>
    /// <param name="videoProvider">Video capture provider</param>
    /// <param name="outputStream">Destination stream</param>
    void Encode(VideoProvider videoProvider, Stream outputStream);

    /// <summary>
    ///   Terminates the capture encoding
    /// </summary>
    /// <param name="outputStream">Destination stream</param>
    void End(Stream outputStream);
  }
}
