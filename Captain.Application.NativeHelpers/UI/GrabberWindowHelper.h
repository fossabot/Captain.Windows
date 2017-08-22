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
      /**
       * \brief Utility class for GrabberWindow UI
       */
      public ref class GrabberWindowHelper {
      protected:
        // underlying grabber window handle
        HWND hwnd;

      public:
        // logger instance
        Logger^ log;

        // handle for the device change notify registration
        HDEVNOTIFY hDevNotify;

        // maximum "virtual desktop" bounds
        int minLeft = 0, minTop = 0, maxRight = 0, maxBottom = 0;

        /**
         * \brief Previous window procedure
         */
        WNDPROC prevWndProc;

        /**
         * Window procedure hook
         */
        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, bool% handled);

        /**
        * \brief Creates an instance of this class
        * \param handle GrabberWindow handle
        * \param pHelper Instance pointer
        */
        GrabberWindowHelper(IntPtr handle);

        /**
         * Updates monitor geometry information
         */
        void UpdateMonitorGeometryInfo();

        /**
         * Class destructor
         */
        ~GrabberWindowHelper();
      };
    }
  }
}