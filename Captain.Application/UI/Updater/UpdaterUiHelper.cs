using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using MyAPKapp.VistaUIFramework;
using MyAPKapp.VistaUIFramework.TaskDialog;
using NuGet;
using Squirrel;
using static Captain.Application.Application;

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
      Icon = Resources.UpdateIcon,
      WindowIcon = Resources.AppIcon,
      AllowDialogCancelation = true,
      CloseEnabled = true,
      CommonButtons = TaskDialogCommonButton.Yes | TaskDialogCommonButton.No,
      Content = String.Format(Resources.UpdaterUI_DialogText,
                              VersionInfo.ProductName,
                              update.ReleasesToApply.Last().Version,
                              VersionString)
    }.ShowDialog().CommonButton == DialogResult.Yes;

    /// <summary>
    ///   Displays a progress dialog for the update procedure
    /// </summary>
    internal static void ShowProgressDialog() {
      var dialog = new TaskDialog {
        WindowTitle = String.Format(Resources.UpdaterUI_DialogCaption, VersionInfo.ProductName),
        Icon = Resources.UpdateIcon,
        WindowIcon = Resources.AppIcon,
        AllowDialogCancelation = false,
        CloseEnabled = false,
        CommonButtons = TaskDialogCommonButton.Cancel,
        Content = Resources.UpdaterUI_DialogProgressText,
        UseProgressBar = true,
        ProgressMinimum = 0,
        ProgressMaximum = 100
      };

      // disable Cancel button, as we can not remove the buttons from a task dialog
      dialog.ButtonClick += (_, e) => e.Cancel = true;

      // bind update manager event handlers
      Application.UpdateManager.OnUpdateStatusChanged += (_, s) => {
        dialog.ProgressValue = 0;

        switch (s) {
          case UpdateStatus.ReadyToRestart:
            Application.UpdateManager.Restart();
            break;

          case UpdateStatus.ApplyingUpdates:
            dialog.Content = Resources.UpdaterUI_DialogInstallProgressText;
            break;
        }
      };

      Application.UpdateManager.OnUpdateProgressChanged += (_, s, p) => dialog.ProgressValue = p;
      dialog.ShowDialog();
    }
  }
}
