#pragma once

#include <Windows.h>
#include <cstdint>

using namespace System;
using namespace System::Drawing;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      public ref class ScreenCaptureProvider abstract : public IDisposable {
      protected:
        int monitor = -1;
        int x = 0, y = 0, cx = 0, cy = 0;

      public:
        virtual LPVOID CaptureData() = 0;
        virtual Bitmap^ CaptureBitmap() = 0;
        virtual ~ScreenCaptureProvider() {};
      };
    }
  }
}