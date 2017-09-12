using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Declares imported functions from cn2helper/shell.c
  /// </summary>
  internal static class Display {
    /// <summary>
    ///   Gets the acceptable capture region
    /// </summary>
    /// <param name="outRect">Output instance of a <see cref="RECT"/> struct which will receive the region</param>
    /// <returns>Whether the operation completed successfully</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2DisplayGetAcceptableBounds", SetLastError = true)]
    internal static extern bool GetAcceptableBounds([Out] out RECT outRect);

    /// <summary>
    ///   Registers display device notifications for the specified window
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="devNotify">Device notification filter handle</param>
    /// <returns>Whether the operation completed successfully</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2DisplayRegisterChangeNotifications", SetLastError = true)]
    internal static extern bool RegisterChangeNotifications([In] IntPtr handle, [Out] out IntPtr devNotify);

    /// <summary>
    ///   Unregisters display device notifications
    /// </summary>
    /// <param name="devNotify">Device notification filter handle</param>
    /// <returns>Whether the operation completed successfully</returns>
    [DllImport("user32.dll", EntryPoint = "UnregisterDeviceNotification", SetLastError = true)]
    internal static extern bool UnregisterChangeNotifications([In] IntPtr devNotify);
  }
}
