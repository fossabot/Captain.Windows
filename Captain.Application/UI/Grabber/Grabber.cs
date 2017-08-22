using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using Captain.Common;

namespace Captain.Application {
  /// <summary>
  ///   Provides a user interface for handpicking a screen region and using additional tools
  /// </summary>
  internal class Grabber : IDisposable {
    /// <summary>
    ///   Grabber window (displays the area rectangle)
    /// </summary>
    private GrabberWindow window;

    /// <summary>
    ///   Grabber tool bar (displays editing tools)
    /// </summary>
    private GrabberToolBarWindow toolBar;

    /// <summary>
    ///   Gets the area selected by the user
    /// </summary>
    internal Rectangle Area => this.window.Area;

    /// <summary>
    ///   Intent receiving delegate
    /// </summary>
    /// <param name="intent">Capture intent instance</param>
    internal delegate void IntentReceivedHandler(object sender, CaptureIntent intent);

    /// <summary>
    ///   Intent received event
    /// </summary>
    internal event IntentReceivedHandler OnIntentReceived;

    /// <summary>
    ///   Creates a new <see cref="Grabber"/> instance
    /// </summary>
    /// <param name="acceptableActionTypes">Acceptable action types</param>
    internal Grabber(ActionType acceptableActionTypes) {
      this.window = new GrabberWindow();
      this.toolBar = new GrabberToolBarWindow(acceptableActionTypes);

      // TODO: restore GrabberWindow bounds
      this.window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      this.window.Width = Screen.PrimaryScreen.WorkingArea.Width / 3d;
      this.window.Height = Screen.PrimaryScreen.WorkingArea.Height / 3d;

      this.window.SizeChanged += UpdateToolBarPosition;
      this.window.LocationChanged += UpdateToolBarPosition;

      this.toolBar.OnCaptureActionInitiated += OnCaptureActionInitiated;

      this.window.Closed += (_, __) => Dispose();
      this.toolBar.Closed += (_, __) => Dispose();
    }

    /// <summary>
    ///   Disposes resources used by the grabber interface
    /// </summary>
    public void Dispose() {
      try {
        this.window.Close();
        this.toolBar.Close();
      } catch {
        // one of them was already closed - don't worry babe'
      }

      this.window = null;
      this.toolBar = null;
    }

    /// <summary>
    ///   Displays the grabber UI
    /// </summary>
    /// <returns>Whether or not the capture is being created</returns>
    internal void Show() {
      this.window.Show();
      this.toolBar.Show();
      UpdateToolBarPosition();
    }

    /// <summary>
    ///   Hides the grabber UI
    /// </summary>
    internal void Hide() {
      this.window.Hide();
      this.toolBar.Hide();
    }

    /// <summary>
    ///   Updates the position of the tool bar so it lays above/below the area selection window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void UpdateToolBarPosition(object sender = null, EventArgs eventArgs = null) {
      if (this.window.Top + this.window.Height + 8 + this.toolBar.Height > SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) {
        this.toolBar.Top = this.window.Top - this.toolBar.Height - 8;
      } else {
        this.toolBar.Top = this.window.Top + this.window.Height + 8;
      }

      this.toolBar.Left = Math.Min(Math.Max(SystemParameters.VirtualScreenLeft, this.window.Left + (this.window.Width - this.toolBar.Width) / 2), SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - this.toolBar.Width);
    }

    /// <summary>
    ///   Triggered when the toolbar initiates an action
    /// </summary>
    /// <param name="type">Action type</param>
    private void OnCaptureActionInitiated(ActionType type) =>
      OnIntentReceived?.Invoke(this, new CaptureIntent(type) {
        VirtualArea = this.window.Area
      });
  }
}
