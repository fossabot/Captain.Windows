using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;
using Factory = SharpDX.Direct2D1.Factory;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Base class for all HUD components
  /// </summary>
  /// <typeparam name="T">Type for the desktop wrapper of this component</typeparam>
  internal abstract class HudComponent<T> : IDisposable where T : Control, new() {
    /// <summary>
    ///   Component bounds
    /// </summary>
    private Rectangle bounds = new Rectangle(-0x7FFF, -0x7FFF, 0, 0);

    /// <summary>
    ///   Whether or not this component is visible
    /// </summary>
    private bool visible = true;

    /// <summary>
    ///   Determines whether or not this component will be rendered
    /// </summary>
    internal bool Visible {
      get => DesktopWrapper?.Visible ?? this.visible;
      set {
        if (DesktopWrapper != null) { DesktopWrapper.Visible = value; }
        this.visible = value;
      }
    }

    /// <summary>
    ///   Wrapper window for displaying this component on the desktop
    /// </summary>
    protected T DesktopWrapper { get; private set; }

    /// <summary>
    ///   Rendering target for this component
    /// </summary>
    protected RenderTarget RenderTarget { get; set; }

    /// <summary>
    ///   Holds information about the underlying HUD container
    /// </summary>
    protected HudContainerInfo Container { get; set; }

    /// <summary>
    ///   Determines whether this HUD component has been yet disposed
    /// </summary>
    internal bool Disposed { get; private set; }

    /// <summary>
    ///   Represents the location and size of the component on the virtual desktop
    /// </summary>
    protected Rectangle Bounds {
      get => DesktopWrapper?.Bounds ?? this.bounds;
      set {
        if (DesktopWrapper != null) {
          DesktopWrapper.Bounds = value;
        }
        this.bounds = value;
      }
    }

    /// <summary>
    ///   Base constructor for the HUD component
    /// </summary>
    /// <param name="container">Container information</param>
    internal HudComponent(HudContainerInfo container) {
      Container = container;

      if (container.ContainerType == HudContainerType.Desktop) {
        // create desktop wrapper
        DesktopWrapper = new T { Visible = Visible };
        DesktopWrapper.Bounds = Bounds;

        // create render target
        RenderTarget = new WindowRenderTarget(
          new Factory(FactoryType.MultiThreaded, DebugLevel.Information),
          new RenderTargetProperties(new PixelFormat(Format.B8G8R8A8_UNorm, AlphaMode.Premultiplied)),
          new HwndRenderTargetProperties {
            PixelSize = new Size2(Bounds.Width, Bounds.Height),
            Hwnd = DesktopWrapper.Handle
          });
        InitializeRenderingObjects();

        void ResizeDelegate(object sender, EventArgs eventArgs) {
          ((WindowRenderTarget) RenderTarget).Resize(new Size2(DesktopWrapper.Width, DesktopWrapper.Height));
          DesktopWrapper.Invalidate();
        }
        
        DesktopWrapper.Resize += ResizeDelegate;
        DesktopWrapper.Paint += delegate {
          RenderTarget.AntialiasMode = AntialiasMode.Aliased;
          RenderTarget.BeginDraw();
          Render();
          RenderTarget.TryEndDraw(out _, out _);
        };
      } else { throw new NotImplementedException(); }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources
    /// </summary>
    public virtual void Dispose() {
      Disposed = true;
      Visible = false;

      DesktopWrapper?.Dispose();
      RenderTarget?.Dispose();
      DestroyRenderingObjects();

      DesktopWrapper = null;
      RenderTarget = null;
    }

    /// <summary>
    ///   Performs UI rendering
    /// </summary>
    protected abstract void Render();

    /// <summary>
    ///   Creates disposable rendering resources
    /// </summary>
    protected virtual void InitializeRenderingObjects() { }

    /// <summary>
    ///   Disposes all rendering resources
    /// </summary>
    protected virtual void DestroyRenderingObjects() { }
  }
}