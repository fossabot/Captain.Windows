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
        /// actual file name
        String ^fileName;

        /// returns a file name for this capture
        static String ^GetFileName();

        /// actual initializer for this class
        FileOutputStream(String ^fileName);

      public:
        /// zero-argument constructor as required by Captain
        FileOutputStream() : FileOutputStream(GetFileName()) {}

        /// commits changes to filesystem
        virtual CaptureResult ^Commit();
      };
    }
  }
}

