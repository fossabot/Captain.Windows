using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using Captain.Application.Native;

namespace Captain.Application {
  /// <summary>
  ///   Interaction logic for GrabberToolBarWindow.xaml
  /// </summary>
  internal partial class GrabberToolBarWindow {
    /// <summary>
    ///   True when the screen may be being recorded
    /// </summary>
    private bool mayBeRecording;

    /// <summary>
    ///   Internal window handle
    /// </summary>
    internal IntPtr Handle { get; private set; }

    /// <summary>
    ///   Handles capture button events
    /// </summary>
    /// <param name="type">Action type</param>
    internal delegate void CaptureActionInitiatedHandler(ActionType type);

    /// <summary>
    ///   Handles grabber UI intents
    /// </summary>
    /// <param name="intentType">Intent type</param>
    /// <param name="userData">Custom user data</param>
    internal delegate void GrabberIntentHandler(GrabberIntentType intentType, object userData = null);

    /// <summary>
    ///   Capture action init event
    /// </summary>
    internal event CaptureActionInitiatedHandler OnCaptureActionInitiated;

    /// <summary>
    ///   Grabber UI intent received event
    /// </summary>
    internal event GrabberIntentHandler OnGrabberIntentReceived;

    /// <summary>
    ///   Creates a new capture toolbar
    /// </summary>
    /// <param name="acceptableActionTypes">Action types that are available</param>
    internal GrabberToolBarWindow(ActionType acceptableActionTypes) {
      InitializeComponent();

      this.ScreenshotButton.Visibility = acceptableActionTypes.HasFlag(ActionType.Screenshot)
                                   ? Visibility.Visible
                                   : Visibility.Collapsed;

      this.RecordingTools.Visibility = acceptableActionTypes.HasFlag(ActionType.Record)
                                             ? Visibility.Visible
                                             : Visibility.Collapsed;
    }

    /// <summary>
    ///   Updates window attachment status
    /// </summary>
    /// <param name="attached">Whether there's a window attached</param>
    /// <param name="enabled">Whether the attachment button is enabled</param>
    internal void SetWindowAttachmentStatus(bool attached, bool enabled = true) {
      if (!enabled) {
        this.PinButton.Visibility = Visibility.Visible;
        this.UnpinButton.Visibility = Visibility.Collapsed;
        this.PinButton.IsEnabled = false;
        this.UnpinButton.IsEnabled = false;
      } else {
        this.PinButton.IsEnabled = true;
        this.UnpinButton.IsEnabled = true;

        if (attached) {
          this.PinButton.Visibility = Visibility.Collapsed;
          this.UnpinButton.Visibility = Visibility.Visible;
          Topmost = false;
        } else {
          this.PinButton.Visibility = Visibility.Visible;
          this.UnpinButton.Visibility = Visibility.Collapsed;
          Topmost = true;
        }
      }
    }

    /// <summary>
    ///   AddHook Handle WndProc messages in WPF
    ///   This cannot be done in a Window's constructor as a handle window handle won't at that point, so there won't
    ///   be a HwndSource.
    /// </summary>
    /// <param name="e"></param>
    protected override void OnSourceInitialized(EventArgs e) {
      base.OnSourceInitialized(e);

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
        source.AddHook(WndProc);
      }
    }

    /// <summary>
    ///   WndProc matches the HwndSourceHook delegate signature so it can be passed to AddHook() as a callback. This is
    ///   is the same as overriding a Windows.Form's WncProc method.
    /// </summary>
    /// <param name="hwnd">The window handle</param>
    /// <param name="msg">The message ID</param>
    /// <param name="wParam">
    ///   The message's wParam value, historically used in the win32 api for handles and integers
    /// </param>
    /// <param name="lParam">The message's lParam value, historically used in the win32 api to pass pointers</param>
    /// <param name="handled">A value that indicates whether the message was handled</param>
    /// <returns>IntPtr.Zero</returns>
    // ReSharper disable InconsistentNaming
    private static IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
      const int WM_SYSCOMMAND = 0x0112;
      const int SC_MOVE = 0xF010;

      if (msg == WM_SYSCOMMAND && (wParam.ToInt32() & 0xFFF0) == SC_MOVE) {
        handled = true;
      }

      return IntPtr.Zero;
    }

    /// <summary>
    ///   Triggered when the "Take screenshot" button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ScreenshotButton_Click(object sender, RoutedEventArgs e) {
      OnCaptureActionInitiated(ActionType.Screenshot);

      if (!this.mayBeRecording && (Keyboard.Modifiers & ModifierKeys.Alt) == 0) {
        // close the grabber UI if this was just a screenshot and the ALT modifier key is not pressed
        OnGrabberIntentReceived(GrabberIntentType.Close);
      }
    }

    /// <summary>
    ///   Triggered when the "Cancel" button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CancelButton_Click(object sender, RoutedEventArgs e) =>
      OnGrabberIntentReceived(GrabberIntentType.Close);

    /// <summary>
    ///   Triggered when the "Attach to window" button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void PinButton_Click(object sender, RoutedEventArgs e) =>
      OnGrabberIntentReceived(GrabberIntentType.AttachToWindow);

    /// <summary>
    ///   Triggered when the "Detach from window" button is clicked
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void UnpinButton_Click(object sender, RoutedEventArgs e) =>
      OnGrabberIntentReceived(GrabberIntentType.DetachFromWindow);

#if false
    /// <summary>
    ///   Triggered when the Cancel button is activated
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="eventArgs">Event args</param>
    private void CancelButton_Click(object sender, RoutedEventArgs eventArgs) => Close();

    /// <summary>
    ///   Triggered when the Capture button is activated
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="eventArgs">Event args</param>
    private void OkButton_OnClick(object sender, RoutedEventArgs eventArgs) {
      OnCaptureActionInitiated(ActionType.Screenshot);

      if (!this.mayBeRecording && (Keyboard.Modifiers & ModifierKeys.Alt) == 0) {
        // close the grabber UI if this was just a capture and the ALT modifier key is not pressed!
        Close();
      }
    }

    /// <summary>
    ///   Triggered when the Record button is activated
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="eventArgs">Event args</param>
    private void RecordToggleButton_Click(object sender, RoutedEventArgs eventArgs) {
      if (this.mayBeRecording) {
        // stop recording
        this.RecordToggleButton.ToolTip = Captain.Application.Resources.GrabberUI_Record;
        this.RecordToggleButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Content/record.png"));
      } else {
        // start recording
        this.RecordToggleButton.ToolTip = Captain.Application.Resources.GrabberUI_StopRecording;
        this.RecordToggleButtonImage.Source = new BitmapImage(new Uri("pack://application:,,,/Content/stop.png"));
      }

      this.mayBeRecording = !this.mayBeRecording;
      OnCaptureActionInitiated(ActionType.Record);
    }

#endif
  }
}
