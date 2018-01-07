using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <summary>
  ///   Control interface for recording sessions
  /// </summary>
  internal class Toolbar : HudComponent<HudToolbarWrapperWindow> {
    /// <summary>
    ///   Toolbar height
    /// </summary>
    private const int toolbarHeight = 32;

    /// <summary>
    ///   Exposes the toolbar <see cref="RenderTarget" /> to child controls
    /// </summary>
    internal RenderTarget ToolbarRenderTarget => RenderTarget;

    /// <summary>
    ///   Toolbar controls
    /// </summary>
    internal OrderedDictionary Controls { get; } = new OrderedDictionary();

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="container">HUD container information</param>
    public Toolbar(HudContainerInfo container) : base(container) {
      if (DesktopWrapper != null) {
        DesktopWrapper.PassThrough = false;
        DesktopWrapper.Resizable = false;

        // bind mouse event handlers
        DesktopWrapper.MouseMove += OnToolbarMouseMove;
        DesktopWrapper.MouseDown += OnToolbarMouseDown;
        DesktopWrapper.MouseUp += OnToolbarMouseUp;
        DesktopWrapper.MouseLeave += delegate {
          foreach (ToolbarControl control in Controls.Values) { control.State &= ~ToolbarControlState.Hovered; }
        };
      }

      Bounds = new Rectangle(320, 320, 256, toolbarHeight);
    }

    private void OnToolbarMouseUp(object sender, MouseEventArgs e) {
      foreach (ToolbarControl control in Controls.Values.Cast<ToolbarControl>()
        .Where(c => (c.State & ToolbarControlState.Active) != 0)) {
        control.State &= ~ToolbarControlState.Active;
        control.Refresh();
      }
    }

    private void OnToolbarMouseDown(object sender, MouseEventArgs e) {
      IEnumerable<ToolbarControl> controls = Controls.Values.Cast<ToolbarControl>()
        .Where(c => c.HitTest(new Vector2(e.X, e.Y)))
        .OrderBy(c => c.Location.Z);

      if (controls.Any()) {
        ToolbarControl control = controls.First();
        control.State |= ToolbarControlState.Active;
        control.Refresh();
      }
    }

    private void OnToolbarMouseMove(object sender, MouseEventArgs e) {
      var vec = new Vector2(e.X, e.Y);
      foreach (ToolbarControl control in Controls.Values) {
        control.State &= ~ToolbarControlState.Hovered;
        if (control.HitTest(vec)) { control.State |= ToolbarControlState.Hovered; }
      }

      Refresh();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates disposable rendering resources
    /// </summary>
    protected override void InitializeRenderingObjects() {
      base.InitializeRenderingObjects();

      // create controls
      Controls["timerText"] = new ToolbarTextControl(this) {
        Content = "00:00",
        Size = new Vector2(2 * toolbarHeight, toolbarHeight)
      };

      Controls["optionsButton"] = new ToolbarButton(this) {
        Bitmap = Resources.SnackBarOptions.ToDirect2DBitmap(RenderTarget),
        Size = new Vector2(toolbarHeight, toolbarHeight)
      };

      Dispatcher dispatcher = Dispatcher.CurrentDispatcher;
      Task.Delay(new TimeSpan(0, 0, 0, 2)).ContinueWith(t => dispatcher.Invoke(() => {
        ((ToolbarPrimaryButton) Controls["primaryButton"]).Bitmap = Resources.SnackBarStop.ToDirect2DBitmap(RenderTarget);
      }));

      Controls["primaryButton"] = new ToolbarPrimaryButton(this) {
        Bitmap = Resources.SnackBarRecord.ToDirect2DBitmap(RenderTarget),
        Location = new Vector3((Bounds.Width - 108) / 2, -16, 1),
        Size = new Vector2(64, 64),
        Gravity = ToolbarControlGravity.Zero
      };

      Controls["closeButton"] = new ToolbarButton(this) {
        Bitmap = Resources.SnackBarClose.ToDirect2DBitmap(RenderTarget),
        Gravity = ToolbarControlGravity.Far,
        Size = new Vector2(toolbarHeight, toolbarHeight),
        HoveredBackgroundColor = new Color(232, 17, 35, 255),
        ActiveBackgroundColor = new Color(231, 16, 34, 153)
      };

      Controls["regionButton"] = new ToolbarButton(this) {
        Bitmap = Resources.ClipperPickRegion.ToDirect2DBitmap(RenderTarget),
        Gravity = ToolbarControlGravity.Far,
        Size = new Vector2(toolbarHeight, toolbarHeight)
      };

      Controls["microphoneButton"] = new ToolbarButton(this) {
        Bitmap = Resources.SnackBarUnmute.ToDirect2DBitmap(RenderTarget),
        Gravity = ToolbarControlGravity.Far,
        Size = new Vector2(toolbarHeight, toolbarHeight)
      };
    }

    /// <inheritdoc />
    /// <summary>
    ///   Disposes all rendering resources
    /// </summary>
    protected override void DestroyRenderingObjects() {
      // dispose all controls
      foreach (KeyValuePair<string, ToolbarControl> nameControlPair in Controls) { nameControlPair.Value?.Dispose(); }
      Controls.Clear();

      // dispose brushes
      base.DestroyRenderingObjects();
    }

    /// <summary>
    ///   Calculates the effective location for a specific control based on its gravity setting
    /// </summary>
    /// <param name="control">The toolbar control</param>
    /// <param name="idx">Control index</param>
    /// <returns>A 3D vector containing the control position</returns>
    /// <remarks>The Y coordinate and Z order are not changed by this method</remarks>
    private Vector3 CalculateEffectiveLocation(ToolbarControl control, int idx) {
      Vector3 location = control.Location;

      switch (control.Gravity) {
        case ToolbarControlGravity.Near:
          location.X = Controls.Values.Cast<ToolbarControl>()
            .Where((c, i) => c.Gravity == ToolbarControlGravity.Near && i >= idx)
            .Sum(c => c.Size.X);
          break;

        case ToolbarControlGravity.Far:
          location.X = Bounds.Width -
                       Controls.Values.Cast<ToolbarControl>()
                         .Where((c, i) => c.Gravity == ToolbarControlGravity.Far && i <= idx)
                         .Sum(c => c.Size.X);
          break;
      }

      return location;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs UI rendering
    /// </summary>
    protected override void Render() {
      RenderTarget.AntialiasMode = AntialiasMode.PerPrimitive;
      RenderTarget.Clear(new RawColor4(0, 0, 0, 0.5f));

      // render the controls following their Z orders
      IEnumerable<ToolbarControl> controls = Controls.Values.Cast<ToolbarControl>()
        .OrderByDescending(c => c.Location.Z)
        .Select((c, i) => {
          // enforce control gravity
          c.Location = CalculateEffectiveLocation(c, i);
          c.Refresh();
          return c;
        });

      foreach (ToolbarControl control in controls) { control.Render(RenderTarget); }
    }

    /// <summary>
    ///   Refreshes toolbar graphics
    /// </summary>
    internal void Refresh() => DesktopWrapper?.Invalidate();
  }
}