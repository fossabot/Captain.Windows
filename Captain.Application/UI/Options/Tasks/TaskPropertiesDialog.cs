using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog which displays the properties of a given task
  /// </summary>
  internal sealed partial class TaskPropertiesDialog : Window {
    /// <summary>
    ///   Underlying task
    /// </summary>
    internal Task Task { get; private set; }

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
        this.actionsPreviewLabel.Text = String.Format(Resources.TaskPropertiesDialog_MultipleActions, Task.OutputStreams);
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
    [SuppressMessage("ReSharper", "CoVariantArrayConversion")]
    private void OnTaskTypeChanged(object sender, EventArgs eventArgs) {
      Task.Type = (TaskType) this.taskTypeComboBox.SelectedIndex;

      this.encoderComboBox.Tag = this.encoderComboBox.SelectedIndex;
      this.encoderComboBox.Items.Clear();
      this.encoderComboBox.Items.AddRange(Task.Type == TaskType.Recording
                                            ? Application.PluginManager.VideoEncoders.ToArray()
                                            : Application.PluginManager.StaticEncoders.ToArray());
      this.encoderComboBox.SelectedIndex = (int) this.encoderComboBox.Tag;

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
  }
}
