#include <Windows.h>
#include <dbt.h>
#include <initguid.h>
#include <Ntddvdeo.h>
#include <easyhook.h>
#include <psapi.h>

#include "GrabberWindowHelper.h"
#include "WindowAttachInfo.h"
#include "../Utils/ntstatus.h"

using namespace System;
using namespace System::ComponentModel;
using namespace System::Diagnostics;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      static gcroot<GrabberWindowHelper^> *pHelper = nullptr;

      /// creates an instance of this class, given the grabber UI window handle
      GrabberWindowHelper::GrabberWindowHelper(IntPtr handle) {
        // HACK: viva le dangerous code!
        pHelper = new gcroot<GrabberWindowHelper^>(this);

        this->hwnd = reinterpret_cast<HWND>(handle.ToPointer());
        this->log = Logger::GetDefault();

        // remove maximize/minimize capabilities from the grabber window
        SetWindowLong(this->hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX));
        SetWindowText(this->hwnd, "CaptainGrabberWindow");

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

      /// updates monitor geometry information and limits/positions the grabber UI accordingly
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
              outputWindowRect.right - outputWindowRect.left, outputWindowRect.bottom - outputWindowRect.top, SWP_NOACTIVATE | SWP_NOZORDER);

            if (this->pwaiAttached) {
              // there's an attached window - detach the window so some flags get reset and copy the new attachment information
              SendMessage(this->hwndAttached, WM_CAPTAINDETACHWINDOW, NULL, NULL);

              // update acceptable bounds
              this->pwaiAttached->rcAcceptableBounds.left = this->minLeft;
              this->pwaiAttached->rcAcceptableBounds.top = this->minTop;
              this->pwaiAttached->rcAcceptableBounds.right = this->maxRight;
              this->pwaiAttached->rcAcceptableBounds.bottom = this->maxBottom;

              // get attached window geometry
              GetWindowRect(this->hwndAttached, &outputWindowRect);

              // force window to reposition in case it is outside the acceptable bounds
              if (outputWindowRect.left < this->minLeft) { outputWindowRect.left = this->minLeft; }
              if (outputWindowRect.top < this->minTop) { outputWindowRect.top = this->minTop; }
              if (outputWindowRect.right > this->maxRight) { outputWindowRect.right = this->maxRight; }
              if (outputWindowRect.bottom > this->maxBottom) { outputWindowRect.bottom = this->maxBottom; }

              // move the attached window
              SetWindowPos(this->hwndAttached, nullptr, outputWindowRect.left, outputWindowRect.top,
                outputWindowRect.right - outputWindowRect.left, outputWindowRect.bottom - outputWindowRect.top, SWP_NOACTIVATE | SWP_NOZORDER);

              // copy WINATTACHINFO struct to remote process thread using WM_COPYDATA
              COPYDATASTRUCT copydata = { 0 };
              copydata.cbData = sizeof(WINATTACHINFO32);
              copydata.dwData = CAPTAIN_COPYDATA_SIGNATURE;
              copydata.lpData = this->pwaiAttached;

              // this will set g_winAttachInfo on CIWindowHelper and trigger the window re-attachment
              SendMessage(this->hwndAttached, WM_COPYDATA, reinterpret_cast<WPARAM>(this->hwnd), reinterpret_cast<LPARAM>(&copydata));
            }
          }
        }
      }

      /// attach the grabber UI to the nearest window
      void GrabberWindowHelper::AttachToNearestWindow() {
        RECT rcBounds = { 0 };
        GetWindowRect(this->hwnd, &rcBounds);

        // get the center point of the window
        POINT pCenter = { 0 };
        pCenter.x = (rcBounds.left + rcBounds.right) / 2;
        pCenter.y = (rcBounds.top + rcBounds.bottom) / 2;

        /* find any window on this point - easy right? */
        // WindowFromPoint() does not include hidden or disabled windows - we gotta hide ourselves so we don't hook our own WndProc!
        ShowWindow(this->hwnd, SW_HIDE);
        HWND hWnd = WindowFromPoint(pCenter);
        ShowWindow(this->hwnd, SW_SHOW);

        // get the root ancestor for this window, if any
        hWnd = GetAncestor(hWnd, GA_ROOT);

        if (!hWnd) {
          log->WriteLine(LogLevel::Error, "WindowFromPoint() failed - no window was found at ({0}, {1})", pCenter.x, pCenter.y);
          throw gcnew NullReferenceException("No suitable window found.");
        }

        CHAR szOutputWndClsName[32] = { 0 };
        GetClassNameA(hWnd, szOutputWndClsName, sizeof(szOutputWndClsName));
        log->WriteLine(LogLevel::Verbose, "selected {0}", gcnew String(szOutputWndClsName));

        // got it - move ourselves right here if we can I guess
        GetWindowRect(hWnd, &rcBounds);

        // make sure it does not overflow the capturable bounds
        if (rcBounds.left < this->minLeft || rcBounds.right > this->maxRight ||
          rcBounds.top < this->minTop || rcBounds.bottom > this->maxBottom) {
          // we fucked
          log->WriteLine(LogLevel::Error, "the selected window is beyond the capturable screen region");
          throw gcnew OverflowException("The window extends beyond the acceptable screen region.");
        }

        DWORD dwProcessId = NULL;
        DWORD dwThreadId = GetWindowThreadProcessId(hWnd, &dwProcessId);

        if (!dwProcessId) {
          log->WriteLine(LogLevel::Error, "GetWindowThreadProcessId() failed; LE=0x{0:x8}", GetLastError());
          throw gcnew Win32Exception(GetLastError());
        }

        // user data passed to the injected DLL
        this->pwaiAttached = new WINATTACHINFO32;
        this->pwaiAttached->hGrabberWndLong = PtrToLong(this->hwnd);
        this->pwaiAttached->hTargetWndLong = PtrToLong(hWnd);
        this->pwaiAttached->rcOrgTargetBounds = rcBounds;
        this->pwaiAttached->rcAcceptableBounds = { this->minLeft, this->minTop, this->maxRight, this->maxBottom };

        log->WriteLine(LogLevel::Debug, "hGrabberWndLong=0x{0:x8}; hTargetWndLong=0x{1:x8}", PtrToLong(this->hwnd), PtrToLong(hWnd));

        // look for the library on the remote process - it may be already injected!
        HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, dwProcessId);
        if (!hProcess) {
          log->WriteLine(LogLevel::Error, "OpenProcess() failed; LE=0x{0:x8}", GetLastError());
          throw gcnew Win32Exception(GetLastError());
        }

        BOOL bInjectionNeeded = TRUE;
        HMODULE hModules[1024];
        DWORD cbNeeded, dwIdx;
        WCHAR szModuleName[MAX_PATH + 1];

        // assume these are on pwd
        LPCWSTR szHelper32 = L"CIWindowHelper32.dll";
        LPCWSTR szHelper64 = L"CIWindowHelper64.dll";
        LPCWSTR szWoW64Helper = L"CIWoW64Helper.exe";

        if (EnumProcessModules(hProcess, hModules, sizeof(hModules), &cbNeeded)) {
          for (dwIdx = 0; dwIdx < cbNeeded / sizeof(HMODULE); dwIdx++) {
            if (GetModuleBaseNameW(hProcess, hModules[dwIdx], szModuleName, _countof(szModuleName))) {
              // if it's our DLL, don't inject it and send a reactivation WM
              log->WriteLine(LogLevel::Debug, "module: {0}", gcnew String(szModuleName));
              if (!wcscmp(szHelper32, szModuleName) || !wcscmp(szHelper64, szModuleName)) {
                log->WriteLine(LogLevel::Warning, "DLL already injected - sending re-attachment message");

                // copy WINATTACHINFO struct to remote process thread using WM_COPYDATA
                COPYDATASTRUCT copydata = { 0 };
                copydata.cbData = sizeof(WINATTACHINFO32);
                copydata.dwData = CAPTAIN_COPYDATA_SIGNATURE;
                copydata.lpData = this->pwaiAttached;

                // this will set g_winAttachInfo on CIWindowHelper and trigger the window re-attachment
                SendMessage(hWnd, WM_COPYDATA, reinterpret_cast<WPARAM>(this->hwnd), reinterpret_cast<LPARAM>(&copydata));

                // no hacks required
                bInjectionNeeded = FALSE;
              }
            }
          }
        }
        else {
          log->WriteLine(LogLevel::Error, "EnumProcessModules() failed; LE=0x{0:x8}", GetLastError());
          CloseHandle(hProcess);
          throw gcnew Win32Exception(GetLastError());
        }

        CloseHandle(hProcess);

        if (bInjectionNeeded) {
          // inject DLL
          ULONG ulInjectionOptions = EASYHOOK_INJECT_STEALTH;
          log->WriteLine(LogLevel::Debug, "injecting library");

          BYTE bAttempts = 3;

        inject:
          NTSTATUS status = RhInjectLibrary(dwProcessId, ulInjectionOptions == EASYHOOK_INJECT_DEFAULT ? dwThreadId : 0, ulInjectionOptions,
            (PWCHAR)szHelper32, (PWCHAR)szHelper64, (LPVOID)pwaiAttached, sizeof(WINATTACHINFO32));

          if (status == STATUS_WOW_ASSERTION) {
            log->WriteLine(LogLevel::Warning, "RhInjectLibrary() can't hook through WoW64 barrier - invoking injection helper");

            ProcessStartInfo ^startInfo = gcnew ProcessStartInfo();
            startInfo->FileName = gcnew String(szWoW64Helper);
            startInfo->Arguments = String::Format("{0} {1} {2} \"{3}\"", dwProcessId, dwThreadId, "default", gcnew String(szHelper32));
            startInfo->CreateNoWindow = true;
            startInfo->UseShellExecute = false;
            startInfo->RedirectStandardInput = true;

            Process ^process = gcnew Process();
            process->StartInfo = startInfo;
            process->Start();

            cli::array<wchar_t> ^data = gcnew cli::array<wchar_t>(sizeof(WINATTACHINFO32));
            cli::pin_ptr<wchar_t> data_start = &data[0];

            memcpy(data_start, reinterpret_cast<LPVOID>(pwaiAttached), sizeof(WINATTACHINFO32));

            process->StartInfo = startInfo;
            process->StandardInput->BaseStream->Write(reinterpret_cast<cli::array<unsigned char>^>(data), 0, sizeof(WINATTACHINFO32));
            process->StandardInput->BaseStream->Flush();
            log->WriteLine(LogLevel::Debug, "wrote {0} bytes to process standard input", sizeof(WINATTACHINFO32));

            process->WaitForExit();
            log->WriteLine(LogLevel::Informational, "WoW64 helper process exited with code 0x{0:x8}", process->ExitCode);

            if (process->ExitCode != EXIT_SUCCESS) {
              throw gcnew Win32Exception(process->ExitCode, gcnew String("Can't hook through WoW64 barrier"));
            }
          }
          else if (status != ERROR_SUCCESS) {
            goto inject;

            // we did our best
            log->WriteLine(LogLevel::Error, "RhInjectLibrary() failed with error 0x{0:x8}: {1}", status, gcnew String(RtlGetLastErrorString()));

            if (ulInjectionOptions == EASYHOOK_INJECT_STEALTH) {
              log->WriteLine(LogLevel::Warning, "retrying in default injection mode");
              ulInjectionOptions = EASYHOOK_INJECT_DEFAULT;
              goto inject;
            }

            throw gcnew Win32Exception(status);
          }

          log->WriteLine(LogLevel::Informational, "library injection succeeded; dwProcessId={0}, dwThreadId={1}", dwProcessId, dwThreadId);
        }
        else {
          log->WriteLine(LogLevel::Debug, "library injection not needed");
        }

        // injection was successful
        this->hwndAttached = hWnd;

        // hide ourselves! We don't really need the area grabber UI, just the lil' toolbar
        // but first adjust our size so that it matches the window's and the toolbar lies at the correct position!
        this->prcOrgBounds = new RECT;
        GetWindowRect(this->hwnd, this->prcOrgBounds);  // save current bounds
        SetWindowPos(this->hwnd, nullptr, rcBounds.left, rcBounds.top, rcBounds.right - rcBounds.left, rcBounds.bottom - rcBounds.top, 0);
        ShowWindow(this->hwnd, SW_HIDE);
      }

      /// detach the grabber UI from the previous window
      void GrabberWindowHelper::DetachFromWindow() {
        if (this->hwndAttached) {
          // send detach message
          SendMessage(this->hwndAttached, WM_CAPTAINDETACHWINDOW, NULL, NULL);
          SetWindowPos(this->hwnd, nullptr, this->prcOrgBounds->left, this->prcOrgBounds->top, this->prcOrgBounds->right - this->prcOrgBounds->left, this->prcOrgBounds->bottom - this->prcOrgBounds->top, 0);
          ShowWindow(this->hwnd, SW_SHOW);

          delete this->pwaiAttached;
        }
        else {
          log->WriteLine(LogLevel::Error, "no window is currently attached");
          throw gcnew NullReferenceException("No window is currently attached.");
        }
      }

      /// window procedure hook
      IntPtr GrabberWindowHelper::WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, bool% handled) {
        switch (msg) {
        case WM_DEVICECHANGE:
          if (wParam.ToInt32() == DBT_DEVNODES_CHANGED) {
            // a video adapter device has been removed/added
            this->UpdateMonitorGeometryInfo();
          }

          break;

        case WM_WINDOWPOSCHANGING: {
          WINDOWPOS *pos = reinterpret_cast<WINDOWPOS*>(lParam.ToPointer());

          // prevent the window from going off-screen
          if (pos->x < this->minLeft) { pos->x = this->minLeft; }
          if (pos->y < this->minTop) { pos->y = this->minTop; }
          if (pos->x + pos->cx > this->maxRight) { pos->x = this->maxRight - pos->cx; }
          if (pos->y + pos->cy > this->maxBottom) { pos->y = this->maxBottom - pos->cy; }

          break;
        }

        case WM_CLOSE:
        case WM_QUIT:
          delete this;
          break;
        }

        return IntPtr::Zero;
      }

      /// class destructor
      GrabberWindowHelper::~GrabberWindowHelper() {
        log->WriteLine(LogLevel::Debug, "releasing resources");
        UnregisterDeviceNotification(this->hDevNotify);

        if (this->hwndAttached) { this->DetachFromWindow(); }
        if (this->prcOrgBounds) { delete this->prcOrgBounds; }
      }
    }
  }
}