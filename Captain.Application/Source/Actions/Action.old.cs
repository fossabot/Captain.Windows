using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management.Instrumentation;
using System.Threading;
using System.Windows.Forms;
using Windows.UI.Notifications;
using Captain.Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using static Captain.Application.Application;
using Guid = System.Guid;

namespace Captain.Application {
  /// <summary>
  ///   Represents an action, which is the set of capture provider, handler and encoder
  /// </summary>
  internal class Action {
    /// <summary>
    ///   Provider instance
    /// </summary>
    private CaptureProvider CaptureProvider { get; set; }

    /// <summary>
    ///   Handler instance
    /// </summary>
    private CaptureHandler CaptureHandler { get; set; }

    /// <summary>
    ///   Encoder instance
    /// </summary>
    private Encoder Encoder { get; set; }

    /// <summary>
    ///   Progressive encoder instance, if any
    /// </summary>
    private Encoder ProgressiveEncoder { get; set; }

    /// <summary>
    ///   Whether or not the Encoder/CaptureProvider supports progressive capture
    /// </summary>
    internal bool SupportsProgressiveCapture => CaptureProvider.SupportsRecording;

    /// <summary>
    ///   Get an instance of the default action
    /// </summary>
    /// <param name="provider">Optional <see cref="CaptureProvider"/> instance. If omitted, the default one will be used</param>
    /// <returns>An <see cref="Action"/> instance</returns>
    internal static Action GetDefault(CaptureProvider provider = null) {
      var action = new Action {
        CaptureProvider = provider
      };
      if (action.CaptureProvider is null) {
        CaptureProvider[] providers = Application.PluginManager.EnumerateCaptureProviders().ToArray();
        if (providers.FirstOrDefault(p => p.TypeName == Application.Options.DefaultProvider) is CaptureProvider defaultProvider) {
          action.CaptureProvider = defaultProvider;
        } else if (providers.Length > 0 && providers.First() is CaptureProvider availableProvider) {
          action.CaptureProvider = availableProvider;
        } else {
          throw new InstanceNotFoundException("No instance of the default CaptureProvider could be created");
        }
      }

      CaptureHandler[] handlers = Application.PluginManager.EnumerateCaptureHandlers().ToArray();
      if (handlers.FirstOrDefault(h => h.TypeName == Application.Options.DefaultHandler) is CaptureHandler handler) {
        action.CaptureHandler = handler;
      } else if (handlers.Length > 0 && handlers.First(h => h.Instance.GetType().GetGenericParameterConstraints()[0] == action.CaptureProvider.Instance.GetType().GetGenericParameterConstraints()[0]) is CaptureHandler availableHandler) {
        action.CaptureHandler = availableHandler;
      } else {
        throw new InstanceNotFoundException("No instance of the default CaptureHandler could be created");
      }

      Encoder[] encoders = Application.PluginManager.EnumerateEncoders().ToArray();
      if (encoders.FirstOrDefault(e => e.TypeName == Application.Options.DefaultEncoder) is Encoder encoder) {
        action.Encoder = encoder;
      } else if (encoders.Length > 0 && encoders.First() is Encoder availableEncoder) {
        action.Encoder = availableEncoder;
      } else {
        throw new InstanceNotFoundException("No instance of the default Encoder could be created");
      }

      if (action.SupportsProgressiveCapture) {
        /*if (encoders.FirstOrDefault(e => e.TypeName == Application.Options.DefaultEncoder) is Encoder encoder) {
          action.Encoder = encoder;
        } else*/
        if (encoders.Length > 0 && encoders.First(e => e.Instance.GetType().GetInterfaces().Any(i => i.Name == "IProgressiveEncoder")) is Encoder availableProgressiveEncoder) {
          action.ProgressiveEncoder = availableProgressiveEncoder;
        } else {
          throw new InstanceNotFoundException("No instance of the default progressive Encoder could be created");
        }
      }

      return action;
    }

    private ProgressiveState progressiveState;

