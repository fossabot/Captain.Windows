using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Toolkit.Uwp.Notifications;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;
using Captain.Common;
using static Captain.Application.Application;
using Guid = System.Guid;

namespace Captain.Application {
  /// <summary>
  ///   Implements an interface for displaying local notifications by using legacy NotifyIcon APIs
  /// </summary>
  internal class ToastNotificationProvider : IToastProvider {
    /// <summary>
    ///   Toast notifier instance
    /// </summary>
    private readonly ToastNotifier toastNotifier = ToastNotificationManager.CreateToastNotifier(VersionInfo.ProductName);

    /// <summary>
    ///   Displays a warning message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="actions">A dictionary of actions, from which the first one may be triggered upon balloon click</param>
    public void PushWarning(string caption, string body, Dictionary<string, Uri> actions = null) => LegacyNotificationProvider.PushMessage(caption, body, ToolTipIcon.Warning, actions?.FirstOrDefault().Value);

    /// <summary>
    ///   Displays an informational/success message
    /// </summary>
    /// <param name="caption">Balloon tip title</param>
    /// <param name="body">Balloon tip text</param>
    /// <param name="subtext">Optional subtext</param>
    /// <param name="previewUri">Preview URI</param>
    /// <param name="previewImage">Preview image</param>
    /// <param name="actions">A dictionary of actions, from which the first one may be triggered upon balloon click</param>
    /// <param name="handler">Custom handler for toast activation</param>
    /// <param name="dismissionHandler">Custom handler for toast dismission</param>
    public void PushObject(string caption, string body, string subtext = null, Uri previewUri = null, Image previewImage = null, Dictionary<string, Uri> actions = null,
      Action<object, object> handler = null, Action<object, object> dismissionHandler = null) {
      string previewSource = previewUri?.ToString();
      bool isTemporaryPreviewSource = false;

      if (previewSource == null && previewImage != null) {
        previewSource = Path.Combine(Application.FsManager.GetSafePath(FsManager.TemporaryPath), Guid.NewGuid().ToString());
        isTemporaryPreviewSource = true;

        Log.WriteLine(LogLevel.Verbose, $"no preview URI but preview image present - saving to temporary path: {previewSource}");
        previewImage.Save(previewSource);
      }

      var content = new ToastContent {
        Duration = ToastDuration.Long,
        Audio = new ToastAudio { Silent = true },
        Actions = new ToastActionsCustom(),
        Launch = actions != null && actions.Any() ? actions.First().Value.ToString() : null,
        Visual = new ToastVisual {
          BindingGeneric = new ToastBindingGeneric {
            Children = {
              new AdaptiveText { Text = caption, HintStyle = AdaptiveTextStyle.Title },
              new AdaptiveText { Text = body, HintStyle = AdaptiveTextStyle.Body }
            }
          }
        }
      };

      if (subtext != null) {
        // add subtext
        content.Visual.BindingGeneric.Children.Add(new AdaptiveText {
          Text = subtext,
          HintStyle = AdaptiveTextStyle.CaptionSubtle
        });
      }

      if (previewSource != null) {
        if (CompatHelpers.HasAnniversaryUpdate) {
          // enable hero image on anniversary update
          content.Visual.BindingGeneric.HeroImage = new ToastGenericHeroImage {
            Source = previewSource
          };
        } else {
          // stick to the generic adaptive image children
          content.Visual.BindingGeneric.Children.Add(new AdaptiveImage {
            Source = previewSource,
            HintAlign = AdaptiveImageAlign.Center,
            HintCrop = AdaptiveImageCrop.None,
            HintRemoveMargin = true
          });
        }
      }

      if (actions != null) {
        foreach (KeyValuePair<string, Uri> action in actions) {
          if (action.Key != null && action.Value?.ToString() != null) {
            Log.WriteLine(LogLevel.Debug, $"adding toast action {action.Key}: {action.Value}");
            ((ToastActionsCustom)content.Actions).Buttons.Add(new ToastButton(action.Key, action.Value.ToString()));
          }
        }
      }

      var doc = new XmlDocument();
      doc.LoadXml(content.GetContent());

      var notification = new ToastNotification(doc) {
        Priority = ToastNotificationPriority.High,
        SuppressPopup = false
      };

      if (handler != null) {
        notification.Activated += (sender, eventArgs) => handler(sender, eventArgs);
      }

      notification.Dismissed += (sender, eventArgs) => {
        Log.WriteLine(LogLevel.Debug, "notification dismissed");

        // delete temporary preview, if any
        if (previewSource != null && isTemporaryPreviewSource) {
          try {
            File.Delete(previewSource);
            Log.WriteLine(LogLevel.Debug, $"deleted temporary preview image: {previewSource}");
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"could not delete temporary preview image: {previewSource} - {exception}");
          }
        }

        // call custom handler
        dismissionHandler?.Invoke(sender, eventArgs);
      };

      this.toastNotifier.Show(notification);
    }
  }
}
