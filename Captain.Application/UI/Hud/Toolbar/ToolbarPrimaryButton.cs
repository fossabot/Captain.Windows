using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct2D1.Effects;
using SharpDX.Mathematics.Interop;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using Color = System.Drawing.Color;
using Image = SharpDX.Direct2D1.Image;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Primary toolbar button
  /// </summary>
  internal class ToolbarPrimaryButton : ToolbarButton<Geometry> {
    /// <summary>
    ///   Whether or not to invert the bitmap for better contrast
    /// </summary>
    private bool invert;

    /// <summary>
    ///   Button back brush
    /// </summary>
    private Brush backBrush;

    /// <summary>
    ///   Direct2D image
    /// </summary>
    private Image image;

    /// <summary>
    ///   Image size
    /// </summary>
    private Size2F imageSize;

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="toolbar">Parent toolbar</param>
    public ToolbarPrimaryButton(Toolbar toolbar) : base(toolbar) {
      if (StyleHelper.GetAccentColor() is Color color) {
        this.backBrush = new SolidColorBrush(toolbar.ToolbarRenderTarget, new SharpDX.Color(color.R, color.G, color.B));
        this.invert = color.ToYiq() > 0x7F;
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Refreshes the control
    /// </summary>
    internal override void Refresh() {
      if (this.invert && Bitmap != null) {
        var effect = new Effect(Toolbar.ToolbarRenderTarget.QueryInterface<DeviceContext>(), Effect.Invert);
        effect.SetInput(0, Bitmap, true);
        this.image = effect.Output;
        this.imageSize = Bitmap.Size;
        Bitmap?.Dispose();
        Bitmap = null;
      }

      base.Refresh();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a geometry object for this control
    /// </summary>
    /// <returns>A new geometry object</returns>
    protected override Geometry RefreshGeometry() {
      var geometry = new PathGeometry(Toolbar.ToolbarRenderTarget.Factory);
      GeometrySink sink = geometry.Open();

      sink.SetFillMode(FillMode.Winding);
      sink.BeginFigure(new RawVector2(Location.X, Location.Y + Size.Y / 2), FigureBegin.Filled);
      sink.AddLines(new[] {
        new RawVector2(Location.X + Size.X / 2, Location.Y),
        new RawVector2(Location.X + Size.X, Location.Y + Size.Y / 2),
        new RawVector2(Location.X + Size.X / 2, Location.Y + Size.Y)
      });

      sink.EndFigure(FigureEnd.Closed);
      sink.Close();
      return geometry;
    }

    internal override void Render(RenderTarget target) {
      target.FillGeometry(Geometry.Value, this.backBrush);
      base.Render(target);

      if (this.image != null) {
        target.QueryInterface<DeviceContext>()
          .DrawImage(this.image,
            new RawVector2(
              Location.X + Size.X / 2 - this.imageSize.Width / 2,
              Location.Y + Size.Y / 2 - this.imageSize.Height / 2));
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources used by the control
    /// </summary>
    public override void Dispose() {
      this.backBrush?.Dispose();
      this.backBrush = null;
      base.Dispose();
    }
  }
}