using Captain.Common;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Captain.Application {
  /// <summary>
  ///   Defines basic logic behind the application
  /// </summary>
  internal static class Application {
    /// <summary>
    ///   Single instance mutex name
    /// </summary>
    private static string SingleInstanceMutexName => $"{VersionInfo.ProductName} Single Instance Mutex ({Guid})";

    /// <summary>
    ///   Single instance mutex
    /// </summary>
    private static Mutex singleInstanceMutex;

    /// <summary>
    ///   Windows Application instance
    /// </summary>
    private static System.Windows.Application application;

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
    ///   Application <see cref="Options"/> instance
    /// </summary>
    private static Options Options { get; set; }

    /// <summary>
    ///   Notification provider
    /// </summary>
    internal static IToastProvider ToastProvider { get; private set; }

    /// <summary>
    ///   Application's assembly GUID
    /// </summary>
    private static string Guid => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as
                                     GuidAttribute)?.Value;

    /// <summary>
    ///   Terminates the program gracefully
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    internal static void Exit(int exitCode = 0) {
      try {
        Log.WriteLine(LogLevel.Warning, "exiting with code {0}", exitCode);

        TrayIcon?.Hide();
        Options?.Save();
      } finally {
        application.Shutdown(exitCode);
      }
    }

    /// <summary>
    ///   Program entry point
    /// </summary>
    [STAThread]
    private static void Main() {
      VersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

      Log = new Logger();
      Log.SetDefault();
      Log.WriteLine(LogLevel.Informational, $"{VersionInfo.ProductName} {VersionInfo.ProductVersion}");

      if (Mutex.TryOpenExisting(SingleInstanceMutexName, out Mutex _)) {
        Log.WriteLine(LogLevel.Warning, "another instance of the application is running - aborting");
        Environment.Exit(1);
      }

      // create mutex to prevent multiple instances of the application to be run
      // ReSharper disable once InconsistentlySynchronizedField
      singleInstanceMutex = new Mutex(true, SingleInstanceMutexName);

      System.Windows.Forms.Application.EnableVisualStyles();
      System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

      FsManager = new FsManager();
      Options = Options.Load() ?? new Options();
      PluginManager = new PluginManager();
      TrayIcon = new TrayIcon();

      try {
        ToastProvider = new ToastNotificationProvider();
      } catch {
        ToastProvider = new LegacyNotificationProvider();
      }

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
        lock (singleInstanceMutex) {
          singleInstanceMutex.ReleaseMutex();
        }
      };

      application.Run();
    }
  }
}
