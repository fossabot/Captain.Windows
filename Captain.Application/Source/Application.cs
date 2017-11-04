using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Captain.Application.Native;
using Captain.Common;
using SharpDX;
using SharpDX.Diagnostics;
using Process = System.Diagnostics.Process;

namespace Captain.Application {
  /// <summary>
  ///   Defines basic logic behind the application
  /// </summary>
  /// <remarks>
  ///   TODO: Refactor all this. Reduce app startup impact in performance and initialize things gradually as they are
  ///   needed - do not forget this is a tray icon, not a full-blown foreground application!
  /// </remarks>
  internal static class Application {
    /// <summary>
    ///   Event handler delegate, triggered when toast notifications provider availability changes
    ///   (i.e. the user disables notifications for the application or these are disallowed by group policies, etc.)
    /// </summary>
    /// <remarks>
    ///   TODO: Move this to somewhere else more appropriate - or delete it altogether if not needed
    /// </remarks>
    /// <param name="newProvider">New toast provider instance</param>
    /// <param name="toastsSupported">Whether Windows >= 8 adaptive toast notifications are supported</param>
    internal delegate void ToastProviderAvailabilityChangedHandler(IToastProvider newProvider, bool toastsSupported);

    /// <summary>
    ///   Current logger file stream
    /// </summary>
    private static Stream loggerStream;

    /// <summary>
    ///   Whether or not toast notifications are supported on this platform
    /// </summary>
    private static bool areToastNotificationsSupported;

    /// <summary>
    ///   HUD instance
    /// </summary>
    private static Hud hud;

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
    ///   HUD instance
    /// </summary>
    internal static Hud Hud {
      get {
        if (hud == null || hud.IsDisposed) {
          hud = new Hud();
        }

        return hud;
      }
    }

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

#if DEBUG
        Log.WriteLine(LogLevel.Warning, ObjectTracker.ReportActiveObjects());
#endif

        loggerStream.Dispose();
        Log.Streams.Clear();

        GC.WaitForPendingFinalizers();
      } catch (Exception exception) {
        // try to log exceptions
        if ((Log.Streams?.Count > 0) && Log.Streams.All(s => s.CanWrite)) {
          Log.WriteLine(LogLevel.Error, $"exception caught: {exception}");
        }
      } finally {
        if (exit) {
          Environment.ExitCode = exitCode;
          System.Windows.Forms.Application.Exit();
        }
      }
    }

    /// <summary>
    ///   Restarts the application
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    internal static void Restart(int exitCode = 0) {
      Log.WriteLine(LogLevel.Warning, "restarting the application");

      try {
        Exit(exitCode, false);
        Process.Start(Assembly.GetExecutingAssembly().Location, $"--kill {Process.GetCurrentProcess().Id}");
      } finally {
        Environment.ExitCode = exitCode;
        System.Windows.Forms.Application.Exit();
      }
    }

    /// <summary>
    ///   Resets the application options
    /// </summary>
    /// <param name="hard">Removes everything under the application directory</param>
    internal static void Reset(bool hard = false) {
      var nodes = new List<string> {Path.Combine(FsManager.GetSafePath(), Options.OptionsFileName)};

      if (hard) {
        Log.WriteLine(LogLevel.Warning, "performing hard reset!");
        nodes.AddRange(new[] {FsManager.LogsPath, FsManager.PluginPath, FsManager.TemporaryPath}
          .Select(FsManager.GetSafePath));
      }

      Log.WriteLine(LogLevel.Verbose, $"deleting nodes: {String.Join("; ", nodes)}");
      Exit(0, false);
      Process.Start(Assembly.GetExecutingAssembly().Location,
        $"--kill {Process.GetCurrentProcess().Id} --rmnodes \"{String.Join("\" \"", nodes)}\"");
      Environment.Exit(0);
    }

    /// <summary>
    ///   Program entry point
    /// </summary>
    /// <param name="args">Command-line arguments passed to the program</param>
    [STAThread]
    private static void Main(string[] args) {
#if DEBUG
      // black magic
      Configuration.EnableObjectTracking = true;
#endif

      // ReSharper disable PatternAlwaysMatches
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
      }

      if (Array.IndexOf(args, "--kill") is int k &&
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

      Log = new Logger();
      Log.WriteLine(LogLevel.Informational, $"{VersionInfo.ProductName} {VersionInfo.ProductVersion}");

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
      if (Options.LastVersion != VersionInfo.ProductVersion) {
        Log.WriteLine(LogLevel.Informational, "this is the first time the user opens the app - welcome!");
        Options.LastVersion = VersionInfo.ProductVersion;

        // create default tasks
        Log.WriteLine(LogLevel.Informational, "creating default tasks");
        Options.Tasks.Add(Task.CreateDefaultScreenshotTask());
        Options.Tasks.Add(Task.CreateDefaultRecordingTask());

        Options.Save();
        FirstTime = true;
      }

      // XXX: create application shortcut
      //      Since Windows 10 1709 (Fall Update) an application **requires** an AppID to be registered within a
      //      start menu entry in order to use toast notifications
      // TODO: reimplement this in managed code
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

      System.Windows.Forms.Application.ApplicationExit += (s, e) => {
        lock (SingleInstanceMutex) {
          SingleInstanceMutex.ReleaseMutex();
        }
      };

      System.Windows.Forms.Application.Run();
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