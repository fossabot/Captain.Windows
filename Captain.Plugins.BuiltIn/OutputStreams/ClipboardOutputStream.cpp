#include "ClipboardOutputStream.h"

using namespace System::Drawing;
using namespace System::Windows::Forms;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      ClipboardOutputStream::ClipboardOutputStream() {
        if (!this->encoderInfo.MediaType->StartsWith("image/")) {
          // this ain't no image!
          throw gcnew UnsupportedMediaException();
        }
      }

      CaptureResult ^ClipboardOutputStream::Commit() {
        Clipboard::SetImage(Image::FromStream(this));

        CaptureResult ^result = gcnew CaptureResult();
        result->ToastTitle = "Screenshot copied!";
        result->ToastContent = "The screenshot has been copied to your clipboard.";
        result->ToastPreview = this->encoderInfo.PreviewBitmap;

        return result;
      }
    }
  }
}

