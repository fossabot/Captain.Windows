using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Color = System.Windows.Media.Color;
using static Captain.Application.Application;
using Captain.Application.Native;
using Captain.Common;
using System.Runtime.InteropServices;

namespace Captain.Application {
  /// <summary>
  ///   Interaction logic for GrabberWindow.xaml
  /// </summary>
  internal partial class GrabberWindow {
    /// <summary>
    ///   Acceptable capture region
    /// </summary>
    private Rectangle acceptableBounds;

    /// <summary>
    ///   Device notification filter handle
    /// </summary>
    private IntPtr devNotify;

    /// <summary>
    ///   HACK: When true, the window will return HTTRANSPARENT for the WM_NCHITTEST message so, for instance, it
    ///   remains invisible to WindowFromPoint() calls from the current thread
    /// </summary>
    internal bool PassThrough { get; set; } = false;

    /// <summary>
    ///   Internal window handle
    /// </summary>
    internal IntPtr Handle { get; private set; }

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
          this.Border.Background = new SolidColorBrush(Color.FromArgb(0x20, 0x7F, 0x7F, 0x7F));
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
    ///   Obtains the area selected by the user
    /// </summary>
    internal Rectangle Area => new Rectangle((int)Left + 9, (int)Top + 9, (int)Width - 18, (int)Height - 18);

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
        User32.SetWindowLongPtr(Handle, (int)User32.WindowLongParam.GWL_STYLE,
          new IntPtr(windowStyle & ~(long)(User32.WindowStyles.WS_MINIMIZEBOX | User32.WindowStyles.WS_MAXIMIZEBOX)));

        // hide from window switcher
        User32.SetWindowLongPtr(Handle, (int)User32.WindowLongParam.GWL_EXSTYLE,
          new IntPtr(windowExStyle | (long)User32.WindowStylesEx.WS_EX_TOOLWINDOW));

        Display.RegisterChangeNotifications(Handle, out this.devNotify);
        source.AddHook(WndProc);
      }
    }

    /// <summary>
    ///   Updates the window geometry according to the acceptable capture bounds
    /// </summary>
    private void UpdateWindowGeometry() {
      this.acceptableBounds = DisplayHelper.GetAcceptableBounds();

      // force window to reposition in case it is outside the acceptable bounds
      if (Left < this.acceptableBounds.Left) { Left = this.acceptableBounds.Left; }
      if (Top < this.acceptableBounds.Top) { Top = this.acceptableBounds.Top; }
      if (Left + Width > this.acceptableBounds.Right) { Left = this.acceptableBounds.Right - Width; }
      if (Top + Height > this.acceptableBounds.Bottom) { Top = this.acceptableBounds.Bottom - Height; }

      // TODO: reposition attached window
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
        case (int)User32.WindowMessage.WM_NCHITTEST:
          handled = PassThrough;
          return new IntPtr((int)User32.HitTestValues.HTTRANSPARENT);

        case (int)User32.WindowMessage.WM_DEVICECHANGE:
          if (wParam.ToInt32() != Dbt.DBT_DEVNODES_CHANGED) { break; }

          // a video adapter device has been removed/added
          UpdateWindowGeometry();
          break;

        case (int)User32.WindowMessage.WM_WINDOWPOSCHANGING:
          var pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

          // prevent the window from going off-screen
          if (pos.x < this.acceptableBounds.Left) { pos.x = this.acceptableBounds.Left; }
          if (pos.y < this.acceptableBounds.Top) { pos.y = this.acceptableBounds.Top; }
          if (pos.x + pos.cx > this.acceptableBounds.Right) { pos.x = this.acceptableBounds.Right - pos.cx; }
          if (pos.y + pos.cy > this.acceptableBounds.Bottom) { pos.y = this.acceptableBounds.Bottom - pos.cy; }

          Marshal.StructureToPtr(pos, lParam, false);
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
