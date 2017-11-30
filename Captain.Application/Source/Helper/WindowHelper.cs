﻿using System;
using System.Runtime.InteropServices;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Implements diverse utilities for working with windows
  /// </summary>
  internal static class WindowHelper {
    /// <summary>
    ///   Retrieves the specified window's bounds, including the window frame
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <returns>A <see cref="RECT"/> structure containing the window bounds</returns>
    internal static RECT GetWindowBounds(IntPtr handle) {
      // try to use DWM to retrieve the extended window frame bounds - fall back to GetWindowRect on failure
      if (DwmApi.DwmGetWindowAttribute(handle,
                                       DwmApi.DwmWindowAttribute.DWMWA_EXTENDED_FRAME_BOUNDS,
                                       out RECT rect,
                                       Marshal.SizeOf(typeof(RECT))) != 0) {
        Log.WriteLine(LogLevel.Warning, "DwmGetWindowAttribute() failed - falling back to GetWindowRect()");
        User32.GetWindowRect(handle, out rect);
      }

      return rect;
    }
  }
}
