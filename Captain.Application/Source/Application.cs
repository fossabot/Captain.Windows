using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using Process = System.Diagnostics.Process;

namespace Captain.Application {
  /// <summary>
  ///   Defines basic logic behind the application
  /// </summary>
  internal static class Application {
    /// <summary>
    ///   Windows Application instance
    /// </summary>
    private static System.Windows.Application application;

    /// <summary>
    ///   Current logger file stream
    /// </summary>
    private static Stream loggerStream;

    /// <summary>
    ///   Whether or not toast notifications are supported on this platform
    /// </summary>
    private static bool areToastNotificationsSupported;

    /// <summary>
    ///   Single instance mutex name
    /// </summary>
    private static string SingleInstanceMutexName => $"{VersionInfo.ProductName} Single Instance Mutex ({Guid})";

    /// <summary>
    ///   Single instance mutex
    /// </summary>
    private static Mutex SingleInstanceMutex { get; set; }

    /// <summary>
    ///   Application's assembly GUID
    /// </summary>
    internal static string Guid => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as
      GuidAttribute)?.Value;

    /// <summary>
    ///   Application-wide logger
    /// </summary>
    internal static Logger Log { get; private set; }

    /// <summary>
    ///   Contains information about the application assembly version
    /// </summary>
    internal static FileVersionInfo VersionInfo { get; private set; }

    /// <summary>
    ///   Contains a semantic version string
    /// </summary>
    internal static string VersionString { get; private set; }

    /// <summary>
    ///   Handles local application filesystem
    /// </summary>
    internal static FsManager FsManager { get; private set; }

    /// <summary>
    ///   Handles add-ins
    /// </summary>
    internal static PluginManager PluginManager { get; private set; }

    /// <summary>
    ///   Handles tray icon
    /// </summary>
    internal static TrayIcon TrayIcon { get; private set; }

    /// <summary>
    ///   Notification provider
    /// </summary>
    internal static IToastProvider ToastProvider { get; set; }

    /// <summary>
    ///   Application <see cref="Options" /> instance
    /// </summary>
    internal static Options Options { get; private set; }

    /// <summary>
    ///   Whether or not toast notifications are supported on this platform
    /// </summary>
    internal static bool AreToastNotificationsSupported {
      get => areToastNotificationsSupported;
      set {
        areToastNotificationsSupported = value;
        OnToastProviderAvailabilityChanged?.Invoke(ToastProvider, value);
      }
    }

    /// <summary>
    ///   Application update manager
    /// </summary>
    internal static UpdateManager UpdateManager { get; private set; }

    /// <summary>
    ///   Indicates whether or not this is the first time the user opens the current version of the app
    /// </summary>
    internal static bool FirstTime { get; private set; }

    /// <summary>
    ///   Event handler delegate, triggered when toast notifications provider availability changes
    ///   (i.e. the user disables notifications for the application or these are disallowed by group policies, etc.)
    /// </summary>
    /// <param name="newProvider">New toast provider instance</param>
    /// <param name="toastsSupported">Whether Windows >= 8 adaptive toast notifications are supported</param>
    internal delegate void ToastProviderAvailabilityChangedHandler(IToastProvider newProvider, bool toastsSupported);

    /// <summary>
    ///   Triggered when the notification provider availability changes
    /// </summary>
    internal static event ToastProviderAvailabilityChangedHandler OnToastProviderAvailabilityChanged;

    /// <summary>
    ///   Terminates the program gracefully
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    /// <param name="exit">Whether to actually exit or just perform cleanup tasks</param>
    internal static void Exit(int exitCode = 0, bool exit = true) {
      try {
        Log.WriteLine(LogLevel.Warning, "exiting with code {0}", exitCode);

        TrayIcon?.Hide();
        Options?.Save();
        UpdateManager?.Dispose();

        GC.WaitForPendingFinalizers();
        loggerStream.Dispose();
        Log.Streams.Clear();
      } finally {
        if (exit) { application.Shutdown(exitCode); }
      }
    }

