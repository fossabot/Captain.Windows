using System;
using System.Runtime.InteropServices;

// ReSharper disable All
namespace Captain.Application.Source.Native {
  /// <summary>
  ///   Defines the message parameters passed to a WH_CALLWNDPROC hook procedure, CallWndProc.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct CWPSTRUCT {
    /// <summary>
    ///   Additional information about the message. The exact meaning depends on the message value.
    /// </summary>
    internal IntPtr lParam;

    /// <summary>
    ///   Additional information about the message. The exact meaning depends on the message value. 
    /// </summary>
    internal IntPtr wParam;
    
    /// <summary>
    ///   The message.
    /// </summary>
    internal int message;

    /// <summary>
    ///   A handle to the window to receive the message. 
    /// </summary>
    internal IntPtr hwnd;
  }
}
