using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;
using Action = Captain.Common.Action;

namespace Captain.Application {
  /// <summary>
  ///   Manages ongoing actions for an application instance.
  /// </summary>
  internal class ActionManager {
    /// <summary>
    ///   A list containing archived actions (e.g., those who are static)
    /// </summary>
    private readonly List<Action> actionHistory = new List<Action>();

    /// <summary>
    ///   A list of actions that are currently being performed.
    /// </summary>
    private readonly List<Action> currentActions = new List<Action>();

    /// <summary>
    ///   Whether actions are being currently processed or not.
    /// </summary>
    internal bool IsBusy =>
      this.currentActions.Any(a => a.Status == ActionStatus.Ongoing || a.Status == ActionStatus.Paused);

    /// <summary>
    ///   Binds application event handlers for an action.
    /// </summary>
    /// <param name="action">The action.</param>
    private void BindEventHandlers(Action action) => action.OnStatusChanged += OnBoundActionStatusChanged;

    /// <summary>
    ///   Triggered when the status of a bound action changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="actionStatus">New status for the action.</param>
    private void OnBoundActionStatusChanged(object sender, ActionStatus actionStatus) {
      if (sender is Action action) {
        if (actionStatus == ActionStatus.Failed || actionStatus == ActionStatus.Success) {
          action.OnStatusChanged -= OnBoundActionStatusChanged;

          if (!IsBusy) {
            // all current actions have finished
            int failedCount = this.currentActions.Count(a => a.Status == ActionStatus.Failed);
            Log.WriteLine(LogLevel.Verbose, $"all current actions have been completed - {failedCount} have failed");

            if (failedCount == this.currentActions.Count) {
              // all failed
              Log.WriteLine(LogLevel.Error, "all actions have failed");
              Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);

              LegacyNotificationProvider.PushMessage("Everything went wrong!",
                action.Codec is IStillImageCodec
                  ? "All actions failed to handle your screenshot. Tap to see error details."
                  : "All actions failed to handle your recording. Tap to see error details.",
                ToolTipIcon.Error,
                null,
                (s, e) => DisplayActionDialog());
            } else if (failedCount > 0) {
              // some failed
              Log.WriteLine(LogLevel.Warning, "some actions have failed");
              Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);

              NotificationProvider.PushObject(action.Codec is IStillImageCodec
                  ? "Screenshot taken!"
                  : "Recording made!",
                "Your screen was captured, but some actions did not succeed.",
                "Tap to see the results.",
                null,
                action.Thumbnail,
                handler: (s, e) => DisplayActionDialog());
            } else {
              // all succeeded
              Log.WriteLine(LogLevel.Informational, "all actions have succeeded");
              Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Success);

              NotificationProvider.PushObject(action.Codec is IStillImageCodec
                  ? "Screenshot taken!"
                  : "Recording made!",
                "Your screen was captured and all actions succeeded.",
                "Tap to see the results.",
                null,
                action.Thumbnail,
                handler: (s, e) => DisplayActionDialog());
            }

            this.actionHistory.AddRange(this.currentActions);
            this.currentActions.Clear();

            try {
              // update entries for the action dialog
              var dialog = ((ActionDialog) System.Windows.Forms.Application.OpenForms.Cast<Form>()
                .First(f => f is ActionDialog));

              dialog.ReplaceActions(this.actionHistory);
              dialog.Focus();
              dialog.BringToFront();
            } catch {
              /* no action dialog is open */
            }
          }
        }
      }
    }

    /// <summary>
    ///   Adds an action.
    /// </summary>
    /// <param name="action">The action to be added.</param>
    /// <param name="bindEventHandlers">Whether to register event handlers for this action.</param>
    internal void AddAction(Action action, bool bindEventHandlers = true) {
      this.currentActions.Add(action);
      if (bindEventHandlers) { BindEventHandlers(action); }
    }

    /// <summary>
    ///   Displays a dialog with the performed actions.
    /// </summary>
    internal void DisplayActionDialog() {
      try {
        var actions = new List<Action>();
        actions.AddRange(this.actionHistory);
        actions.AddRange(this.currentActions);

        new ActionDialog(actions).Show();
      } catch (ApplicationException) {
        /* already open */
      }
    }
  }
}