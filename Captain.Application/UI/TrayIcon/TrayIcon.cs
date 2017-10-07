using System;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Icon shown on the notification area from which the user can access diverse actions and settings
  /// </summary>
  internal class TrayIcon {
    /// <summary>
    ///   Renders tray icons
    /// </summary>
    private readonly IIndicatorRenderer iconRenderer;

    /// <summary>
    ///   Current indicator status
    /// </summary>
    private IndicatorStatus currentStatus;

    /// <summary>
    ///   Whether the current indicator is animated or not
    /// </summary>
    private bool isIndicatorAnimated;

    /// <summary>
    ///   Animation thread
    /// </summary>
    private Thread indicatorAnimationThread;

    /// <summary>
    ///   Hint circle lets the user know where to start when first opening the application
    /// </summary>
    private readonly TrayIconHintCircle hintCircle;

    /// <summary>
    ///   Exposes underlying NotifyIcon
    /// </summary>
    internal NotifyIcon NotifyIcon { get; }

    /// <summary>
    ///   Instantiates a new TrayIcon
    /// </summary>
    internal TrayIcon() {
      var contextMenu = new ContextMenu();
      NotifyIcon = new NotifyIcon { ContextMenu = contextMenu };
      NotifyIcon.MouseDown += (_, __) => this.hintCircle?.Close();

      contextMenu.MenuItems.AddRange(new[] {
        new MenuItem(Resources.AppMenu_Capture) {
          DefaultItem = true,
          Visible = true
        },
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Options),
        new MenuItem(Resources.AppMenu_About, (_, __) => new AboutWindow().Show()),
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Exit, (_, __) => Exit())
      });

      // get the platform-dependent indicator style variant
      if (Environment.OSVersion.Version.Major > 6) {
        this.iconRenderer = new FluentIndicatorRenderer();
      } else if (Environment.OSVersion.Version.Minor > 2) {
        throw new NotImplementedException("Icon renderer for Windows 8/8.1");
      } else {
        throw new NotImplementedException("Icon renderer for Windows Vista/7");
      }

      // set initial icon
      SetIndicator(IndicatorStatus.Idle);

      // display the tray icon
      Show();

      // this is the first time the user uses the app - highlight the tray icon so the user knows where to start
      if (true) {
        RECT iconRect = GetIconRect();

        this.hintCircle = new TrayIconHintCircle();
        this.hintCircle.Show();
        this.hintCircle.Left = iconRect.left;
        this.hintCircle.Top = iconRect.top;
        this.hintCircle.Width = this.hintCircle.Height = Math.Max(iconRect.right - iconRect.left, iconRect.bottom - iconRect.top);
      }
    }

    /// <summary>
    ///   TrayIcon instance destructor
    /// </summary>
    ~TrayIcon() {
      Log.WriteLine(LogLevel.Debug, "releasing resources");
      Hide();
      NotifyIcon.Dispose();
    }

    /// <summary>
    ///   Stops indicator animation
    /// </summary>
    private void StopIndicatorAnimation() {
      this.isIndicatorAnimated = false;

      this.indicatorAnimationThread?.Join();
      this.indicatorAnimationThread = null;
    }

    /// <summary>
    ///   Displays the tray icon
    /// </summary>
    private void Show() => NotifyIcon.Visible = true;

    /// <summary>
    ///   Hides the tray icon
    /// </summary>
    internal void Hide() => NotifyIcon.Visible = false;

    /// <summary>
    ///   Updates the current indicator status/animation frame
    /// </summary>
    /// <param name="status">Indicator status</param>
    internal void SetIndicator(IndicatorStatus status) {
      if (status != this.currentStatus && this.isIndicatorAnimated) { StopIndicatorAnimation(); }
      NotifyIcon.Icon = this.iconRenderer.RenderFrame(this.currentStatus = status);
    }

    /// <summary>
    ///   Animates the indicator with the specified status until another indicator is set
    /// </summary>
    /// <param name="status">Indicator status</param>
    /// <param name="frameTtl">Animation frame lifetime</param>
    internal void AnimateIndicator(IndicatorStatus status, int frameTtl = 100) {
      StopIndicatorAnimation();
      SetIndicator(status);

      this.isIndicatorAnimated = true;

      (this.indicatorAnimationThread = new Thread(() => {
        while (this.isIndicatorAnimated) {
          Thread.Sleep(frameTtl);
          SetIndicator(status);
        }
      })).Start();
    }

    /// <summary>
    ///   Updates the current indicator status for a specified amount of time and then sets back the previous status
    /// </summary>
    /// <param name="status">Indicator status</param>
    /// <param name="ttl">Indicator lifetime</param>
    internal void SetTimedIndicator(IndicatorStatus status, int ttl = 5_000) {
      SetIndicator(status);

      new Thread(() => {
        Thread.Sleep(ttl);
        SetIndicator(IndicatorStatus.Idle);
      }).Start();
    }

    /// <summary>
    ///   Tries to obtain the bounds of the notify icon
    /// </summary>
    /// <returns>A <see cref="RECT"/> structure containing the notify icon rectangle</returns>
    /// <exception cref="InvalidOperationException">Thrown when some data could not be retrieved</exception>
    /// <exception cref="Win32Exception">Thrown when the shell fails to obtain the notify icon rectangle</exception>
    private RECT GetIconRect() {
      /* get notify icon handle */
      // get "window" field
      FieldInfo windowField = typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
      if (windowField == null) {
        throw new InvalidOperationException("Could not retrieve window field for NotifyIcon");
      }

      // get native window instance
      var window = windowField.GetValue(NotifyIcon) as NativeWindow;
      if (window == null) {
        throw new InvalidOperationException("Could not retrieve native window instance for NotifyIcon");
      }

      /* get notify icon ID */
      // get "id" field
      FieldInfo idField = typeof(NotifyIcon).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);
      if (idField == null) {
        throw new InvalidOperationException("Could not retrieve ID field for NotifyIcon");
      }

      // create notify icon identifier structure
      var notifyIconId = new NOTIFYICONIDENTIFIER {
        cbSize = Marshal.SizeOf(typeof(NOTIFYICONIDENTIFIER)),
        hWnd = window.Handle,
        uID = (int)idField.GetValue(NotifyIcon)
      };

      int result = Shell32.NotifyIconGetRect(ref notifyIconId, out RECT rect);
      if (result == 0) {
        if (rect.bottom == Screen.PrimaryScreen.Bounds.Height) {
          // HACK: tray area is only displayed on the primary screen - if the icon is NOT on the fly-out window then
          //       substract 8px so the hint circle fits perfectly
          rect.left -= 8;
        }

        return rect;
      }

      if (result == 1) {
        // perhaps the icon is hidden in the tray fly-out - we want to find that lil' son of a beach
        IntPtr trayWindowHandle = User32.FindWindow("Shell_TrayWnd");

        if (trayWindowHandle != IntPtr.Zero) {
          IntPtr trayNotifyWindowHandle = User32.FindWindowEx(trayWindowHandle, lpszClass: "TrayNotifyWnd");

          if (trayNotifyWindowHandle != IntPtr.Zero) {
            // we found it, try to find the show/hide icons button
            IntPtr notifyFlyOutButtonHandle = User32.FindWindowEx(trayNotifyWindowHandle, lpszClass: "Button");

            if (notifyFlyOutButtonHandle != IntPtr.Zero &&
                User32.GetWindowRect(notifyFlyOutButtonHandle, out RECT flyOutRect)) {
              // got it!
              flyOutRect.left -= 8;
              return flyOutRect;
            }
          }
        }
      }

      // something went wrong
      throw new Win32Exception(result);
    }
  }
}