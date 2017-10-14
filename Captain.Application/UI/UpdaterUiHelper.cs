using System;
using System.Linq;
using Captain.Application.Native;
using Ookii.Dialogs.Wpf;
using Squirrel;
using static Captain.Application.Application;
using ProgressBarStyle = Ookii.Dialogs.Wpf.ProgressBarStyle;

namespace Captain.Application {
  /// <summary>
  ///   Contains helper functions for the Updater UI
  /// </summary>
  internal static class UpdaterUiHelper {
    /// <summary>
    ///   Displays a dialog prompting the user to download and install updates
    /// </summary>
    /// <param name="update">Update information</param>
    /// <returns>The result of the dialog.</returns>
    internal static bool ShowPromptDialog(UpdateInfo update) => new TaskDialog {
      WindowTitle = String.Format(Resources.UpdaterUI_DialogCaption, VersionInfo.ProductName),
      CustomMainIcon = Resources.UpdateIcon,
      WindowIcon = Resources.AppIcon,
      AllowDialogCancellation = true,
      Width = 200,
      Buttons = {
        new TaskDialogButton(Resources.UpdaterUI_UpdateButton) { Default = true },
        new TaskDialogButton(Resources.UpdaterUI_RemindLaterButton)
      },

      Content = String.Format(Resources.UpdaterUI_DialogText,
                                VersionInfo.ProductName,
                                update.ReleasesToApply.Last().Version,
                                VersionString)
    }.ShowDialog().Default;

    /// <summary>
    ///   Displays a progress dialog for the update procedure
    /// </summary>
    internal static void ShowProgressDialog() {
      var dialog = new TaskDialog {
        WindowTitle = String.Format(Resources.UpdaterUI_DialogCaption, VersionInfo.ProductName),
        CustomMainIcon = Resources.UpdateIcon,
        WindowIcon = Resources.AppIcon,
        AllowDialogCancellation = false,
        Width = 200,
        Buttons = { new TaskDialogButton(ButtonType.Cancel) { Enabled = false } },
        Content = Resources.UpdaterUI_DialogProgressText,
        ProgressBarStyle = ProgressBarStyle.ProgressBar,
        ProgressBarMinimum = 0,
        ProgressBarMaximum = 100
      };

      // bind update manager event handlers
      Application.UpdateManager.OnUpdateProgressChanged += (m, s, p) => dialog.ProgressBarValue = p;
      Application.UpdateManager.OnUpdateStatusChanged += (m, s) => {
        dialog.ProgressBarValue = 0;

        switch (s) {
          case UpdateStatus.ReadyToRestart:
            User32.DestroyWindow(dialog.Handle);  // task dialog on main thread is blocking and prevents app shutdown
            Application.UpdateManager.Restart();
            break;

          case UpdateStatus.ApplyingUpdates:
            dialog.Content = Resources.UpdaterUI_DialogInstallProgressText;
            break;
        }
      };

      dialog.ShowDialog();
    }
  }
}
