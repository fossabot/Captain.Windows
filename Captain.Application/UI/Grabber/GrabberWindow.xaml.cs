using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using Captain.Application.Native;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Brushes = System.Windows.Media.Brushes;
using Orientation = System.Windows.Controls.Orientation;

namespace Captain.Application {
  /// <summary>
  ///   Interaction logic for GrabberWindow.xaml
  /// </summary>
  internal partial class GrabberWindow {
    /// <summary>
    ///   <see cref="Grabber"/>  instance
    /// </summary>
    private readonly Grabber grabber;

    /// <summary>
    ///   Acceptable capture region
    /// </summary>
    private Rectangle[] acceptableRectangles;

    /// <summary>
    ///   Device notification filter handle
    /// </summary>
    private IntPtr devNotify;

    /// <summary>
    ///   Toolbar UI padding
    /// </summary>
    private static Padding ToolBarPadding => new Padding(8);

    /// <summary>
    ///   UI padding
    /// </summary>
    private new Padding Padding => new Padding((int)(this.Border.Margin.Left + this.Border.BorderThickness.Left),
                                               (int)(this.Border.Margin.Top + this.Border.BorderThickness.Top),
                                               (int)(this.Border.Margin.Right + this.Border.BorderThickness.Right),
                                               (int)(this.Border.Margin.Bottom + this.Border.BorderThickness.Bottom));

    /// <summary>
    ///   HACK: When true, the window will return HTTRANSPARENT for the WM_NCHITTEST message so, for instance, it
    ///   remains invisible to WindowFromPoint() calls from the current thread
    /// </summary>
    internal bool PassThrough { private get; set; }

    /// <summary>
    ///   Internal window handle
    /// </summary>
    internal IntPtr Handle { get; private set; }

    /// <summary>
    ///   Actual capture bounds
    /// </summary>
    internal Rectangle Area => new Rectangle((int)Left + Padding.Left,
                                             (int)Top + Padding.Top,
                                             (int)Width - Padding.Left - Padding.Right,
                                             (int)Height - Padding.Top - Padding.Bottom);

    /// <summary>
    ///   Changes/gets whether the window can be resized
    /// </summary>
    internal bool CanBeResized {
      set {
        if (value) {
          // set resize mode
          ResizeMode = ResizeMode.CanResize;

          // show size grips
          this.EllipseGrid.Visibility = Visibility.Visible;

          // set background/border colors (TODO: move this to some resource file)
          Background = new SolidColorBrush(Color.FromArgb(0x01, 0x7F, 0x7F, 0x7F));
          this.Border.Background = Brushes.Transparent;
        } else {
          // set resize mode
          ResizeMode = ResizeMode.NoResize;

          // hide size grips
          this.EllipseGrid.Visibility = Visibility.Hidden;

          // remove background and borders
          Background = null;
          this.Border.Background = null;
        }
      }
    }

    /// <summary>
    ///   Class constructor
    /// </summary>
    internal GrabberWindow(Grabber grabber) {
      this.grabber = grabber;
      InitializeComponent();
    }

    /// <summary>
    ///   Adjusts window properties and sets hooks
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnSourceInitialized(EventArgs eventArgs) {
      base.OnSourceInitialized(eventArgs);

      if (PresentationSource.FromVisual(this) is HwndSource source) {
        Handle = source.Handle;

        long windowStyle = User32.GetWindowLongPtr(Handle, (int)User32.WindowLongParam.GWL_STYLE).ToInt64();
        long windowExStyle = User32.GetWindowLongPtr(Handle, (int)User32.WindowLongParam.GWL_EXSTYLE).ToInt64();

        // remove maximize/minimize capabilities
        User32.SetWindowLongPtr(Handle,
                                (int)User32.WindowLongParam.GWL_STYLE,
                                new IntPtr(windowStyle &
                                           ~(long)(User32.WindowStyles.WS_MINIMIZEBOX |
                                                   User32.WindowStyles.WS_MAXIMIZEBOX)));

        // hide from window switcher
        User32.SetWindowLongPtr(Handle,
                                (int)User32.WindowLongParam.GWL_EXSTYLE,
                                new IntPtr(windowExStyle | (long)User32.WindowStylesEx.WS_EX_TOOLWINDOW));

        Display.RegisterChangeNotifications(Handle, out this.devNotify);
        source.AddHook(WndProc);
      }

      UpdateWindowGeometry();
    }

