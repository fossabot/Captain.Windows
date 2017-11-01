using System;
using System.Linq;
using System.Windows.Forms;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog which displays the properties of a given task
  /// </summary>
  internal sealed partial class TaskPropertiesDialog : Window {
    /// <summary>
    ///   Underlying task
    /// </summary>
    internal Task Task { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="task">An optional instance of <see cref="Task" /></param>
    public TaskPropertiesDialog(Task task = null) {
      InitializeComponent();

      this.encoderOptionsLinkButton.Image = Resources.EncoderOptions;

      Task = task ?? new Task();

      this.taskNameTextBox.Text = Task.Name;
      this.taskTypeComboBox.SelectedIndex = (int) Task.Type;
      this.notificationPolicyComboBox.SelectedIndex = (int) Task.NotificationOptions;
      this.regionTypeComboBox.SelectedIndex = (int) Task.Parameters.RegionType;

      UpdateActionsPreview();
      UpdateValidationStatus();
    }

    /// <summary>
    ///   Validates the task data
    /// </summary>
    private void UpdateValidationStatus() {
      bool ok = !String.IsNullOrWhiteSpace(Task.Name);
      if (!Task.OutputStreams.Any()) { ok = false; }
      this.okButton.Enabled = ok;
    }

    /// <summary>
    ///   Updates actions preview label text
    /// </summary>
    private void UpdateActionsPreview() {
      if (Task.OutputStreams.Count == 1) {
        this.actionsPreviewLabel.Text = Resources.TaskPropertiesDialog_SingleAction;
      } else if (Task.OutputStreams.Count > 1) {
        this.actionsPreviewLabel.Text =
          String.Format(Resources.TaskPropertiesDialog_MultipleActions, Task.OutputStreams.Count);
      } else {
        this.actionsPreviewLabel.Text = Resources.TaskPropertiesDialog_NoActions;
      }
    }

    /// <summary>
    ///   Triggered when the task name changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTaskNameChanged(object sender, EventArgs eventArgs) {
      this.taskNameTextBox.Text = Task.Name = this.taskNameTextBox.Text.Trim();
      UpdateValidationStatus();
    }

    /// <summary>
    ///   Triggered when the task type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTaskTypeChanged(object sender, EventArgs eventArgs) {
      Task.Type = (TaskType) this.taskTypeComboBox.SelectedIndex;

      // ReSharper disable CoVariantArrayConversion
      this.encoderComboBox.Items.Clear();
      this.encoderComboBox.Items.AddRange(Task.Type == TaskType.Recording
                                            ? Application.PluginManager.VideoEncoders.ToArray()
                                            : Application.PluginManager.StaticEncoders.ToArray());

      if (this.encoderComboBox.Tag == null && Task != null) {
        this.encoderComboBox.SelectedItem =
          this.encoderComboBox.Items.Cast<PluginObject>().First(o => o.Type.FullName == Task.Parameters.Encoder);
        this.encoderComboBox.Tag = this.encoderComboBox.SelectedIndex;
      } else {
        this.encoderComboBox.Tag = this.encoderComboBox.SelectedIndex;
        this.encoderComboBox.SelectedIndex =
          Math.Min((int) this.encoderComboBox.Tag, this.encoderComboBox.Items.Count - 1);
      }

      UpdateValidationStatus();
    }

    /// <summary>
    ///   Triggered when the notification policy changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnNotificationPolicyChanged(object sender, EventArgs eventArgs) {
      Task.NotificationOptions = (NotificationDisplayOptions) this.notificationPolicyComboBox.SelectedIndex;
      UpdateValidationStatus();
    }

    /// <summary>
    ///   Triggered when the region type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnRegionTypeChanged(object sender, EventArgs eventArgs) {
      var regionType = (TaskRegionType) this.regionTypeComboBox.SelectedIndex;

      // TODO: handle specific region types
      Task.Parameters.RegionType = regionType;
      UpdateValidationStatus();
    }

    /// <summary>
    ///   Triggered when the encoder type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnEncoderChanged(object sender, EventArgs eventArgs) {
      Task.Parameters.Encoder = ((PluginObject) this.encoderComboBox.SelectedItem).Type.FullName;
      this.encoderOptionsLinkButton.Enabled = ((PluginObject) this.encoderComboBox.SelectedItem).Configurable;
      UpdateValidationStatus();
    }

    /// <summary>
    ///   Triggered when the "Change..." link label gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnActionsChangeClicked(object sender, EventArgs eventArgs) {
      var streamDialog = new OutputStreamPropertiesDialog(Task.OutputStreams);

      if (streamDialog.ShowDialog(this) == DialogResult.OK) {
        Task.OutputStreams = streamDialog.OutputStreams;
        UpdateActionsPreview();
        UpdateValidationStatus();
      }
    }

    /// <summary>
    ///   Triggered when the "Encoder options" link button gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnEncoderOptionsLinkButtonClicked(object sender, EventArgs eventArgs) {
      try {
        Log.WriteLine(LogLevel.Verbose,
                      $"displaying configuration interface for encoder \"{((PluginObject) this.encoderComboBox.SelectedItem).Type}\"");
        var configurableObject =
          Activator.CreateInstance(((PluginObject) this.encoderComboBox.SelectedItem).Type) as IConfigurableObject;
        configurableObject?.DisplayConfigurationInterface(this);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"configuration interface error: {exception}");
        new TaskDialog(Container) {
          Content = Resources.PluginManager_EncoderConfigurationUIError,
          Width = 200,
          ExpandedInformation = $@"{exception.GetType()}: {exception.Message}",
          ExpandFooterArea = true,
          Buttons = {new TaskDialogButton(ButtonType.Ok)},
          MainIcon = TaskDialogIcon.Error,
          WindowTitle = this.encoderComboBox.Text
        }.ShowDialog();
      }
    }

    /// <summary>
    ///   Triggered when one of the dialog main buttons get clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnButtonClicked(object sender, EventArgs eventArgs) => Close();
  }
}