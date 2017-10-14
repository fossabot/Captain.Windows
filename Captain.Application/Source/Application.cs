using Captain.Common;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

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
    ///   Windows Application instance
    /// </summary>
    private static System.Windows.Application application;

    /// <summary>
    ///   Current logger file stream
    /// </summary>
    private static Stream loggerStream;

    /// <summary>
    ///   Single instance mutex
    /// </summary>
    private static Mutex SingleInstanceMutex { get; set; }

    /// <summary>
    ///   Application's assembly GUID
    /// </summary>
    private static string Guid => (Assembly.GetExecutingAssembly().GetCustomAttribute(typeof(GuidAttribute)) as
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
    internal static IToastProvider ToastProvider { get; private set; }

    /// <summary>
    ///   Application <see cref="Options"/> instance
    /// </summary>
    internal static Options Options { get; private set; }

    /// <summary>
    ///   Whether or not toast notifications are supported on this platform
    /// </summary>
    internal static bool AreToastNotificationsSupported { get; private set; }

    /// <summary>
    ///   Application update manager
    /// </summary>
    internal static UpdateManager UpdateManager { get; private set; }

    /// <summary>
    ///   Terminates the program gracefully
    /// </summary>
    /// <param name="exitCode">Optional exit code</param>
    internal static void Exit(int exitCode = 0) {
      try {
        Log.WriteLine(LogLevel.Warning, "exiting with code {0}", exitCode);

        TrayIcon?.Hide();
        Options?.Save();
        UpdateManager?.Dispose();

        GC.WaitForPendingFinalizers();
        loggerStream.Dispose();
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

      try {
        ToastProvider = new ToastNotificationProvider();
        AreToastNotificationsSupported = true;
        Log.WriteLine(LogLevel.Verbose, "toast notifications are supported");
      } catch {
        Log.WriteLine(LogLevel.Verbose, "toast notifications are not supported by this platform");
      } finally {
        if (!AreToastNotificationsSupported || Options.UseLegacyNotificationProvider) {
          ToastProvider = new LegacyNotificationProvider();
        }

        Log.WriteLine(LogLevel.Verbose, $"initialized {ToastProvider.GetType().Name}");
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
      var assembly = Assembly.GetExecutingAssembly();
      var metadataAttributes = assembly
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
                         metadataAttributes
                           .Where(a => a.Value.Length == 0)
                           .Select(a => a.Key + (a.Value.Length > 0 ? '.' + a.Value : ""))))
                   .TrimEnd('.', '+');
    }

    /// <summary>
    ///   Gets a value from the assembly metadata attributes
    /// </summary>
    /// <param name="key">Metadata key</param>
    /// <returns>The requested value, or <c>null</c> if none was present.</returns>
    internal static string GetMetadataValue(string key) {
      var assembly = Assembly.GetExecutingAssembly();
      AssemblyMetadataAttribute[] metadataAttributes = assembly
        .GetCustomAttributes(typeof(AssemblyMetadataAttribute))
        .Cast<AssemblyMetadataAttribute>()
        .Where(a => a.Key == key)
        .ToArray();

      if (!metadataAttributes.Any()) { return null; }
      return metadataAttributes.First().Value;
    }
  }
}