    /// <summary>
    ///   Triggered when the window is closed
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnClosed(EventArgs eventArgs) {
      Display.UnregisterChangeNotifications(this.devNotify);
      base.OnClosed(eventArgs);
    }

    /// <summary>
    ///   Updates the window geometry according to the acceptable capture bounds
    /// </summary>
    private void UpdateWindowGeometry() {
      // update displays' bounds
      this.acceptableRectangles = DisplayHelper.GetOutputInfo().Select(i => i.Bounds).ToArray();
      UpdatePosition();
    }

    /// <summary>
    ///   Window procedure hook
    /// </summary>
    /// <param name="hwnd">Window handle</param>
    /// <param name="msg">Window message</param>
    /// <param name="wParam">Reserved</param>
    /// <param name="lParam">Reserved</param>
    /// <param name="handled">Whether the message was handled or not</param>
    /// <returns></returns>
    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      switch (msg) {
        case (int)User32.WindowMessage.WM_NCHITTEST: // hit test
          handled = PassThrough;
          return new IntPtr((int)User32.HitTestValues.HTTRANSPARENT);

        case (int)User32.WindowMessage.WM_DEVICECHANGE: // a video adapter device has been removed/added
          if (wParam.ToInt32() != Dbt.DBT_DEVNODES_CHANGED) { break; }
          UpdateWindowGeometry();
          break;

        case (int)User32.WindowMessage.WM_WINDOWPOSCHANGING: // limit window position/size to the screen bounds
          // TODO: Windows does not currently allow a window to be resized beyond the screen limits (i.e. when resizing
          //       a small part of the window will remain onscreen). Hack this down so the user can completely cover
          //       the screen
          // TODO: reduce flicker somehow! The larger the window gets, the more noticeable the flicker is!
          var pos = (User32.WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(User32.WINDOWPOS));
          var rect = new Rectangle(pos.x,
                                   pos.y,
                                   pos.cx,
                                   (int)(pos.cy +
                                         ToolBarPadding.Top +
                                         ToolBarPadding.Bottom +
                                         this.grabber.ToolBar.Thickness));

          if ((pos.flags & (int)User32.SetWindowPosFlags.SWP_NOACTIVATE) == 0) {
            // HACK: when windows get moved beyond the top limits of the screen, windows automatically snaps the bounds
            //       so that the "title bar" (which in our case is non-existant) remains visible. SWP_NOACTIVATE flag
            //       is set by Windows, so we can filter position change messages and ignore those with the flag.
            //       This way the user is allowed to size the grabber UI however they want
            pos.flags |= (int)User32.SetWindowPosFlags.SWP_NOMOVE | (int)User32.SetWindowPosFlags.SWP_NOSIZE;
          }

          // make sure the next position is not off-screen
          //if (this.acceptableRectangles.Count(r => Rectangle.Intersect(r, rect).Contains(rect)) == 0) {
          // retrieve the bounds of the displays containing at least a part from the window
          Rectangle[] containers = this.acceptableRectangles.Where(r => rect.IntersectsWith(r)).ToArray();

          // in case no display is containing the window, default to the primary screen
          if (!containers.Any()) { containers = new[] { Screen.PrimaryScreen.Bounds }; }

          // calculate the acceptable bounds for ALL the displays at once
          int minLeft = containers.Min(c => c.Left);
          int maxTop = containers.Max(c => c.Top);
          int maxRight = containers.Max(c => c.Right);
          int minBottom = containers.Min(c => c.Bottom);

          // limit window coordinates
          pos.x = pos.x + Padding.Left < minLeft
                    ? minLeft - Padding.Left
                    : (pos.x + pos.cx - Padding.Right > maxRight
                         ? maxRight - pos.cx + Padding.Right
                         : pos.x);

          pos.y = Math.Min(minBottom - maxTop,
                           Math.Max(maxTop,
                                    pos.y + Padding.Top < maxTop
                                      ? maxTop - Padding.Top
                                      : (pos.y + pos.cy - Padding.Bottom > minBottom
                                           ? minBottom - pos.cy + Padding.Bottom
                                           : pos.y)));

          pos.cx = Math.Min(maxRight - minLeft, pos.cx);
          pos.cy = Math.Min(minBottom - maxTop, pos.cy);

          if ((pos.flags & (int)User32.SetWindowPosFlags.SWP_NOMOVE) == 0) {
            if (!this.grabber.ToolBar.IsVisible) {
              this.grabber.ToolBar.Show();
            }

            // move toolbar accordingly
            if (pos.y + pos.cy + ToolBarPadding.Top + this.grabber.ToolBar.Thickness > minBottom) {
              if (pos.y - ToolBarPadding.Bottom - this.grabber.ToolBar.Thickness > maxTop) {
                this.grabber.ToolBar.Orientation = Orientation.Horizontal;
                this.grabber.ToolBar.Top = pos.y - ToolBarPadding.Bottom - this.grabber.ToolBar.Thickness;
              } else {
                this.grabber.ToolBar.Orientation = Orientation.Vertical;

                if (pos.x + pos.cx + ToolBarPadding.Left + this.grabber.ToolBar.Thickness < maxRight) {
                  this.grabber.ToolBar.Left = pos.x + pos.cx + ToolBarPadding.Left;
                } else {
                  this.grabber.ToolBar.Left = pos.x - ToolBarPadding.Right - this.grabber.ToolBar.Thickness;
                }

                this.grabber.ToolBar.Top =
                  Math.Min(Math.Max(maxTop, pos.y + (pos.cy - this.grabber.ToolBar.Height) / 2),
                           minBottom - this.grabber.ToolBar.Height);
              }
            } else {
              this.grabber.ToolBar.Orientation = Orientation.Horizontal;
              this.grabber.ToolBar.Top = pos.y + pos.cy + ToolBarPadding.Top;
            }

            if (this.grabber.ToolBar.Orientation == Orientation.Horizontal) {
              this.grabber.ToolBar.Left = Math.Min(maxRight - this.grabber.ToolBar.Width,
                                                   Math.Max(minLeft,
                                                            pos.x + (pos.cx - this.grabber.ToolBar.Width) / 2));
            }

            // move target window, if any
            if (this.grabber.AttachedWindowHandle != IntPtr.Zero) {
              User32.MoveWindow(this.grabber.AttachedWindowHandle, pos.x, pos.y, pos.cx, pos.cy, false);
            }
          }

          Marshal.StructureToPtr(pos, lParam, false);
          handled = true;
          break;
      }

      return IntPtr.Zero;
    }

    /// <summary>
    ///   Handles mouse down events
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnMouseDown(object sender, MouseButtonEventArgs eventArgs) {
      if (eventArgs.ChangedButton == MouseButton.Left) {
        // move window
        DragMove();
      }
    }

    /// <summary>
    ///   Triggers window position update mechanisms
    /// </summary>
    /// <param name="handle">Target window handle</param>
    /// <remarks>
    ///   When the <c>handle</c> parameter is set, the UI will adopt the bounds of the window with the specified handle
    /// </remarks>
    internal void UpdatePosition(IntPtr? handle = null) {
      if (handle.HasValue && handle != Handle && handle != IntPtr.Zero) {
        // HACK!
        RECT bounds = WindowHelper.GetWindowBounds(handle.Value);
        User32.MoveWindow(handle.Value,
                          bounds.left - Padding.Left,
                          bounds.top - Padding.Top,
                          bounds.right - bounds.left + Padding.Left + Padding.Right,
                          bounds.bottom - bounds.top + Padding.Top + Padding.Bottom,
                          true);
      } else {
        User32.MoveWindow(Handle, (int)Left, (int)Top, (int)Width, (int)Height, true);
      }
    }

  }
}