    internal void InitializeProgressive(Rectangle? area) {
      if (!SupportsProgressiveCapture) {
        throw new InvalidOperationException("The CaptureProvider of this Action does not support recording");
      }

      string fileName;
      if (Application.Options.KeepLocalCopy) {
        fileName = Path.Combine(Application.Options.LocalDirectory,
                                DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss") +
                                "." +
                                ProgressiveEncoder.Instance.FileExtension.TrimStart('.'));
      } else {
        fileName = Path.Combine(Application.FsManager.GetSafePath(FsManager.TemporaryPath), Guid.NewGuid().ToString());
      }

      this.progressiveState =
        new ProgressiveState(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)) {
          Wait = 50,
          Recording = true
        };
      (ProgressiveEncoder.Instance as IProgressiveEncoder).Initialize(area.Value.Size, this.progressiveState.OutputStream);

      while (this.progressiveState.Recording) {
        object capture = CaptureProvider.CaptureMethod.Invoke(CaptureProvider.Instance, new object[] { area });
        ProgressiveEncoder.Encode.Invoke(ProgressiveEncoder.Instance, new[] { capture, this.progressiveState.OutputStream });
        System.Windows.Forms.Application.DoEvents();
      }
    }

    internal void FinalizeProgressive() {
      if (!SupportsProgressiveCapture) {
        throw new InvalidOperationException("The CaptureProvider of this Action does not support recording");
      }

      this.progressiveState.Recording = false;
      (ProgressiveEncoder.Instance as IProgressiveEncoder).Finalize(this.progressiveState.OutputStream);
      this.progressiveState.OutputStream.Flush();
      this.progressiveState.OutputStream.Dispose();
    }

