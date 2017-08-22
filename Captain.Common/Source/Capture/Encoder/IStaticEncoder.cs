using System.Drawing;
using System.IO;

namespace Captain.Common {
  public interface IStaticEncoder : IEncoderBase {
    void Encode(Bitmap bitmap, Stream outputStream);
  }
}
