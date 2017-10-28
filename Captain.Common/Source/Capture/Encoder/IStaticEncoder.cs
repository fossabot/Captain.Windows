using System.Drawing;
using System.IO;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Provides the logic for encoding static captures
  /// </summary>
  public interface IStaticEncoder : IEncoderBase {
    /// <summary>
    ///   Encodes a single frame
    /// </summary>
    /// <param name="bitmap">Bitmap to be encoded</param>
    /// <param name="outputStream">Destination stream</param>
    void Encode(Bitmap bitmap, Stream outputStream);
  }
}
