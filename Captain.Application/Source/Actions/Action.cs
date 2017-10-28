﻿using System;
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
using Captain.Application.Native;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents an action, which is a set of capture provider, handler and encoder
  /// </summary>
  internal sealed class Action : IDisposable {
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
    ///   Video recording streams
    /// </summary>
    private (Stream MasterStream, IEnumerable<Stream> OutputStreams, IEnumerable<(PluginObject OutputStream,
      Exception Exception)> Failed)? recordingStreams;

    /// <summary>
    ///   Video encoder instance for recording screen
    /// </summary>
    private IVideoEncoder recordingVideoEncoder;

    /// <summary>
    ///   Video provider for recording screen
    /// </summary>
    private VideoProvider recordingVideoProvider;

    /// <summary>
    ///   Available action types
    /// </summary>
    internal ActionType ActionTypes {
      get {
        var types = ActionType.None;

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
    private PluginObject StaticEncoder { get; }

    /// <summary>
    ///   Video encoder object (may be null)
    /// </summary>
    private PluginObject VideoEncoder { get; }

    /// <summary>
    ///   Output streams
    /// </summary>
    private List<PluginObject> OutputStreams { get; } = new List<PluginObject>();

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
    /// <param name="grabber">The <see cref="Grabber" /> instance</param>
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
      if (intent.ActionType == ActionType.Screenshot) {
        if (!OutputStreams.Any()) {
          throw new InvalidOperationException("No streams were set for this action.");
        }

        try {
          if (Activator.CreateInstance(StaticEncoder.Type) as IStaticEncoder is IStaticEncoder encoder) {
            Application.TrayIcon.AnimateIndicator(IndicatorStatus.Progress);
            VideoProvider videoProvider = VideoProviderFactory.Create(intent.VirtualArea,
                                                                      intent.WindowHandle == IntPtr.Zero
                                                                        ? (IntPtr?) null
                                                                        : intent.WindowHandle);
            this.boundGrabber.Hide();

            // perform screen capture
            videoProvider.AcquireFrame();
            Bitmap bmp = videoProvider.CreateFrameBitmap();
            videoProvider.Dispose();

            // encode static image and obtain task results
            var results = (List<CaptureResult>) EncodeStatic(bmp, encoder);

            // get number of failed tasks
            int failedCount = results.Count(r => r is UnsuccessfulCaptureResult);
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
              var actions = new Dictionary<string, Uri> {{Resources.Toast_Open, results.First().ToastUri}};

              if (results.First().ToastUri.IsFile) {
                // local file - add "View in folder" action
                actions.Add(Resources.Toast_ViewInFolder,
                            new UriBuilder(results.First().ToastUri) {Query = "select"}.Uri);
              }

              if ((Application.Options.NotificationOptions == NotificationDisplayOptions.OnSuccess) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.ExceptProgress) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.Always)) {
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
              }
            } else if ((results.Count > 0) && (results.Count == failedCount)) {
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
              Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);

              if ((Application.Options.NotificationOptions == NotificationDisplayOptions.OnSuccess) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.ExceptProgress) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.Always)) {
                ToastProvider.PushObject(Resources.Toast_StaticDoneCaption,
                                         Resources.Toast_StaticDonePartialSuccessContent,
                                         Resources.Toast_StaticDonePartialSuccessSubtext,
                                         previewUri,
                                         bmp,

                                         // build toast actions
                                         new Dictionary<string, Uri> {
                                           {Resources.Toast_ViewResultsActionText, ToastViewResultsUri},
                                           {Resources.Toast_ViewErrorsActionText, ToastViewErrorsUri}
                                         },
                                         (sender, data) => {
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
                                                                       .Select(r => ((UnsuccessfulCaptureResult) r)
                                                                                 .Exception));
                                           }
                                         });
              }
            } else {
              // everything worked! Fireworks! Green lights! Yay I'm so sick of handling errors
              Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Success);

              if ((Application.Options.NotificationOptions == NotificationDisplayOptions.OnSuccess) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.ExceptProgress) ||
                  (Application.Options.NotificationOptions == NotificationDisplayOptions.Always)) {
                ToastProvider.PushObject(Resources.Toast_StaticDoneCaption,
                                         Resources.Toast_StaticDoneContent,
                                         results.Any() ? Resources.Toast_StaticDoneDetailsSubtext : null,
                                         previewUri,
                                         bmp,
                                         handler: (_, __) => DisplayResultsDialog(results));
              }
            }
          } else {
            throw new InvalidOperationException("Static encoder activation yielded a nil result.");
          }
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"capture/encode error: {exception}");

          Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Success);
          DisplayErrorToast(exception.Data["caption"] as string ?? Resources.Toast_CaptureFailedCaption,
                            exception.Data["content"] as string ?? Resources.Toast_CaptureFailedContent,
                            exception.Data.Contains("results")
                              // unsuccessful capture results' exceptions
                              // NOTE: under normal conditions, `results` is an IEnumerable<UnsuccessfulCaptureResult>,
                              // but just for the sake of safety, let's take make sure we take actual unsuccessful
                              // results from it
                              ? ((List<CaptureResult>) exception.Data["results"])
                              .Where(r => r is UnsuccessfulCaptureResult)
                              .Select(r => ((UnsuccessfulCaptureResult) r).Exception)

                              // single exception
                              : new[] {
                                exception
                              });
        }
      } else if (intent.ActionType == ActionType.Record) {
        if (this.recordingStreams == null) {
          // not recording
          Application.TrayIcon.AnimateIndicator(IndicatorStatus.Recording);

          this.recordingVideoProvider = VideoProviderFactory.Create(intent.VirtualArea, intent.WindowHandle);
          if (Activator.CreateInstance(VideoEncoder.Type) is IVideoEncoder videoEncoder) {
            this.recordingVideoEncoder = videoEncoder;
            StartEncodeVideo();
            this.boundGrabber.Window.CanBeResized = false;
          }
        } else {
          // stop recording
          Application.TrayIcon.AnimateIndicator(IndicatorStatus.Idle);
          EndEncodeVideo();
          this.recordingStreams = null;
          this.boundGrabber.Window.CanBeResized = true;
        }
      }
    }

    /// <summary>
    ///   Selects or creates a <see cref="Stream" /> for the encoder to write its output and wraps the rest under an
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
      var isMasterReadable = true;

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
      var streamInstances = new List<Stream>(streams.Select(o => {
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
      }));

      // hold on! were we fucked?
      if (!isMasterReadable) {
        // all the arse
        streamInstances.Add(masterStream);
        masterStream = new MemoryStream();
        Log.WriteLine(LogLevel.Warning, "fell back to MemoryStream due a non-readable master stream");
      }

      return (masterStream, streamInstances.Where(s => s != null), failed);
    }

    /// <summary>
    ///   Encodes a still capture and writes the data to all the output streams
    /// </summary>
    /// <param name="bmp"><see cref="Bitmap" /> instance</param>
    /// <param name="encoder">An static encoder instance</param>
    /// <returns>A list containing the results for all output streams</returns>
    private IEnumerable<CaptureResult> EncodeStatic(Bitmap bmp, IStaticEncoder encoder) {
      if (encoder == null) {
        throw new ArgumentNullException(nameof(encoder));
      }

      // arrange output streams so we get the fastest master
      (Stream MasterStream, IEnumerable<Stream> OutputStreams, IEnumerable<(PluginObject OutputStream,
        Exception Exception)> Failed) streams = ArrangeOutputStreams(encoder.EncoderInfo);
      var results = new List<CaptureResult>();

      // ArrangeOutputStreams() may have omitted one or more streams due to initialization exceptions.
      // In this case, the failed output streams are contained, alongside the respective exceptions, in the `Failed`
      // member of the tuple. So let's create UnsuccessfulCaptureResult's for each failed one, for we want the user to
      // acknowledge this errors
      results.AddRange(streams.Failed.ToList().Select(fs => new UnsuccessfulCaptureResult(fs.Exception)));

      // XXX: if IStaticEncoder.Encode() fails, an exception is thrown instead of pushing an unsuccessful CaptureResult
      //      We have no way to determine whether the exception was an encoder exception or an error while writing to
      //      the master stream, or do we? Think 'bout this later
      // TODO: wrap this line around a try-catch block and the underlying exception under some sort of EncoderException
      Log.WriteLine(LogLevel.Verbose, "encoding static capture");
      encoder.Encode(bmp, streams.MasterStream);

      // take every output stream instance
      var threads = new List<Thread>(streams.OutputStreams.Select(s => {
        var thread = new Thread(() => {
          // copy the data from the master stream
          try {
            // ReSharper disable All AccessToDisposedClosure
            // although MasterStream is disposed below, it is guaranteed to never be disposed at this point,
            // for we block the calling thread while waiting for all threads to end
            streams.MasterStream.Position = 0; // we need to reset position on the master stream
            streams.MasterStream.CopyTo(s); // Stream.CopyTo() reads the source stream to end
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Error, $"error copying to stream {s}: {exception}");
            results.Add(new UnsuccessfulCaptureResult(exception));
            return;
          }

          // commit and receive CaptureResult, then release stream
          if (((IOutputStream) s).Commit() is CaptureResult result) {
            results.Add(result);
          }

          s.Dispose();
        });

        // some streams may require certain apartment state in order to work - change the thread apartment state here
        object[] attributes = s.GetType().GetCustomAttributes(typeof(ThreadApartmentState), true);
        if (attributes.Length > 0) {
          // apartment state attribute is present
          var state = (ThreadApartmentState) attributes.First(); // only one attribute is allowed so this is safe

          // only STA/MTA apartment states can be set
          if (state.ApartmentState != ApartmentState.Unknown) {
            // set thread apartment state
            if (thread.TrySetApartmentState(state.ApartmentState)) {
              Log.WriteLine(LogLevel.Debug, $"set apartment state of {s} to {state.ApartmentState}");
            } else {
              Log.WriteLine(LogLevel.Warning, $"could not set apartment state of {s} to {state.ApartmentState}");
            }
          }
        }

        return thread;
      }));

      // start all the threads at the same time
      threads.ForEach(t => t.Start());

      // now wait for every single one to finish
      threads.ForEach(t => t.Join());

      // release master
      try {
        // NOTE: beware, unwise homunculi! MasterStream is the only one that just *may* not implement IOutputStream.
        //       In the remote case everything blew up, forcing us to fall back to a MemoryStream, a direct cast to
        //       IOutputStream would fail
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

    private bool recording = false;
    private Thread recordingThread;

    /// <summary>
    ///   Starts encoding video
    /// </summary>
    private void StartEncodeVideo() {
      this.recordingStreams = ArrangeOutputStreams(this.recordingVideoEncoder.EncoderInfo);
      this.recordingVideoEncoder.Start(this.recordingVideoProvider.CaptureBounds.Size,
                                       this.recordingStreams.Value.MasterStream);

      this.recording = true;
      this.recordingThread = new Thread(() => {
        while (this.recording) {
          this.recordingVideoEncoder.Encode(this.recordingVideoProvider, this.recordingStreams.Value.MasterStream);
          Thread.Sleep(new TimeSpan((long) Math.Round((1 / (decimal) 30) * 10_000_000)));
        }
      });
      
      this.recordingThread.SetApartmentState(ApartmentState.STA);
      this.recordingThread.Start();
    }

    private IEnumerable<CaptureResult> EndEncodeVideo() {
      this.recording = false;
      this.recordingThread.Join();
      
      var results = new List<CaptureResult>();

      // ArrangeOutputStreams() may have omitted one or more streams due to initialization exceptions.
      // In this case, the failed output streams are contained, alongside the respective exceptions, in the `Failed`
      // member of the tuple. So let's create UnsuccessfulCaptureResult's for each failed one, for we want the user to
      // acknowledge this errors
      results.AddRange(this.recordingStreams.Value.Failed.ToList()
                           .Select(fs => new UnsuccessfulCaptureResult(fs.Exception)));

      // XXX: if IStaticEncoder.Encode() fails, an exception is thrown instead of pushing an unsuccessful CaptureResult
      //      We have no way to determine whether the exception was an encoder exception or an error while writing to
      //      the master stream, or do we? Think 'bout this later
      // TODO: wrap this line around a try-catch block and the underlying exception under some sort of EncoderException
      Log.WriteLine(LogLevel.Verbose, "ending video capture on master");
      this.recordingVideoEncoder.End(this.recordingStreams.Value.MasterStream);

      // take every output stream instance
      var threads = new List<Thread>(this.recordingStreams.Value.OutputStreams.Select(s => {
        var thread = new Thread(() => {
          // copy the data from the master stream
          try {
            // ReSharper disable AccessToDisposedClosure
            // although MasterStream is disposed below, it is guaranteed to never be disposed at this point,
            // for we block the calling thread while waiting for all threads to end
            this.recordingStreams.Value.MasterStream.Position = 0; // we need to reset position on the master stream
            this.recordingStreams.Value.MasterStream.CopyTo(s); // Stream.CopyTo() reads the source stream to end
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Error, $"error copying to stream {s}: {exception}");
            results.Add(new UnsuccessfulCaptureResult(exception));
            return;
          }

          // commit and receive CaptureResult, then release stream
          if (((IOutputStream) s).Commit() is CaptureResult result) {
            results.Add(result);
          }

          s.Dispose();
        });

        // some streams may require certain apartment state in order to work - change the thread apartment state here
        object[] attributes = s.GetType().GetCustomAttributes(typeof(ThreadApartmentState), true);
        if (attributes.Length > 0) {
          // apartment state attribute is present
          var state = (ThreadApartmentState) attributes.First(); // only one attribute is allowed so this is safe

          // only STA/MTA apartment states can be set
          if (state.ApartmentState != ApartmentState.Unknown) {
            // set thread apartment state
            if (thread.TrySetApartmentState(state.ApartmentState)) {
              Log.WriteLine(LogLevel.Debug, $"set apartment state of {s} to {state.ApartmentState}");
            } else {
              Log.WriteLine(LogLevel.Warning, $"could not set apartment state of {s} to {state.ApartmentState}");
            }
          }
        }

        return thread;
      }));

      // start all the threads at the same time
      threads.ForEach(t => t.Start());

      // now wait for every single one to finish
      threads.ForEach(t => t.Join());

      // release master
      try {
        // NOTE: beware, unwise homunculi! MasterStream is the only one that just *may* not implement IOutputStream.
        //       In the remote case everything blew up, forcing us to fall back to a MemoryStream, a direct cast to
        //       IOutputStream would fail
        if (this.recordingStreams.Value.MasterStream is IOutputStream masterStream &&
            masterStream.Commit() is CaptureResult masterResult) {
          results.Add(masterResult);
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error,
                      $"error committing to master stream of type {this.recordingStreams.Value.MasterStream.GetType()}: {exception}");
        results.Add(new UnsuccessfulCaptureResult(exception));
      }

      this.recordingStreams.Value.MasterStream.Dispose();
      this.recordingVideoProvider.Dispose();
      return results;
    }

    /// <summary>
    ///   Displays a generic error dialog
    /// </summary>
    /// <param name="caption">Instruction text</param>
    /// <param name="body">Dialog body</param>
    /// <param name="exceptions">Exception list</param>
    private static void DisplayErrorDialog(string caption, string body, IEnumerable<Exception> exceptions) {
      var dialog = new TaskDialog {
        WindowTitle = VersionInfo.ProductName,
        MainIcon = TaskDialogIcon.Warning,
        MainInstruction = caption,
        Content = body,
        Buttons = {new TaskDialogButton(ButtonType.Close)},

        ExpandFooterArea = true,
        ExpandedByDefault = false,
        ExpandedControlText = Resources.GenericActionErrorDialog_HideDetails,
        CollapsedControlText = Resources.GenericActionErrorDialog_ShowDetails,
        ExpandedInformation = String.Join(Environment.NewLine, exceptions.Select(e => e.Message))
      };

      dialog.Destroyed += (_, __) => Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
      dialog.Show();
    }

    /// <summary>
    ///   Displays a generic error toast notification
    /// </summary>
    /// <param name="caption">Toast title</param>
    /// <param name="body">Toast content</param>
    /// <param name="exceptions">Underlying exceptions</param>
    private static void DisplayErrorToast(string caption, string body, IEnumerable<Exception> exceptions) {
      if ((Application.Options.NotificationOptions == NotificationDisplayOptions.OnFailure) ||
          (Application.Options.NotificationOptions == NotificationDisplayOptions.ExceptProgress) ||
          (Application.Options.NotificationOptions == NotificationDisplayOptions.Always)) {
        LegacyNotificationProvider.PushMessage(caption,
                                               body,
                                               ToolTipIcon.Warning,
                                               handler: (_, __) => DisplayErrorDialog(caption, body, exceptions),
                                               closeHandler: (_, __) =>
                                                 Application.TrayIcon.SetIndicator(IndicatorStatus.Idle));
      }
    }

    /// <summary>
    ///   Displays a dialog containing all the result information for each output stream
    /// </summary>
    /// <param name="results">A list of <see cref="CaptureResult" /></param>
    private void DisplayResultsDialog(List<CaptureResult> results) {
      if (results.Count == 0) {
        throw new ArgumentException(@"Value cannot be an empty collection.", nameof(results));
      }

      throw new NotImplementedException();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      this.boundGrabber?.Dispose();
      this.recordingVideoProvider?.Dispose();
      GC.SuppressFinalize(this);
    }
  }
}