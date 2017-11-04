﻿using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Icon shown on the notification area from which the user can access diverse actions and settings
  /// </summary>
  internal sealed class TrayIcon {
    /// <summary>
    ///   Renders tray icons
    /// </summary>
    private readonly IIndicatorRenderer iconRenderer;

    /// <summary>
    ///   Current indicator status
    /// </summary>
    private IndicatorStatus currentStatus;

    /// <summary>
    ///   Animation thread
    /// </summary>
    private Thread indicatorAnimationThread;

    /// <summary>
    ///   Whether the current indicator is animated or not
    /// </summary>
    private bool isIndicatorAnimated;

    /// <summary>
    ///   Exposes underlying NotifyIcon
    /// </summary>
    internal NotifyIcon NotifyIcon { get; }

    /// <summary>
    ///   Instantiates a new TrayIcon
    /// </summary>
    internal TrayIcon() {
      var contextMenu = new ContextMenu();
      contextMenu.MenuItems.AddRange(new[] {
        new MenuItem("Take screenshot",
          (s, e) => Application.Options.Tasks[Application.Options.DefaultScreenshotTask].Start()) {
            DefaultItem = true,
            Visible = true
          },
        new MenuItem("Record screen", (s, e) => throw new NotImplementedException()),
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Options,
          (s, e) => {
            try {
              new OptionsWindow().Show();
            } catch (ApplicationException) {
              /* already open */
            }
          }),
        new MenuItem(Resources.AppMenu_About,
          (s, e) => {
            try {
              new AboutWindow().Show();
            } catch (ApplicationException) {
              /*already open */
            }
          }),
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Exit, (s, e) => Exit())
      });

      NotifyIcon = new NotifyIcon {ContextMenu = contextMenu};
      NotifyIcon.MouseClick += (s, e) => {
        if (e.Button == MouseButtons.Left && NotifyIcon.ContextMenu.MenuItems[0].Enabled) {
          NotifyIcon.ContextMenu.MenuItems[0].PerformClick();
        } else if (e.Button == MouseButtons.Middle && NotifyIcon.ContextMenu.MenuItems[1].Enabled) {
          NotifyIcon.ContextMenu.MenuItems[1].PerformClick();
        }
      };

      // display the tray icon
      Show();

      // get the platform-dependent indicator style variant
      if (Environment.OSVersion.Version.Major > 6) {
        this.iconRenderer = new FluentIndicatorRenderer(GetIconHandle());
      } else if (Environment.OSVersion.Version.Minor > 2) {
        // TODO: create assets for Windows 8/8.1
        this.iconRenderer = new AeroIndicatorRenderer(GetIconHandle());
      } else {
        this.iconRenderer = new AeroIndicatorRenderer(GetIconHandle());
      }

      // set initial icon
      SetIndicator(IndicatorStatus.Idle);
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
      if ((status != this.currentStatus) && this.isIndicatorAnimated) { StopIndicatorAnimation(); }
      NotifyIcon.Icon = this.iconRenderer.RenderFrame(this.currentStatus = status);
    }

    /// <summary>
    ///   Animates the indicator with the specified status until another indicator is set
    /// </summary>
    /// <param name="status">Indicator status</param>
    /// <param name="frameTtl">Animation frame lifetime</param>
    internal void AnimateIndicator(IndicatorStatus status, int frameTtl = 60) {
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
    ///   Tries to obtain the handle of the window associated with the notify icon
    /// </summary>
    /// <returns>The window handle</returns>
    /// <exception cref="InvalidOperationException">Thrown when some data could not be retrieved</exception>
    private IntPtr GetIconHandle() {
      /* get notify icon handle */
      // get "window" field
      FieldInfo windowField = typeof(NotifyIcon).GetField("window", BindingFlags.NonPublic | BindingFlags.Instance);
      if (windowField == null) {
        throw new InvalidOperationException("Could not retrieve window field for NotifyIcon");
      }

      // get native window instance
      if (!(windowField.GetValue(NotifyIcon) is NativeWindow window)) {
        throw new InvalidOperationException("Could not retrieve native window instance for NotifyIcon");
      }

      return window.Handle;
    }
  }
}