using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using Point = System.Drawing.Point;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides access to a minimal tooling when capturing screen recordings
  /// </summary>
  internal sealed class InstructionOverlay : IDisposable {
    /// <summary>
    ///   Generic, common constructor
    /// </summary>
    private InstructionOverlay() { }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a snack bar instance for an existing wrapper window
    /// </summary>
    /// <param name="wrapper">Wrapper instance</param>
    /// <param name="instructionText">Instruction text.</param>
    internal InstructionOverlay(Control wrapper, string instructionText) : this() {
      this.instructionText = instructionText;

      CreateResources();

      this.wrapper = wrapper;
      this.wrapper.Width = (int) this.textLayout.Metrics.Width + 64;
      this.wrapper.Height = (int) this.textLayout.Metrics.Height + 24;

      var screen = Screen.FromPoint(Control.MousePosition);
      this.wrapper.Location = new Point(screen.Bounds.X + (screen.Bounds.Width - this.wrapper.Width) / 2,
        screen.Bounds.Y + (screen.Bounds.Height - this.wrapper.Height) / 2);

      this.textLayout.MaxWidth = this.textLayout.Metrics.Width + 64;
      this.textLayout.MaxHeight = this.textLayout.Metrics.Height + 24;

      this.renderTarget = new WindowRenderTarget(
        new Factory(FactoryType.MultiThreaded, DebugLevel.Information),
        new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)),
        new HwndRenderTargetProperties {
          PixelSize = new Size2(this.wrapper.Width, this.wrapper.Height),
          Hwnd = this.wrapper.Handle
        });

      CreateBrushes();

      // bind events
      this.wrapper.Paint += OnWrapperOnPaint;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      // unbind wrapper events
      this.wrapper.Paint -= OnWrapperOnPaint;

      // release resources
      this.dwFactory?.Dispose();
      this.textLayout?.Dispose();
      this.textFormat?.Dispose();
      this.textBrush?.Dispose();

      // release resources
      this.renderTarget?.Dispose();
    }

    #region Wrapper events

    /// <summary>
    ///   Triggered when the wrapper window gets painted
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnWrapperOnPaint(object sender, PaintEventArgs eventArgs) => Render();

    #endregion

    #region UI layout creation

    /// <summary>
    ///   Creates disposable resources.
    /// </summary>
    private void CreateResources() {
      this.dwFactory = new SharpDX.DirectWrite.Factory();
      this.textFormat = new TextFormat(this.dwFactory,
        @"Segoe UI",
        FontWeight.Normal,
        FontStyle.Normal,
        FontStretch.Normal,
        16.0f) {
        ParagraphAlignment = ParagraphAlignment.Center,
        TextAlignment = TextAlignment.Center
      };
      this.textLayout = new TextLayout(this.dwFactory, this.instructionText, this.textFormat, 768.0f, 96.0f);
    }

    /// <summary>
    ///   Creates brushes.
    /// </summary>
    private void CreateBrushes() =>
      this.textBrush = new SolidColorBrush(this.renderTarget, new RawColor4(1.0f, 1.0f, 1.0f, 0.6666f));

    #endregion

    #region Colors

    /// <summary>
    ///   Snack bar background color
    /// </summary>
    private readonly Color backgroundColor = new Color(0, 0, 0, 127);

    #endregion

    #region Disposable rendering resources

    /// <summary>
    ///   DirectWrite factory.
    /// </summary>
    private SharpDX.DirectWrite.Factory dwFactory;

    /// <summary>
    ///   Text layout instance.
    /// </summary>
    private TextLayout textLayout;

    /// <summary>
    ///   Text format instance.
    /// </summary>
    private TextFormat textFormat;

    /// <summary>
    ///   Text brush instance.
    /// </summary>
    private Brush textBrush;

    #endregion

    #region Private members

    /// <summary>
    ///   Instruction text.
    /// </summary>
    private string instructionText;

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
      this.renderTarget.DrawTextLayout(new RawVector2(), this.textLayout, this.textBrush);
      this.renderTarget.EndDraw();
    }

    #endregion
  }
}