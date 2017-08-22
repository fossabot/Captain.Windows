using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using Captain.Application.NativeHelpers;
using Captain.Common;
using Microsoft.Toolkit.Uwp.Notifications;

namespace Captain.Application {
  /// <summary>
  ///   Defines basic logic behind the application
  /// </summary>
  internal static class Application {
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
    internal static HotkeyManager HotkeyManager { get; private set; }

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
    ///   Terminates the program gracefully
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    internal static void Exit(int exitCode = 0) {
      Log.WriteLine(LogLevel.Warning, "exiting with code {0}", exitCode);
      TrayIcon?.Hide();
      Options?.Save();
      Environment.Exit(exitCode);
    }

    /// <summary>
    ///   Program entry point
    /// </summary>
    [STAThread]
    private static void Main() {
      Log = new Logger();
      Log.SetDefault();

      // try to retrieve version information
      if (Assembly.GetExecutingAssembly().Location is string fileName) {
        VersionInfo = FileVersionInfo.GetVersionInfo(fileName);
      } else {
        Log.WriteLine(LogLevel.Error, "can't retrieve version info - has the assembly been loaded from a file?");
        Environment.Exit(1);
      }

      Log.WriteLine(LogLevel.Informational, $"{VersionInfo.ProductName} {VersionInfo.ProductVersion}");

      FsManager = new FsManager();
      Options = Options.Load() ?? new Options();
      PluginManager = new PluginManager();
      TrayIcon = new TrayIcon();
      TrayIcon.Show();

      try {
        ToastProvider = new ToastNotificationProvider();

        var content = new ToastContent {
          Duration = ToastDuration.Long,
          Audio = new ToastAudio { Silent = true },
          Actions = new ToastActionsCustom(),
          Visual = new ToastVisual {
            BindingGeneric = new ToastBindingGeneric {
              Children = {
                new AdaptiveProgressBar {
                  Status = "Uploading to Imgur",
                  Value = AdaptiveProgressBarValue.Indeterminate,
                  Title = "Title"
                }
              }
            }
          }
        };


        var doc = new XmlDocument();
        doc.LoadXml(content.GetContent());

        var notification = new ToastNotification(doc);
        ToastNotificationManager.CreateToastNotifier(VersionInfo.ProductName).Show(notification);
      } catch {
        ToastProvider = new LegacyNotificationProvider();
      }

      HotkeyManager = new HotkeyManager();
      HotkeyManager.Register((int)(Keys.LControlKey | Keys.RMenu | Keys.Return),
                             (exclusive, monitor) => {
                               Action action = ActionManager.CreateDefault();

                               if (exclusive) {
                                 action.Start(new CaptureIntent(ActionType.Screenshot) {
                                   Monitor = monitor
                                 });
                               } else {
                                 var grabber = new Grabber(action.ActionTypes);
                                 grabber.OnIntentReceived += (_, intent) => action.Start(intent);
                                 grabber.Show();
                               }

                               return true;
                             });

      new System.Windows.Application {
        ShutdownMode = ShutdownMode.OnExplicitShutdown
      }.Run();

      Exit();
    }
  }
}
