using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Threading;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;
using Action = Captain.Common.Action;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a Task control
  /// </summary>
  internal sealed partial class ActionControl : UserControl {
    /// <summary>
    ///   Current progress value.
    /// </summary>
    private double progress = 1;

    /// <summary>
    ///   Holds a list of plot points for displaying speed changes.
    /// </summary>
    private List<uint> ratePlotPoints = new List<uint>();

    /// <summary>
    ///   Dispatcher used for invoking methods from action event handlers.
    /// </summary>
    private Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

    /// <summary>
    ///   Action associated with this control
    /// </summary>
    internal Action Action { get; set; }

    /// <summary>
    ///   Exception associated with this control
    /// </summary>
    internal Exception Exception { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Control initializer
    /// </summary>
    /// <param name="action">Action instance</param>
    public ActionControl(Action action) {
      SetStyle(ControlStyles.SupportsTransparentBackColor |
               ControlStyles.CacheText |
               ControlStyles.UserMouse |
               ControlStyles.StandardClick |
               ControlStyles.StandardDoubleClick,
        true);
      Action = action;
      InitializeComponent();

      try {
        this.actionNameLabel.Text =
          Application.PluginManager.Actions.First(p => p.Type.Name == action.GetType().Name).ToString();
      } catch (InvalidOperationException) {
        // no such action?
        this.actionNameLabel.Text = Resources.PluginManager_DefaultActionName;
      }

      UpdateAction();
      UpdateThumbnail();

      // bind action events
      Action.OnStatusMessageChanged += (s, e) => this.dispatcher.Invoke(UpdateAction);
      Action.OnStatusChanged += (s, e) => this.dispatcher.Invoke(UpdateAction);

      if (Action is IReportsProgress reportingProgressAction) {
        this.progress = 0;
        reportingProgressAction.OnProgressChanged += OnActionProgressChanged;
      }

      foreach (Control control in Controls) {
        control.MouseMove += OnControlMouseMove;
        control.MouseLeave += OnControlMouseLeave;
      }
    }

    /// <summary>
    ///   Triggered when the underlying <see cref="Action"/> progress changes.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="progress">The new progress value.</param>
    private void OnActionProgressChanged(object sender, double progress) {
      this.progress = progress;

      // calculate rate and add plot point
      if (Action is IReportsRate reportingRateAction) { this.ratePlotPoints.Add(reportingRateAction.GetRate()); }
      Invalidate();
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
    /// <param name="eventArgs">
    ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnPaint(PaintEventArgs eventArgs) {
      Color color = Action.Status == ActionStatus.Success
        ? Color.FromArgb(0x40, 6, 176, 37)
        : Action.Status == ActionStatus.Failed
          ? Color.FromArgb(0x40, 219, 36, 39)
          : Action.Status == ActionStatus.Paused
            ? Color.FromArgb(0x40, 174, 153, 0)
            : Color.Transparent;

      if (Action is IReportsProgress reportingProgressAction) {
        eventArgs.Graphics.FillRectangle(new SolidBrush(color),
          new Rectangle(0, 0, (int) (Width * reportingProgressAction.Progress), Height));
      } else if (Action.Status == ActionStatus.Ongoing) {
        Rectangle rect = Bounds;
        using (var gradientBrush = new LinearGradientBrush(Bounds,
          Color.Transparent,
          Color.Transparent,
          LinearGradientMode.Horizontal)) {
          gradientBrush.InterpolationColors = new ColorBlend(3) {
            Colors = new[] {Color.Transparent, color, Color.Transparent},
            Positions = new[] {0.0f, 0.5f, 1.0f}
          };

          eventArgs.Graphics.FillRectangle(gradientBrush, rect);
        }
      }

      eventArgs.Graphics.DrawLine(new Pen(Color.FromArgb(0x10, Color.Black)),
        0,
        Height - 1,
        (int) (Width * this.progress),
        Height - 1);
      base.OnPaint(eventArgs);
    }

    /// <summary>
    ///   Triggered when the mouse moves over a control
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnControlMouseMove(object sender, MouseEventArgs eventArgs) {
      BackColor = Color.FromArgb(234, 234, 234);
      Refresh();
    }

    /// <summary>
    ///   Triggered when the mouse leaves a control
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnControlMouseLeave(object sender, EventArgs eventArgs) {
      if (!RectangleToScreen(ClientRectangle).Contains(MousePosition)) {
        BackColor = Color.Transparent;
        Refresh();
      }
    }

    /// <summary>
    ///   Updates the preview bitmap.
    /// </summary>
    private void UpdateThumbnail() {
      if (this.previewOverlay.Image == null) { return; }

      int margin = 24 * (this.previewOverlay.Image.Width / this.previewOverlay.Width) / 2;
      using (var bmp = new Bitmap(this.previewOverlay.Width - margin, this.previewOverlay.Height - margin)) {
        using (var graphics = Graphics.FromImage(bmp)) {
          // draw thumbnail on the center of the preview overlay
          Image image = Action.Thumbnail ?? Resources.CapturePreviewUnavailable;
          graphics.DrawImage(image,
            new Rectangle(Point.Empty, bmp.Size),
            new Rectangle(Point.Empty, image.Size),
            GraphicsUnit.Pixel);
        }

        // set as background
        this.previewOverlay.BackgroundImage = bmp.Clone() as Bitmap;
      }
    }

    /// <summary>
    ///   Updates the control to reflect changes in the underlying <see cref="Action"/> instance.
    /// </summary>
    private void UpdateAction() {
      // set preview overlay
      switch (Action.Status) {
        case ActionStatus.Paused:
        case ActionStatus.Ongoing:
          this.previewOverlay.Image = Resources.NeutralResultOverlay;
          break;

        case ActionStatus.Failed:
          this.previewOverlay.Image = Resources.UnsuccessfulResultOverlay;
          break;

        case ActionStatus.Success:
          this.previewOverlay.Image = Resources.SuccessfulResultOverlay;
          break;
      }

      this.errorDetailsLinkButton.Click -= OnErrorDetailsLinkButtonClick;

      this.statusLabel.Text = Action.StatusMessage;

      this.errorDetailsLinkButton.Image = Resources.CaptureResultErrorDetails;
      this.errorDetailsLinkButton.Visible = Action.Status == ActionStatus.Failed;

      this.uriLinkButton.Image = Resources.CaptureResultUri;
      this.uriLinkButton.Visible = false;

      try {
        if (Action.Status == ActionStatus.Success && Action is IYieldsUri uriEnabledAction) {
          Uri uri = uriEnabledAction.GetUri();

          this.uriLinkButton.Visible = Action.Status == ActionStatus.Success;
          this.uriLinkButton.ContextMenu = new ContextMenu(new[] {
            // TODO: ensure this Process.Start() is not dangerous or replace by some obscure shell P/Invokes
            //       so that no process is created and no RCE is exposed to action implementors
            new MenuItem(Resources.ActionControl_ContextMenu_Open, (s, e) => Process.Start(uri.ToString())) {
              DefaultItem = true
            },
            new MenuItem(Resources.ActionControl_ContextMenu_OpenInFolder,
              (s, e) => {
                if (uri.IsFile) { ShellHelper.RevealInExplorerAsync(uri.LocalPath); }
              }) {Enabled = uri.IsFile && !uri.IsUnc},
            new MenuItem(@"-"),
            new MenuItem(Resources.ActionControl_ContextMenu_CopyUri, (s, e) => Clipboard.SetText(uri.ToString()))
          });

          this.uriLinkButton.MouseClick += (s, e) => {
            if (e.Button == MouseButtons.Left) { this.uriLinkButton.ContextMenu.MenuItems[0].PerformClick(); } else if (
              e.Button == MouseButtons.Middle && this.uriLinkButton.ContextMenu.MenuItems[1].Enabled) {
              this.uriLinkButton.ContextMenu.MenuItems[1].PerformClick();
            }
          };
        }
      } catch (Exception exception) { Log.WriteLine(LogLevel.Warning, $"action URI UI exception: {exception}"); }

      if (this.errorDetailsLinkButton.Visible) { this.errorDetailsLinkButton.Click += OnErrorDetailsLinkButtonClick; }

      Invalidate();
    }

    /// <summary>
    ///   Triggered when the "Display error details" button is clicked.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnErrorDetailsLinkButtonClick(object sender, EventArgs eventArgs) => new TaskDialog {
      WindowTitle = this.actionNameLabel.Text,
      MainIcon = TaskDialogIcon.Error,
      WindowIcon = Resources.AppIcon,
      AllowDialogCancellation = true,
      Width = 200,
      Buttons = {
        new TaskDialogButton(ButtonType.Close) {Default = true},
        new TaskDialogButton(Resources.TaskDialog_GenericReportButtonContent) {
          Enabled = false
        } // TODO: implement bug reporter helper
      },

      Content = Action.InnerException?.Message ?? Resources.ActionControl_ErrorDetailsDefaultContent,
      ExpandFooterArea = true,
      ExpandedInformation = Action.InnerException?.ToString()
    }.ShowDialog();

    /// <summary>
    ///   Updates the thumbnail size on preview overlay image size change.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnPreviewOverlaySizeChanged(object sender, EventArgs eventArgs) => UpdateThumbnail();

    private void timer1_Tick(object sender, EventArgs e) => Invalidate();
  }
}