using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Declares imported functions from cn2helper/capture.c
  /// </summary>
  internal static class Capture {
    /// <summary>
    ///   Captures a single frame from the display
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="ptr"></param>
    /// <returns></returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2CaptureSingleFrame", SetLastError = true)]
    internal static extern bool CaptureSingleFrame([In] long x,
                                                   [In] long y,
                                                   [In] long width,
                                                   [In] long height,
                                                   [Out] IntPtr ptr);
  }
}
