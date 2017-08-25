#pragma once

using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace Captain::Common;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      ///
      ///  the FileOutputStream simply saves captures to disk
      ///
      [DisplayName("Copy to clipboard")]
      [DisplayName("es", "Copiar al portapapeles")]
      public ref class ClipboardOutputStream : public IOutputStream, public MemoryStream {
      private:
        /// encoder information
        Common::EncoderInfo encoderInfo;

      public:
        virtual property Common::EncoderInfo EncoderInfo {
          virtual Common::EncoderInfo get() sealed { return this->encoderInfo; };
          virtual void set(Common::EncoderInfo info) sealed { this->encoderInfo = info; }
        };

        /// zero-argument constructor as required by Captain
        ClipboardOutputStream();

        /// commits changes to filesystem
        virtual CaptureResult ^Commit();
      };
    }
  }
}

