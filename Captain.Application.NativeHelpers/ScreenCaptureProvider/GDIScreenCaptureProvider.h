#pragma once

#include <Windows.h>

#include "ScreenCaptureProvider.h"

using namespace Captain::Common;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      public ref class GDIScreenCaptureProvider : public ScreenCaptureProvider {
      private:
        int monitor = -1;
        int x, y;
        int cx, cy;

        Logger^ log;

        HWND hWnd;
        HDC hdcWnd;
        HDC hdcDest;
        HBITMAP hBmpWnd;

        GDIScreenCaptureProvider();

      public:
        LPVOID CaptureData() override;
        Bitmap^ CaptureBitmap() override;

        GDIScreenCaptureProvider(int monitor);
        GDIScreenCaptureProvider(int x, int y, int cx, int cy);
        GDIScreenCaptureProvider(IntPtr handle);
        ~GDIScreenCaptureProvider();
      };
    }
  }
}