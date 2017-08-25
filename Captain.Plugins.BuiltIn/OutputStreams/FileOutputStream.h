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
      [DisplayName("Save to disk")]
      [DisplayName("es", "Guardar en disco")]
      public ref class FileOutputStream : public IOutputStream, public FileStream {
      private:
        /// encoder information
        Common::EncoderInfo encoderInfo;

        /// actual file name
        String ^fileName;

        /// returns a file name for this capture
        static String ^GetFileName(String ^extension);

        /// actual initializer for this class
        FileOutputStream(String ^fileName);

      public:
        virtual property Common::EncoderInfo EncoderInfo {
          virtual Common::EncoderInfo get() sealed { return this->encoderInfo; };
          virtual void set(Common::EncoderInfo info) sealed { this->encoderInfo = info; }
        };

        /// zero-argument constructor as required by Captain
        FileOutputStream() : FileOutputStream(GetFileName(this->EncoderInfo.Extension)) {}

        /// commits changes to filesystem
        virtual CaptureResult ^Commit();
      };
    }
  }
}

