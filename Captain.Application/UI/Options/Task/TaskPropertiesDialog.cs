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
    ///   Keyboard hook for getting the hotkey for this task.
    /// </summary>
    private IKeyboardHookProvider keyboardHook;

    /// <summary>
    ///   Underlying task
    /// </summary>
    internal Task Task { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="taskIntent">An optional instance of <see cref="Task" /></param>
    public TaskPropertiesDialog(Task taskIntent = null) {
      InitializeComponent();

      // dialog result will be set to "OK" once the data has been validated
      DialogResult = DialogResult.Cancel;

      this.keyboardHook = new SystemKeyboardHookProvider();
      this.keyboardHook.OnKeyUp += OnHotkeyKeyUp;

      this.encoderOptionsLinkButton.Image = Resources.EncoderOptions;

      Task = taskIntent ?? new Task();

      this.taskNameTextBox.Text = Task.Name;
      this.taskTypeComboBox.SelectedIndex = (int) Task.TaskType;
      this.notificationPolicyComboBox.SelectedIndex = (int) Task.NotificationPolicy;
      this.regionTypeComboBox.SelectedIndex = (int) Task.RegionType;
      this.hotkeyTextBox.Text = Task.Hotkey.ToString();

      UpdateActionsPreview();
    }

    /// <summary>
    ///   Triggered when a keyboard key is released.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnHotkeyKeyUp(object sender, KeyEventArgs eventArgs) {
      Keys keys = (Keys) eventArgs.KeyValue | (eventArgs.KeyData & Keys.Modifiers);
      if (keys == Keys.Back) { keys = default; }

      Task.Hotkey = keys;
      this.hotkeyTextBox.Text = Task.Hotkey.ToString();
      this.taskNameTextBox.Focus();
    }

    /// <summary>
    ///   Updates actions preview label text
    /// </summary>
    private void UpdateActionsPreview() {
      if (Task.Actions.Count == 1) {
        this.actionsPreviewLabel.Text = Resources.TaskPropertiesDialog_SingleAction;
      } else if (Task.Actions.Count > 1) {
        this.actionsPreviewLabel.Text =
          String.Format(Resources.TaskPropertiesDialog_MultipleActions, Task.Actions.Count);
      } else {
        this.actionsPreviewLabel.Text = Resources.TaskPropertiesDialog_NoActions;
      }
    }

    /// <summary>
    ///   Triggered when the task name changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTaskNameChanged(object sender, EventArgs eventArgs) => Task.Name = this.taskNameTextBox.Text;

    /// <summary>
    ///   Triggered when the task type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTaskTypeChanged(object sender, EventArgs eventArgs) {
      Task.TaskType = (TaskType) this.taskTypeComboBox.SelectedIndex;

      // ReSharper disable CoVariantArrayConversion
      this.encoderComboBox.Items.Clear();
      this.encoderComboBox.Items.AddRange(Task.TaskType == TaskType.Video
        ? Application.PluginManager.VideoCodecs.ToArray()
        : Application.PluginManager.StillImageCodecs.ToArray());

      if (this.encoderComboBox.Tag == null && Task != null) {
        try {
          this.encoderComboBox.SelectedItem =
            this.encoderComboBox.Items.Cast<PluginObject>().First(o => o.Type.FullName == Task.Codec.CodecType);
          this.encoderComboBox.Tag = this.encoderComboBox.SelectedIndex;
        } catch {
          // maybe this is an empty task
          this.encoderComboBox.SelectedIndex = 0;
        }
      } else {
        this.encoderComboBox.Tag = this.encoderComboBox.SelectedIndex;
        this.encoderComboBox.SelectedIndex =
          Math.Min((int) this.encoderComboBox.Tag, this.encoderComboBox.Items.Count - 1);
      }
    }

    /// <summary>
    ///   Triggered when the notification policy changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnNotificationPolicyChanged(object sender, EventArgs eventArgs) =>
      Task.NotificationPolicy = (NotificationPolicy) this.notificationPolicyComboBox.SelectedIndex;

    /// <summary>
    ///   Triggered when the region type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnRegionTypeChanged(object sender, EventArgs eventArgs) {
      /*var regionType = (TaskRegionType) this.regionTypeComboBox.SelectedIndex;

      // TODO: handle specific region types
      Task.Parameters.RegionType = regionType;
      UpdateValidationStatus();*/
    }

    /// <summary>
    ///   Triggered when the encoder type changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnEncoderChanged(object sender, EventArgs eventArgs) {
      Task.Codec = (((PluginObject) this.encoderComboBox.SelectedItem).Type.FullName, null);
      this.encoderOptionsLinkButton.Enabled = ((PluginObject) this.encoderComboBox.SelectedItem).Configurable;
    }

    /// <summary>
    ///   Triggered when the "Change..." link label gets clicked
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnActionsChangeClicked(object sender, EventArgs eventArgs) {
      var streamDialog = new ActionPropertiesDialog(Task.Actions.ToList());

      if (streamDialog.ShowDialog(this) == DialogResult.OK) {
        Task.Actions = streamDialog.Actions;
        UpdateActionsPreview();
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
          Activator.CreateInstance(((PluginObject) this.encoderComboBox.SelectedItem).Type) as IHasOptions<object>;
        configurableObject?.DisplayOptionsInterface(this);
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
    ///   Triggered when the hotkey text box is focused.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnHotkeyTextBoxEnter(object sender, EventArgs eventArgs) {
      DesktopKeyboardHook.Release();
      this.keyboardHook.Acquire();
    }

    /// <summary>
    ///   Triggered when the hotkey text box loses focus.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnHotkeyTextBoxLeave(object sender, EventArgs eventArgs) {
      this.keyboardHook.Release();
      DesktopKeyboardHook.Acquire();
    }
  }
}