#include "PNGCaptureEncoder.h"

using namespace System::Drawing::Imaging;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      String^ PNGCaptureEncoder::GetFileExtension() { return "png"; }
      void PNGCaptureEncoder::Encode(Bitmap^ bmp, Stream^ outStream) {
        reinterpret_cast<Image^>(bmp)->Save(outStream, ImageFormat::Png);
      }
    }
  }
}