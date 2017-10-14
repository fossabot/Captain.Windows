using System;
using System.IO;
using System.Reflection;
using System.Security;
using Captain.Common;
using Microsoft.Win32;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Handles the autostart feature by reading from/writing to the OS registry
  /// </summary>
  internal class AutoStartManager {
    /// <summary>
    ///   Contains the Startup registry key path
    /// </summary>
    private const string StartupRegistryKeyPath = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    /// <summary>
    ///   Contains the StartupApproved registry key path
    /// </summary>
    private const string ApprovedStartupRegistryKeyPath =
      @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\Run";

    /// <summary>
    ///   Current instance of the Startup registry key
    /// </summary>
    private RegistryKey startupRegistryKey;

    /// <summary>
    ///   Current instnace of the StartupApproved registry key
    /// </summary>
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
    ///   Opens the Startup registry key
    /// </summary>
    /// <returns>Whether or not the operation completed successfully</returns>
    private void OpenStartupRegistryKey() {
      try {
        Log.WriteLine(LogLevel.Verbose, "opening generic startup key");
        this.startupRegistryKey = Registry.CurrentUser.OpenSubKey(StartupRegistryKeyPath,
                                                                  RegistryKeyPermissionCheck.ReadWriteSubTree);

        Log.WriteLine(LogLevel.Verbose, "opening approved startup key");
        this.approvedStartupRegistryKey = Registry.CurrentUser.OpenSubKey(ApprovedStartupRegistryKeyPath,
                                                                          RegistryKeyPermissionCheck.ReadWriteSubTree);
      } catch (SecurityException) {
        Log.WriteLine(LogLevel.Warning, "access is denied to the registry key - some features may be unavailable");
      }
    }

    /// <summary>
    ///   Determines whether the application is set to run at login
    /// </summary>
    /// <returns>The current auto-start policy</returns>
    internal AutoStartPolicy GetAutoStartPolicy() {
      // make sure there's a valid path in the generic registry key and that matches exactly with the current
      // executable path
      try {
        if (!String.Equals(Path.GetFullPath(this.startupRegistryKey.GetValue(VersionInfo.ProductName, null).ToString()),
                           Path.GetFullPath(Assembly.GetExecutingAssembly().Location),
                           StringComparison.InvariantCultureIgnoreCase)) {
          // application executables do not match
          return AutoStartPolicy.Disapproved;
        }
      } catch (NullReferenceException) {
        // startup path is null (no auto-start entry found)
        return AutoStartPolicy.Disapproved;
      }

      try {
        if (this.approvedStartupRegistryKey != null &&
            this.approvedStartupRegistryKey.GetValue(VersionInfo.ProductName) is byte[] data) {
          // sanity check: make sure the application startup is approved!
          return (AutoStartPolicy)BitConverter.ToInt32(data, 0);
        }
      } catch (Exception exception) when (exception is SecurityException ||
                                          exception is IOException ||
                                          exception is UnauthorizedAccessException) {
        Log.WriteLine(LogLevel.Warning, $"could not open approved startup registry key - ${exception}");
      }
      // startup approval is likely to be unsupported on this platform
      return AutoStartPolicy.Approved;
    }

    /// <summary>
    ///   Creates or deletes the value on the Startup registry key
    /// </summary>
    /// <param name="policy">Explicitly set the policy</param>
    /// <param name="hard">If true, all entries will be recreated/deleted</param>
    /// <returns>The policy after completing the operation</returns>
    internal AutoStartPolicy ToggleAutoStart(AutoStartPolicy? policy = null, bool hard = false) {
      try {
        if (this.approvedStartupRegistryKey != null &&
            this.approvedStartupRegistryKey.GetValue(VersionInfo.ProductName) is byte[] data) {
          // has approved startup key - retrieve the policy to be set
          policy = policy ??
                   ((AutoStartPolicy)BitConverter.ToInt32(data, 0) == AutoStartPolicy.Approved // invert current policy
                      ? AutoStartPolicy.Disapproved
                      : AutoStartPolicy.Approved);

          // update auto-start policy
          if (hard && policy == AutoStartPolicy.Disapproved) {
            Log.WriteLine(LogLevel.Warning, "[hard mode] deleting approved startup value");
            this.approvedStartupRegistryKey.DeleteValue(VersionInfo.ProductName);
          } else if (!hard) {
            Log.WriteLine(LogLevel.Verbose, $"updating automatic startup policy: {policy}");
            this.approvedStartupRegistryKey.SetValue(VersionInfo.ProductName, BitConverter.GetBytes((int)policy));
            return policy.Value;
          }
        }

        if (!policy.HasValue) {
          // no value provided and no approved startup key whatsoever - get and invert the current policy
          policy = GetAutoStartPolicy() == AutoStartPolicy.Approved
                     ? AutoStartPolicy.Disapproved
                     : AutoStartPolicy.Approved;
        }

        // delete/set the registry entry
        if (policy == AutoStartPolicy.Approved) {
          Log.WriteLine(LogLevel.Verbose, "setting auto-start value in generic registry key");
          this.startupRegistryKey.SetValue(VersionInfo.ProductName,
                                           Assembly.GetExecutingAssembly().Location,
                                           RegistryValueKind.String);

          // create entry in the approved startup key if applicable
          this.approvedStartupRegistryKey?.SetValue(VersionInfo.ProductName,
                                                    BitConverter.GetBytes((int)AutoStartPolicy.Approved));
        } else {
          Log.WriteLine(LogLevel.Verbose, "deleting auto-start value in generic registry key");
          this.startupRegistryKey.DeleteValue(VersionInfo.ProductName, false);
        }

        return policy.Value;
      } catch (SecurityException) {
        Log.WriteLine(LogLevel.Warning, "access to the registry key is denied!");
        return policy.GetValueOrDefault(AutoStartPolicy.Approved) == AutoStartPolicy.Approved
                 ? AutoStartPolicy.Disapproved
                 : AutoStartPolicy.Approved;
      }
    }
  }

}