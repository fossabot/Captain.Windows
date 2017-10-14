using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Exported functions from the shell32.dll Windows library.
  /// </summary>
  internal static class Shell32 {
    [DllImport(nameof(Shell32), EntryPoint = "Shell_NotifyIconGetRect", SetLastError = true)]
    internal static extern int NotifyIconGetRect([In] ref NOTIFYICONIDENTIFIER identifier,
                                                 [Out] out RECT iconLocation);
  }
}
