using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using SharpDX;
using SharpDX.Diagnostics;

namespace Captain.Application {
  /// <summary>
  ///   Defines application entry logic
  /// </summary>
  internal static class Application {
    #region Private fields and constants

    /// <summary>
    ///   Current logger file stream
    /// </summary>
    private static Stream loggerStream;

    /// <summary>
    ///   HUD instance
    /// </summary>
    private static Hud hud;

    /// <summary>
    ///   Single instance mutex name
    /// </summary>
    private static string SingleInstanceMutexName
      => $"{System.Windows.Forms.Application.ProductName} Single Instance Mutex ({Guid})";

    #endregion

    #region App execution control methods

    /// <summary>
    ///   Terminates the program gracefully
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    /// <param name="exit">Whether to actually exit or just perform cleanup tasks</param>
    internal static void Exit(int exitCode = 0, bool exit = true) {
      try {
        Log.WriteLine(LogLevel.Warning, $"exiting with code 0x{exitCode:x8}");

        DesktopKeyboardHook?.Dispose();
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
        if (Log.Streams?.Count > 0 && Log.Streams.All(s => s.CanWrite)) {
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
      Log.WriteLine(LogLevel.Warning, $"restarting with code 0x{exitCode:x8}");

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

    #endregion

    #region App globals

    /// <summary>
    ///   Single instance mutex
    /// </summary>
    private static Mutex SingleInstanceMutex { get; set; }

    /// <summary>
    ///   Whether or not toast notifications are supported on this platform
    /// </summary>
    internal static bool AreToastNotificationsSupported { get; set; }

    /// <summary>
    ///   Assembly GUID
    /// </summary>
    internal static string Guid =>
      Assembly.GetExecutingAssembly().GetCustomAttribute<GuidAttribute>().Value;

    /// <summary>
    ///   Assembly product version
    /// </summary>
    internal static Version Version => Assembly.GetExecutingAssembly().GetName().Version;

    #endregion

    #region App components

    /// <summary>
    ///   Application-wide logger
    /// </summary>
    internal static Logger Log { get; private set; }

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
    internal static INotificationProvider NotificationProvider { get; set; }

    /// <summary>
    ///   Application <see cref="Options" /> instance
    /// </summary>
    internal static Options Options { get; private set; }

    /// <summary>
    ///   HUD instance
    /// </summary>
    internal static Hud Hud {
      get {
        if (hud == null || hud.IsDisposed) { hud = new Hud(); }

        return hud;
      }
    }

    /// <summary>
    ///   Application update manager
    /// </summary>
    internal static UpdateManager UpdateManager { get; private set; }

    /// <summary>
    ///   Keyboard hook provider for system UI.
    /// </summary>
    internal static IKeyboardHookProvider DesktopKeyboardHook { get; private set; }

    #endregion

    /// <summary>
    ///   Handles command line arguments
    /// </summary>
    /// <param name="args">CLI arguments</param>
    private static void HandleCommandLineArgs(string[] args) {
      // ReSharper disable PatternAlwaysMatches
      if (Array.IndexOf(args, "--rmnodes") is int i && i != -1) {
        for (int j = i + 1; j < args.Length; j++) {
          try {
            FileAttributes attributes = File.GetAttributes(args[j]);

            if ((attributes & FileAttributes.Directory) != 0) {
              try { Directory.Delete(args[j], true); } catch {
                Console.WriteLine(@"failed to delete directory - {0}", args[j]);
              }
            } else {
              try { File.Delete(args[j]); } catch { Console.WriteLine(@"failed to delete file - {0}", args[j]); }
            }
          } catch { Console.WriteLine(@"failed to retrieve file attributes - {0}", args[j]); }
        }
      }

      // ReSharper disable once PatternAlwaysMatches
      if (Array.IndexOf(args, "--kill") is int k &&
          k != -1 &&
          1 + k < args.Length &&
          UInt16.TryParse(args[1 + k], out ushort pid)) {
        try { Process.GetProcessById(pid).Kill(); } catch {
          Console.WriteLine(@"failed to kill process with ID {0} (already dead?)", pid);
        }
      }
    }

    /// <summary>
    ///   Performs initial setup tasks, such as displaying the Welcome dialog if pertinent or install application
    ///   shortcuts
    /// </summary>
    private static void InitialSetup() {
      // has the application been updated or perhaps is it the first time the user opens it?
      if (Options.LastVersion != Version.ToString() || Debugger.IsAttached /* TODO: remove me */) {
        Log.WriteLine(LogLevel.Informational, "this is the first time the user opens the app - welcome!");
        Options.LastVersion = Version.ToString();

        Log.WriteLine(LogLevel.Informational, "displaying first-time tour");

        // TODO: create default tasks or show welcome dialog with a wizard for doing so
      }

      try {
        // create application shortcut
        // since Windows 10 1709 (Fall Update) an application **requires** an AppID to be registered within a start menu
        // entry in order to use toast notifications
        ShellHelper.InstallAppShortcut();

        // we may now display toast notifications on supported platforms
        try {
          NotificationProvider = new ToastNotificationProvider();
          AreToastNotificationsSupported = true;
          Log.WriteLine(LogLevel.Verbose, "toast notifications are supported");
        } catch (Exception exception) {
          // it's likely the type ToastNotificationManager could not be found
          Log.WriteLine(LogLevel.Verbose, $"toast notifications are not supported by this platform: {exception}");
        } finally {
          if (!AreToastNotificationsSupported || Options.UseLegacyNotificationProvider) {
            NotificationProvider = new LegacyNotificationProvider();
          }

          Log.WriteLine(LogLevel.Verbose, $"initialized {NotificationProvider.GetType().Name}");
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Warning, $"could not install app shortcut: {exception}");
        AreToastNotificationsSupported = false;
        NotificationProvider = new LegacyNotificationProvider();
      }
    }

    /// <summary>
    ///   Program entry point
    /// </summary>
    /// <param name="args">Command-line arguments passed to the program</param>
    [STAThread]
    private static void Main(string[] args) {
#if DEBUG
      // black magic - tracks unmanaged D2D/D3D objects and prints out unreleased resources at exit
      Configuration.EnableObjectTracking = true;
#endif

      HandleCommandLineArgs(args);

      Log = new Logger();
      Log.WriteLine(LogLevel.Informational,
        $"{System.Windows.Forms.Application.ProductName} {Version}");

      // is another instance of the application currently running?
      if (Mutex.TryOpenExisting(SingleInstanceMutexName, out Mutex _)) {
        Log.WriteLine(LogLevel.Warning, "another instance of the application is running - aborting");
        Environment.Exit(1);
      }

      // create a mutex that will prevent multiple instances of the application to be run
      SingleInstanceMutex = new Mutex(true, SingleInstanceMutexName);

      // Windows Forms setup
      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
      System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException, true);
      System.Windows.Forms.Application.ThreadException += (s, e) => {
        Log.WriteLine(LogLevel.Error, $"BUG: unhandled exception: ${e.Exception}");
#if DEBUG
        Debugger.Break();
#endif
        Restart(e.Exception.HResult);
      };

      // initialize file system manager and log file stream
      FsManager = new FsManager();
      try {
        // create/open log file stream
        loggerStream = new FileStream(Path.Combine(FsManager.GetSafePath(FsManager.LogsPath),
            DateTime.UtcNow.ToString("yy.MM.dd") + ".log"),
          FileMode.Append);
        Log.Streams.Add(loggerStream);
      } catch (Exception exception) { Log.WriteLine(LogLevel.Warning, $"could not open logger stream: {exception}"); }

      // initialize main components
      Options = Options.Load() ?? new Options();
      PluginManager = new PluginManager();
      InitialSetup();
      TrayIcon = new TrayIcon();
      UpdateManager = new UpdateManager();

      DesktopKeyboardHook = new SystemKeyboardHookProvider();
      DesktopKeyboardHook.OnKeyUp += (s, e) => {
        // get the full key combination
        Keys keys = (Keys) e.KeyValue | (e.KeyData & Keys.Modifiers);

        if (keys == Keys.None ||
            !((keys & Keys.Shift) != 0 ||
              (keys & Keys.Alt) != 0 ||
              (keys & Keys.Control) != 0 ||
              (keys & Keys.LWin) != Keys.LWin ||
              (keys & Keys.RWin) != Keys.RWin)) {
          // don't process the key
          return;
        }

        // retrieve the tasks matching the hotkey
        IEnumerable<Task> tasks = Options.Tasks.Where(t => t.Hotkey == keys);

        if (tasks.Any()) {
          Log.WriteLine(LogLevel.Verbose, $"launching tasks matching hotkey: {keys}");
          TaskHelper.StartTask(tasks.First());
        }
      };

      DesktopKeyboardHook.Acquire();

      // release the mutex when the application is terminated
      System.Windows.Forms.Application.ApplicationExit += (s, e) => {
        lock (SingleInstanceMutex) { SingleInstanceMutex.ReleaseMutex(); }
      };

      // start application event loop
      System.Windows.Forms.Application.Run();
    }
  }
}