using System;
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
    internal bool IsFeatureAvailable => Manager != null && Manager.IsInstalledApp;

    /// <summary>
    ///   Current update status
    /// </summary>
    internal UpdateStatus Status { get; private set; } = UpdateStatus.Idle;

    /// <summary>
    ///   Triggered when the update manager availability changes
    /// </summary>
    /// <param name="manager">Update manager instance</param>
    /// <param name="available">Whether or not the update manager is aviailable</param>
    internal delegate void AvailabilityChangedHandler(UpdateManager manager, bool available);

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
      if (Application.Options.UpdatePolicy == UpdatePolicy.Disabled) {
        Log.WriteLine(LogLevel.Warning, "automatic updates are not allowed - aborting");
      } else {
        InitializeUnderlyingManager();
      }
    }

    /// <summary>
    ///   Restarts the app, launching the newest version
    /// </summary>
    internal void Restart() {
      try {
        RestartApp();
      } catch {
        Exit();
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
          } else if (UpdaterUiHelper.ShowPromptDialog(t.Result)) {
            DownloadUpdates(dispatcher, t.Result);
            UpdaterUiHelper.ShowProgressDialog();
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
      Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
      var updateManagerHandler = new Action<Task<Squirrel.UpdateManager>>(task => {
        if (task.IsFaulted) {
          Log.WriteLine(LogLevel.Warning, $"could not initialize underlying UpdateManager - {task.Exception}");

          // try again on network change?
          NetworkChange.NetworkAddressChanged += OnNetworkChange;
          NetworkChange.NetworkAvailabilityChanged += OnNetworkChange;

          if (Manager != null) {
            Manager = null;
            dispatcher.Invoke(() => OnAvailabilityChanged?.Invoke(this, false));
          }
        } else {
          Manager = task.Result;
          dispatcher.Invoke(() => OnAvailabilityChanged?.Invoke(this, true));
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
                                             Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
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