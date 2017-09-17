// ReSharper disable All

using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Contains data to be passed to another application by the WM_COPYDATA message.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct COPYDATASTRUCT {
    /// <summary>
    ///   The data to be passed to the receiving application.
    /// </summary>
    internal IntPtr dwData;

    /// <summary>
    ///   The size, in bytes, of the data pointed to by the lpData member.
    /// </summary>
    internal IntPtr cbData;

    /// <summary>
    ///   The data to be passed to the receiving application. This member can be NULL.
    /// </summary>
    //[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.R8)]
    internal IntPtr lpData;
  }
}
