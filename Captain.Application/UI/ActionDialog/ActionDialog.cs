using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Action = Captain.Common.Action;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog in which the user can pick one or more output streams
  /// </summary>
  internal sealed partial class ActionDialog : Window {
    /// <summary>
    ///   Enumeration of <see cref="Action"/>s.
    /// </summary>
    private IEnumerable<Action> Actions { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal ActionDialog(IEnumerable<Action> actions) {
      InitializeComponent();

      Actions = actions;
      Icon = Resources.AppIcon;

      UpdateActionList();
    }

    /// <summary>
    ///   Updates the action control container contents
    /// </summary>
    private void UpdateActionList() {
      this.actionControlContainer.Controls.Clear();
      this.actionControlContainer.Controls.AddRange(Actions.Select(a => new ActionControl(a) {
          Dock = DockStyle.Top
        })
        .ToArray<Control>());
    }

    /// <summary>
    ///   Triggered when the size of the action container has changed.
    /// </summary>4
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnActionContainerSizeChanged(object sender, EventArgs eventArgs) =>
      // resize the dialog to match the action container height
      Height = this.actionControlContainer.Height + this.buttonPane.Height;

    /// <summary>
    ///   Triggered when the Close button gets clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnCloseButtonClicked(object sender, EventArgs eventArgs) => Close();
  }
}