using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using Color = SharpDX.Color;
using Factory = SharpDX.DirectWrite.Factory;
using FontStyle = SharpDX.DirectWrite.FontStyle;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Displays a temporary informational tip beneath the mouse cursor
  /// </summary>
  internal class Tidbit : HudComponent<HudBlurredWrapperWindow> {
    /// <summary>
    ///   Global padding
    /// </summary>
    private const int Padding = 6;

    /// <summary>
    ///   Width of left border
    /// </summary>
    private const int LeftBorderWidth = 2;

    /// <summary>
    ///   Background color
    /// </summary>
    private readonly RawColor4 backgroundColor = new Color(0, 0, 0, 192);

    /// <summary>
    ///   Accent brush
    /// </summary>
    private Brush accentBrush;

    /// <summary>
    ///   Text content
    /// </summary>
    private string content;

    /// <summary>
    ///   DirectWrite factory
    /// </summary>
    private Factory directWriteFactory;

    /// <summary>
    ///   Direct2D bitmap representing the acctual tidbit icon
    /// </summary>
    private Bitmap iconBitmap;

    /// <summary>
    ///   Indicates whether to show an icon alongside the tidbit
    /// </summary>
    private bool showIcon = true;

    /// <summary>
    ///   Text brush instance
    /// </summary>
    private Brush textBrush;

    /// <summary>
    ///   Text format instance
    /// </summary>
    private TextFormat textFormat;

    /// <summary>
    ///   Text layout instance
    /// </summary>
    private TextLayout textLayout;

    /// <summary>
    ///   Icon size, in device-independent pixels
    /// </summary>
    private Size2F IconSize => ShowIcon ? (this.iconBitmap?.Size ?? Size2F.Empty) : Size2F.Empty;

    /// <summary>
    ///   Gets or sets the current tidbit status
    /// </summary>
    internal TidbitStatus Status { get; set; } = TidbitStatus.Information;

    /// <summary>
    ///   Gets or sets the tidbit position in the HUD
    /// </summary>
    internal Point Location {
      get => Bounds.Location;
      set => Bounds = new Rectangle(value, Bounds.Size);
    }

    /// <summary>
    ///   Gets the size of the tidbit
    /// </summary>
    internal Size Size => Bounds.Size;

    /// <summary>
    ///   When set to <c>false</c>, no icon is displayed on the tidbit
    /// </summary>
    internal bool ShowIcon {
      get => this.showIcon && this.iconBitmap != null;
      set {
        this.showIcon = value;
        RefreshIcon();
      }
    }

    /// <summary>
    ///   Specifies a custom icon for the tidbit
    /// </summary>
    internal System.Drawing.Bitmap CustomIcon {
      set {
        this.iconBitmap?.Dispose();
        this.iconBitmap = value?.ToDirect2DBitmap(RenderTarget);
        RefreshIcon();
      }
    }

    /// <summary>
    ///   Custom accent color for the tidbit
    /// </summary>
    internal System.Drawing.Color CustomAccent {
      set {
        this.accentBrush?.Dispose();
        this.accentBrush = new SolidColorBrush(RenderTarget, new Color(value.R, value.G, value.B, value.A));
      }
    }

    /// <summary>
    ///   Text to be displayed with the tidbit
    /// </summary>
    internal string Content {
      get => this.content;
      set {
        this.content = value;
        RefreshTextLayout();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="container">HUD container information</param>
    /// <param name="timeout">Lifetime for this tidbit</param>
    internal Tidbit(HudContainerInfo container, TimeSpan? timeout = null) : base(container) {
      container.TidbitManager.RegisterTidbit(this);
      Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
      Task.Delay(timeout ?? new TimeSpan(0, 0, 0, 2)).ContinueWith(t => dispatcher.Invoke(Dispose));
    }

    /// <inheritdoc />
    /// <summary>
    ///   Renders the tidbit
    /// </summary>
    protected override void Render() {
      RenderTarget.Clear(this.backgroundColor);

      // draw colored left border
      RenderTarget.FillRectangle(new RawRectangleF(0, 0, LeftBorderWidth, Bounds.Height), this.accentBrush);

      // draw icon
      if (ShowIcon) {
        RenderTarget.DrawBitmap(this.iconBitmap,
          new RawRectangleF(
            LeftBorderWidth + Padding,
            (Bounds.Height - IconSize.Height) / 2,
            IconSize.Width + Padding + LeftBorderWidth,
            (Bounds.Height - IconSize.Height) / 2 + IconSize.Height),
          1.0f,
          BitmapInterpolationMode.NearestNeighbor);
      }

      // draw text
      if (this.textLayout != null) {
        RenderTarget.DrawTextLayout(
          new RawVector2(LeftBorderWidth + IconSize.Width + 2 * Padding, Padding),
          this.textLayout,
          this.textBrush);
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates disposable rendering resources
    /// </summary>
    protected override void InitializeRenderingObjects() {
      base.InitializeRenderingObjects();

      // create text resources
      this.directWriteFactory = new Factory();
      this.textFormat = new TextFormat(this.directWriteFactory,
        SystemFonts.MessageBoxFont.Name,
        FontWeight.Normal,
        FontStyle.Normal,
        FontStretch.Normal,
        12.0f) {
        ParagraphAlignment = ParagraphAlignment.Near,
        TextAlignment = TextAlignment.Leading
      };

      this.textBrush = new SolidColorBrush(RenderTarget, new RawColor4(1.0f, 1.0f, 1.0f, 0.75f));

      RefreshIcon();
      RefreshAccentBrush();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Disposes all rendering resources
    /// </summary>
    protected override void DestroyRenderingObjects() {
      // release icon bitmap
      this.iconBitmap?.Dispose();

      // release color brushes
      this.accentBrush?.Dispose();

      // release text facilities
      this.textBrush.Dispose();
      this.textLayout.Dispose();
      this.textFormat.Dispose();
      this.directWriteFactory.Dispose();

      this.iconBitmap = null;
      this.accentBrush = null;
      this.textBrush = null;
      this.textLayout = null;
      this.textFormat = null;
      this.directWriteFactory = null;

      base.DestroyRenderingObjects();
    }

    /// <summary>
    ///   Creates a new text layout so that the tidbit content gets updated.
    /// </summary>
    private void RefreshTextLayout() {
      if (this.directWriteFactory != null && this.textFormat != null) {
        // dispose existing layout
        this.textLayout?.Dispose();

        // create a new layout
        (string strippedString, Action<TextLayout> formatter) = DirectWriteFormatHelper.CreateFormatter(this.content);

        formatter(this.textLayout = new TextLayout(this.directWriteFactory,
          strippedString,
          this.textFormat,
          256,
          32));

        // resize the wrapper
        Bounds = new Rectangle(
          Bounds.Location,
          new Size(
            (int) this.textLayout.Metrics.Width + 4 * Padding + LeftBorderWidth + (int) IconSize.Width,
            (int) this.textLayout.Metrics.Height + 2 * Padding));
      }
    }

    /// <summary>
    ///   Refresh the tidbit icon
    /// </summary>
    private void RefreshIcon() {
      if (this.showIcon && !(RenderTarget?.IsDisposed ?? true) && this.iconBitmap == null) {
        switch (Status) {
          case TidbitStatus.Error:
            this.iconBitmap = Resources.TidbitError.ToDirect2DBitmap(RenderTarget);
            break;
          case TidbitStatus.Warning:
            this.iconBitmap = Resources.TidbitWarning.ToDirect2DBitmap(RenderTarget);
            break;
          case TidbitStatus.Information:
            this.iconBitmap = Resources.TidbitInfo.ToDirect2DBitmap(RenderTarget);
            break;
          case TidbitStatus.Success:
            this.iconBitmap = Resources.TidbitOk.ToDirect2DBitmap(RenderTarget);
            break;
        }
      }
    }

    /// <summary>
    ///   Updates the accent brush
    /// </summary>
    private void RefreshAccentBrush() {
      if (!(RenderTarget?.IsDisposed ?? true) && this.accentBrush == null) {
        switch (Status) {
          case TidbitStatus.Error:
            this.accentBrush = new SolidColorBrush(RenderTarget, new RawColor4(1.0f, 0.0f, 0.0f, 1.0f));
            break;
          case TidbitStatus.Warning:
            this.accentBrush = new SolidColorBrush(RenderTarget, new RawColor4(1.0f, 1.0f, 0.0f, 1.0f));
            break;
          case TidbitStatus.Information:
            this.accentBrush = new SolidColorBrush(RenderTarget, new RawColor4(0.0f, 0.5f, 1.0f, 1.0f));
            break;
          case TidbitStatus.Success:
            this.accentBrush = new SolidColorBrush(RenderTarget, new RawColor4(0.0f, 1.0f, 0.5f, 1.0f));
            break;
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources
    /// </summary>
    public override void Dispose() {
      Container.TidbitManager.UnregisterTidbit(this);
      base.Dispose();
    }
  }
}