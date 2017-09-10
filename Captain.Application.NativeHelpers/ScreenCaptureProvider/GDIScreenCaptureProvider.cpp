#include "GDIScreenCaptureProvider.h"
#include "../Utils/MonitorEnumerator.h"

using namespace System::ComponentModel;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      LPVOID GDIScreenCaptureProvider::CaptureData() {
        SelectObject(this->hdcDest, this->hBmpWnd);
        BitBlt(this->hdcDest, 0, 0, this->cx, this->cy, this->hdcWnd, this->x, this->y, SRCCOPY);

        return this->hBmpWnd;
      }

      Bitmap^ GDIScreenCaptureProvider::CaptureBitmap() {
        this->CaptureData();
        Bitmap^ bmp = Image::FromHbitmap(IntPtr(hBmpWnd));

        ReleaseDC(this->hWnd, this->hdcWnd);
        this->hdcWnd = nullptr;

        ReleaseDC(this->hWnd, this->hdcDest);
        this->hdcDest = nullptr;

        DeleteObject(this->hBmpWnd);
        this->hBmpWnd = nullptr;

        return bmp;
      }

      GDIScreenCaptureProvider::GDIScreenCaptureProvider() {
        this->log = Logger::GetDefault();

        this->hWnd = GetDesktopWindow();
        this->hdcWnd = GetWindowDC(this->hWnd);
        this->hdcDest = CreateCompatibleDC(this->hdcWnd);
      }

      GDIScreenCaptureProvider::GDIScreenCaptureProvider(int monitor) : GDIScreenCaptureProvider::GDIScreenCaptureProvider() {
        this->monitor = monitor;
        this->log->WriteLine(LogLevel::Debug, "capturing monitor #{0}", monitor);

        MonitorEnumerator monEnum;
        if (monEnum.vrcMonitors.size() >= monitor) {
          this->log->WriteLine(LogLevel::Error, "monitor #{0} not present", monitor);
          throw gcnew ArgumentException("No such monitor");
        }

        RECT rcMon = monEnum.vrcMonitors.at(monitor);
        this->x = rcMon.left;
        this->y = rcMon.top;
        this->cx = rcMon.right - rcMon.left;
        this->cy = rcMon.bottom - rcMon.top;

        this->hBmpWnd = CreateCompatibleBitmap(this->hdcWnd, cx, cy);
      }

      GDIScreenCaptureProvider::GDIScreenCaptureProvider(int x, int y, int cx, int cy) : GDIScreenCaptureProvider::GDIScreenCaptureProvider() {
        this->log->WriteLine(LogLevel::Debug, "capturing region ({0} {1}, {2} {3})", x, y, cx, cy);

        this->x = x;
        this->y = y;
        this->cx = cx;
        this->cy = cy;

        this->hBmpWnd = CreateCompatibleBitmap(this->hdcWnd, cx, cy);
      }

      GDIScreenCaptureProvider::GDIScreenCaptureProvider(IntPtr handle) : GDIScreenCaptureProvider::GDIScreenCaptureProvider() {
        this->log->WriteLine(LogLevel::Debug, "capturing window (0x{0:x8})", handle);

        RECT rcWnd;
        if (!GetWindowRect(reinterpret_cast<HWND>(handle.ToPointer()), &rcWnd)) {
          this->log->WriteLine(LogLevel::Error, "could not obtain window bounds");
          throw gcnew Win32Exception(GetLastError());
        }

        this->x = rcWnd.left;
        this->y = rcWnd.top;
        this->cx = rcWnd.right - rcWnd.left;
        this->cy = rcWnd.bottom - rcWnd.top;

        this->hBmpWnd = CreateCompatibleBitmap(this->hdcWnd, rcWnd.right - rcWnd.left, rcWnd.bottom - rcWnd.top);
      }

      GDIScreenCaptureProvider::~GDIScreenCaptureProvider() {
        this->log->WriteLine(LogLevel::Debug, "releasing resources");

        if (this->hdcWnd) { ReleaseDC(this->hWnd, this->hdcWnd); }
        if (this->hdcDest) { ReleaseDC(this->hWnd, this->hdcDest); }
        if (this->hBmpWnd) { DeleteObject(this->hBmpWnd); }
      }
    }
  }
}