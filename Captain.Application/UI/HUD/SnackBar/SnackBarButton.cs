using System;
using System.Drawing.Imaging;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using PixelFormat = System.Drawing.Imaging.PixelFormat;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a snack bar button
  /// </summary>
  internal sealed class SnackBarButton : IDisposable {
    /// <summary>
    ///   Button shape geometry
    /// </summary>
    private readonly Geometry geometry;

    /// <summary>
    ///   Render target
    /// </summary>
    private readonly RenderTarget render;

    /// <summary>
    ///   Shape instance
    /// </summary>
    private readonly object shape;

    /// <summary>
    ///   Direct2D bitmap
    /// </summary>
    private Bitmap bitmap;

    /// <summary>
    ///   Whether or not the button will be rendered as hovered
    /// </summary>
    internal bool Hovered { get; set; }

    /// <summary>
    ///   Whether or not the button will be rendered as active
    /// </summary>
    internal bool Active { get; set; }

    /// <summary>
    ///   Normal fill brush
    /// </summary>
    internal Brush NormalFill { get; set; }

    /// <summary>
    ///   Hover fill brush
    /// </summary>
    internal Brush HoverFill { get; set; }

    /// <summary>
    ///   Active fill brush
    /// </summary>
    internal Brush ActiveFill { get; set; }

    /// <summary>
    ///   Click handler
    /// </summary>
    internal System.Action Action { get; set; }

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
        this.bitmap = new Bitmap(this.render, new Size2(value.Width, value.Height), stream, data.Stride, props);
        stream.Dispose();
        value.UnlockBits(data);
      }
    }

    /// <summary>
    ///   Creates a new snackbar button
    /// </summary>
    /// <param name="render">Direct2D render target</param>
    /// <param name="shape">Shape instance</param>
    internal SnackBarButton(RenderTarget render, object shape) {
      this.render = render;
      this.shape = shape;

      // instantiate geometry from shape
      switch (this.shape) {
        case RectangleF rect:
          this.geometry = new RectangleGeometry(render.Factory, rect);
          break;

        case Ellipse ellip:
          this.geometry = new EllipseGeometry(render.Factory, ellip);
          break;
      }
    }

    /// <summary>
    ///   Tests whether or not a point is contained in the button shape
    /// </summary>
    /// <param name="x">X coordinate</param>
    /// <param name="y">Y coordinate</param>
    /// <returns>Whether the point is contained or not</returns>
    internal bool HitTest(int x, int y) => this.geometry.FillContainsPoint(new RawVector2(x, y));

    /// <summary>
    ///   Performs the button action
    /// </summary>
    internal void PerformClick() => Action?.Invoke();

    /// <summary>
    ///   Draws the button in its render target
    /// </summary>
    internal void Draw() {
      Brush brush = Active ? ActiveFill : Hovered ? HoverFill : NormalFill;
      Vector2? center = null;

      // obtain the center point for the button
      switch (this.shape) {
        case RectangleF rect:
          if (brush != null) { this.render.FillRectangle(rect, brush); }
          center = rect.Center;
          break;

        case Ellipse ellip:
          if (brush != null) { this.render.FillEllipse(ellip, brush); }
          center = ellip.Point;
          break;
      }

      if (center.HasValue && (this.bitmap != null)) {
        // draw the bitmap associated to this button in its center
        this.render.DrawBitmap(this.bitmap,
          new RawRectangleF(
            center.Value.X - (this.bitmap.Size.Width / 2),
            center.Value.Y - (this.bitmap.Size.Height / 2),
            center.Value.X + (this.bitmap.Size.Width / 2),
            center.Value.Y + (this.bitmap.Size.Height / 2)),
          Hovered || Active ? 1 : 0.85f,
          BitmapInterpolationMode.Linear,
          new RawRectangleF(0, 0, this.bitmap.Size.Width, this.bitmap.Size.Height));
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      this.geometry?.Dispose();
      this.render?.Dispose();
      this.bitmap?.Dispose();
      NormalFill?.Dispose();
      HoverFill?.Dispose();
      ActiveFill?.Dispose();
    }
  }
}