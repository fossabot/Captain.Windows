using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using static Captain.Application.Application;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Brush = SharpDX.Direct2D1.Brush;
using Task = System.Threading.Tasks.Task;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides the user with an interface to select a screen region
  /// </summary>
  internal class Clipper : HudComponent<HudCommonWrapperWindow> {
    /// <summary>
    ///   Minimum selection width
    /// </summary>
    private const int MinimumWidth = 38;

    /// <summary>
    ///   Minimum selection height
    /// </summary>
    private const int MinimumHeight = 38;

    /// <summary>
    ///   Whether or not the Alt key is held
    /// </summary>
    private bool altDown = (Control.ModifierKeys & Keys.Alt) != 0;

    /// <summary>
    ///   Bitmap containing the locked UI corner images
    /// </summary>
    private Bitmap cornerBitmap;

    /// <summary>
    ///   Current corner padding
    /// </summary>
    private Padding cornerPadding;

    /// <summary>
    ///   Initial coordinates selected by the user on Pick selection mode
    /// </summary>
    private Point? initialPosition;

    /// <summary>
    ///   Border brush for the inner selection rectangle on locked clipper
    /// </summary>
    private Brush innerBorderBrush;

    /// <summary>
    ///   Border brush for the outer selection rectangle
    /// </summary>
    private Brush outerBorderBrush;

    /// <summary>
    ///   Informational tidbit
    /// </summary>
    private Tidbit tidbit;

    /// <summary>
    ///   Mutex used for returning from Unlock() immediately after Lock() is called
    /// </summary>
    private Mutex uiLockMutex;

    /// <summary>
    ///   Handle of the selected window
    /// </summary>
    private IntPtr windowHandle;

    /// <summary>
    ///   Gets or sets the current selection mode for the clipper UI
    /// </summary>
    internal ClippingMode Mode { get; private set; }

    /// <summary>
    ///   Whether or not the clipper UI is locked
    /// </summary>
    internal bool Locked { get; private set; } = true;

    /// <summary>
    ///   Exposes the clipper UI bounds
    /// </summary>
    internal Rectangle Area {
      get => Rectangle.Inflate(Bounds,
        (-this.cornerPadding.Left - this.cornerPadding.Right) / 2,
        (-this.cornerPadding.Top - this.cornerPadding.Bottom) / 2);
      set => Bounds = Rectangle.Inflate(value,
        (this.cornerPadding.Left + this.cornerPadding.Right) / 2,
        (this.cornerPadding.Top + this.cornerPadding.Bottom) / 2);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="container">HUD container</param>
    public Clipper(HudContainerInfo container) : base(container) { }

    /// <summary>
    ///   Updates the clipper UI layout based on the current selection mode
    /// </summary>
    private void RefreshLayout() {
      Rectangle oldArea = Area;

      switch (Mode) {
        case ClippingMode.Pick:
          this.cornerPadding = new Padding(1);

          this.outerBorderBrush?.Dispose();
          this.outerBorderBrush = new SolidColorBrush(RenderTarget, new RawColor4(0.5f, 0.5f, 0.5f, 0.75f));
          break;

        case ClippingMode.Rescale:
          this.cornerPadding = new Padding(5);

          this.outerBorderBrush?.Dispose();
          this.innerBorderBrush?.Dispose();

          this.outerBorderBrush = new SolidColorBrush(RenderTarget, new RawColor4(0, 0, 0, 0.25f));
          this.innerBorderBrush = new SolidColorBrush(RenderTarget, new RawColor4(0.5f, 0.5f, 0.5f, 1));

          if (this.cornerBitmap?.IsDisposed ?? true) {
            this.cornerBitmap = Resources.ClipperCorners.ToDirect2DBitmap(RenderTarget);
          }

          break;
      }

      Area = oldArea;
      DesktopWrapper?.Invalidate();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources
    /// </summary>
    public override void Dispose() {
      // make sure the mouse hook is unlocked or, at least, that we unbind our event handlers
      Lock();
      base.Dispose();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Renders the clipper UI
    /// </summary>
    protected override void Render() {
      RenderTarget.Clear(Locked ? (RawColor4?) null : new RawColor4(0.5f, 0.5f, 0.5f, 0.25f));
      if (Locked) {
        // render outer border
        RenderTarget.DrawRectangle(new RawRectangleF(1.5f, 1.5f, Bounds.Width - 1.5f, Bounds.Height - 1.5f),
          this.outerBorderBrush,
          3);

        // render corners
        RenderTarget.DrawBitmap(this.cornerBitmap,
          new RawRectangleF(0, 0, this.cornerBitmap.Size.Width / 2, this.cornerBitmap.Size.Height / 2),
          1,
          BitmapInterpolationMode.Linear,
          new RawRectangleF(0, 0, this.cornerBitmap.Size.Width / 2, this.cornerBitmap.Size.Height / 2));
        RenderTarget.DrawBitmap(this.cornerBitmap,
          new RawRectangleF(Bounds.Width - this.cornerBitmap.Size.Width / 2,
            0,
            Bounds.Width,
            this.cornerBitmap.Size.Width / 2),
          1,
          BitmapInterpolationMode.Linear,
          new RawRectangleF(this.cornerBitmap.Size.Width / 2,
            0,
            this.cornerBitmap.Size.Width,
            this.cornerBitmap.Size.Height / 2));
        RenderTarget.DrawBitmap(this.cornerBitmap,
          new RawRectangleF(0,
            Bounds.Height - this.cornerBitmap.Size.Height / 2,
            this.cornerBitmap.Size.Width / 2,
            Bounds.Height),
          1,
          BitmapInterpolationMode.Linear,
          new RawRectangleF(0,
            this.cornerBitmap.Size.Height / 2,
            this.cornerBitmap.Size.Width / 2,
            this.cornerBitmap.Size.Height));
        RenderTarget.DrawBitmap(this.cornerBitmap,
          new RawRectangleF(Bounds.Width - this.cornerBitmap.Size.Width / 2,
            Bounds.Height - this.cornerBitmap.Size.Height / 2,
            Bounds.Width,
            Bounds.Height),
          1,
          BitmapInterpolationMode.Linear,
          new RawRectangleF(this.cornerBitmap.Size.Width / 2,
            this.cornerBitmap.Size.Height / 2,
            this.cornerBitmap.Size.Width,
            this.cornerBitmap.Size.Height));

        // render inner border
        RenderTarget.DrawRectangle(new RawRectangleF(5, 5, Bounds.Width - 4.5f, Bounds.Height - 4.5f),
          this.innerBorderBrush);
      } else {
        RenderTarget.DrawRectangle(new RawRectangleF(
            (int) Math.Ceiling(this.cornerPadding.Left / 2f),
            (int) Math.Ceiling(this.cornerPadding.Top / 2f),
            (int) Math.Ceiling(Bounds.Width - this.cornerPadding.Right / 2f),
            (int) Math.Ceiling(Bounds.Height - this.cornerPadding.Bottom / 2f)),
          this.outerBorderBrush,
          this.cornerPadding.All);
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates disposable rendering resources
    /// </summary>
    protected override void InitializeRenderingObjects() {
      base.InitializeRenderingObjects();
      RefreshLayout();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Disposes all rendering resources
    /// </summary>
    protected override void DestroyRenderingObjects() {
      this.outerBorderBrush?.Dispose();
      this.outerBorderBrush = null;
      base.DestroyRenderingObjects();
    }

    /// <summary>
    ///   Unlocks the clipper UI so that the user can select a new region or modify the existing one
    /// </summary>
    /// <param name="mode">New selection mode</param>
    internal async Task Unlock(ClippingMode mode = ClippingMode.Pick) {
      if (!Locked && Mode == mode) { return; }

      // create mutex
      this.uiLockMutex?.ReleaseMutex();
      this.uiLockMutex = new Mutex(true);

      // "hide" the clipper UI
      Bounds = new Rectangle(-0x7FFF, -0x7FFF, 0, 0);

      // set the new mode and update the UI layout to match
      Mode = mode;
      RefreshLayout();

      // lock mouse hook
      // on Rescale mode we should not need mouse hooks in order to retrieve events;
      // However, it is reasonable to do so taking into account that, depending on the current HUD container,
      // we may not have window events available, or even a window at all
      if (Container.MouseHookBehaviour is IMouseHookProvider mouseHookProvider) {
        Container.MouseHookBehaviour.RequestLock();

        // bind hook event handlers
        mouseHookProvider.OnMouseDown += OnHookedMouseDown;
        mouseHookProvider.OnMouseMove += OnHookedMouseMove;
        mouseHookProvider.OnMouseUp += OnHookedMouseUp;
      }

      // lock keyboard hook
      // we want to respond to the Alt key immediately, not just when the mouse is moved.
      // For this, we have no other choice
      if (Container.KeyboardHookBehaviour is IKeyboardHookProvider kbdHookProvider) {
        Container.KeyboardHookBehaviour.RequestLock();

        // bind hook event handlers
        kbdHookProvider.OnKeyDown += OnHookedKeyDown;
        kbdHookProvider.OnKeyUp += OnHookedKeyUp;
      }

      Locked = false;
      if (DesktopWrapper != null) {
        DesktopWrapper.PassThrough = true;
        DesktopWrapper.MinimumSize = default;
      }

      RefreshTidbit();
      await Task.Factory.StartNew(this.uiLockMutex.WaitOne);
    }

    /// <summary>
    ///   Refreshes the clipper UI tidbit
    /// </summary>
    private void RefreshTidbit() {
      if (Mode == ClippingMode.Pick) {
        if (this.tidbit?.Disposed ?? true) { this.tidbit = new Tidbit(Container, Timeout.InfiniteTimeSpan); }

        if (this.altDown) {
          this.tidbit.Content = "Click the window to be captured";
          this.tidbit.CustomIcon = Resources.ClipperPickWindow;
          this.tidbit.CustomAccent = Color.White;
          this.tidbit.Visible = true;
        } else if (Area.Width == 0 || Area.Height == 0) {
          this.tidbit.Content = "Select the region you want to capture";
          this.tidbit.CustomIcon = Resources.ClipperPickRegion;
          this.tidbit.CustomAccent = Color.White;
          this.tidbit.Visible = true;
        } else if (Area.Width < MinimumWidth || Area.Height < MinimumHeight) {
          this.tidbit.Status = TidbitStatus.Warning;
          this.tidbit.Content = "This region is too small";
          this.tidbit.CustomIcon = null;
          this.tidbit.CustomAccent = Color.Yellow;
          this.tidbit.Visible = true;
        } else { this.tidbit.Visible = false; }
      }
    }

    /// <summary>
    ///   Triggered when a key is held
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Keyboard event arguments</param>
    private void OnHookedKeyDown(object sender, KeyEventArgs eventArgs) {
      if (eventArgs.KeyData == Keys.LMenu || eventArgs.KeyCode == Keys.RMenu) {
        this.altDown = true;
        OnHookedMouseMove(sender,
          new ExtendedEventArgs<MouseEventArgs, bool>(new MouseEventArgs(MouseButtons.None,
            0,
            Control.MousePosition.X,
            Control.MousePosition.Y,
            0)));
        RefreshTidbit();
      }
    }

    /// <summary>
    ///   Triggered when a key is released
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Keyboard event arguments</param>
    private void OnHookedKeyUp(object sender, KeyEventArgs eventArgs) {
      if (eventArgs.KeyCode == Keys.LMenu || eventArgs.KeyCode == Keys.RMenu) {
        this.altDown = false;
        Bounds = new Rectangle(-0x7FFF, -0x7FFF, 0, 0);
        RefreshTidbit();
      }
    }

    /// <summary>
    ///   Locks the clipper UI so that the user may not modify its region
    /// </summary>
    private void Lock() {
      if (Locked) { return; }

      // lock the clipper UI and set the new mode
      Mode = ClippingMode.Rescale;
      Locked = true;
      RefreshLayout();
      this.tidbit?.Dispose();
      this.uiLockMutex?.ReleaseMutex();

      if (DesktopWrapper != null) {
        // we want to display custom cursors for desktop wrapper corners, so we want the window to process mouse events
        DesktopWrapper.PassThrough = false;
        DesktopWrapper.MinimumSize =
          new Size(MinimumWidth + this.cornerPadding.Horizontal, MinimumHeight + this.cornerPadding.Vertical);
      }

      // unlock mouse hook
      if (Container.MouseHookBehaviour is IMouseHookProvider hookProvider) {
        Container.MouseHookBehaviour.RequestUnlock();

        // unbind hook event handlers
        hookProvider.OnMouseDown -= OnHookedMouseDown;
        hookProvider.OnMouseMove -= OnHookedMouseMove;
        hookProvider.OnMouseUp -= OnHookedMouseUp;
      }

      // unlock keyboard hook
      if (Container.KeyboardHookBehaviour is IKeyboardHookProvider kbdHookProvider) {
        Container.KeyboardHookBehaviour.RequestUnlock();

        // bind hook event handlers
        kbdHookProvider.OnKeyDown -= OnHookedKeyDown;
        kbdHookProvider.OnKeyUp -= OnHookedKeyUp;
      }
    }

    /// <summary>
    ///   Handles hooked mouse button down events
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="extendedEventArgs">
    ///   Extended event arguments, being the extended data a boolean value that, when <c>false</c>, forces the event
    ///   to be passed to the next handler
    /// </param>
    private void OnHookedMouseDown(object sender, ExtendedEventArgs<MouseEventArgs, bool> extendedEventArgs) {
      Log.WriteLine(LogLevel.Debug,
        "{0} down at {1}",
        extendedEventArgs.EventArgs.Button,
        extendedEventArgs.EventArgs.Location);

      if (this.windowHandle != IntPtr.Zero) { return; }
      if (extendedEventArgs.EventArgs.Button == MouseButtons.Left) {
        this.initialPosition = extendedEventArgs.EventArgs.Location;
        Area = new Rectangle(extendedEventArgs.EventArgs.Location, Size.Empty);
        extendedEventArgs.ExtendedData = true; // capture event
      } else { Lock(); }
    }

    /// <summary>
    ///   Handles hooked mouse movements
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="extendedEventArgs">
    ///   Extended event arguments, being the extended data a boolean value that, when <c>false</c>, forces the event
    ///   to be passed to the next handler
    /// </param>
    private void OnHookedMouseMove(object sender, ExtendedEventArgs<MouseEventArgs, bool> extendedEventArgs) {
      if (this.initialPosition.HasValue) {
        Rectangle rect = default;

        if (this.initialPosition.Value.X < extendedEventArgs.EventArgs.X) {
          rect.X = this.initialPosition.Value.X;
          rect.Width = extendedEventArgs.EventArgs.X - this.initialPosition.Value.X;
        } else {
          rect.X = extendedEventArgs.EventArgs.X;
          rect.Width = this.initialPosition.Value.X - extendedEventArgs.EventArgs.X;
        }

        if (this.initialPosition.Value.Y < extendedEventArgs.EventArgs.Y) {
          rect.Y = this.initialPosition.Value.Y;
          rect.Height = extendedEventArgs.EventArgs.Y - this.initialPosition.Value.Y;
        } else {
          rect.Y = extendedEventArgs.EventArgs.Y;
          rect.Height = this.initialPosition.Value.Y - extendedEventArgs.EventArgs.Y;
        }

        Area = rect;
        RefreshTidbit();

        return;
      }

      if (Container.ContainerType == HudContainerType.Desktop && this.altDown) {
        this.windowHandle =
          User32.WindowFromPoint(new POINT { x = extendedEventArgs.EventArgs.X, y = extendedEventArgs.EventArgs.Y });

        if (this.windowHandle != IntPtr.Zero) {
          var area = WindowHelper.GetWindowBounds(this.windowHandle).ToRectangle();
          if (area != default) {
            Area = area;
            return;
          }

          this.windowHandle = IntPtr.Zero;
        }
      }

      Bounds = new Rectangle(-0x7FFF, -0x7FFF, 0, 0);
    }

    /// <summary>
    ///   Handles hooked mouse button up events
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="extendedEventArgs">
    ///   Extended event arguments, being the extended data a boolean value that, when <c>false</c>, forces the event
    ///   to be passed to the next handler
    /// </param>
    private void OnHookedMouseUp(object sender, ExtendedEventArgs<MouseEventArgs, bool> extendedEventArgs) {
      Log.WriteLine(LogLevel.Debug,
        "mouse up at {0}",
        extendedEventArgs.EventArgs.Location);

      if (Area.Width < MinimumWidth || Area.Height < MinimumHeight) {
        Log.WriteLine(LogLevel.Debug, "area too small - disposing");
        Dispose();
      } else {
        this.initialPosition = null;
        extendedEventArgs.ExtendedData = true; // capture event
        Lock();
      }
    }
  }
}