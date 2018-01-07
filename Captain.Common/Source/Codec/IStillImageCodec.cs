using System.IO;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides the logic for encoding static captures
  /// </summary>
  public interface IStillImageCodec : ICodecBase {
    /// <summary>
    ///   Encodes a still image
    /// </summary>
    /// <param name="data">Bitmap data</param>
    /// <param name="stream">Output stream</param>
    void Encode(BitmapData data, Stream stream);
  }
}