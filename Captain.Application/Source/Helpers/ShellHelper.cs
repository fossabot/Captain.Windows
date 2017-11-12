using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;
using Task = System.Threading.Tasks.Task;

namespace Captain.Application {
  internal static class ShellHelper {
    /// <summary>
    ///   Reveals a file in a File Explorer window.
    /// </summary>
    /// <param name="path">Path of the file to be revealed.</param>
    internal static async void RevealInExplorerAsync(string path) {
      var thread = new Thread(() => {
        string folderPath = Path.GetDirectoryName(path);
        int hr;

        // parse containing folder and file path
        if ((hr = Shell32.SHParseDisplayName(folderPath, IntPtr.Zero, out IntPtr nativeFolder, 0, out _)) != 0 ||
            nativeFolder == default ||
            (hr = Shell32.SHParseDisplayName(path, IntPtr.Zero, out IntPtr nativeFile, 0, out _)) != 0 ||
            nativeFile == default) {
          // shell invoke failed or a null pointer was yielded for native shell items
          Log.WriteLine(LogLevel.Warning, $"SHParseDisplayName() failed: HRESULT 0x{hr:x8}");

          // release used resources
          if (nativeFolder != default) { Marshal.FreeCoTaskMem(nativeFolder); }
          return;
        }

        // open the file explorer window
        if ((hr = Shell32.SHOpenFolderAndSelectItems(nativeFolder, 1, new[] {nativeFile}, 0)) != 0) {
          Log.WriteLine(LogLevel.Warning, $"SHOpenFolderAndSelectItems() failed: HRESULT 0x{hr:x8}");
        }

        // release resources
        Marshal.FreeCoTaskMem(nativeFolder);
        Marshal.FreeCoTaskMem(nativeFile);
      });

      thread.TrySetApartmentState(ApartmentState.MTA);
      thread.Start();

      await new Task(() => thread.Join());
    }
    /// <summary>
    ///   Creates an app shortcut in the user's start menu directory
    /// </summary>
    internal static void InstallAppShortcut() {
      string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
        System.Windows.Forms.Application.ProductName + ".lnk");
      string appPath = Assembly.GetExecutingAssembly().Location;

      using (var shellLink = new ShellLink()) {
        if (File.Exists(shortcutPath)) {
          // shortcut already created - make sure it points to the same application version
          shellLink.Load(shortcutPath);

          if (shellLink.TargetPath != appPath || shellLink.AppUserModelID != Application.Guid) {
            try {
              Log.WriteLine(LogLevel.Warning, "shortcut data mismatch - correcting");
              shellLink.TargetPath = appPath;
              shellLink.AppUserModelID = Application.Guid;
              shellLink.Save();
            } catch (COMException) {
              Log.WriteLine(LogLevel.Warning, "shortcut correction failed - deleting shortcut");
              File.Delete(shortcutPath);
              InstallAppShortcut();
            }
          }
        } else {
          // create new shortcut
          shellLink.TargetPath = appPath;
          shellLink.AppUserModelID = Application.Guid;
          shellLink.Arguments = "/task:default";
          shellLink.Save(shortcutPath);
          Log.WriteLine(LogLevel.Informational, "application shortcut created successfully");
        }
      }
    }
  }
}