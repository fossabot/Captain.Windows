using System;
using System.Collections.Generic;
using System.Linq;
using Captain.Application.Native;
using Captain.Common;
using Microsoft.WindowsAPICodePack.Dialogs;
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
    internal static UpdaterDialogResult ShowDialog(UpdateInfo update) {
      Log.WriteLine(LogLevel.Verbose, "fetching release notes");
      Dictionary<ReleaseEntry, string> releases = update.FetchReleaseNotes();

      // create dialog
      var dialog = new TaskDialog {
        Caption = String.Format(Resources.UpdaterUI_DialogCaption, VersionInfo.ProductName),
        Icon = (TaskDialogStandardIcon)User32.SystemResources.IDI_APPLICATION,
        Text = String.Format(Resources.UpdaterUI_DialogText,
                             VersionInfo.ProductName,
                             releases.Last().Key.Version,
                             VersionString),

        ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandContent,
        DetailsExpanded = false,

        DetailsExpandedLabel = Resources.UpdaterUI_DialogReleaseNotesCollapse,
        DetailsCollapsedLabel = Resources.UpdaterUI_DialogReleaseNotesExpand,
        DetailsExpandedText = releases.Last().Value,

        Cancelable = true
      };

      // create buttons
      var updateButton = new TaskDialogButton(null, Resources.UpdaterUI_DialogUpdateButton);
      var remindLaterButton = new TaskDialogButton(null, Resources.UpdaterUI_DialogRemindLaterButton);
      var skipVersionButton = new TaskDialogButton(null, Resources.UpdaterUI_DialogSkipVersionButton);

      // bind button event handlers
      updateButton.Click += (_, __) => dialog.Close((TaskDialogResult)UpdaterDialogResult.Update);
      remindLaterButton.Click += (_, __) => dialog.Close((TaskDialogResult)UpdaterDialogResult.RemindLater);
      skipVersionButton.Click += (_, __) => dialog.Close((TaskDialogResult)UpdaterDialogResult.SkipVersion);

      // add buttons to the dialog
      dialog.Controls.AddRange(new[] { updateButton, remindLaterButton, skipVersionButton });

      // set icon on dialog open
      //dialog.Opened += (_, __) => dialog.Icon = (TaskDialogStandardIcon)User32.SystemResources.IDI_APPLICATION;

      return (UpdaterDialogResult)dialog.Show();
    }
  }
}
