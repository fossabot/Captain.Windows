#pragma once

#include <Windows.h>
#include <msclr/gcroot.h>
#include <d3d9.h>
#include <dxgi.h>

#include <vector>

using namespace std;
using namespace msclr;
using namespace System;
using namespace Captain::Common;

namespace Captain {
  namespace Application {
    namespace NativeHelpers {
      /// utility class for the grabber UI
      public ref class GrabberWindowHelper {
      protected:
        /// grabber area-selection window handle
        HWND hwnd;

        /// handle of the window the UI is attached ot
        HWND hwndAttached;

        /// original grabber window bounds (before attaching to window)
        PRECT prcOrgBounds = nullptr;

        /// logger instance
        Logger^ log;

        /// handle for the device change notify registration
        HDEVNOTIFY hDevNotify;

        /// remote AttachWindow() procedure
        FARPROC fpAttachWindowRemote = nullptr;

        /// maximum "virtual desktop" bounds
        int minLeft = 0, minTop = 0, maxRight = 0, maxBottom = 0;

      public:
        /// creates an instance of this class, given the grabber UI window handle
        GrabberWindowHelper(IntPtr handle);

        /// updates monitor geometry information and limits/positions the grabber UI accordingly
        void UpdateMonitorGeometryInfo();

        /// attach the grabber UI to the nearest window
        void AttachToNearestWindow();

        /// detach the grabber UI from the previous window
        void DetachFromWindow();

        /// window procedure hook
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, bool% handled);

        /// class destructor
        ~GrabberWindowHelper();
      };
    }
  }
}