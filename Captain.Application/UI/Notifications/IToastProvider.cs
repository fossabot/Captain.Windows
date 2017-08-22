using System;
using System.Collections.Generic;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Provides an interface for displaying toast notifications
  /// </summary>
  internal interface IToastProvider {
    /// <summary>
    ///   Displays a warning message
    /// </summary>
    /// <param name="caption">Notification title</param>
    /// <param name="body">Notification body</param>
    /// <param name="actions">A dictionary with the action names and their URIs</param>
    void PushWarning(string caption, string body, Dictionary<string, Uri> actions = null);

    /// <summary>
    ///   Displays an object
    /// </summary>
    /// <param name="caption">Notification title</param>
    /// <param name="body">Notification body</param>
    /// <param name="subtext">Optional subtext</param>
    /// <param name="previewUri">Preview image URI</param>
    /// <param name="previewImage">Preview Image</param>
    /// <param name="actions">A dictionary with the action names and their URIs</param>
    /// <param name="handler">Handles activation events</param>
    void PushObject(string caption, string body, string subtext = null, Uri previewUri = null, Image previewImage = null, Dictionary<string, Uri> actions = null, Action<object, object> handler = null);
  }
}
