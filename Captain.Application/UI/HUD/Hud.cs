using System;
using System.Drawing;
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
    ///   Task bound to the HUD instance
    /// </summary>
    private Task task;

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

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      IsDisposed = true;
      OnClose?.Invoke(this, EventArgs.Empty);
      this.mouseHook?.Release();
      this.mouseHook?.Dispose();
      SnackBar?.Dispose();
      this.snackBarWrapper?.Dispose();
      this.cropRectangleWrapper?.Dispose();
    }

    /// <summary>
    ///   Displays the HUD for a generic task
    /// </summary>
    internal void DisplayForTask(Task taskInstance = null) {
      Display();
      this.task = taskInstance;

      void OnCrop(object sender, Rectangle rectangle) {
        DisplaySnackBar(location: new Point(rectangle.X + (rectangle.Width - 192) / 2, rectangle.Y + rectangle.Height));
      }

      OnScreenCrop += OnCrop;
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
    ///   Snack bar
    /// </summary>
    internal SnackBar SnackBar { get; private set; }

    /// <summary>
    ///   Snack bar wrapper window
    /// </summary>
    private SnackBarWrapper snackBarWrapper;

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
          this.cropRectangleWrapper.DisplayHelpLabel = false;
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

      // trigger event handlers
      this.cropRectangleWrapper?.Close();
      OnScreenCrop?.Invoke(this, this.selectedRegion);

      // reset values
      this.initialMouseLocation = null;
      this.selectedRegion = default;
    }

    #endregion

    #region Display

    /// <summary>
    ///   Shows/hides the HUD
    /// </summary>
    /// <param name="show">Whether to display the HUD or not</param>
    internal void Display(bool show = true) {
      if (show) {
        if (this.cropRectangleWrapper == null || this.cropRectangleWrapper.IsDisposed) {
          this.cropRectangleWrapper = new CropRectangleWrapper();
        }

        this.cropRectangleWrapper.Size = new Size(512, 48);
        this.cropRectangleWrapper.StartPosition = FormStartPosition.CenterScreen;
        this.cropRectangleWrapper.Show();

        this.snackBarWrapper?.Show();
        this.mouseHook?.Acquire();
      } else {
        Dispose();
      }
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

          if (location == default) {
            this.snackBarWrapper.StartPosition = FormStartPosition.CenterScreen;
          }
        }

        if (SnackBar == null) {
          SnackBar = new SnackBar(this.snackBarWrapper);
          SnackBar.OnIntentReceived += OnSnackBarIntentReceived;
        }

        this.snackBarWrapper.Location = location;
        this.snackBarWrapper.Show();
      } else {
        this.snackBarWrapper?.Close();
      }
    }

    /// <summary>
    ///   Triggered when a snack bar action intent is received
    /// </summary>
    /// <param name="sender">Sender snack bar instance</param>
    /// <param name="intent">Intent type</param>
    private void OnSnackBarIntentReceived(SnackBar sender, SnackBarIntent intent) {
      switch (intent) {
        case SnackBarIntent.Screenshot:
          if (this.task == null) {
            TaskHelper.StartTask(Application.Options.Tasks[Application.Options.DefaultScreenshotTask]);
          } else {
            TaskHelper.StartTask(this.task);
          }

          break;

        case SnackBarIntent.Close:
          Display(false);
          break;

        case SnackBarIntent.Options:
          try {
            new OptionsWindow().Show();
          } catch (ApplicationException) {
            /* already open */
          }
          break;

        case SnackBarIntent.ToggleMute:
          break;

        case SnackBarIntent.ToggleRecord:
          break;
      }
    }

    #endregion
  }
}