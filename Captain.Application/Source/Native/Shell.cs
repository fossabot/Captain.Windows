using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Declares imported functions from cn2helper/shell.c
  /// </summary>
  internal static class Shell {
    /// <summary>
    ///   Reveals a path in a File Explorer window
    /// </summary>
    /// <param name="path">The path</param>
    /// <returns>A value representing the operation status</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2ShellRevealInFileExplorer")]
    [return: MarshalAs(UnmanagedType.Error)]
    internal static extern int RevealInFileExplorer([In, MarshalAs(UnmanagedType.LPWStr)] string path);

    /// <summary>
    ///   Creates an app shortcut in the user's start menu directory
    /// </summary>
    /// <param name="productName">Name o the shortcut to be created</param>
    /// <param name="path">Executable path for the application to be launched</param>
    /// <param name="appId">Application ID</param>
    /// <returns>A value representing the operation result</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2InstallAppShortcut")]
    [return: MarshalAs(UnmanagedType.Error)]
    internal static extern int InstallAppShortcut(
      [In, MarshalAs(UnmanagedType.LPWStr)] string productName,
      [In, MarshalAs(UnmanagedType.LPWStr)] string path,
      [In, MarshalAs(UnmanagedType.LPWStr)] string appId);
  }
}
