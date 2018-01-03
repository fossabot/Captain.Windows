using System;
using System.Drawing.Imaging;
using System.Windows.Forms;
using Captain.Common;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using BitmapData = System.Drawing.Imaging.BitmapData;
using Brush = SharpDX.Direct2D1.Brush;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides access to a minimal tooling when capturing screen recordings
  /// </summary>
  internal sealed class Tidbit : IDisposable {
    /// <summary>
    ///   GDI-compatible bitmap to be displayed alongside the button
    /// </summary>
    internal System.Drawing.Bitmap Bitmap {
      set {
        BitmapData data = value.LockBits(new Rectangle(0, 0, value.Width, value.Height),
          ImageLockMode.ReadOnly,
          PixelFormat.Format32bppPArgb);

        // copy GDI bitmap data to Direct2D one
        var stream = new DataStream(data.Scan0, data.Stride * data.Height, true, false);
        var format = new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied);
        var props = new BitmapProperties(format);

        // create Direct2D bitmap and release resources
        this.bitmap = new Bitmap(this.renderTarget, new Size2(value.Width, value.Height), stream, data.Stride, props);
        stream.Dispose();
        value.UnlockBits(data);
      }
    }

    /// <summary>
    ///   Generic, common constructor
    /// </summary>
    private Tidbit() { }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a snack bar instance for an existing wrapper window
    /// </summary>
    /// <param name="wrapper">Wrapper instance</param>
    /// <param name="status">Status level</param>
    /// <param name="text">Instruction text</param>
    /// <param name="progress">Progress value</param>
    internal Tidbit(Control wrapper, TidbitStatus status, string text, double? progress = null) : this() {
      this.status = status;
      this.text = text;
      this.progress = progress;

      CreateResources();

      this.wrapper = wrapper;
      this.wrapper.Width = (int) this.textLayout.Metrics.Width + 46;
      this.wrapper.Height = (int) this.textLayout.Metrics.Height + 12;

      this.textLayout.MaxWidth = this.textLayout.Metrics.Width + 26;
      this.textLayout.MaxHeight = this.textLayout.Metrics.Height + 12;

      this.renderTarget = new WindowRenderTarget(
        new Factory(FactoryType.MultiThreaded, DebugLevel.Information),
        new RenderTargetProperties(new SharpDX.Direct2D1.PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)),
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
        12.0f) {
        ParagraphAlignment = ParagraphAlignment.Center,
        TextAlignment = TextAlignment.Center
      };
      this.textLayout = new TextLayout(this.dwFactory, this.text, this.textFormat, 128, 16.0f);
    }

    /// <summary>
    ///   Creates brushes.
    /// </summary>
    private void CreateBrushes() {
      this.textBrush = new SolidColorBrush(this.renderTarget, new RawColor4(1.0f, 1.0f, 1.0f, 0.6666f));

      switch (this.status) {
        case TidbitStatus.Success:
          this.accentBrush = new SolidColorBrush(this.renderTarget, new RawColor4(0.0f, 1.0f, 0.5f, 1.0f));
          Bitmap = Resources.TidbitOk;
          break;

        case TidbitStatus.Information:
          this.accentBrush = new SolidColorBrush(this.renderTarget, new RawColor4(0.0f, 0.5f, 1.0f, 1.0f));
          Bitmap = Resources.TidbitInfo;
          break;

        case TidbitStatus.Warning:
          this.accentBrush = new SolidColorBrush(this.renderTarget, new RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
          Bitmap = Resources.TidbitWarning;
          break;

        case TidbitStatus.Error:
          this.accentBrush = new SolidColorBrush(this.renderTarget, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
          Bitmap = Resources.TidbitError;
          break;
      }
    }

    #endregion

    #region Colors

    /// <summary>
    ///   Snack bar background color
    /// </summary>
    private readonly Color backgroundColor = new Color(0, 0, 0, 192);

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

    /// <summary>
    ///   Accent brush instance.
    /// </summary>
    private Brush accentBrush;

    /// <summary>
    ///   Tidbit bitmap.
    /// </summary>
    private Bitmap bitmap;

    #endregion

    #region Private members

    /// <summary>
    ///   Status level
    /// </summary>
    private readonly TidbitStatus status;

    /// <summary>
    ///   Instruction text.
    /// </summary>
    private readonly string text;

    /// <summary>
    ///   Progress value.
    /// </summary>
    private double? progress;

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
      this.renderTarget.DrawRectangle(new RawRectangleF(0, 0, 2, this.wrapper.Height), this.accentBrush);
      this.renderTarget.DrawBitmap(this.bitmap, new RawRectangleF(10, 6, 26, 22), 1.0f, BitmapInterpolationMode.Linear);
      this.renderTarget.DrawTextLayout(new RawVector2(18, 0), this.textLayout, this.textBrush);
      this.renderTarget.EndDraw();
    }

    #endregion
  }
}