using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Declares imported functions from cn2helper/shell.c
  /// </summary>
  internal static class Process {
    /// <summary>
    ///   Determins whether a specific module has been loaded into a remote process or not
    /// </summary>
    /// <param name="dwProcessId">Process ID</param>
    /// <param name="szModuleBase">Module base name</param>
    /// <returns>Whether the module was found or not</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2ProcessFindModule", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool FindModule([In] uint dwProcessId, [In] string szModuleBase);

    /// <summary>
    ///   Find the parent process ID for the specified process
    /// </summary>
    /// <param name="dwProcessId">The target process ID</param>
    /// <returns>On success, returns the parent process ID; otherwise returns zero.</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2ProcessFindParentProcessId", SetLastError = true)]
    internal static extern uint FindParentProcessId([In] uint dwProcessId);
  }
}
