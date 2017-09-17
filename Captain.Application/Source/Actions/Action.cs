using Captain.Application.Native;
using Captain.Common;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using Windows.UI.Notifications;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Represents an action, which is a set of capture provider, handler and encoder
  /// </summary>
  internal class Action {
    /// <summary>
    ///   URI for the "View results" action on toast notifications
    /// </summary>
    private static readonly Uri ToastViewResultsUri = new Uri("captain://action/view/results-info");

    /// <summary>
    ///   URI for the "View errors" action on toast notification
    /// </summary>
    private static readonly Uri ToastViewErrorsUri = new Uri("captain://action/view/errors");

    /// <summary>
    ///   Grabber UI instance bound to this action
    /// </summary>
    private Grabber boundGrabber;

    /// <summary>
    ///   Capture helper instance
    /// </summary>
    private CaptureHelper captureHelper;

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
    ///   Binds a grabber UI to this action
    /// </summary>
    /// <param name="grabber">The <see cref="Grabber"/> instance</param>
    /// <exception cref="InvalidOperationException">Thrown when a grabber has already been bound</exception>
    internal void BindGrabberUi(Grabber grabber) {
      if (this.boundGrabber != null) {
        throw new InvalidOperationException("A Grabber UI has already been bound to this Action instance.");
      }

      this.boundGrabber = grabber;
      this.boundGrabber.OnIntentReceived += (_, intent) => Start(intent);
      this.boundGrabber.Show();
    }

    /// <summary>
    ///   Starts this action
    /// </summary>
    /// <param name="intent">Capture intent</param>
    /// <exception cref="InvalidOperationException">Thrown when called with no handlers set up</exception>
    internal void Start(CaptureIntent intent) {
      if (!OutputStreams.Any()) {
        throw new InvalidOperationException("No streams were set for this action.");
      }

      if (this.captureHelper is null) {
        // instantiate capture helper shall it be required
        this.captureHelper = new CaptureHelper();
      }

      if (intent.ActionType == ActionType.Screenshot) {
        try {
          if (Activator.CreateInstance(StaticEncoder.Type) as IStaticEncoder is IStaticEncoder encoder) {
            Application.TrayIcon.PlayLoopingIconAnimation();

            // perform screen capture
            Bitmap bmp = this.captureHelper.CaptureFromScreen(intent.VirtualArea);

            // encode static image and obtain task results
            var results = (List<CaptureResult>)EncodeStatic(bmp, encoder);

            // get number of failed tasks
            int failedCount = results.Count(r => r is UnsuccessfulCaptureResult);
            Application.TrayIcon.StopLoopingIconAnimation();

            Uri previewUri = null;

            try {
              // get local preview URI
              previewUri = results.First(r => r.ToastUri?.IsFile ?? false).ToastUri;
            } catch (InvalidOperationException) {
              /* no preview URI for anyone kiddo */
            }

            if (results.Count == 1) {
              if (results.First() is UnsuccessfulCaptureResult result) {
                // the only task failed
                result.Exception.Data["caption"] = Resources.Toast_OutputStreamFailedCaption;
                result.Exception.Data["content"] = Resources.Toast_OutputStreamFailedContent;

                throw result.Exception;
              }

              /* the only task succeeded */
              // build toast actions
              var actions = new Dictionary<string, Uri> { { Resources.Toast_Open, results.First().ToastUri } };

              if (results.First().ToastUri.IsFile) {
                // local file - add "View in folder" action
                actions.Add(Resources.Toast_ViewInFolder,
                            new UriBuilder(results.First().ToastUri) { Query = "select" }.Uri);
              }

              ToastProvider.PushObject(results.First().ToastTitle,
                                       results.First().ToastContent,
                                       previewUri: results.First().ToastUri.IsFile ? results.First().ToastUri : null,
                                       previewImage: bmp,
                                       actions: actions,
                                       handler: (sender, data) => {
                                         if (data is ToastActivatedEventArgs eventArgs) {
                                           Shell.RevealInFileExplorer(eventArgs.Arguments);
                                         } else {
                                           Shell.RevealInFileExplorer(results.First().ToastUri.ToString());
                                         }
                                       });
            } else if (results.Count > 0 && results.Count == failedCount) {
              /* WTF! everything crashed and burnt */
              // What a Terrible Failure
              var exception = new Win32Exception(0x1C3, "All tasks failed.");

              // "It's perpetual motion; the thing man wanted to invent but never did.
              //  Or almost perpetual motion. If you let it go on, it'd burn our lifetimes out."
              exception.Data["results"] = results;

              // halt and catch fire
              throw exception;
            } else if (failedCount > 0) {
              /* some failed, some not */
              ToastProvider.PushObject(Resources.Toast_StaticDoneCaption,
                                       Resources.Toast_StaticDonePartialSuccessContent,
                                       Resources.Toast_StaticDonePartialSuccessSubtext,

                                       previewUri: previewUri,
                                       previewImage: bmp,

                                       // build toast actions
                                       actions: new Dictionary<string, Uri> {
                                         { Resources.Toast_ViewResultsActionText, ToastViewResultsUri },
                                         { Resources.Toast_ViewErrorsActionText, ToastViewErrorsUri }
                                       },

                                       handler: (sender, data) => {
                                         if (data as ToastActivatedEventArgs is null) {
                                           return; // go fuck thyself
                                         }

                                         var eventArgs = data as ToastActivatedEventArgs;
                                         // ReSharper disable PossibleNullReferenceException
                                         if (eventArgs.Arguments == ToastViewResultsUri.ToString()) {
                                           DisplayResultsDialog(results);
                                         } else if (eventArgs.Arguments == ToastViewErrorsUri.ToString()) {
                                           DisplayErrorDialog(Resources.ActionErrorDialog_PartialFailureCaption,
                                                              Resources.ActionErrorDialog_PartialFailureContent,
                                                              results.Where(r => r is UnsuccessfulCaptureResult)
                                                                     .Select(r => ((UnsuccessfulCaptureResult)r)
                                                                               .Exception));
                                         }
                                       });
            } else {
              // everything worked! Fireworks! Green lights! Yay I'm so sick of handling errors
              ToastProvider.PushObject(Resources.Toast_StaticDoneCaption,
                                       Resources.Toast_StaticDoneContent,
                                       results.Any() ? Resources.Toast_StaticDoneDetailsSubtext : null,

                                       previewUri: previewUri,
                                       previewImage: bmp,

                                       handler: (_, __) => DisplayResultsDialog(results));
            }
          } else {
            throw new InvalidOperationException("Static encoder activation yielded a nil result.");
          }
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"capture/encode error: {exception}");
          DisplayErrorToast(exception.Data["caption"] as string ?? Resources.Toast_CaptureFailedCaption,
                            exception.Data["content"] as string ?? Resources.Toast_CaptureFailedContent,

                            exception.Data.Contains("results")
                              // unsuccessful capture results' exceptions
                              // NOTE: under normal conditions, `results` is an IEnumerable<UnsuccessfulCaptureResult>,
                              // but just for the sake of safety, let's take make sure we take actual unsuccessful
                              // results from it
                              ? ((List<CaptureResult>)exception.Data["results"])
                              .Where(r => r is UnsuccessfulCaptureResult)
                              .Select(r => ((UnsuccessfulCaptureResult)r).Exception)

                              // single exception
                              : new[] {
                                exception
                              });
        }
      } else if (intent.ActionType == ActionType.Record) {
        throw new NotImplementedException();
      }
    }

    /// <summary>
    ///   Selects or creates a <see cref="Stream"/> for the encoder to write its output and wraps the rest under an
    ///   enumerable type.
    /// </summary>
    /// <returns>
    ///   A tuple containing the "master" stream as first value and the rest on the second value. A third value
    ///   containing the streams that failed to be initialized is also included
    /// </returns>
    private (Stream MasterStream, IEnumerable<Stream> OutputStreams,
      IEnumerable<(PluginObject OutputStream, Exception Exception)> Failed)
      ArrangeOutputStreams(EncoderInfo encoderInfo) {
      Stream masterStream;
      List<PluginObject> streams = OutputStreams;
      bool isMasterReadable = true;

      try {
        PluginObject preferred = streams.OrderBy(os => os.Type.GetNestedType("MemoryStream") is null).First();

        // select the first output stream to act as a "master", preferring MemoryStream-based ones as they are usually
        // faster [Citation needed]
        masterStream = FormatterServices.GetUninitializedObject(preferred.Type) as Stream;

        // set encoder info
        // NOTE: masterStream is guaranteed to implement IOutputStream
        typeof(IOutputStream).GetProperty("EncoderInfo")?.SetValue(masterStream, encoderInfo);

        // call object constructor
        preferred.Type.GetConstructor(Type.EmptyTypes)?.Invoke(masterStream, null);

        if (!masterStream.CanRead) {
          // u fuckd mush
          isMasterReadable = false;
          Log.WriteLine(LogLevel.Warning, "master stream is unreadable!");
        }

        // we don't want a duplicate stream
        streams.Remove(preferred);
      } catch (TargetInvocationException targetInvocationException) {
        Log.WriteLine(LogLevel.Warning,
                      "could not create master stream due to a initializer exception - throwing underlying exception");

        if (targetInvocationException.InnerException != null) {
          // throw inner exception so we handle it down after this method call
          throw targetInvocationException.InnerException;
        }

        // ugh
        throw;
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not create master stream - falling back to MemoryStream: {exception}");
        masterStream = new MemoryStream();
      }

      // instantiate all streams and return the result
      var failed = new List<(PluginObject OutputStream, Exception Exception)>();

      // initialize the rest of streams
      var streamInstances = streams.Select(o => {
        try {
          // create uninitialized stream instance
          var stream = FormatterServices.GetUninitializedObject(o.Type) as Stream;

          // set encoder info property
          typeof(IOutputStream).GetProperty("EncoderInfo")?.SetValue(stream, encoderInfo);

          // call object constructor
          o.Type.GetConstructor(Type.EmptyTypes)?.Invoke(stream, null);
          return stream;
        } catch (TargetInvocationException targetInvocationException) {
          Log.WriteLine(LogLevel.Error,
                        $"could not initialize output stream \"{o}\": {targetInvocationException.InnerException}");
          failed.Add((o, targetInvocationException.InnerException));
          return null;
        }
      })
                                   .Where(s => s != null)
                                   .ToList();

      // hold on! were we fucked?
      if (!isMasterReadable) {
        // all the arse
        streamInstances.Add(masterStream);
        masterStream = new MemoryStream();
        Log.WriteLine(LogLevel.Warning, "fell back to MemoryStream due a non-readable master stream");
      }

      return (masterStream, streamInstances, failed);
    }

    /// <summary>
    ///   Encodes a still capture and writes the data to all the output streams
    /// </summary>
    /// <param name="bmp"><see cref="Bitmap"/> instance</param>
    /// <param name="encoder">An static encoder instance</param>
    /// <returns>A list containing the results for all output streams</returns>
    private IEnumerable<CaptureResult> EncodeStatic(Bitmap bmp, IStaticEncoder encoder) {
      if (encoder == null) {
        throw new ArgumentNullException(nameof(encoder));
      }

      // create encoder information
      EncoderInfo encoderInfo = encoder.EncoderInfo;
      encoderInfo.PreviewBitmap = bmp;

      // arrange output streams so we get the fastest master
      (Stream MasterStream, IEnumerable<Stream> OutputStreams, IEnumerable<(PluginObject OutputStream,
        Exception Exception)> Failed) streams = ArrangeOutputStreams(encoderInfo);
      var results = new List<CaptureResult>();

      // ArrangeOutputStrams() may have omitted one or more streams due to initialization exceptions.
      // In this case, the failed output streams are contained, alongside the respective exceptions, in the `Failed` member of the tuple.
      // So let's create UnsuccessfulCaptureResult's for each failed one, for we want the user to acknowledge this errors
      results.AddRange(streams.Failed.ToList().Select(fs => new UnsuccessfulCaptureResult(fs.Exception)));

      // XXX: if IStaticEncoder.Encode() fails, an exception is thrown instead of pushing an unsuccessful CaptureResult.
      //      We have no way to determine whether the exception was an encoder exception or an error while writing to the
      //      master stream, or do we? Think 'bout this later
      encoder.Encode(bmp, streams.MasterStream);

      // take every output stream instance

      var threads = streams.OutputStreams.Select(s => new Thread(() => {
        // copy the data from the master stream

        try {
          // ReSharper disable AccessToDisposedClosure
          // although MasterStream is disposed below, it is guaranteed to never be disposed at this point,
          // for we block the calling thread while waiting for all threads to end
          streams.MasterStream.Position = 0; // we need to reset position on the master stream
          streams.MasterStream.CopyTo(s); // Stream.CopyTo() reads the source stream to end
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error,
                        $"error copying to stream of type {s.GetType()}: {exception}");
          results.Add(new UnsuccessfulCaptureResult(exception));
          return;
        }

        // commit and receive CaptureResult, then release stream
        if (((IOutputStream)s).Commit() is CaptureResult result) {
          results.Add(result);
        }

        s.Dispose();
      }))
                           .ToList();

      // start all the threads at the same time
      threads.ForEach(t => {
        // HACK: OK let's be honest on this one. Core Captain functionality requires this because I'm using some
        //       Windows Forms Clipboard functions on the built-in plugin 
        //       (see /Captain.Plugins.BuiltIn/OutputStreams/ClipboardOutputStream.cpp). Ideally we'd look for an STA 
        //       thread attribute on the output stream class or the implementation could create a new STA thread for
        //       doing whatever they have to. But this Just Works (TM)
        t.TrySetApartmentState(ApartmentState.STA);
        t.Start();
      });

      // now wait for every single one to finish
      threads.ForEach(t => t.Join());

      // release master
      try {
        // XXX: beware, unwise homunculi! MasterStream is the only one that just *may* not implement IOutputStream.
        //      In the remote case everything blew up, forcing us to fall back to a MemoryStream, a direct cast to
        //      IOutputStream would fail
        if (streams.MasterStream is IOutputStream masterStream && masterStream.Commit() is CaptureResult masterResult) {
          results.Add(masterResult);
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error,
                      $"error committing to master stream of type {streams.MasterStream.GetType()}: {exception}");
        results.Add(new UnsuccessfulCaptureResult(exception));
      }

      streams.MasterStream.Dispose();
      return results;
    }

    /// <summary>
    ///   Displays a generic error dialog
    /// </summary>
    /// <param name="caption">Instruction text</param>
    /// <param name="body">Dialog body</param>
    /// <param name="exceptions">Exception list</param>
    private void DisplayErrorDialog(string caption, string body, IEnumerable<Exception> exceptions) {
      if (TaskDialog.IsPlatformSupported) {
        var dialog = new TaskDialog {
          Caption = VersionInfo.ProductName,
          InstructionText = caption,
          Text = body,
          StandardButtons = TaskDialogStandardButtons.Close,

          ExpansionMode = TaskDialogExpandedDetailsLocation.ExpandFooter,
          DetailsExpanded = false,
          DetailsExpandedLabel = Resources.GenericActionErrorDialog_HideDetails,
          DetailsCollapsedLabel = Resources.GenericActionErrorDialog_ShowDetails,
          DetailsExpandedText = String.Join(Environment.NewLine, exceptions.Select(e => e.Message))
        };

        dialog.Opened += (sender, eventArgs) =>
            // ReSharper disable once AccessToDisposedClosure
            // HACK: setting Icon property before the dialog is open has no effect
            dialog.Icon = TaskDialogStandardIcon.Warning;

        // remove warning badge by setting the original indicator back
        dialog.Closing += (sender, eventArgs) => Application.TrayIcon.SetIcon();
        dialog.Show();
      } else {
        // TaskDialog is not supported by the current platform
        Log.WriteLine(LogLevel.Warning, "falling back to MessageBox");
        MessageBox.Show(caption +
                        Environment.NewLine +
                        body +
                        Environment.NewLine +
                        Environment.NewLine +
                        String.Join(Environment.NewLine, exceptions.Select(e => e.Message)),
                        VersionInfo.ProductName,
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
        Application.TrayIcon.SetIcon();
      }
    }

    /// <summary>
    ///   Displays a generic error toast notification
    /// </summary>
    /// <param name="caption">Toast title</param>
    /// <param name="body">Toast content</param>
    /// <param name="exceptions">Underlying exceptions</param>
    private void DisplayErrorToast(string caption, string body, IEnumerable<Exception> exceptions) {
      Application.TrayIcon.StopLoopingIconAnimation();
      Application.TrayIcon.SetIcon(TrayIconClass.Warning);

      new Thread(() => {
        // oh c'mon dude! are you really doing this?
        Thread.Sleep(TrayIcon.WarningBadgeTtl);

        // reset the icon after a timeout, just in case our previous handlers don't get called
        Application.TrayIcon.SetIcon();
      }).Start();

      LegacyNotificationProvider.PushMessage(caption,
                                             body,
                                             ToolTipIcon.Warning,
                                             handler: (_, __) => DisplayErrorDialog(caption, body, exceptions),
                                             closeHandler: (_, __) => Application.TrayIcon.SetIcon());
    }

    /// <summary>
    ///   Displays a dialog containing all the result information for each output stream
    /// </summary>
    /// <param name="results"></param>
    private void DisplayResultsDialog(List<CaptureResult> results) => throw new NotImplementedException();
  }
}