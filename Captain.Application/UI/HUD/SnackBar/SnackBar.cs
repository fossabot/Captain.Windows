using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides access to a minimal tooling when capturing screen recordings
  /// </summary>
  internal sealed class SnackBar : IDisposable {
    /// <summary>
    ///   Handler for action intent events
    /// </summary>
    internal delegate void IntentReceivedEventHandler(SnackBar sender, SnackBarIntent intent);
    
    /// <summary>
    ///   Record/Stop button
    /// </summary>
    private SnackBarButton recordButton;

    /// <summary>
    ///   Button list
    /// </summary>
    private readonly List<SnackBarButton> buttons = new List<SnackBarButton>();

    /// <summary>
    ///   Generic, common constructor
    /// </summary>
    private SnackBar() { }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a snack bar instance for an existing wrapper window
    /// </summary>
    /// <param name="wrapper">Wrapper instance</param>
    internal SnackBar(Control wrapper) : this() {
      this.wrapper = wrapper;
      this.renderTarget = new WindowRenderTarget(
        new Factory(FactoryType.MultiThreaded, DebugLevel.Information),
        new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)),
        new HwndRenderTargetProperties {
          PixelSize = new Size2(this.wrapper.Width, this.wrapper.Height),
          Hwnd = this.wrapper.Handle
        });

      // bind events
      this.wrapper.MouseMove += OnWrapperOnMouseMove;
      this.wrapper.MouseLeave += OnWrapperOnMouseLeave;
      this.wrapper.MouseDown += OnWrapperOnMouseDown;
      this.wrapper.MouseUp += OnWrapperOnMouseUp;
      this.wrapper.Paint += OnWrapperOnPaint;

      CreateBrushes();
      CreateButtons();
    }

    /// <summary>
    ///   Toggles Record/Stop button behaviour
    /// </summary>
    /// <param name="state">Recording state</param>
    internal void MorphRecordButton(RecordingState state = RecordingState.None) {
      switch (state) {
        case RecordingState.Recording:
          this.recordButton.NormalFill = this.stopNormalButtonBrush;
          this.recordButton.HoverFill = this.stopHoverButtonBrush;
          this.recordButton.ActiveFill = this.stopActiveButtonBrush;
          this.recordButton.Bitmap = Resources.SnackBarPause;
          break;

        case RecordingState.None:
          this.recordButton.NormalFill = this.recordNormalButtonBrush;
          this.recordButton.HoverFill = this.recordHoverButtonBrush;
          this.recordButton.ActiveFill = this.recordActiveButtonBrush;
          this.recordButton.Bitmap = Resources.SnackBarRecord;
          break;

        case RecordingState.Paused:
          this.recordButton.NormalFill = this.recordNormalButtonBrush;
          this.recordButton.HoverFill = this.recordHoverButtonBrush;
          this.recordButton.ActiveFill = this.recordActiveButtonBrush;
          this.recordButton.Bitmap = Resources.SnackBarResume;
          break;
      }

      Render();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      // unbind wrapper events
      this.wrapper.MouseMove -= OnWrapperOnMouseMove;
      this.wrapper.MouseLeave -= OnWrapperOnMouseLeave;
      this.wrapper.MouseDown -= OnWrapperOnMouseDown;
      this.wrapper.MouseUp -= OnWrapperOnMouseUp;
      this.wrapper.Paint -= OnWrapperOnPaint;

      // release button resources
      if (this.buttons?.Any() == true) { this.buttons.ForEach(b => b.Dispose()); }

      // release resources
      this.genericHoverButtonBrush?.Dispose();
      this.genericActiveButtonBrush?.Dispose();
      this.closeHoverButtonBrush?.Dispose();
      this.closeActiveButtonBrush?.Dispose();
      this.recordNormalButtonBrush?.Dispose();
      this.recordHoverButtonBrush?.Dispose();
      this.recordActiveButtonBrush?.Dispose();
      this.renderTarget?.Dispose();
    }

    /// <summary>
    ///   Triggered when a snack bar action is performed
    /// </summary>
    internal event IntentReceivedEventHandler OnIntentReceived;

    #region Wrapper events

    /// <summary>
    ///   Triggered when the wrapper window gets painted
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnPaint(object sender, PaintEventArgs eventArgs) => Render();

    /// <summary>
    ///   Triggered when the wrapper window receives a mouse up event
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnMouseUp(object sender, MouseEventArgs eventArgs) =>
      OnMouseUp(eventArgs.X, eventArgs.Y, eventArgs.Button);

    /// <summary>
    ///   Triggered when the wrapper window receives a mouse down event
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnMouseDown(object sender, MouseEventArgs eventArgs) =>
      OnMouseDown(eventArgs.X, eventArgs.Y, eventArgs.Button);

    /// <summary>
    ///   Triggered when the wrapper window receives a mouse move event
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnMouseLeave(object sender, EventArgs eventArgs) => OnMouseMove(-1, -1);

    /// <summary>
    ///   Triggered when the wrapper window receives a mouse move event
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnMouseMove(object sender, MouseEventArgs eventArgs) =>
      OnMouseMove(eventArgs.X, eventArgs.Y);

    #endregion

    #region Mouse events

    /// <summary>
    ///   Triggered when the mouse moves over the snack bar
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    private void OnMouseMove(int x, int y) {
      this.buttons.ForEach(b => b.Hovered = false);

      try {
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
        this.buttons.First(b => b.Hovered = b.HitTest(x, y));
      } catch (InvalidOperationException) { }

      Render();
    }

    /// <summary>
    ///   Triggered when a mouse button is held
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="mouseButtons">The buttons</param>
    private void OnMouseDown(int x, int y, MouseButtons mouseButtons) {
      if (mouseButtons != MouseButtons.Left) { return; }
      foreach (SnackBarButton button in this.buttons) { button.Active = button.HitTest(x, y); }

      Render();
    }

    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <param name="mouseButtons">The buttons</param>
    private void OnMouseUp(int x, int y, MouseButtons mouseButtons) {
      if (mouseButtons != MouseButtons.Left) { return; }

      try {
        SnackBarButton button = this.buttons.First(b => b.Active);
        button.Active = false;
        Render();

        if (button.Enabled && button.HitTest(x, y)) { button.PerformClick(); }
      } catch (InvalidOperationException) {
        /* no held buttons */
      }
    }

    #endregion

    #region UI layout creation

    /// <summary>
    ///   Creates brushes for painting button fills
    /// </summary>
    private void CreateBrushes() {
      this.genericHoverButtonBrush = new SolidColorBrush(this.renderTarget, this.hoverColor);
      this.genericActiveButtonBrush = new SolidColorBrush(this.renderTarget, this.activeColor);

      this.recordNormalButtonBrush = new SolidColorBrush(this.renderTarget, this.recordNormalColor);
      this.recordHoverButtonBrush = new SolidColorBrush(this.renderTarget, this.recordHoverColor);
      this.recordActiveButtonBrush = new SolidColorBrush(this.renderTarget, this.recordActiveColor);

      this.stopNormalButtonBrush = new SolidColorBrush(this.renderTarget, this.stopNormalColor);
      this.stopHoverButtonBrush = new SolidColorBrush(this.renderTarget, this.stopHoverColor);
      this.stopActiveButtonBrush = new SolidColorBrush(this.renderTarget, this.stopActiveColor);

      this.closeHoverButtonBrush = new SolidColorBrush(this.renderTarget, this.closeHoverColor);
      this.closeActiveButtonBrush = new SolidColorBrush(this.renderTarget, this.closeActiveColor);
    }

    /// <summary>
    ///   Creates the buttons and binds fill brushes
    /// </summary>
    private void CreateButtons() {
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(0, 0, 16, 32)) {
        Bitmap = Resources.SnackBarGrip,
        Enabled = false
      });

      // screenshot
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(16, 0, 32, 32)) {
        Bitmap = Resources.SnackBarScreenshot,
        Action = () => OnIntentReceived?.Invoke(this, SnackBarIntent.Screenshot)
      });

      // mute/unmute
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(48, 0, 36, 32)) {
        Enabled = false,
        Bitmap = Resources.SnackBarMute,
        Action = () => OnIntentReceived?.Invoke(this, SnackBarIntent.ToggleMute)
      });

      // options
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(140, 0, 36, 32)) {
        Bitmap = Resources.SnackBarOptions,
        Action = () => OnIntentReceived?.Invoke(this, SnackBarIntent.Options)
      });

      /* bind generic fill brushes */
      foreach (SnackBarButton button in this.buttons) {
        button.HoverFill = this.genericHoverButtonBrush;
        button.ActiveFill = this.genericActiveButtonBrush;
      }

      // close
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(176, 0, 32, 32)) {
        Bitmap = Resources.SnackBarClose,
        Action = () => OnIntentReceived?.Invoke(this, SnackBarIntent.Close)
      });

      /* bind brushes */
      this.buttons.Last().HoverFill = this.closeHoverButtonBrush;
      this.buttons.Last().ActiveFill = this.closeActiveButtonBrush;

      // record/stop
      // This button is rendered last so that it's above the rest of buttons. This creates a nice style for the side
      // buttons (mute and options) that seem to fit perfectly with the elliptic shape
      this.recordButton = new SnackBarButton(this.renderTarget, new Ellipse(new Vector2(112, 16), 32, 32)) {
        Bitmap = Resources.SnackBarRecord,
        Action = () => OnIntentReceived?.Invoke(this, SnackBarIntent.ToggleRecord)
      };
      this.buttons.Add(this.recordButton);

      /* bind brushes */
      this.buttons.Last().NormalFill = this.recordNormalButtonBrush;
      this.buttons.Last().HoverFill = this.recordHoverButtonBrush;
      this.buttons.Last().ActiveFill = this.recordActiveButtonBrush;
    }

    #endregion

    #region Colors

    /// <summary>
    ///   Snack bar background color
    /// </summary>
    private readonly Color backgroundColor = new Color(0, 0, 0, 127);

    /// <summary>
    ///   Normal button hover color
    /// </summary>
    private readonly Color hoverColor = new Color(254, 254, 254, 25);

    /// <summary>
    ///   Normal button active color
    /// </summary>
    private readonly Color activeColor = new Color(250, 250, 245, 54);

    /// <summary>
    ///   Hovered close button color
    /// </summary>
    private readonly Color closeHoverColor = new Color(232, 17, 35, 255);

    /// <summary>
    ///   Active close button color
    /// </summary>
    private readonly Color closeActiveColor = new Color(231, 16, 34, 153);

    /// <summary>
    ///   Normal record button color
    /// </summary>
    private readonly Color recordNormalColor = new Color(84, 39, 42);

    /// <summary>
    ///   Hover record button color
    /// </summary>
    private readonly Color recordHoverColor = new Color(158, 27, 38);

    /// <summary>
    ///   Active record button color
    /// </summary>
    private readonly Color recordActiveColor = new Color(102, 30, 36);

    /// <summary>
    ///   Normal stop button color
    /// </summary>
    private readonly Color stopNormalColor = new Color(39, 46, 84);

    /// <summary>
    ///   Hover stop button color
    /// </summary>
    private readonly Color stopHoverColor = new Color(27, 49, 158);

    /// <summary>
    ///   Active stop button color
    /// </summary>
    private readonly Color stopActiveColor = new Color(30, 42, 102);

    #endregion

    #region Disposable rendering resources

    /// <summary>
    ///   Brush used to paint hovered generic buttons
    /// </summary>
    private Brush genericHoverButtonBrush;

    /// <summary>
    ///   Brush used to paint active generic buttons
    /// </summary>
    private Brush genericActiveButtonBrush;

    /// <summary>
    ///   Brush used to paint the hovered close button
    /// </summary>
    private Brush closeHoverButtonBrush;

    /// <summary>
    ///   Brush used to paint the active close button
    /// </summary>
    private Brush closeActiveButtonBrush;

    /// <summary>
    ///   Brush used to paint the record button
    /// </summary>
    private Brush recordNormalButtonBrush;

    /// <summary>
    ///   Brush used to paint the hovered record button
    /// </summary>
    private Brush recordHoverButtonBrush;

    /// <summary>
    ///   Brush used to paint the active record button
    /// </summary>
    private Brush recordActiveButtonBrush;

    /// <summary>
    ///   Brush used to paint the stop button
    /// </summary>
    private Brush stopNormalButtonBrush;

    /// <summary>
    ///   Brush used to paint the hovered stop button
    /// </summary>
    private Brush stopHoverButtonBrush;

    /// <summary>
    ///   Brush used to paint the active stop button
    /// </summary>
    private Brush stopActiveButtonBrush;

    #endregion

    #region Direct2D

    /// <summary>
    ///   Render target
    /// </summary>
    private readonly RenderTarget renderTarget;

    /// <summary>
    ///   Snack bar wrapper window
    /// </summary>
    private readonly Control wrapper;

    /// <summary>
    ///   Renders the snack bar UI
    /// </summary>
    private void Render() {
      this.renderTarget.BeginDraw();
      this.renderTarget.Clear(this.backgroundColor);
      
      foreach (SnackBarButton button in this.buttons) { button.Draw(); }
      
      this.renderTarget.EndDraw();
    }

    #endregion
  }
}