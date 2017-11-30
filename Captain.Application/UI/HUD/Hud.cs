using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides an interface for selecting a screen region and controlling screen recording
  /// </summary>
  internal sealed class Hud : IDisposable {
    /// <summary>
    ///   Mouse hook provider instance
    /// </summary>
    private readonly IMouseHookProvider mouseHook;

    /// <summary>
    ///   Recording session currently bound to the HUD.
    /// </summary>
    private RecordingSession recordingSession;

    /// <summary>
    ///   Whether this HUD instance has been disposed or not
    /// </summary>
    internal bool IsDisposed { get; private set; }

    /// <summary>
    ///   Creates a HUD instance for the system UI
    /// </summary>
    internal Hud() {
      // install global mouse hook
      this.mouseHook = new SystemMouseHookProvider();
      this.mouseHook.OnMouseDown += OnMouseDown;
      this.mouseHook.OnMouseMove += OnMouseMove;
      this.mouseHook.OnMouseUp += OnMouseUp;
    }

    /// <summary>
    ///   Binds a recording session to the HUD.
    /// </summary>
    /// <param name="session">Recording session.</param>
    internal void BindRecordingSession(RecordingSession session) {
      Log.WriteLine(LogLevel.Debug, "bound recording session to HUD");
      this.recordingSession = session;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      IsDisposed = true;
      OnClose?.Invoke(this, EventArgs.Empty);
      this.mouseHook?.Release();
      this.mouseHook?.Dispose();
      InstructionOverlay?.Dispose();
      this.instructionOverlayWrapper?.Dispose();
      SnackBar?.Dispose();
      this.snackBarWrapper?.Dispose();
      this.cropRectangleWrapper?.Dispose();
    }

    #region Events

    /// <summary>
    ///   Triggered when the screen is cropped
    /// </summary>
    internal event EventHandler<Rectangle> OnScreenCrop;

    /// <summary>
    ///   Triggered when the HUD is closed
    /// </summary>
    internal event EventHandler OnClose;

    #endregion

    #region HUD components

    /// <summary>
    ///   Snack bar.
    /// </summary>
    internal SnackBar SnackBar { get; private set; }

    /// <summary>
    ///   Instruction overlay.
    /// </summary>
    private InstructionOverlay InstructionOverlay { get; set; }

    /// <summary>
    ///   Snack bar wrapper window
    /// </summary>
    private SnackBarWrapper snackBarWrapper;

    /// <summary>
    ///   Instruction overlay wrapper window.
    /// </summary>
    private InstructionOverlayWrapper instructionOverlayWrapper;

    /// <summary>
    ///   Crop rectangle wrapper window
    /// </summary>
    private CropRectangleWrapper cropRectangleWrapper;

    #endregion

    #region Selected region

    /// <summary>
    ///   Initial mouse location, saved right after the left mouse button is down
    /// </summary>
    private Point? initialMouseLocation;

    /// <summary>
    ///   Currently selected crop region
    /// </summary>
    private Rectangle selectedRegion;

    /// <summary>
    ///   Whether or not the user is currently handpicking a screen region.
    /// </summary>
    internal bool IsCropUiVisible => this.cropRectangleWrapper?.Visible == true;

    /// <summary>
    ///   Determines whether the specified region is valid.
    /// </summary>
    /// <param name="region">The region to be validated.</param>
    internal static bool IsValidRegion(Rectangle region) => region.Width >= 8 && region.Height >= 8;

    #endregion

    #region Mouse hook event handlers

    /// <summary>
    ///   Triggered when a mouse button is held
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="mouseEventArgs">Event arguments</param>
    private void OnMouseDown(object sender, MouseEventArgs mouseEventArgs) {
      if (mouseEventArgs.Button == MouseButtons.Left) {
        Log.WriteLine(LogLevel.Debug, $"mouse down at ({mouseEventArgs.X}, {mouseEventArgs.Y})");
        this.initialMouseLocation = new Point(mouseEventArgs.X, mouseEventArgs.Y);
        this.instructionOverlayWrapper.Close();
        this.cropRectangleWrapper.Show();
      } else if (this.initialMouseLocation.HasValue) {
        OnMouseUp(this, mouseEventArgs);
      }
    }

    /// <summary>
    ///   Triggered when the mouse is moved
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="mouseEventArgs">Event arguments</param>
    private void OnMouseMove(object sender, MouseEventArgs mouseEventArgs) {
      if (this.initialMouseLocation.HasValue) {
        if (this.initialMouseLocation.Value.X < mouseEventArgs.X) {
          this.selectedRegion.X = this.initialMouseLocation.Value.X;
          this.selectedRegion.Width = mouseEventArgs.X - this.initialMouseLocation.Value.X;
        } else {
          this.selectedRegion.X = mouseEventArgs.X;
          this.selectedRegion.Width = this.initialMouseLocation.Value.X - mouseEventArgs.X;
        }

        if (this.initialMouseLocation.Value.Y < mouseEventArgs.Y) {
          this.selectedRegion.Y = this.initialMouseLocation.Value.Y;
          this.selectedRegion.Height = mouseEventArgs.Y - this.initialMouseLocation.Value.Y;
        } else {
          this.selectedRegion.Y = mouseEventArgs.Y;
          this.selectedRegion.Height = this.initialMouseLocation.Value.Y - mouseEventArgs.Y;
        }

        if (this.cropRectangleWrapper != null) {
          this.cropRectangleWrapper.Bounds = this.selectedRegion;
        }
      }
    }

    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="mouseEventArgs">Event arguments</param>
    private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs) {
      // release mouse hook
      Log.WriteLine(LogLevel.Debug, $"mouse up at ({mouseEventArgs.X}, {mouseEventArgs.Y})");
      this.mouseHook.Release();

      if (IsValidRegion(this.selectedRegion)) {
        // trigger event handlers
        this.cropRectangleWrapper?.Hide();
        OnScreenCrop?.Invoke(this, this.selectedRegion);
      } else {
        Log.WriteLine(LogLevel.Debug, "invalid screen region selected");
        this.cropRectangleWrapper?.Close();
      }

      this.instructionOverlayWrapper?.Close();

      // reset values
      this.initialMouseLocation = null;
    }

    #endregion

    #region Display

    /// <summary>
    ///   Shows/hides the HUD
    /// </summary>
    /// <param name="show">Whether to display the HUD or not</param>
    internal void Display(bool show = true) {
      if (show) {
        if (this.instructionOverlayWrapper == null || this.instructionOverlayWrapper.IsDisposed) {
          this.instructionOverlayWrapper = new InstructionOverlayWrapper();
        }

        if (InstructionOverlay == null) {
          InstructionOverlay = new InstructionOverlay(this.instructionOverlayWrapper, Resources.HUD_CropRectangleHint);
        }

        if (this.cropRectangleWrapper == null || this.cropRectangleWrapper.IsDisposed) {
          this.cropRectangleWrapper = new CropRectangleWrapper();
        }

        this.instructionOverlayWrapper.Show();
        this.snackBarWrapper?.Show();
        this.mouseHook?.Acquire();
      } else { Dispose(); }
    }

    /// <summary>
    ///   Shows/hides the snack bar
    /// </summary>
    /// <param name="show">Whether to display the snack bar or not</param>
    /// <param name="location">Optional position for the snack bar</param>
    internal void DisplaySnackBar(bool show = true, Point location = default) {
      if (show) {
        if (this.snackBarWrapper == null) {
          this.snackBarWrapper = new SnackBarWrapper();

          if (location == default) { this.snackBarWrapper.StartPosition = FormStartPosition.CenterScreen; }
        }

        if (SnackBar == null) {
          this.cropRectangleWrapper.Fixed = true;
          SnackBar = new SnackBar(this.snackBarWrapper);
          SnackBar.OnIntentReceived += OnSnackBarIntentReceived;
        }

        this.snackBarWrapper.Location = location;
        this.snackBarWrapper.Show();
      } else { this.snackBarWrapper?.Close(); }
    }

    /// <summary>
    ///   Triggered when a snack bar action intent is received
    /// </summary>
    /// <param name="sender">Sender snack bar instance</param>
    /// <param name="intent">Intent type</param>
    private void OnSnackBarIntentReceived(SnackBar sender, SnackBarIntent intent) {
      switch (intent) {
        case SnackBarIntent.Screenshot:
          TaskHelper.StartTask(Application.Options.Tasks.First(t => t.TaskType == TaskType.StillImage),
            this.selectedRegion);
          break;

        case SnackBarIntent.Close:
          Display(false);
          break;

        case SnackBarIntent.Options:
          try { new OptionsWindow().Show(); } catch (ApplicationException) {
            /* already open */
          }
          break;

        case SnackBarIntent.ToggleMute: break;

        case SnackBarIntent.ToggleRecord:
          if (this.recordingSession?.State == RecordingState.None) { this.recordingSession.Start(); } else {
            this.recordingSession?.Stop();
          }

          break;
      }
    }

    #endregion
  }
}