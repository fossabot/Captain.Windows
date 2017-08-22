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
    internal static void PushMessage(string title, string text, ToolTipIcon icon, Uri uri = null, EventHandler handler = null) {
      Log.WriteLine(LogLevel.Debug, $"pushing generic message as balloon notification ({icon} | {uri})");
      Application.TrayIcon.NotifyIcon.ShowBalloonTip(BalloonTipDuration, title, text, icon);

      if (uri != null || handler != null) {
        Log.WriteLine(LogLevel.Debug, "non-null handler or action URI was provided");
        Application.TrayIcon.NotifyIcon.BalloonTipClicked += handler ?? (handler = (sender, args) => Process.Start(uri.ToString()));

        new Thread(() => {
          Thread.Sleep(BalloonTipDuration);
          Application.TrayIcon.NotifyIcon.BalloonTipClicked -= handler;
        }).Start();
      }
    }

    /// <summary>
    ///   Displays a warning message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="actions">A dictionary of actions, from which the first one may be triggered upon balloon click</param>
    public void PushWarning(string caption, string body, Dictionary<string, Uri> actions = null) => PushMessage(caption, body, ToolTipIcon.Warning, actions?.FirstOrDefault().Value);

    /// <summary>
    ///   Displays an informational/success message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="subtext">Optional subtext</param>
    /// <param name="previewUri">Ignored</param>
    /// <param name="previewImage">Ignored</param>
    /// <param name="actions">A dictionary of actions, from which the first one may be triggered upon balloon click</param>
    /// <param name="handler">Handles activation events</param>
    public void PushObject(string caption, string body, string subtext = null, Uri previewUri = null, Image previewImage = null, Dictionary<string, Uri> actions = null, Action<object, object> handler = null) =>
      PushMessage(caption, (body + Environment.NewLine + (subtext ?? "")).Trim(), ToolTipIcon.None, actions?.FirstOrDefault().Value, (sender, args) => handler?.Invoke(sender, args));
  }
}
