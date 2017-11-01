using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides access to a minimal tooling when capturing screen recordings
  /// </summary>
  internal sealed class SnackBar : IDisposable {
    /// <summary>
    ///   Button list
    /// </summary>
    private readonly List<SnackBarButton> buttons = new List<SnackBarButton>();

    /// <summary>
    ///   Generic, common constructor
    /// </summary>
    private SnackBar() {
#if DEBUG || TRACE
      this.factory = new Factory(FactoryType.SingleThreaded, DebugLevel.Information);
#else
      this.factory = new Factory(FactoryType.SingleThreaded, DebugLevel.None);
#endif
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a snack bar instance for an existing wrapper window
    /// </summary>
    /// <param name="wrapper">Wrapper instance</param>
    internal SnackBar(Control wrapper) : this() {
      this.renderTarget = new WindowRenderTarget(this.factory,
        new RenderTargetProperties(),
        new HwndRenderTargetProperties {
          PixelSize = new Size2(wrapper.Width, wrapper.Height),
          Hwnd = wrapper.Handle
        });

      // bind events
      wrapper.MouseMove += (s, e) => OnMouseMove(e.X, e.Y);
      wrapper.MouseLeave += (s, e) => OnMouseMove(-1, -1);
      wrapper.MouseDown += (s, e) => OnMouseDown(e.X, e.Y, e.Button);
      wrapper.MouseUp += (s, e) => OnMouseUp(e.Button);

      wrapper.Paint += (s, e) => Render();

      CreateBrushes();
      CreateButtons();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      this.genericHoverButtonBrush?.Dispose();
      this.genericActiveButtonBrush?.Dispose();
      this.closeHoverButtonBrush?.Dispose();
      this.closeActiveButtonBrush?.Dispose();
      this.recordNormalButtonBrush?.Dispose();
      this.recordHoverButtonBrush?.Dispose();
      this.recordActiveButtonBrush?.Dispose();
      this.factory?.Dispose();
      this.renderTarget?.Dispose();
    }

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
      foreach (SnackBarButton button in this.buttons) {
        button.Active = button.HitTest(x, y);
      }

      Render();
    }

    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    /// <param name="mouseButtons">The buttons</param>
    private void OnMouseUp(MouseButtons mouseButtons) {
      if (mouseButtons != MouseButtons.Left) { return; }
      this.buttons.Where(b => b.Active).ToList().ForEach(b => {
        b.Active = false;
        b.PerformClick();
      });
      Render();
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

      this.closeHoverButtonBrush = new SolidColorBrush(this.renderTarget, this.closeHoverColor);
      this.closeActiveButtonBrush = new SolidColorBrush(this.renderTarget, this.closeActiveColor);
    }

    /// <summary>
    ///   Creates the buttons and binds fill brushes
    /// </summary>
    private void CreateButtons() {
      // screenshot
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(0, 0, 32, 32)) {
        Bitmap = Resources.SnackBarScreenshot,
        Action = OnScreenshotButtonClick
      });

      // mute/unmute
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(32, 0, 36, 32)) {
        Bitmap = Resources.SnackBarMute,
        Action = OnMuteButtonClick
      });

      // options
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(124, 0, 36, 32)) {
        Bitmap = Resources.SnackBarOptions,
        Action = OnOptionsButtonClick
      });

      /* bind generic fill brushes */
      foreach (SnackBarButton button in this.buttons) {
        button.HoverFill = this.genericHoverButtonBrush;
        button.ActiveFill = this.genericActiveButtonBrush;
      }

      // close
      this.buttons.Add(new SnackBarButton(this.renderTarget, new RectangleF(160, 0, 32, 32)) {
        Bitmap = Resources.SnackBarClose,
        Action = OnCloseButtonClick
      });

      /* bind brushes */
      this.buttons.Last().HoverFill = this.closeHoverButtonBrush;
      this.buttons.Last().ActiveFill = this.closeActiveButtonBrush;

      // record/stop
      // This button is rendered last so that it's above the rest of buttons. This creates a nice style for the side
      // buttons (mute and options) that seem to fit perfectly with the elliptic shape
      this.buttons.Add(new SnackBarButton(this.renderTarget, new Ellipse(new Vector2(96, 16), 32, 32)) {
        Bitmap = Resources.SnackBarRecord,
        Action = OnRecordButtonClick
      });

      /* bind brushes */
      this.buttons.Last().NormalFill = this.recordNormalButtonBrush;
      this.buttons.Last().HoverFill = this.recordHoverButtonBrush;
      this.buttons.Last().ActiveFill = this.recordActiveButtonBrush;
    }

    #endregion

    #region Button events

    /// <summary>
    ///   Triggered when the Record button is clicked
    /// </summary>
    private void OnRecordButtonClick() { throw new NotImplementedException(); }

    /// <summary>
    ///   Triggered when the Close button is clicked
    /// </summary>
    private void OnCloseButtonClick() { throw new NotImplementedException(); }

    /// <summary>
    ///   Triggered when the Options button is clicked
    /// </summary>
    private static void OnOptionsButtonClick() {
      try {
        new OptionsWindow().Show();
      } catch (ApplicationException) { /* already open */ }
    }

    /// <summary>
    ///   Triggered when the Mute button is clicked
    /// </summary>
    private void OnMuteButtonClick() { throw new NotImplementedException(); }

    /// <summary>
    ///   Triggered when the Screenshot button is clicked
    /// </summary>
    private void OnScreenshotButtonClick() { throw new NotImplementedException(); }

    #endregion

    #region Colors

    /// <summary>
    ///   Snack bar background color
    /// </summary>
    private readonly Color backgroundColor = new Color(34, 34, 34);

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
    private readonly Color recordNormalColor = new Color(76, 29, 33);

    /// <summary>
    ///   Hover record button color
    /// </summary>
    private readonly Color recordHoverColor = new Color(109, 32, 38);

    /// <summary>
    ///   Active record button color
    /// </summary>
    private readonly Color recordActiveColor = new Color(138, 44, 52);

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

    #endregion

    #region Direct2D

    /// <summary>
    ///   Direct2D factory
    /// </summary>
    private readonly Factory factory;

    /// <summary>
    ///   Render target
    /// </summary>
    private readonly RenderTarget renderTarget;

    /// <summary>
    ///   Renders the snack bar UI
    /// </summary>
    private void Render() {
      this.renderTarget.BeginDraw();
      this.renderTarget.Clear(this.backgroundColor);

      foreach (SnackBarButton button in this.buttons) { button.Draw(); }

      this.renderTarget.Flush();
      this.renderTarget.EndDraw();
    }

    #endregion
  }
}