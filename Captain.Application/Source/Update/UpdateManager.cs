using System;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows.Threading;
using Captain.Common;
using Squirrel;
using static Captain.Application.Application;
using static Squirrel.UpdateManager;

namespace Captain.Application {
  /// <summary>
  ///   Abstracts <see cref="Squirrel.UpdateManager"/> logic
  /// </summary>
  internal class UpdateManager : IDisposable {
    /// <summary>
    ///   Underlying update manager instance
    /// </summary>
    private Squirrel.UpdateManager Manager { get; set; }

    /// <summary>
    ///   Determines whether or not the update manager is available
    /// </summary>
    internal UpdaterAvailability Availability { get; private set; }

    /// <summary>
    ///   Current update status
    /// </summary>
    internal UpdateStatus Status { get; private set; } = UpdateStatus.Idle;

    /// <summary>
    ///   Triggered when the update manager availability changes
    /// </summary>
    /// <param name="manager">Update manager instance</param>
    /// <param name="availability">Updater status</param>
    internal delegate void AvailabilityChangedHandler(UpdateManager manager, UpdaterAvailability availability);

    /// <summary>
    ///   Triggered when the update status changes
    /// </summary>
    /// <param name="manager">Update manager instance</param>
    /// <param name="status">Status</param>
    internal delegate void UpdateStatusChangedHandler(UpdateManager manager, UpdateStatus status);

    /// <summary>
    ///   Triggered when the progress of the underlying update operation changes
    /// </summary>
    /// <param name="manager">Update manager instance</param>
    /// <param name="status">Status</param>
    /// <param name="progress">Operation progress</param>
    internal delegate void UpdateProgressChangedHandler(UpdateManager manager, UpdateStatus status, int progress);

    /// <summary>
    ///   Triggered when the update manager availability changes
    /// </summary>
    internal event AvailabilityChangedHandler OnAvailabilityChanged;

    /// <summary>
    ///   Triggered when the update manager status changes
    /// </summary>
    internal event UpdateStatusChangedHandler OnUpdateStatusChanged;

    /// <summary>
    ///   Triggered when the update manager progress changes
    /// </summary>
    internal event UpdateProgressChangedHandler OnUpdateProgressChanged;

    /// <summary>
    ///   Initializes the update manager asynchronously
    /// </summary>
    internal UpdateManager() {
      try {
        InitializeUnderlyingManager();
      } catch (FileNotFoundException) {
        Log.WriteLine(LogLevel.Warning, "updates are not supported - aborting");
      }
    }

    /// <summary>
    ///   Restarts the app, launching the newest version
    /// </summary>
    internal static void Restart() {
      Log.WriteLine(LogLevel.Warning, "restarting the application");

      try {
        RestartApp();
      } catch {
        Log.WriteLine(LogLevel.Warning, "could not restart to latest version - surely in portable mode");
        Restart();
      }
    }

    /// <summary>
    ///   Silently checks for updates if we are allowed to
    /// </summary>
    /// <param name="dispatcher"></param>
    private void CheckForUpdates(Dispatcher dispatcher) {
      Status = UpdateStatus.CheckingForUpdates;
      dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));

      Log.WriteLine(LogLevel.Verbose, "checking for updates");
      Manager.CheckForUpdate(progress: p => {
        Log.WriteLine(LogLevel.Debug, $"checking for updates ({p}%)");
        dispatcher.Invoke(() => OnUpdateProgressChanged?.Invoke(this, Status, p));
      }).ContinueWith(t => {
        Status = UpdateStatus.Idle;
        dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));

        if (t.IsFaulted || t.IsCanceled) {
          Log.WriteLine(LogLevel.Warning, $"update checker task canceled or failed - {t.Exception}");
          return;
        }

