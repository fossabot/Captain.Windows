using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Captain.Application.NativeHelpers;
using Color = System.Windows.Media.Color;

namespace Captain.Application {
  /// <summary>
  ///   Interaction logic for GrabberWindow.xaml
  /// </summary>
  internal partial class GrabberWindow {
    /// <summary>
    ///   Native window helper
    /// </summary>
    private GrabberWindowHelper helper;

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
    ///   Removes maximize/minimize capabilities from the native window
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnSourceInitialized(EventArgs eventArgs) {
      base.OnSourceInitialized(eventArgs);

      if (PresentationSource.FromVisual(this) is HwndSource source) {
        this.helper = new GrabberWindowHelper(source.Handle);
        source.AddHook(this.helper.WndProc);
      }
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
