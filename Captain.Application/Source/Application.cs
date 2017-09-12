using Captain.Common;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;

namespace Captain.Application {
  /// <summary>
  ///   Defines basic logic behind the application
  /// </summary>
  internal static class Application {
    /// <summary>
    ///   Single instance mutex name
    /// </summary>
    private static string SiMutexName => $"{VersionInfo.ProductName} Single Instance Mutex ({Guid})";

    /// <summary>
    ///   Single instance mutex
    /// </summary>
    private static Mutex SiMutex { get; set; }

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
    ///   Handles hotkeys
    /// </summary>
    //internal static HotkeyManager HotkeyManager { get; private set; }

    /// <summary>
    ///   Handles tray icon
    /// </summary>
    internal static TrayIcon TrayIcon { get; private set; }

    /// <summary>
    ///   Application <see cref="Options"/> instance
    /// </summary>
    internal static Options Options { get; private set; }

    /// <summary>
    ///   Notification provider
    /// </summary>
    internal static IToastProvider ToastProvider { get; private set; }

    /// <summary>
    ///   Application's assembly GUID
    /// </summary>
    internal static string Guid => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as
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
        SiMutex?.ReleaseMutex();
        Environment.Exit(exitCode);
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

      if (Mutex.TryOpenExisting(SiMutexName, out Mutex _)) {
        Log.WriteLine(LogLevel.Warning, "another instance of the application is running - aborting");
        Environment.Exit(1);
      }

      // create mutex to prevent multiple instances of the application to be ran
      SiMutex = new Mutex(true, SiMutexName);

      FsManager = new FsManager();
      Options = Options.Load() ?? new Options();
      PluginManager = new PluginManager();
      TrayIcon = new TrayIcon();
      TrayIcon.Show();

      try {
        ToastProvider = new ToastNotificationProvider();
#if false
        var content = new ToastContent {
          Duration = ToastDuration.Long,
          Audio = new ToastAudio { Silent = true },
          Actions = new ToastActionsCustom(),
          Visual = new ToastVisual {
            BindingGeneric = new ToastBindingGeneric {
              Children = {
                new AdaptiveText { Text = VersionInfo.ProductName },
                new AdaptiveText { Text = "Your capture is being processed.", HintStyle = AdaptiveTextStyle.Body },
                new AdaptiveProgressBar {
                  Status = "Uploading to Imgur",
                  Value = 0.5
                }
              }
            }
          }
        };

        var doc = new XmlDocument();
        doc.LoadXml(content.GetContent());

        var notification = new ToastNotification(doc);
        ToastNotificationManager.CreateToastNotifier(VersionInfo.ProductName).Show(notification);
#endif
      } catch {
        ToastProvider = new LegacyNotificationProvider();
      }

      void MyLittleCallback(bool exclusive, int monitor) {
        Action action = ActionManager.CreateDefault();

        if (exclusive) {
          action.Start(new CaptureIntent(ActionType.Screenshot) {
            Monitor = monitor
          });
        } else {
          action.BindGrabberUI(new Grabber(action.ActionTypes));
        }
      }

      /*HotkeyManager = new HotkeyManager();
      HotkeyManager.Register((int)(Keys.LControlKey | Keys.RMenu | Keys.Return), MyLittleCallback);*/

      TrayIcon.NotifyIcon.Click += (_, __) => MyLittleCallback(false, -1);

      new System.Windows.Application {
        ShutdownMode = ShutdownMode.OnExplicitShutdown
      }.Run();

      Exit();
    }
  }
}
