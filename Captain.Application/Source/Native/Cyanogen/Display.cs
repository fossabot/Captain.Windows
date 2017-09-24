using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Declares imported functions from cn2helper/display.c
  /// </summary>
  internal static class Display {
    /// <summary>
    ///   Registers display device notifications for the specified window
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="devNotify">Device notification filter handle</param>
    /// <returns>Whether the operation completed successfully</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2DisplayRegisterChangeNotifications", SetLastError = true,
      CallingConvention = CallingConvention.Winapi)]
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
