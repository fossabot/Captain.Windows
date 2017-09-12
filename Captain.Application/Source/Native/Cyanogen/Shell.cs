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
    /// <returns>Whether the operation completed successfully</returns>
    [DllImport("cn2helper.dll", EntryPoint = "CN2ShellRevealInFileExplorer", CharSet = CharSet.Unicode,
      SetLastError = true)]
    internal static extern bool RevealInFileExplorer(string path);
  }
}
