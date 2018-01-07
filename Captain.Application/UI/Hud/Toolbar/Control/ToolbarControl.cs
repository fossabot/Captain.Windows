using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

namespace Captain.Application {
  /// <summary>
  ///   Abstracts common behaviour for toolbar components
  /// </summary>
  internal abstract class ToolbarControl : IDisposable {
    /// <summary>
    ///   Parent toolbar
    /// </summary>
    protected Toolbar Toolbar { get; }

    /// <summary>
    ///   Control location
    /// </summary>
    /// <remarks>
    ///   The Z value does not affect appearance but is instead interpreted relatively and specifies the order in which
    ///   controls should be rendered
    /// </remarks>
    internal Vector3 Location { get; set; }

    /// <summary>
    ///   Control size
    /// </summary>
    internal Vector2 Size { get; set; }

    /// <summary>
    ///   Control gravity
    /// </summary>
    internal ToolbarControlGravity Gravity { get; set; } = ToolbarControlGravity.Near;

    /// <summary>
    ///   Control state
    /// </summary>
    internal ToolbarControlState State { get; set; } = ToolbarControlState.None;

    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="toolbar">Parent toolbar</param>
    internal ToolbarControl(Toolbar toolbar) => Toolbar = toolbar;

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources used by the control
    /// </summary>
    public virtual void Dispose() { }

    /// <summary>
    ///   Renders the control
    /// </summary>
    /// <param name="target">Render target</param>
    internal abstract void Render(RenderTarget target);

    /// <summary>
    ///   Performs hit testing against a point
    /// </summary>
    /// <param name="point">Point to be tested against</param>
    /// <returns>Whether the point belongs to the control region or not</returns>
    internal virtual bool HitTest(Vector2 point) =>
      point.X >= Location.X &&
      point.Y >= Location.Y &&
      point.X <= Location.X + Size.X &&
      point.Y <= Location.Y + Size.Y;

    /// <summary>
    ///   Refreshes the control
    /// </summary>
    internal virtual void Refresh() => Toolbar.Refresh();
  }
}