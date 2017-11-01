using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  internal sealed class Hud : IDisposable {
    /// <summary>
    ///   Mouse hook provider instance
    /// </summary>
    private readonly IMouseHookProvider mouseHook;

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
    public void Dispose() { this.mouseHook?.Dispose(); }

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
      }
    }

    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="mouseEventArgs">Event arguments</param>
    private void OnMouseUp(object sender, MouseEventArgs mouseEventArgs) {
      Log.WriteLine(LogLevel.Debug, $"mouse up at ({mouseEventArgs.X}, {mouseEventArgs.Y})");
      this.mouseHook.Release();


    }

    #endregion

    #region Display

    /// <summary>
    ///   Displays the HUD
    /// </summary>
    internal void Show() { this.mouseHook.Acquire(); }

    /// <summary>
    ///   Hides the HUD
    /// </summary>
    internal void Hide() { this.mouseHook.Release(); }

    #endregion
  }
}