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
        virtual String^ GetFileExtension();
        virtual void Encode(Bitmap^ bmp, Stream^ outStream);
      };
    }
  }
}