        if (t.Result.ReleasesToApply.Any()) {
          Log.WriteLine(LogLevel.Verbose, $"found {t.Result.ReleasesToApply.Count} update(s)");

          if (Application.Options.UpdatePolicy == UpdatePolicy.Automatic) {
            DownloadUpdates(dispatcher, t.Result);
          } else if (dispatcher.Invoke(() => UpdaterUiHelper.ShowPromptDialog(t.Result))) {
            DownloadUpdates(dispatcher, t.Result);
            dispatcher.Invoke(UpdaterUiHelper.ShowProgressDialog);
          } else {
            Log.WriteLine(LogLevel.Verbose, "operation cancelled by the user");
          }
        }
      });
    }
    /// <summary>
    ///   Downloads and installs updates
    /// </summary>
    /// <param name="dispatcher"></param>
    /// <param name="updates">Update information</param>
    private void DownloadUpdates(Dispatcher dispatcher, UpdateInfo updates) {
      Status = UpdateStatus.DownloadingUpdates;
      dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));

      Log.WriteLine(LogLevel.Verbose, "downloading updates");
      Manager.DownloadReleases(updates.ReleasesToApply, p => {
        Log.WriteLine(LogLevel.Debug, $"downloading updates ({p}%)");
        dispatcher.Invoke(() => OnUpdateProgressChanged?.Invoke(this, Status, p));
      }).ContinueWith(t => {
        Status = UpdateStatus.Idle;
        dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));

        if (t.IsFaulted || t.IsCanceled) {
          Log.WriteLine(LogLevel.Warning, $"update downloader task canceled or failed - {t.Exception}");
          return;
        }

        Log.WriteLine(LogLevel.Verbose, $"downloaded {updates.ReleasesToApply.Sum(r => r.Filesize) / 1024 / 1024}MiB");
        ApplyUpdates(dispatcher, updates);
      });
    }

    private void ApplyUpdates(Dispatcher dispatcher, UpdateInfo updates) {
      Status = UpdateStatus.ApplyingUpdates;
      dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));

      Log.WriteLine(LogLevel.Verbose, "applying updates");
      Manager.ApplyReleases(updates, p => {
        Log.WriteLine(LogLevel.Debug, $"applying updates ({p}%)");
        dispatcher.Invoke(() => OnUpdateProgressChanged?.Invoke(this, Status, p));
      }).ContinueWith(t => {
        if (t.IsFaulted || t.IsCanceled) {
          Status = UpdateStatus.Idle;
          Log.WriteLine(LogLevel.Warning, $"update install task canceled or failed - {t.Exception}");
        } else {
          Status = UpdateStatus.ReadyToRestart;
          Log.WriteLine(LogLevel.Verbose, $"successfully applied {updates.ReleasesToApply.Count} update(s)");
        }

        dispatcher.Invoke(() => OnUpdateStatusChanged?.Invoke(this, Status));
        // TODO: restart the app automatically if it's been idle for some time
      });
    }

    /// <summary>
    ///   Initializes the underlying update manager
    /// </summary>
    private void InitializeUnderlyingManager() {
      try {
        new Squirrel.UpdateManager("").Dispose();
      } catch (FileNotFoundException) {
        Log.WriteLine(LogLevel.Warning, "operation is not supported - aborting");
        Availability = UpdaterAvailability.NotSupported;
        return;
      } catch {
        Availability = UpdaterAvailability.NotAvailable;
      }

      if (Application.Options.UpdatePolicy == UpdatePolicy.Disabled) {
        Log.WriteLine(LogLevel.Warning, "automatic updates are not allowed - aborting");
        Availability = UpdaterAvailability.FullyAvailable;
        return;
      }

      Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

      var updateManagerHandler = new Action<Task<Squirrel.UpdateManager>>(task => {
        if (task.IsFaulted) {
          Log.WriteLine(LogLevel.Warning, $"could not initialize underlying UpdateManager - {task.Exception}");

          // try again on network change?
          NetworkChange.NetworkAddressChanged += OnNetworkChange;
          NetworkChange.NetworkAvailabilityChanged += OnNetworkChange;

          if (Manager != null) {
            Manager = null;
            Availability = UpdaterAvailability.NotAvailable;
            dispatcher.Invoke(() => OnAvailabilityChanged?.Invoke(this, Availability));
          }
        } else {
          Manager = task.Result;
          Availability = UpdaterAvailability.FullyAvailable;
          dispatcher.Invoke(() => OnAvailabilityChanged?.Invoke(this, Availability));
          CheckForUpdates(dispatcher);
        }
      });

      if (GetMetadataValue("githubRepo") is string githubUrl) {
        GitHubUpdateManager(githubUrl,
                            VersionInfo.ProductName,
                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData))
          .ContinueWith(updateManagerHandler);
      } else if (GetMetadataValue("updateUrl") is string updateUrl) {
        Manager = new Squirrel.UpdateManager(updateUrl,
                                             VersionInfo.ProductName,
                                             Environment.GetFolderPath(Environment.SpecialFolder
                                                                                  .LocalApplicationData));
        Availability = UpdaterAvailability.FullyAvailable;
        CheckForUpdates(dispatcher);
      } else {
        Log.WriteLine(LogLevel.Warning, "no update source was configured for this assembly - aborting");
      }

    }

    /// <summary>
    ///   Triggered when a network change is detected and the update manager failed to connect
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Arguments associated to the event</param>
    private void OnNetworkChange(object sender, EventArgs eventArgs) {
      // unregister event handlers
      NetworkChange.NetworkAddressChanged -= OnNetworkChange;
      NetworkChange.NetworkAvailabilityChanged -= OnNetworkChange;

      // reinitialize manager
      InitializeUnderlyingManager();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources
    /// </summary>
    public void Dispose() => Manager?.Dispose();
  }
}