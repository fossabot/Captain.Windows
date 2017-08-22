#include <Windows.h>
#include <dbt.h>
#include <initguid.h>
#include <Ntddvdeo.h>

#include "GrabberWindowHelper.h"

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      /**
      * \brief Creates an instance of this class
      * \param handle GrabberWindow handle
      * \param pHelper Instance pointer
      */
      GrabberWindowHelper::GrabberWindowHelper(IntPtr handle) {
        this->hwnd = reinterpret_cast<HWND>(handle.ToPointer());
        this->log = Logger::GetDefault();

        // save original window procedure
        this->prevWndProc = reinterpret_cast<WNDPROC>(GetWindowLongPtr(this->hwnd, GWLP_WNDPROC));

        // remove maximize/minimize capabilities from the grabber window
        SetWindowLong(this->hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX));

        // register notifications for changes on display devices
        DEV_BROADCAST_DEVICEINTERFACE devBroadcastFilter = { 0 };
        devBroadcastFilter.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
        devBroadcastFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
        devBroadcastFilter.dbcc_classguid = GUID_DEVINTERFACE_MONITOR;

        if (!(this->hDevNotify = RegisterDeviceNotification(hwnd, &devBroadcastFilter, DEVICE_NOTIFY_WINDOW_HANDLE))) {
          log->WriteLine(LogLevel::Warning, "could not register device notification handler");
        }

        this->UpdateMonitorGeometryInfo();
      }

      /**
       * Window procedure hook
       */
      IntPtr GrabberWindowHelper::WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, bool% handled) {
        if (msg == WM_DEVICECHANGE) {
          if (wParam.ToInt32() == DBT_DEVNODES_CHANGED) {
            // a video adapter device has been removed/added
            this->UpdateMonitorGeometryInfo();
          }
        }
        else if (msg == WM_WINDOWPOSCHANGING) {
          WINDOWPOS *pos = reinterpret_cast<WINDOWPOS*>(lParam.ToPointer());

          // prevent the window from going off-screen
          if (pos->x < this->minLeft) { pos->x = this->minLeft; }
          if (pos->y < this->minTop) { pos->y = this->minTop; }
          if (pos->x + pos->cx > this->maxRight) { pos->x = this->maxRight - pos->cx; }
          if (pos->y + pos->cy > this->maxBottom) { pos->y = this->maxBottom - pos->cy; }
        }

        return IntPtr::Zero;
      }

      /**
       * Updates monitor geometry information
       */
      void GrabberWindowHelper::UpdateMonitorGeometryInfo() {
        HRESULT hr;

        // reset "virtual desktop" bounds
        this->minLeft = this->minTop = this->maxRight = this->maxBottom = 0;
        log->WriteLine(LogLevel::Informational, "updating monitor geometry information");

        /* initialize DXGI */
        IDXGIFactory *pFactory;
        if (FAILED(hr = CreateDXGIFactory(__uuidof(IDXGIFactory), (void**)&pFactory))) {
          log->WriteLine(LogLevel::Error, "CreateDXGIFactory() failed - hr=0x{0:8x}", hr);
          throw gcnew InvalidOperationException("Could not create DXGI factory.");
        }

        HWND hAssociatedWnd;
        if (FAILED(hr = pFactory->GetWindowAssociation(&hAssociatedWnd))) {
          log->WriteLine(LogLevel::Error, "pFactory->GetWindowAssociation() failed - hr=0x{0:8x}", hr);
          throw gcnew InvalidOperationException("Could not get window association.");
        }

        // enumerate outputs
        IDXGIAdapter *pAdapter;
        IDXGIOutput *pOutput;

        DXGI_ADAPTER_DESC adapterDesc = { 0 };
        DXGI_OUTPUT_DESC outputDesc = { 0 };
        POINT outputTopLeftEdge = { 0 };
        RECT outputWindowRect = { 0 };
        HWND outputHwnd = nullptr;
        CHAR szOutputWndClsName[32];

        for (UINT uiAdapterIdx = 0; pFactory->EnumAdapters(uiAdapterIdx, &pAdapter) != DXGI_ERROR_NOT_FOUND; uiAdapterIdx++, pAdapter->Release()) {
          pAdapter->GetDesc(&adapterDesc);
          log->WriteLine(LogLevel::Debug, "[adapter #{0}] {1}", uiAdapterIdx, gcnew String(adapterDesc.Description));

          for (UINT uiOutputIdx = 0; pAdapter->EnumOutputs(uiOutputIdx, &pOutput) != DXGI_ERROR_NOT_FOUND; uiOutputIdx++, pOutput->Release()) {
            pOutput->GetDesc(&outputDesc);
            log->WriteLine(LogLevel::Debug, "\t[output #{0}] {1} ({2} {3}, {4} {5}); attached={6}", uiOutputIdx, gcnew String(outputDesc.DeviceName),
              outputDesc.DesktopCoordinates.left, outputDesc.DesktopCoordinates.top,
              outputDesc.DesktopCoordinates.right - outputDesc.DesktopCoordinates.left,
              outputDesc.DesktopCoordinates.bottom - outputDesc.DesktopCoordinates.top,
              outputDesc.AttachedToDesktop);

            if (!outputDesc.AttachedToDesktop) {
              log->WriteLine(LogLevel::Warning, "\tnot attached to desktop - excluding output device");
              continue;
            }

            // construct the point that is on the top-left edge of the output device area
            outputTopLeftEdge.x = outputDesc.DesktopCoordinates.left;
            outputTopLeftEdge.y = outputDesc.DesktopCoordinates.top;

            // try to find a window on that position
            if (outputHwnd = WindowFromPoint(outputTopLeftEdge)) {
              GetClassName(outputHwnd, szOutputWndClsName, sizeof(szOutputWndClsName));
              log->WriteLine(LogLevel::Debug, "\tfound window on the top-left edge of this output; outputHwnd=0x{0:8x}, lpszOutputWndClsName={1}", IntPtr(outputHwnd),
                gcnew String(szOutputWndClsName));

              // get window bounds
              if (GetWindowRect(outputHwnd, &outputWindowRect)) {
                // compare the window bounds to those of the output device area - if they match the window is on full-screen mode.
                // If this is the case, we may not be able to capture that screen region
                if (!memcmp(&outputWindowRect, &outputDesc.DesktopCoordinates, sizeof(RECT))) {
                  log->WriteLine(LogLevel::Warning, "\twindow and output geometries are identical");

                  if (GetWindowLong(outputHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) {
                    // now we are certain this window does not intend to be captured under normal methods!
                    log->WriteLine(LogLevel::Warning, "\twon't be able to draw over window - excluding output device");
                    continue;
                  }
                }
              }
              else {
                log->WriteLine(LogLevel::Warning, "\tGetWindowRect() failed; LE=0x{0:8x}", GetLastError());
              }
            }

            // we can draw on this output device!
            this->minLeft = min(this->minLeft, outputDesc.DesktopCoordinates.left);
            this->minTop = min(this->minTop, outputDesc.DesktopCoordinates.top);
            this->maxRight = max(this->maxRight, outputDesc.DesktopCoordinates.right);
            this->maxBottom = max(this->maxBottom, outputDesc.DesktopCoordinates.bottom);

            // get grabber window geometry
            GetWindowRect(this->hwnd, &outputWindowRect);

            // force window to reposition in case it is outside the acceptable bounds
            if (outputWindowRect.left < this->minLeft) { outputWindowRect.left = this->minLeft; }
            if (outputWindowRect.top < this->minTop) { outputWindowRect.top = this->minTop; }
            if (outputWindowRect.right > this->maxRight) { outputWindowRect.right = this->maxRight; }
            if (outputWindowRect.bottom > this->maxBottom) { outputWindowRect.bottom = this->maxBottom; }

            SetWindowPos(this->hwnd, nullptr, outputWindowRect.left, outputWindowRect.top,
              outputWindowRect.right - outputWindowRect.left, outputWindowRect.bottom - outputWindowRect.top, 0);
          }
        }
      }

      /**
       * Class destructor
       */
      GrabberWindowHelper::~GrabberWindowHelper() {
        log->WriteLine(LogLevel::Debug, "releasing resources");
        UnregisterDeviceNotification(this->hDevNotify);
        SetWindowLongPtr(this->hwnd, GWLP_WNDPROC, reinterpret_cast<LONG_PTR>(this->prevWndProc));
        SetWindowLongPtr(this->hwnd, GWLP_USERDATA, NULL);
      }
    }
  }
}