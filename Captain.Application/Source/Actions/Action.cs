using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Captain.Application.NativeHelpers;
using Captain.Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using static Captain.Application.Application;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Represents an action, which is a set of capture provider, handler and encoder
  /// </summary>
  internal class Action {
    /// <summary>
    ///   Available action types
    /// </summary>
    internal ActionType ActionTypes {
      get {
        ActionType types = ActionType.None;

        if (StaticEncoder != null) {
          types |= ActionType.Screenshot;
        }

        if (VideoEncoder != null) {
          types |= ActionType.Record;
        }

        return types;
      }
    }

    /// <summary>
    ///   Static encoder object (may be null)
    /// </summary>
    internal PluginObject StaticEncoder { get; }

    /// <summary>
    ///   Video encoder object (may be null)
    /// </summary>
    internal PluginObject VideoEncoder { get; }

    /// <summary>
    ///   Output streams
    /// </summary>
    internal List<PluginObject> OutputStreams { get; } = new List<PluginObject>();

    /// <summary>
    ///   When set to <c>true</c>, the data will be encoded as it's captured
    /// </summary>
    internal bool ParallelEncoding { get; }

    /// <summary>
    ///   Creates a new action
    /// </summary>
    /// <param name="staticEncoder">Static encoder</param>
    /// <param name="videoEncoder">Video encoder</param>
    internal Action(PluginObject staticEncoder = null, PluginObject videoEncoder = null) =>
      (StaticEncoder, VideoEncoder) = (staticEncoder, videoEncoder);

    /// <summary>
    ///   Adds an output stream to this action
    /// </summary>
    /// <param name="stream">Stream plugin object</param>
    internal void AddOutputStream(PluginObject stream) => OutputStreams.Add(stream);

    /// <summary>
    ///   Starts this action
    /// </summary>
    /// <param name="intent">Capture intent</param>
    /// <exception cref="InvalidOperationException">Thrown when called with no handlers set up</exception>
    internal void Start(CaptureIntent intent) {
      if (!OutputStreams.Any()) { throw new InvalidOperationException("No streams were set for this action."); }

      if (intent.ActionType == ActionType.Screenshot) {
        ScreenCaptureProvider captureProvider;

        if (intent.Monitor != -1) {
          captureProvider = new GDIScreenCaptureProvider(intent.Monitor);
        } else {
          captureProvider = new GDIScreenCaptureProvider(intent.VirtualArea.X,
                                                         intent.VirtualArea.Y,
                                                         intent.VirtualArea.Width,
                                                         intent.VirtualArea.Height);
        }

        try {
          if (Activator.CreateInstance(StaticEncoder.Type) as IStaticEncoder is IStaticEncoder encoder) {
            IEnumerable<CaptureResult> results = EncodeStatic(captureProvider.CaptureBitmap(), encoder);
            ;
            /*Stream stream = GetOutputStream(encoder);
            encoder.Encode(captureProvider.CaptureBitmap(), stream);
            stream.Close();

            if (Application.Options.KeepLocalCopy && this.localCaptureFileName is null) {
              try {
                using (var fs = new FileStream(this.localCaptureFileName = GetLocalFileName(encoder),
                                               FileMode.CreateNew,
                                               FileAccess.Write)) { }
              } catch (Exception exception) {
                Log.WriteLine(LogLevel.Error, $"error opening file stream: {exception}");
                this.localCaptureFileName = null;
              }
            }*/
          } else {
            throw new InvalidOperationException("Static encoder activation yielded a nil result.");
          }
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"capture/encode error: {exception}");
          DisplayErrorToast(Resources.Toast_CaptureFailedCaption, Resources.Toast_CaptureFailedContent, exception);
        } finally {
          captureProvider.Dispose();
        }
      } else if (intent.ActionType == ActionType.Record) {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Selects or creates a <see cref="Stream"/> for the encoder to write its output and wraps the rest under an enumerable type.
    /// </summary>
    /// <returns>
    ///   A tuple containing the "master" stream as first value and the rest on the second value. A third value containing the streams
    ///   that failed to be initialized is also included
    /// </returns>
    private (Stream MasterStream, IEnumerable<Stream> OutputStreams, IEnumerable<(PluginObject OutputStream, Exception Exception)> Failed) ArrangeOutputStreams() {
      Stream masterStream;
      List<PluginObject> streams = OutputStreams;

      try {
        PluginObject preferred = streams.OrderBy(os => os.Type.GetNestedType("MemoryStream") is null)
                                        .First();

        // select the first output stream to act as a "master", preferring MemoryStream-based ones as they are usually faster [Citation needed]
        masterStream = Activator.CreateInstance(preferred.Type) as Stream;
        streams.Remove(preferred);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not create master stream - falling back to MemoryStream: {exception}");
        masterStream = new MemoryStream();
      }

      // instantiate all streams and return the result
      var failed = new List<(PluginObject OutputStream, Exception Exception)>();
      return (masterStream, streams.Select(o => {
        try {
          return Activator.CreateInstance(o.Type) as Stream;
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"could not initialize output stream \"{o}\": {exception}");
          failed.Add((o, exception));

          return null;
        }
      }).Where(s => s != null), failed);
    }

    /// <summary>
    ///   Encodes a still capture and writes the data to all the output streams
    /// </summary>
    /// <param name="bmp"><see cref="Bitmap"/> instance</param>
    /// <param name="encoder">An static encoder instance</param>
    /// <returns>A list containing the results for all output streams</returns>
    private IEnumerable<CaptureResult> EncodeStatic(Bitmap bmp, IStaticEncoder encoder) {
      (Stream MasterStream, IEnumerable<Stream> OutputStreams, IEnumerable<(PluginObject OutputStream, Exception Exception)> Failed) streams = ArrangeOutputStreams();
      var results = new List<CaptureResult>();

      encoder.Encode(bmp, streams.MasterStream);

      // take every output stream instance
      (streams.OutputStreams.Select(s => new Thread(() => {
        // copy the data from the master stream
        // ReSharper disable once AccessToDisposedClosure
        // although MasterStream is disposed below, it is guaranteed to never be disposed at this point,
        // for we block the calling thread while waiting for all threads to end
        streams.MasterStream.CopyTo(s);

        // commit and receive CaptureResult, then release stream
        if (((IOutputStream)s).Commit() is CaptureResult result) {
          results.Add(result);
        }
        s.Dispose();
      })) as List<Thread>)?.ForEach(t => t.Join());  // wait for all threads to finish

      // release master
      if (((IOutputStream)streams.MasterStream).Commit() is CaptureResult masterResult) {
        results.Add(masterResult);
      }

      streams.MasterStream.Dispose();
      return results;
    }

    private void DisplayErrorToast(string caption, string body, Exception exception) =>
      LegacyNotificationProvider.PushMessage(caption, body, ToolTipIcon.Warning, handler: (_, __) => {
        if (TaskDialog.IsPlatformSupported) {
          var dialog = new TaskDialog {
            InstructionText = caption,
            Caption = VersionInfo.ProductName,
            Text = body,
            StandardButtons = TaskDialogStandardButtons.Close,
            ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter,
            DetailsExpanded = false,
            DetailsExpandedLabel = Resources.GenericActionErrorDialog_HideDetails,
            DetailsCollapsedLabel = Resources.GenericActionErrorDialog_ShowDetails,
            DetailsExpandedText = exception.Message
          };

          dialog.Opened += (sender, eventArgs) =>
              // ReSharper disable once AccessToDisposedClosure
              // HACK: setting Icon property before the dialog is open has no effect
              dialog.Icon = TaskDialogStandardIcon.Warning;

          dialog.Show();
        } else {
          Log.WriteLine(LogLevel.Warning, "falling back to MessageBox");
          MessageBox.Show(caption +
                          Environment.NewLine +
                          body +
                          Environment.NewLine +
                          Environment.NewLine +
                          exception.Message,
                          VersionInfo.ProductName,
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Warning);
        }
      });
  }
}
