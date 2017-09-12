using System;
using System.Reflection;
using Captain.Common;
using Microsoft.Win32;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Handles the autostart feature by reading from/writing to the OS registry
  /// </summary>
  internal class AutoStartManager {
    private const string StartupRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string ApprovedStartupRegistryKeyPath =
      @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    private RegistryKey startupRegistryKey;
    private RegistryKey approvedStartupRegistryKey;

    /// <summary>
    ///   Whether or not this feature is available
    /// </summary>
    internal bool IsFeatureAvailable => this.startupRegistryKey != null;

    /// <summary>
    ///   Constructs an instance of this class
    /// </summary>
    internal AutoStartManager() => OpenStartupRegistryKey();

    /// <summary>
    ///   Returns whether or not the app has a value on the Startup registry key
    /// </summary>
    /// <returns>Whether or not the operation completed successfully</returns>
    internal bool IsAutoStartEnabled() {
      if (this.startupRegistryKey.GetValue(VersionInfo.ProductName, null).ToString()
              .ToLowerInvariant() != Assembly.GetExecutingAssembly().Location?.ToLowerInvariant()) {
        // the application executable path does not match
        return false;
      }

      if (this.approvedStartupRegistryKey != null &&
          this.approvedStartupRegistryKey.GetValue(VersionInfo.ProductName) is byte[] data) {
        // read the first 4 bytes and convert to integer - make sure the application startup is approved
        return BitConverter.ToInt32(data, 0) == (int)AutoStartStatus.Approved;
      }

      return false;
    }

    /// <summary>
    ///   Creates or deletes the value on the Startup registry key
    /// </summary>
    /// <returns>Whether or not the operation completed successfully</returns>
    internal bool ToggleAutoStart() {


      // the application is on the approved startup key - we'll enable/disable the entry instead of deleting and
      // creating it every time
      if (this.approvedStartupRegistryKey != null &&
          this.approvedStartupRegistryKey.GetValue(VersionInfo.ProductName) is byte[] data) {

        // get enum value bytes and copy them to the binary data, overwriting existing ones
        BitConverter.GetBytes(BitConverter.ToInt32(data, 0) == (int)AutoStartStatus.Approved
                                ? (int)AutoStartStatus.Disapproved
                                : (int)AutoStartStatus.Approved).CopyTo(data, 0);

        // write new value
        this.approvedStartupRegistryKey.SetValue(VersionInfo.ProductName, data);
      } else {
        bool onStartupKey = IsAutoStartEnabled();

        // just stick with deleting and setting the value each time
        if (onStartupKey) {
          this.startupRegistryKey.DeleteValue(VersionInfo.ProductName);
        } else {
          // ReSharper disable once AssignNullToNotNullAttribute
          this.startupRegistryKey.SetValue(VersionInfo.ProductName, Assembly.GetExecutingAssembly().Location,
                                           RegistryValueKind.String);
        }

        return !onStartupKey;
      }

      return false;
    }

    /// <summary>
    ///   Opens the Startup registry key
    /// </summary>
    /// <returns>Whether or not the operation completed successfully</returns>
    private bool OpenStartupRegistryKey() {
      try {
        this.startupRegistryKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKeyPath,
                                                                  RegistryKeyPermissionCheck.ReadWriteSubTree);
        this.approvedStartupRegistryKey = Registry.CurrentUser.OpenSubKey(ApprovedStartupRegistryKeyPath,
                                                                          RegistryKeyPermissionCheck.ReadWriteSubTree);
        Log.WriteLine(LogLevel.Informational, "operation completed successfully");
        return true;
      } catch {
        Log.WriteLine(LogLevel.Warning, "could not open startup registry key - some related features may be unavailable");
        return false;
      }
    }
  }

}