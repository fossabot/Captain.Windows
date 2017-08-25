#pragma once

using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace Captain::Common;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      [DisplayName("PNG")]
      public ref class PNGCaptureEncoder : public IStaticEncoder {
      public:
        virtual property EncoderInfo EncoderInfo {
          virtual Common::EncoderInfo get() sealed {
            Common::EncoderInfo info;
            info.Extension = "png";
            info.MediaType = "image/png";

            return info;
          }
        }

        virtual void Encode(Bitmap^ bmp, Stream^ outStream);
      };
    }
  }
}