    /// <summary>
    ///   Performs the underlying operations for this capture
    /// </summary>
    /// <param name="area">A nullable area supposedly selected by the user</param>
    private void PerformEx(Rectangle? area) {
      Log.WriteLine(LogLevel.Debug, "performing underlying action");

      // capture
      object capture = CaptureProvider.CaptureMethod.Invoke(CaptureProvider.Instance, new object[] { area });
      Log.WriteLine(LogLevel.Debug, $"provider '{CaptureProvider.DisplayName}' yielded a {capture.GetType().Name}");

      // encode
      using (var stream = new MemoryStream()) {
        Encoder.Encode.Invoke(Encoder.Instance, new[] { capture, stream });
        Log.WriteLine(LogLevel.Debug, $"encoding suceeded - encoder '{Encoder.DisplayName}' wrote {stream.Length} bytes");

        // handle
        var result = (CaptureResult)CaptureHandler.Handle.Invoke(CaptureHandler.Instance, new object[] { stream });
        Log.WriteLine(LogLevel.Debug, $"handler URI: {result.ToastUri}");

        Uri captureUri = result.ToastUri;
        if (Application.Options.KeepLocalCopy) {
          string fileName = Path.Combine(Application.Options.LocalDirectory,
                                         DateTime.Now.ToString("dd-MM-yyyy hh.mm.ss") + "." +
                                         Encoder.Instance.FileExtension.TrimStart('.')),
                 subtext = Resources.Toast_LocalSaveFailedSubText;
          Log.WriteLine(LogLevel.Verbose, $"saving local copy as: {fileName}");

          try {
            trySave:
            try {
              using (var fileStream = new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Write)) {
                stream.Seek(0, SeekOrigin.Begin);
                stream.CopyTo(fileStream);
                fileStream.Flush(true);
                subtext = null;
              }
            } catch (DirectoryNotFoundException directoryNotFoundException) {
              Log.WriteLine(LogLevel.Warning,
                            $"could not find local directory: {directoryNotFoundException} - trying to create directory");

              try {
                Directory.CreateDirectory(Application.Options.LocalDirectory);
                goto trySave;
              } catch (Exception exception) {
                Log.WriteLine(LogLevel.Error, $"operation failed: {exception} - ignoring");
              }
            }

            if (captureUri is null) {
              captureUri = new Uri(fileName);
            }

            ToastProvider.PushObject(result.ToastTitle, result.ToastContent, subtext, previewUri: captureUri,
                                     actions: new Dictionary<string, Uri> {
                                       { Resources.Toast_Open, captureUri },
                                       { Resources.Toast_ViewInFolder, new UriBuilder(captureUri) { Query = "select" }.Uri }
                                     }, handler: (sender, eventArgs) => {
                                       if (eventArgs is ToastActivatedEventArgs toastEventArgs) {
                                         if (String.IsNullOrEmpty(toastEventArgs.Arguments)) {
                                           Log.WriteLine(LogLevel.Warning, "attempting to launch an empty URI from toast");
                                           return;
                                         }

                                         var uri = new Uri(toastEventArgs.Arguments);
                                         if (uri.IsFile && uri.Query.StartsWith("?select")) {
                                           Log.WriteLine(LogLevel.Debug, $"launching File Explorer window with selected: {toastEventArgs.Arguments}");
                                           Process.Start("explorer", $"/select, \"{toastEventArgs.Arguments}\"");
                                         } else {
                                           Log.WriteLine(LogLevel.Debug, $"launching URI: {toastEventArgs.Arguments}");
                                           Process.Start(toastEventArgs.Arguments);
                                         }
                                       }
                                     });
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"could not save local copy: {exception}");

            if (captureUri is null) {
              Log.WriteLine(LogLevel.Warning, "received a null capture URI");
              LegacyNotificationProvider.PushMessage(Resources.Toast_LocalSaveFailedCaption,
                                                     Resources.Toast_LocalSaveFailedContent, ToolTipIcon.Warning,
                                                     handler: (_, __) => {
                                                       if (TaskDialog.IsPlatformSupported) {
                                                         var dialog = new TaskDialog {
                                                           Caption = VersionInfo.ProductName,
                                                           HyperlinksEnabled = true,
                                                           InstructionText = Resources.Toast_LocalSaveFailedCaption,
                                                           Text = Resources.Toast_LocalSaveFailedContent,
                                                           StandardButtons = TaskDialogStandardButtons.Close,
                                                           ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter,
                                                           DetailsExpanded = false,
                                                           DetailsExpandedLabel = Resources.LocalSaveFailed_HideErrorDetails,
                                                           DetailsCollapsedLabel = Resources.LocalSaveFailed_ShowErrorDetails,
                                                           DetailsExpandedText = exception.Message,
                                                         };

                                                         dialog.Opened += (sender, eventArgs) => {
                                                           // ReSharper disable once AccessToDisposedClosure
                                                           dialog.Icon = TaskDialogStandardIcon.Warning;
                                                         };

                                                         dialog.Show();
                                                         dialog.Dispose();
                                                       } else {
                                                         Log.WriteLine(LogLevel.Debug, "displaying compatible MessageBox");
                                                         MessageBox.Show(Resources.Toast_LocalSaveFailedCaption + Environment.NewLine + Resources.Toast_LocalSaveFailedContent + Environment.NewLine +
                                                                         exception.Message, VersionInfo.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                                       }
                                                     });
            } else {
              ToastProvider.PushObject(result.ToastTitle, result.ToastContent, Resources.Toast_LocalSaveFailedSubText, previewImage: result.ToastPreview, actions: new Dictionary<string, Uri> {
                { Resources.Toast_Open, captureUri }
              });
            }
          }
        } else {
          ToastProvider.PushObject(result.ToastTitle, result.ToastContent, previewImage: result.ToastPreview, actions: new Dictionary<string, Uri> {
            { Resources.Toast_Open, captureUri }
          });
        }
      }
    }

    /**
     * Performs a single capture with the specified area
     */
    internal void Perform(Rectangle area) => PerformEx(area);

    /// <summary>
    ///   Performs this action
    /// </summary>
    internal void Perform() {
      Log.WriteLine(LogLevel.Verbose, $"performing action (provider: {CaptureProvider}; handler: {CaptureHandler}; encoder: {Encoder})");

      if (CaptureProvider.RequestsArea) {
        Log.WriteLine(LogLevel.Verbose, "instantiating grabber as the current provider is requesting user selection");
        var grabber = new Grabber(this);

        grabber.Show(result => {
          Log.WriteLine(LogLevel.Debug, $"grabber result: {result}");

          if (result == true) {
            grabber.Hide();
            PerformEx(grabber.Area);
            grabber.Dispose();
          }
        });
      } else {
        PerformEx(null);
      }
    }
  }
}
