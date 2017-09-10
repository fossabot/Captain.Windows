using System;
using System.IO;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Manages directories and files and ensures file system access is allowed
  /// </summary>
  internal class FsManager {
    /// <summary>
    ///   Add-in path
    /// </summary>
    internal const string PluginPath = "Plugins";

    /// <summary>
    ///   Temporary path
    /// </summary>
    internal const string TemporaryPath = "Temporary";

    /// <summary>
    ///   Root application data directory
    /// </summary>
    private string RootDirectoryPath { get; }

    /// <summary>
    ///   Instantiates a filesystem manager
    /// </summary>
    internal FsManager() {
      RootDirectoryPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), VersionInfo.ProductName);

      try {
        Directory.CreateDirectory(RootDirectoryPath);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not bootstrap local application directory - using temporary path: {exception}");
        RootDirectoryPath = Path.GetTempPath();
      }
    }

    /// <summary>
    ///   Returns a directory path that is almost-certainly guaranteed to be writable
    /// </summary>
    /// <remarks>If the user doesn't have write permissions to the temporary directory, this may fail</remarks>
    /// <param name="name">Relative path name</param>
    /// <returns>The full path to the directory</returns>
    internal string GetSafePath(string name = "") {
      try {
        return Directory.CreateDirectory(Path.Combine(RootDirectoryPath, name)).FullName;
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"requested path could not be created: {exception}");
        return Path.GetTempPath();
      }
    }

    /// <summary>
    ///   Cleanups filesystem
    /// </summary>
    ~FsManager() {
      try {
        Directory.Delete(Path.Combine(RootDirectoryPath, TemporaryPath), true);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not clean up filesystem: {exception}");
      }
    }
  }
}
