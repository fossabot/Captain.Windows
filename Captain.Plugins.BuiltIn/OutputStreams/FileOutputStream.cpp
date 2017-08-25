#pragma once

#include "FileOutputStream.h"

using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace Captain::Common;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      String^ FileOutputStream::GetFileName(String ^extension) {
        return gcnew String(Path::Combine(Environment::GetFolderPath(Environment::SpecialFolder::MyPictures), DateTime::Now.ToString("dd-MM-yyyy hh.mm.ss.") + extension));
      }

      FileOutputStream::FileOutputStream(String ^fileName) : FileStream(fileName, FileMode::CreateNew, FileAccess::Write) {
        this->fileName = fileName;
      }

      CaptureResult^ FileOutputStream::Commit() {
        CaptureResult ^res = gcnew CaptureResult();
        res->ToastTitle = "Capture saved";
        res->ToastContent = "The file has been saved to your Captures folder.";
        res->ToastUri = gcnew Uri(this->fileName);
        res->ToastPreview = this->encoderInfo.PreviewBitmap;
        return res;
      }
    }
  }
}

