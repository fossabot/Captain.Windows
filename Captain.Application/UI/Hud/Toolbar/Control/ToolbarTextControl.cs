using System;
using System.Drawing;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Brush = SharpDX.Direct2D1.Brush;
using Factory = SharpDX.DirectWrite.Factory;
using FontStyle = SharpDX.DirectWrite.FontStyle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides simple text rendering as a toolbar control
  /// </summary>
  internal class ToolbarTextControl : ToolbarControl {
    /// <summary>
    ///   Textual content
    /// </summary>
    private string content;

    /// <summary>
    ///   Text brush
    /// </summary>
    private Brush brush;

    /// <summary>
    ///   Text format
    /// </summary>
    private TextFormat textFormat;

    /// <summary>
    ///   Text layout
    /// </summary>
    private TextLayout textLayout;

    /// <summary>
    ///   DirectWrite factory
    /// </summary>
    private Factory directWriteFactory;

    /// <summary>
    ///   Gets or sets the textual content for the control
    /// </summary>
    internal string Content {
      get => this.content;
      set {
        this.content = value;
        RefreshTextLayout();
      }
    }

    /// <summary>
    ///   Updates the text layout for the control
    /// </summary>
    private void RefreshTextLayout() {
      if (this.directWriteFactory?.IsDisposed ?? true) { this.directWriteFactory = new Factory(); }
      if (this.textFormat?.IsDisposed ?? true) {
        this.textFormat = new TextFormat(this.directWriteFactory,
          SystemFonts.MessageBoxFont.Name,
          FontWeight.Normal,
          FontStyle.Normal,
          FontStretch.Normal,
          12.0f) {
          ParagraphAlignment = ParagraphAlignment.Center,
          TextAlignment = TextAlignment.Center
        };
      }

      this.textLayout?.Dispose();
      this.textLayout = new TextLayout(this.directWriteFactory, this.content, this.textFormat, Size.X, Size.Y);

      if (this.brush?.IsDisposed ?? true) {
        this.brush = new SolidColorBrush(Toolbar.ToolbarRenderTarget, new RawColor4(1, 1, 1, 0.75f));
      }
    }

    public override void Dispose() {
      this.brush?.Dispose();
      this.directWriteFactory?.Dispose();
      this.textFormat?.Dispose();
      this.textLayout?.Dispose();

      this.brush = null;
      this.directWriteFactory = null;
      this.textFormat = null;
      this.content = null;

      base.Dispose();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="toolbar">Parent toolbar</param>
    public ToolbarTextControl(Toolbar toolbar) : base(toolbar) { }

    /// <inheritdoc />
    /// <summary>
    ///   Refreshes the control
    /// </summary>
    internal override void Refresh() {
      RefreshTextLayout();
      base.Refresh();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Renders the control
    /// </summary>
    /// <param name="target">Render target</param>
    internal override void Render(RenderTarget target) =>
      target.DrawTextLayout(new RawVector2(Location.X, Location.Y), this.textLayout, this.brush);
  }
}