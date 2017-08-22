#pragma once

#include "FileOutputStream.h"

using namespace System;
using namespace System::IO;
using namespace System::Drawing;
using namespace Captain::Common;

namespace Captain {
  namespace Plugins {
    namespace BuiltIn {
      String^ FileOutputStream::GetFileName() {
        return gcnew String(Path::Combine(Environment::GetFolderPath(Environment::SpecialFolder::MyPictures), "test"));
      }

      FileOutputStream::FileOutputStream(String ^fileName) : FileStream(fileName, FileMode::CreateNew, FileAccess::Write) {
        this->fileName = fileName;
      }

      CaptureResult^ FileOutputStream::Commit() {
        // just to be sure
        this->Flush();

        CaptureResult ^res = gcnew CaptureResult();
        res->ToastTitle = "Capture saved";
        res->ToastContent = "The file has been saved to your Captures folder.";
        res->ToastUri = gcnew Uri(this->fileName);
        return res;
      }
    }
  }
}

