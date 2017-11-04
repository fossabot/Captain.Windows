using System;
using System.IO;
using System.Reflection;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  internal static class ShellHelper {
    /// <summary>
    ///   Creates an app shortcut in the user's start menu directory
    /// </summary>
    internal static void InstallAppShortcut() {
      string shortcutPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs),
        VersionInfo.ProductName + ".lnk");
      string appPath = Assembly.GetExecutingAssembly().Location;

      using (var shellLink = new ShellLink()) {
        if (File.Exists(shortcutPath)) {
          // shortcut already created - make sure it points to the same application version
          shellLink.Load(shortcutPath);

          if (shellLink.TargetPath != appPath) {
            Log.WriteLine(LogLevel.Warning, "application shortcut not pointing to the same path - correcting");
            shellLink.TargetPath = appPath;
            shellLink.Save();
          }
        } else {
          // create new shortcut
          shellLink.TargetPath = appPath;
          shellLink.AppUserModelID = $"{VersionInfo.ProductName}.{VersionInfo.ProductVersion}";
          shellLink.Arguments = "/task:default";
          shellLink.Save(shortcutPath);
          Log.WriteLine(LogLevel.Informational, "application shortcut created successfully");
        }
      }
    }
  }
}