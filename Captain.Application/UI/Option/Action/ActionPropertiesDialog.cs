using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog that lets the user pick one or more output streams and change their configurations
  /// </summary>
  internal sealed partial class ActionPropertiesDialog : Window {
    /// <summary>
    ///   Contains all the output streams alongside their configurations
    /// </summary>
    internal List<(string ActionType, Dictionary<string, object> Options)> Actions { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    public ActionPropertiesDialog(List<(string ActionType, Dictionary<string, object> Options)> outputStreams = null) {
      InitializeComponent();

      this.addActionLinkButton.Image = Resources.TaskAdd;
      this.taskOptionsLinkButton.Image = Resources.EncoderOptions;
      this.deleteTaskLinkButton.Image = Resources.TaskDelete;

      Actions = outputStreams ?? new List<(string ActionType, Dictionary<string, object> Options)>();
      UpdateList();
    }

    /// <summary>
    ///   Updates the action list
    /// </summary>
    private void UpdateList() {
      this.streamListView.Clear();
      this.streamListView.Columns.Add("action", this.streamListView.Width);

      this.streamListView.Items.AddRange(Actions.Select(o => {
          try {
            var pluginObject = new PluginObject(Type.GetType(o.ActionType));
            return new ListViewItem {
              Text = pluginObject.ToString(),
              Tag = pluginObject
            };
          } catch {
            // something bad happened (not a valid type?)
            return new ListViewItem {
              Text = String.Format(Resources.ActionPropertiesDialog_DefaultActionName, o.ActionType),
              Tag = null
            };
          }
        })
        .ToArray());

      OnStreamSelectionChanged(this, null);
      this.okButton.Enabled = Actions.Count > 0;
    }

    /// <summary>
    ///   Triggered when the "Add action" link button gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event args</param>
    private void OnAddActionLinkButtonClick(object sender, EventArgs eventArgs) {
      var dialog = new ActionSelectionDialog();
      if (dialog.ShowDialog(this) == DialogResult.OK) {
        foreach (PluginObject stream in dialog.Streams) { Actions.Add((stream.Type.ToString(), null)); }

        UpdateList();

        if (dialog.Streams.Count() == 1 && dialog.Streams.First().Configurable) {
          try {
            Log.WriteLine(LogLevel.Verbose,
              $"displaying configuration interface for stream \"{dialog.Streams.First().Type}\"");
            var configurableObject =
              FormatterServices.GetUninitializedObject(dialog.Streams.First().Type) as IHasOptions;
            configurableObject?.DisplayOptionsInterface(this);
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"configuration interface error: {exception}");
          }
        }
      }
    }

    /// <summary>
    ///   Triggered when a item from the stream list has been activated
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnStreamListItemActivated(object sender, EventArgs eventArgs) {
      if (this.streamListView.SelectedItems[0].Tag is PluginObject pluginObject && pluginObject.Configurable) {
        try {
          Log.WriteLine(LogLevel.Verbose, $"displaying configuration interface for stream \"{pluginObject.Type}\"");

          if (Activator.CreateInstance(pluginObject.Type,
            BindingFlags.CreateInstance,
            null,
            new object[] { null },
            null) is IHasOptions configurableObject) {
            if (Actions[this.streamListView.SelectedIndices[0]].Options is Dictionary<string, object> userOptions) {
              foreach (KeyValuePair<string, object> pair in userOptions) {
                configurableObject.Options[pair.Key] = pair.Value;
              }
            }

            if (configurableObject.DisplayOptionsInterface(this) == DialogResult.OK) {
              if (configurableObject.Options is Dictionary<string, object> newOptions) {
                Actions[this.streamListView.SelectedIndices[0]] = (pluginObject.Type.Name, newOptions);
              }
            }
          }
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"configuration interface error: {exception}");
          new TaskDialog(Container) {
            Content = Resources.PluginManager_EncoderConfigurationUIError,
            Width = 200,
            ExpandedInformation = $@"{exception.GetType()}: {exception.Message}",
            ExpandFooterArea = true,
            Buttons = { new TaskDialogButton(ButtonType.Ok) },
            MainIcon = TaskDialogIcon.Error,
            WindowTitle = pluginObject.ToString()
          }.ShowDialog();
        }
      }
    }

    /// <summary>
    ///   Triggered when the stream selection changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnStreamSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs eventArgs) {
      if (this.streamListView.SelectedItems.Count > 0) {
        this.taskOptionsLinkButton.Visible = this.deleteTaskLinkButton.Visible = true;

        this.taskOptionsLinkButton.Enabled = this.streamListView.SelectedItems.Count == 1 &&
                                             this.streamListView.SelectedItems[0].Tag is PluginObject pluginObject &&
                                             pluginObject.Configurable;
      } else { this.taskOptionsLinkButton.Visible = this.deleteTaskLinkButton.Visible = false; }
    }

    /// <summary>
    ///   Triggered when the "Task options" link button gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTaskOptionsClicked(object sender, EventArgs eventArgs) {
      OnStreamListItemActivated(sender, eventArgs);
    }

    /// <summary>
    ///   Triggered when the "Delete task" link button gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnDeleteTaskClicked(object sender, EventArgs eventArgs) {
      foreach (int index in this.streamListView.SelectedIndices.Cast<int>().OrderByDescending(v => v)) {
        Actions.RemoveAt(index);
      }

      UpdateList();
    }

    /// <summary>
    ///   Triggered when one of the dialog main buttons get clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnButtonClick(object sender, EventArgs eventArgs) {
      Close();
    }
  }
}