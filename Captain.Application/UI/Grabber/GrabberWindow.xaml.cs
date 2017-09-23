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

namespace Captain.Application {
  /// <summary>
  ///   Interaction logic for GrabberWindow.xaml
  /// </summary>
  internal partial class GrabberWindow {
    /// <summary>
    ///   Window padding required for a better resizing target
    /// </summary>
    private new const int Padding = 16;

    /// <summary>
    ///   Acceptable capture region
    /// </summary>
    private Rectangle[] acceptableRectangles;

    /// <summary>
    ///   Device notification filter handle
    /// </summary>
    private IntPtr devNotify;

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
    internal Rectangle Area => new Rectangle((int)Left + Padding,
                                             (int)Top + Padding,
                                             (int)Width - Padding,
                                             (int)Height - Padding);

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
    internal GrabberWindow() => InitializeComponent();

    /// <summary>
    ///   Class destructor
    /// </summary>
    ~GrabberWindow() => Display.UnregisterChangeNotifications(this.devNotify);

    /// <summary>
    ///   Adjusts window properties and sets hooks
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnSourceInitialized(EventArgs eventArgs) {
      base.OnSourceInitialized(eventArgs);
      UpdateWindowGeometry();

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
    }

    /// <summary>
    ///   Updates the window geometry according to the acceptable capture bounds
    /// </summary>
    private void UpdateWindowGeometry() {
      // update displays' bounds
      this.acceptableRectangles = DisplayHelper.GetOutputInfo().Select(i => i.Bounds).ToArray();

      // HACK! 1/(2^42) is the minimum double value that, added to a coordinate, does not apparently chnage it but
      // but does, in fact, trigger window reposition mechanisms (i.e. we receive WM_WINDOWPOSCHANGING et al.)
      Left += Math.Pow(2, -42);
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
          var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
          var rect = new Rectangle(pos.x, pos.y, pos.cx, pos.cy);

          if ((pos.flags & (int)User32.SetWindowPosFlags.SWP_NOACTIVATE) == 0) {
            // HACK: when windows get moved beyond the top limits of the screen, windows automatically snaps the bounds
            //       so that the "title bar" (which in our case is non-existant) remains visible. SWP_NOACTIVATE flag
            //       is set by Windows, so we can filter position change messages and ignore those with the flag.
            //       This way the user is allowed to size the grabber UI however they want
            pos.flags |= (int)User32.SetWindowPosFlags.SWP_NOMOVE | (int)User32.SetWindowPosFlags.SWP_NOSIZE;
          }

          // make sure the next position is not off-screen
          if (this.acceptableRectangles.Count(r => Rectangle.Intersect(r, rect).Contains(rect)) == 0) {
            // retrieve the bounds of the displays containing at least a part from the window
            Rectangle[] containers = this.acceptableRectangles.Where(r => rect.IntersectsWith(r)).ToArray();

            // in case no display is containing the window, default to the primary screen
            if (!containers.Any()) { containers = new[] { Screen.PrimaryScreen.Bounds }; }

            // calculate the acceptable bounds for ALL the displays at once
            int minLeft = containers.Min(c => c.Left);
            int maxTop = containers.Max(c => c.Top);
            int maxRight = containers.Max(c => c.Right);
            int minBottom = containers.Min(c => c.Bottom);

            // limit window coordinatesç
            pos.x = pos.x - Padding < minLeft
                      ? minLeft - Padding
                      : (pos.x + pos.cx - Padding > maxRight
                           ? maxRight - pos.cx + Padding
                           : pos.x);

            pos.y = pos.y + Padding < maxTop
                      ? maxTop - Padding
                      : (pos.y + pos.cy - Padding > minBottom
                           ? minBottom - pos.cy + Padding
                           : pos.y);
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
  }
}
