using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Specifies Desktop Window Manager (DWM) blur-behind properties.
  ///   Used by the <see cref="DwmApi.DwmEnableBlurBehindWindow"/> function
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct DWM_BLURBEHIND {
    /// <summary>
    ///   A bitwise combination of DWM Blur Behind constant values that indicates which of the members of this
    ///   structure have been set
    /// </summary>
    internal DwmBbFlags dwFlags;

    /// <summary>
    ///   <c>true</c> to register the window handle to DWM blur behind
    /// </summary>
    internal bool fEnable;

    /// <summary>
    ///   The region within the client area where the blur behind will be applied
    /// </summary>
    internal IntPtr hRgnBlur;

    /// <summary>
    ///   <c>true</c> if the window's colorization should transition to match the maximized windows
    /// </summary>
    internal bool fTransitionOnMaximized;
  }
}