    /// <summary>
    ///   Restarts the application
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    internal static void Restart(int exitCode = 0) {
      Log.WriteLine(LogLevel.Warning, "restarting the application");

      try {
        Exit(exit: false);
        Process.Start(Assembly.GetExecutingAssembly().Location, $"--kill {Process.GetCurrentProcess().Id}");
      } finally {
        application.Shutdown(exitCode);
      }
    }

    /// <summary>
    ///   Resets the application options
    /// </summary>
    /// <param name="hard">Removes everything under the application directory</param>
    internal static void Reset(bool hard = false) {
      var nodes = new List<string> { Path.Combine(FsManager.GetSafePath(), Options.OptionsFileName) };

      if (hard) {
        Log.WriteLine(LogLevel.Warning, "performing hard reset!");
        nodes.AddRange(new[] { FsManager.LogsPath, FsManager.PluginPath, FsManager.TemporaryPath }
                         .Select(FsManager.GetSafePath));
      }

      Log.WriteLine(LogLevel.Verbose, $"deleting nodes: {String.Join(";", nodes)}");
      Exit(0, false);
      Process.Start(Assembly.GetExecutingAssembly().Location, $"--rmnodes \"{String.Join("\" \"", nodes)}\"");
    }

    /// <summary>
    ///   Program entry point
    /// </summary>
    /// <param name="args">Command-line arguments passed to the program</param>
    [STAThread, SuppressMessage("ReSharper", "PatternAlwaysMatches")]
    private static void Main(string[] args) {
      if (Array.IndexOf(args, "--rmnodes") is int i && (i != -1)) {
        for (int j = i + 1; j < args.Length; j++) {
          try {
            FileAttributes attributes = File.GetAttributes(args[j]);

            if ((attributes & FileAttributes.Directory) != 0) {
              try {
                Directory.Delete(args[j], true);
              } catch {
                Console.WriteLine(@"Failed to delete tree - {0}", args[j]);
              }
            } else {
              try {
                File.Delete(args[j]);
              } catch {
                Console.WriteLine(@"Failed to delete node - {0}", args[j]);
              }
            }
          } catch {
            Console.WriteLine(@"Failed to retrieve attributes for node - {0}", args[j]);
          }
        }

        Process.Start(Assembly.GetExecutingAssembly().Location);
        Environment.Exit(0);
      } else if (Array.IndexOf(args, "--kill") is int k &&
                 (k != -1) &&
                 ((1 + k) < args.Length) &&
                 UInt16.TryParse(args[1 + k], out ushort pid)) {
        try {
          Process.GetProcessById(pid).Kill();
        } catch {
          Console.WriteLine(@"Failed to kill process with ID {0}", pid);
        }
      }

      VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);
      VersionString = GetVersionString();

      Log = new Logger();
      Log.WriteLine(LogLevel.Informational, $"{VersionInfo.ProductName} {VersionString}");

      if (Mutex.TryOpenExisting(SingleInstanceMutexName, out Mutex _)) {
        Log.WriteLine(LogLevel.Warning, "another instance of the application is running - aborting");
        Environment.Exit(1);
      }

      // create a mutex that will prevent multiple instances of the application to be run
      SingleInstanceMutex = new Mutex(true, SingleInstanceMutexName);

      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

      FsManager = new FsManager();

      try {
        // create/open log file stream
        loggerStream = new FileStream(Path.Combine(FsManager.GetSafePath(FsManager.LogsPath),
                                                   DateTime.UtcNow.ToString("yy.MM.dd") + ".log"),
                                      FileMode.Append);
        Log.Streams.Add(loggerStream);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Warning, $"could not open logger stream: {exception}");
      }

      Options = Options.Load() ?? new Options();
      PluginManager = new PluginManager();
      UpdateManager = new UpdateManager();

      // has the application been updated or perhaps is it the first time the user opens it?
      if (Options.LastVersion != VersionString) {
        Log.WriteLine(LogLevel.Informational, "this is the first time the user opens the app - welcome!");
        Options.LastVersion = VersionString;
        Options.Save();
        FirstTime = true;
      }

      // XXX: create application shortcut
      //      Since Windows 10 1709 (Fall Update) an application **requires** an AppID to be registered within a
      //      start menu entry in order to use toast notifications
      int hr = Shell.InstallAppShortcut(VersionInfo.ProductName, Assembly.GetExecutingAssembly().Location, Guid);
      if (hr != 0) {
        Log.WriteLine(LogLevel.Debug, $"InstallAppShortcut() did not succeed (0x{hr:x8}) - toasts won't be available");
        AreToastNotificationsSupported = false;
        ToastProvider = new LegacyNotificationProvider();
      } else {
        // we may display toast notifications on supported platforms
        try {
          ToastProvider = new ToastNotificationProvider();
          AreToastNotificationsSupported = true;
          Log.WriteLine(LogLevel.Verbose, "toast notifications are supported");
        } catch {
          // it's likely the type ToastNotificationManager could not be found
          Log.WriteLine(LogLevel.Verbose, "toast notifications are not supported by this platform");
        } finally {
          if (!AreToastNotificationsSupported || Options.UseLegacyNotificationProvider) {
            ToastProvider = new LegacyNotificationProvider();
          }

          Log.WriteLine(LogLevel.Verbose, $"initialized {ToastProvider.GetType().Name}");
        }
      }

      TrayIcon = new TrayIcon();

      void MyLittleCallback(bool exclusive) {
        Action action = ActionManager.CreateDefault();

        if (exclusive) {
          action.Start(new CaptureIntent(ActionType.Screenshot));
        } else {
          action.BindGrabberUi(Grabber.Create(action.ActionTypes));
        }
      }

      TrayIcon.NotifyIcon.MouseClick += (_, mouseEventArgs) => {
        if (mouseEventArgs.Button == MouseButtons.Left) {
          MyLittleCallback(false);
        }
      };

      application = new System.Windows.Application { ShutdownMode = ShutdownMode.OnExplicitShutdown };
      application.Exit += (_, __) => {
        lock (SingleInstanceMutex) {
          SingleInstanceMutex.ReleaseMutex();
        }
      };

      application.Run();
    }

    /// <summary>
    ///   Gets a semantic version string for the application
    /// </summary>
    /// <returns>A string representing the application version</returns>
    private static string GetVersionString() {
      string version =
        $"{VersionInfo.ProductMajorPart}.{VersionInfo.ProductMinorPart}.{VersionInfo.ProductPrivatePart}";
      Assembly assembly = Assembly.GetExecutingAssembly();
      List<AssemblyMetadataAttribute> metadataAttributes = assembly
        .GetCustomAttributes(typeof(AssemblyMetadataAttribute))
        .Cast<AssemblyMetadataAttribute>()
        .ToList();

      if (!metadataAttributes.Any()) { return version; }
      if (metadataAttributes.Any(a => a.Key == "prerelease")) {
        AssemblyMetadataAttribute attribute = metadataAttributes.First(a => a.Key == "prerelease");
        version += '-' + attribute.Value;
        metadataAttributes.Remove(attribute);
      }

      return (version +
              '+' +
              String.Join(".",
                          metadataAttributes.Where(a => a.Value.Length == 0)
                                            .Select(a => a.Key + (a.Value.Length > 0 ? '.' + a.Value : ""))))
        .TrimEnd('.', '+');
    }

    /// <summary>
    ///   Gets a value from the assembly metadata attributes
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <returns>The requested value, or <c>null</c> if none was present.</returns>
    internal static string GetMetadataValue(string key) {
      Assembly assembly = Assembly.GetExecutingAssembly();
      AssemblyMetadataAttribute[] metadataAttributes = assembly
        .GetCustomAttributes(typeof(AssemblyMetadataAttribute))
        .Cast<AssemblyMetadataAttribute>()
        .Where(a => a.Key == key)
        .ToArray();
      return !metadataAttributes.Any() ? null : metadataAttributes.First().Value;
    }
  }
}