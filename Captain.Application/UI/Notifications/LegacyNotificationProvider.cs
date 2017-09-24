using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Implements an interface for displaying local notifications by using legacy NotifyIcon APIs
  /// </summary>
  internal class LegacyNotificationProvider : IToastProvider {
    /// <summary>
    ///   Timeout for balloon tips - ignored since Windows Vista
    /// </summary>
    private const int BalloonTipDuration = 5000;

    /// <summary>
    ///   Displays a notification
    /// </summary>
    /// <param name="title">Balloon tip title</param>
    /// <param name="text">Balloon tip text</param>
    /// <param name="icon">Balloon tip icon</param>
    /// <param name="uri">Optional URI to be open on click</param>
    /// <param name="handler">Custom handler callback</param>
    /// <param name="closeHandler">Custom closing handler callback</param>
    internal static void PushMessage(string title,
                                     string text,
                                     ToolTipIcon icon,
                                     Uri uri = null,
                                     EventHandler handler = null,
                                     EventHandler closeHandler = null) {
      Log.WriteLine(LogLevel.Debug, $"pushing generic message as balloon notification ({icon} | {uri})");
      Application.TrayIcon.NotifyIcon.ShowBalloonTip(BalloonTipDuration, title, text, icon);

      if (uri != null || handler != null) {
        EventHandler originalCloseHandler = closeHandler;

        Log.WriteLine(LogLevel.Debug, "non-null handler or action URI was provided");
        Application.TrayIcon.NotifyIcon.BalloonTipClicked +=
          handler ?? (handler = (_, __) => Process.Start(uri.ToString()));
        Application.TrayIcon.NotifyIcon.BalloonTipClosed += closeHandler = (sender, eventArgs) => {
          // detach event handlers
          Application.TrayIcon.NotifyIcon.BalloonTipClicked -= handler;

          // ReSharper disable once AccessToModifiedClosure
          // NOTE: there's no other way to detach event handlers, but it's 120% safe.
          //       Or so I hope
          Application.TrayIcon.NotifyIcon.BalloonTipClosed -= closeHandler;

          // invoke original close handler, if any
          originalCloseHandler?.Invoke(sender, eventArgs);
        };

        new Thread(() => {
          Thread.Sleep(BalloonTipDuration);
          Application.TrayIcon.NotifyIcon.BalloonTipClicked -= handler;
          Application.TrayIcon.NotifyIcon.BalloonTipClosed -= closeHandler;
        }).Start();
      }
    }

    /// <summary>
    ///   Displays a warning message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="actions">
    ///   A dictionary of actions, from which the first one may be triggered upon balloon click
    /// </param>
    public void PushWarning(string caption, string body, Dictionary<string, Uri> actions = null) =>
      PushMessage(caption, body, ToolTipIcon.Warning, actions?.FirstOrDefault().Value);

    /// <summary>
    ///   Displays an informational/success message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="subtext">Optional subtext</param>
    /// <param name="previewUri">Ignored</param>
    /// <param name="previewImage">Ignored</param>
    /// <param name="actions">
    ///   A dictionary of actions, from which the first one may be triggered upon balloon click
    /// </param>
    /// <param name="handler">Handles activation events</param>
    /// <param name="dismissionHandler">Handles notification dismission</param>
    public void PushObject(string caption,
                           string body,
                           string subtext = null,
                           Uri previewUri = null,
                           Image previewImage = null,
                           Dictionary<string, Uri> actions = null,
                           Action<object, object> handler = null,
                           Action<object, object> dismissionHandler = null) =>
      PushMessage(caption,
                  (body + Environment.NewLine + (subtext ?? "")).Trim(),
                  ToolTipIcon.None,
                  actions?.FirstOrDefault().Value,
                  (sender, args) => {
                    handler?.Invoke(sender, args);
                    dismissionHandler?.Invoke(sender, args);
                  });
  }
}
