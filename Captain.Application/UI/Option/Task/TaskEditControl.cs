﻿using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Common;
using Action = System.Action;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a Task control
  /// </summary>
  internal sealed partial class TaskEditControl : UserControl {
    /// <summary>
    ///   Task associated with this control
    /// </summary>
    private Task task;

    /// <summary>
    ///   Task associated with this control
    /// </summary>
    internal Task Task {
      private get => this.task;
      set {
        this.task = value;
        UpdateTask();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Control initializer
    /// </summary>
    /// <remarks>
    ///   TODO: replace onEdit, onDelete by events
    /// </remarks>
    /// <param name="task">Task</param>
    /// <param name="onEdit">Action to execute on edit</param>
    /// <param name="onDelete">Action to execute on delete</param>
    public TaskEditControl(Task task = null, Action onEdit = null, Action onDelete = null) {
      SetStyle(ControlStyles.SupportsTransparentBackColor |
               ControlStyles.CacheText |
               ControlStyles.UserMouse |
               ControlStyles.StandardClick |
               ControlStyles.StandardDoubleClick,
        true);
      InitializeComponent();

      this.editButton.Image = Resources.TaskEdit;
      this.deleteButton.Image = Resources.TaskDelete;

      foreach (Control control in Controls) {
        control.MouseMove += OnControlMouseMove;
        control.MouseLeave += OnControlMouseLeave;
      }

      Task = task;

      this.editButton.Click += (s, e) => onEdit?.Invoke();
      this.deleteButton.Click += (s, e) => onDelete?.Invoke();
    }

    /// <summary>
    ///   Updates task information
    /// </summary>
    private void UpdateTask() {
      if (!DesignMode && Task != null) {
        this.taskType.Image =
          Task.TaskType == TaskType.StillImage ? Resources.TaskTypeScreenshot : Resources.TaskTypeRecording;
        this.taskRegionType.Image = Task.RegionType == RegionType.AllScreens ||
                                    Task.RegionType == RegionType.CurrentScreen
          ? Resources.TaskRegionFullScreen
          : Task.RegionType == RegionType.Fixed
            ? Resources.TaskRegionFixed
            : Resources.TaskRegionGrab;
        this.nameLabel.Text = Task.Name;
        this.hotKeyLabel.Text = Task.Hotkey.ToString();
      }
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
    /// <param name="eventArgs">
    ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnPaint(PaintEventArgs eventArgs) {
      eventArgs.Graphics.DrawLine(new Pen(Color.FromArgb(0x10, Color.Black)), 0, Height - 1, Width, Height - 1);
      base.OnPaint(eventArgs);
    }

    /// <summary>
    ///   Triggered when the mouse moves over a control
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnControlMouseMove(object sender, MouseEventArgs eventArgs) {
      BackColor = Color.FromArgb(234, 234, 234);
    }

    /// <summary>
    ///   Triggered when the mouse leaves a control
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnControlMouseLeave(object sender, EventArgs eventArgs) {
      if (!RectangleToScreen(ClientRectangle).Contains(MousePosition)) { BackColor = Color.Transparent; }
    }
  }
}