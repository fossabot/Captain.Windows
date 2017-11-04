using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.UI.Notifications;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;

// ReSharper disable MemberCanBeInternal
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Captain.Application {
  /// <summary>
  ///   Tasks are the main functional unit of the application
  /// </summary>
  [Serializable]
  public sealed class Task {
    /// <summary>
    ///   URI for result details view action
    /// </summary>
    private const string ViewResultDetailsUri = "captain://view-task-details/results";

    /// <summary>
    ///   URI for error details view action
    /// </summary>
    private const string ViewErrorDetailsUri = "captain://view-task-details/errors";

    /// <summary>
    ///   Encoder instance
    /// </summary>
    private IEncoderBase encoder;

    /// <summary>
    /// </summary>
    private List<(Stream Stream, PluginObject PluginObject, Exception Exception)> streams;

    /// <summary>
    ///   Video provider instance
    /// </summary>
    private VideoProvider videoProvider;

    /// <summary>
    ///   Task type
    /// </summary>
    public TaskType Type { get; set; } = TaskType.Screenshot;

    /// <summary>
    ///   Task parameters
    /// </summary>
    public TaskParameters Parameters { get; set; } = new TaskParameters();

    /// <summary>
    ///   Task name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    ///   Output streams
    /// </summary>
    /// <remarks>
    ///   Keys contains the type name for the output stream, while values hold an optional dictionary with the user
    ///   options for such stream
    /// </remarks>
    public List<(string, SerializableDictionary<object, object>)> OutputStreams { get; set; } =
      new List<(string, SerializableDictionary<object, object>)>();

    /// <summary>
    ///   Keyboard shortcut assigned to this task
    /// </summary>
    public Keys HotKey { get; set; } = Keys.None;

    /// <summary>
    ///   Custom notification options
    /// </summary>
    public NotificationDisplayOptions NotificationOptions { get; set; } = NotificationDisplayOptions.Inherit;

    /// <summary>
    ///   Creates a default screenshot task using predefined actions (i.e. for first application startup)
    /// </summary>
    /// <returns>A <see cref="Task" /> instance</returns>
    internal static Task CreateDefaultScreenshotTask() => new Task {
      Name = "Default screenshot task",
      Type = TaskType.Screenshot,
      Parameters = new TaskParameters {
        Encoder = typeof(PngCaptureEncoder).FullName,
        RegionType = TaskRegionType.Grab
      },
      OutputStreams = new List<(string, SerializableDictionary<object, object>)> {
        (typeof(ClipboardOutputStream).FullName, new SerializableDictionary<object, object>()),
        (typeof(FileOutputStream).FullName, new SerializableDictionary<object, object>())
      },
      NotificationOptions = NotificationDisplayOptions.Inherit
    };

    /// <summary>
    ///   Creates a default screenshot task using predefined actions (i.e. for first application startup)
    /// </summary>
    /// <returns>A <see cref="Task" /> instance</returns>
    internal static Task CreateDefaultRecordingTask() => new Task {
      Name = "Default recording task",
      Type = TaskType.Recording,
      Parameters = new TaskParameters {
        Encoder = typeof(H264CaptureEncoder).FullName,
        RegionType = TaskRegionType.Grab
      },
      OutputStreams = new List<(string, SerializableDictionary<object, object>)> {
        (typeof(FileOutputStream).FullName, new SerializableDictionary<object, object>())
      },
      NotificationOptions = NotificationDisplayOptions.Inherit
    };

    /// <summary>
    ///   Starts this task
    /// </summary>
    internal async void Start() {
      Log.WriteLine(LogLevel.Debug, $"starting action: {Name} ({Type})");

      switch (Type) {
        case TaskType.Recording:
          throw new NotImplementedException();

        case TaskType.Screenshot:
          try {
            // capture the screen
            List<CaptureResult> results = null;
            switch (Parameters.RegionType) {
              case TaskRegionType.Grab:
                Log.WriteLine(LogLevel.Debug, "waiting for HUD to yield screen region");
                Application.Hud.Display();

                // create completion source for crop event and bind handler
                var completionSource = new TaskCompletionSource<Rectangle>();
                void OnScreenCrop(object sender, Rectangle bounds) { completionSource.SetResult(bounds); }
                Application.Hud.OnScreenCrop += OnScreenCrop;
                
                // wait for a screen region to be selected
                Rectangle rect = await completionSource.Task;
                
                // unbind event handler so it does not get called once again on next screenshot (remember the HUD is
                // unlikely to be disposed!)
                Application.Hud.OnScreenCrop -= OnScreenCrop;
                Log.WriteLine(LogLevel.Debug, $"got rectangle: {rect.Width}x{rect.Height} at ({rect.X}, {rect.Y})");
                results = TakeScreenshot(rect);
                break;
              case TaskRegionType.Fixed:
                results = TakeScreenshot(Parameters.FixedRegion);
                break;
              case TaskRegionType.FullScreen:
                results = TakeScreenshot(Screen.FromPoint(Control.MousePosition).Bounds);
                break;
            }

            // at this point, everything should be fine
            Uri previewUri = null;
            Bitmap previewBmp = null;

            try {
              // find a local URI for toast preview image
              // `results` is non-null at this point
              // ReSharper disable once AssignNullToNotNullAttribute
              previewUri = results.First(r => (r.Uri != null) && r.Uri.IsFile && File.Exists(r.Uri.LocalPath)).Uri;
            } catch {
              // find a suitable preview bitmap
              // TODO: maybe get the bitmap with the best resolution?
              try {
                // `results` is non-null at this point
                // ReSharper disable once AssignNullToNotNullAttribute
                previewBmp = results.First(r => r.PreviewBitmap != null).PreviewBitmap;
              } catch {
                /* no nothing for anyone */
              }
            }

            Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Success);
            Log.WriteLine(LogLevel.Informational, "capture succeeded");
            ToastProvider.PushObject(Resources.Task_SuccessCaption,
              Resources.Task_SuccessBody,
              previewUri: previewUri,
              previewImage: previewBmp,
              actions: new Dictionary<string, Uri> {
                {Resources.Task_ViewDetailsAction, new Uri(ViewResultDetailsUri)}
              },
              handler: (s, d) => {
                if (d is ToastActivatedEventArgs) {
                  Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Idle);
                  // TODO
                }
              },
              dismissionHandler: (s, e) => Application.TrayIcon.SetIndicator(IndicatorStatus.Idle));

            // release resources after handling capture
            previewBmp?.Dispose();
          } catch (InvalidOperationException exception) {
            Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);
            Log.WriteLine(LogLevel.Error, $"internal error occurred: {exception}");
            LegacyNotificationProvider.PushMessage(Resources.Task_CaptureFailedCaption,
              Resources.Task_CaptureFailedBody,
              ToolTipIcon.Error,
              handler:
                (s, e) =>
                  DisplayErrorDialog(Resources.Task_CaptureFailedExtendedCaption,
                    Resources.Task_CaptureFailedExtendedBody,
                    new[] {exception},
                    TaskDialogIcon.Error),
              closeHandler: (s, e) => Application.TrayIcon.SetIndicator(IndicatorStatus.Idle));
          } catch (AggregateException exception) {
            Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);
            if (exception.InnerExceptions.Count >= OutputStreams.Count) {
              Log.WriteLine(LogLevel.Error, $"all actions failed: {exception}");
            } else {
              Log.WriteLine(LogLevel.Warning, $"{exception.InnerExceptions.Count} action(s) failed");
              ToastProvider.PushObject(Resources.Task_PartialSuccessCaption,
                Resources.Task_PartialSuccessBody,
                actions: new Dictionary<string, Uri> {
                  {Resources.Task_ViewResultsAction, new Uri(ViewResultDetailsUri)},
                  {Resources.Task_ViewFailuresAction, new Uri(ViewErrorDetailsUri)}
                },
                handler: (s, d) => {
                  if (d is ToastActivatedEventArgs eventArgs) {
                    Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Idle);
                    switch (eventArgs.Arguments) {
                      case ViewResultDetailsUri:
                        break;

                      case ViewErrorDetailsUri:
                        DisplayErrorDialog(Resources.ActionErrorDialog_PartialFailureCaption,
                          Resources.ActionErrorDialog_PartialFailureContent,
                          exception.InnerExceptions);
                        break;
                    }
                  }
                },
                dismissionHandler: (s, d) => Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Idle));
            }
          }
          break;
      }
    }

    #region Dialogs and UI

    /// <summary>
    ///   Displays a generic error dialog
    /// </summary>
    /// <param name="caption">Instruction text</param>
    /// <param name="body">Dialog body</param>
    /// <param name="exceptions">Exception list</param>
    /// <param name="mainIcon">Optionally specify task dialog icon</param>
    private static void DisplayErrorDialog(string caption, string body, IEnumerable<Exception> exceptions,
                                           TaskDialogIcon mainIcon = TaskDialogIcon.Warning) {
      var dialog = new TaskDialog {
        WindowTitle = VersionInfo.ProductName,
        MainIcon = mainIcon,
        MainInstruction = caption,
        Content = body,
        Buttons = {new TaskDialogButton(ButtonType.Close)},
        ExpandFooterArea = true,
        ExpandedByDefault = false,
        ExpandedControlText = Resources.GenericActionErrorDialog_HideDetails,
        CollapsedControlText = Resources.GenericActionErrorDialog_ShowDetails,
        ExpandedInformation = String.Join(Environment.NewLine, exceptions.Select(e => e.Message))
      };

      dialog.Destroyed += (s, e) => Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
      dialog.Show();
    }

    #endregion

    /// <summary>
    ///   Takes a single screenshot
    /// </summary>
    /// <param name="rect">Optional rectangle</param>
    /// <exception cref="ArgumentNullException">
    ///   Thrown when <paramref name="rect" /> is not set and the task requires a region
    ///   to be handpicked by the user.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   Thrown when an error initializing helper classes, devices or encoders occurs.
    /// </exception>
    /// <exception cref="AggregateException">
    ///   Thrown when a non-internal error occurs. That is, errors unrelated to application's implementation but rather
    ///   relative to output streams (i.e. network/disk errors, plugin bugs...)
    ///   Inner <see cref="IOException" />s indicate problems copying the data to a certain stream.
    ///   Inner <see cref="ThreadStateException" />s indicate problems setting thread apartment state
    /// </exception>
    /// <returns>A <see cref="CaptureResult" /> list for each successful action</returns>
    private List<CaptureResult> TakeScreenshot(Rectangle? rect = null) {
      if (!rect.HasValue) {
        switch (Parameters.RegionType) {
          case TaskRegionType.Grab:
            // when this task requires a region to be grabbed, rect must not be null
            throw new ArgumentNullException(nameof(rect));

          case TaskRegionType.Fixed:
            rect = Parameters.FixedRegion;
            break;

          case TaskRegionType.FullScreen:
            throw new NotImplementedException();
        }
      }

      var exceptions = new List<Exception>();

      // ensure we've got an encoder instance
      CreateStaticEncoder();

      // initialize output streams if required
      if (this.streams == null) {
        this.streams = CreateStreams();
        exceptions.AddRange(this.streams.Where(s => s.Exception != null).Select(s => s.Exception).ToList());
      }

      if (!this.streams.Any() || this.streams.All(s => s.Exception != null)) {
        Log.WriteLine(LogLevel.Warning, "no suitable stream could be initialized");
        throw new AggregateException(exceptions);
      }

      // at this point, `rect` is guaranteed not to be null
      // ReSharper disable once PossibleInvalidOperationException

      // ensure we've got a video provider
      try {
        CreateVideoProvider(rect.Value);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Warning, $"could not (re)create video provider: {exception}");

        if (this.videoProvider == null) {
          throw new InvalidOperationException("No suitable video provider could be initialized");
        }
      }

      this.videoProvider.AcquireFrame();

      var results = new List<CaptureResult>();
      try {
        using (Bitmap bmp = this.videoProvider.CreateFrameBitmap()) {
          Log.WriteLine(LogLevel.Debug, $"created frame bitmap ({bmp.Width}x{bmp.Height}, {bmp.PixelFormat})");

          // encode the frame and copy it to a temporary memory stream
          byte[] data;
          using (var tempStream = new MemoryStream()) {
            ((IStaticEncoder) this.encoder).Encode(bmp, tempStream);
            //tempStream.Flush();
            tempStream.Seek(0, SeekOrigin.Begin);
            tempStream.Read(data = new byte[tempStream.Length], 0, (int) tempStream.Length);
          }

          // shit's encoded - write to output streams in parall
          List<Thread> threads = this.streams.Where(s => s.Stream != null).Select(s => {
            // create thread
            var id = 0;
            var thread = new Thread(() => {
              try {
                s.Stream.Write(data, 0, data.Length);
                s.Stream.Seek(0, SeekOrigin.Begin);

                // `bmp` is guaranteed not to be disposed at this point, because we wait for all threads to join before
                // doing so implicitly by closing the using block
                // ReSharper disable AccessToDisposedClosure
                if (s.Stream is IOutputStream outputStream) {
                  CaptureResult result = outputStream.Commit();
                  if (result.PreviewBitmap == null) { result.PreviewBitmap = bmp.Clone() as Bitmap; }
                  results.Add(result);
                } else {
                  results.Add(new CaptureResult {PreviewBitmap = bmp.Clone() as Bitmap});
                }

                // ReSharper restore AccessToDisposedClosure
              } catch (Exception exception) {
                // TODO: retry a couple times?
                // ReSharper disable once AccessToModifiedClosure
                Log.WriteLine(LogLevel.Error, $"[thread #{id}] copy failed (aborting): {exception}");
                exceptions.Add(new IOException("Could not copy data to output stream", exception) {
                  Data = {{"stream", s}}
                });
              }
            });

            id = thread.ManagedThreadId;
            Log.WriteLine(LogLevel.Debug, $"[thread #{id}] created thread for {s.PluginObject.Type}");

            // find custom thread apartment state attributes, as some streams may require STA
            var apartmentStateAttrs = s.PluginObject.Type.GetCustomAttributes(typeof(ThreadApartmentState), true)
              as ThreadApartmentState[];
            if (apartmentStateAttrs?.Length == 1) {
              // if exactly one apartment state attribute is present, try to set it
              ApartmentState state = apartmentStateAttrs.First().ApartmentState;
              Log.WriteLine(LogLevel.Informational, $"[thread #{id}] thread apartment state: {state}");
              if (!thread.TrySetApartmentState(state)) {
                Log.WriteLine(LogLevel.Warning, $"[thread #{id}] failed to set apartment state");
                exceptions.Add(new ThreadStateException("Could not set apartment state") {
                  Data = {{"stream", s}}
                });
              }
            }

            // copy to this stream
            thread.Start();
            Log.WriteLine(LogLevel.Debug, $"[thread #{id}] started");
            return thread;
          }).ToList();

          // by now the thing is getting copied to each output stream - if we get outside this `using` block, `bmp`
          // will get disposed and garbage collected. We want to keep this alive so we wait until every stream is done
          // with it
          threads.ForEach(t => {
            t.Join();
            Log.WriteLine(LogLevel.Debug, $"[thread #{t.ManagedThreadId}] joined");

            // TODO: handle progress here?
          });
        }
      } finally {
        Log.WriteLine(LogLevel.Debug, "releasing screenshot resources");

        this.videoProvider.Dispose();
        this.videoProvider = null;

        this.streams.ForEach(s => s.Stream?.Dispose());
        this.streams = null;
      }

      if (exceptions.Any()) { throw new AggregateException("Task completed with errors", exceptions); }
      return results;
    }

    /// <summary>
    ///   Creates a video provider for the specified virtual desktop rectangle
    /// </summary>
    /// <param name="rect">Screen region</param>
    private void CreateVideoProvider(Rectangle rect) {
      // initialize video provider if not already initialized or when bounds changed
      if ((this.videoProvider == null) || (this.videoProvider.CaptureBounds != rect)) {
        this.videoProvider?.Dispose(); // dispose previous provider if rectangle has changed
        this.videoProvider = VideoProviderFactory.Create(rect);
      }
    }

    /// <summary>
    ///   Creates an static encoder for this task
    /// </summary>
    private void CreateStaticEncoder() {
      PluginObject encoderObject =
        Application.PluginManager.StaticEncoders.First(po => po.Type.FullName == Parameters.Encoder);

      // create encoder instance
      this.encoder = FormatterServices.GetSafeUninitializedObject(encoderObject.Type) as IEncoderBase;

      if (encoderObject.Configurable) {
        // encoder type is guaranteed to implement IConfigurableObject because Configurable property is true
        // ReSharper disable once PossibleNullReferenceException
        ((IConfigurableObject) this.encoder).UserConfiguration = Parameters.EncoderOptions;
      }

      // invoke parameterless constructor for the encoder
      encoderObject.Type.GetConstructor(new Type[] {})?.Invoke(this.encoder, new object[] {});
    }

    #region Private output stream (a.k.a. "actions") methods

    /// <summary>
    ///   Creates output stream instances
    /// </summary>
    /// <returns>
    ///   A triplet enumeration containing the stream instance, the plugin object representing it and an exception in
    ///   case the object activation failed
    /// </returns>
    private List<(Stream Stream, PluginObject PluginObject, Exception Exception)> CreateStreams() =>
      Application.PluginManager.OutputStreams
                 .Where(po => OutputStreams.Any(po2 => po2.Item1 == po.Type.FullName))
                 .Select(po => {
                   (Stream Stream, PluginObject PluginObject, Exception Exception) stream = (null, po, null);

                   try {
                     Log.WriteLine(LogLevel.Debug, $"activating {po.Type} instance");
                     stream.Stream = FormatterServices.GetSafeUninitializedObject(po.Type) as Stream;

                     if (stream.Stream is IOutputStream) {
                       Log.WriteLine(LogLevel.Informational, "setting encoder information");
                       po.Type.GetProperty("EncoderInfo")?.SetValue(stream.Stream, this.encoder.EncoderInfo);
                     }

                     // invoke constructor
                     po.Type.GetConstructor(new Type[] {})?.Invoke(stream.Stream, new object[] {});
                   } catch (Exception exception) {
                     Log.WriteLine(LogLevel.Warning, $"error creating instance: {exception}");
                     stream.Exception = exception;
                   }

                   return stream;
                 }).ToList();

    #endregion

    #region Recording state methods

    /// <summary>
    ///   Pauses the recording
    /// </summary>
    internal void Pause() {
      if (Type != TaskType.Recording) { throw new InvalidOperationException("Only recordings can be paused"); }
    }

    /// <summary>
    ///   Stops the recording
    /// </summary>
    internal void Stop() {
      if (Type != TaskType.Recording) { throw new InvalidOperationException("Only recordings can be stopped"); }
    }

    /// <summary>
    ///   Discards this recording
    /// </summary>
    internal void Cancel() {
      if (Type != TaskType.Recording) { throw new InvalidOperationException("Only recordings can be cancelled"); }
    }

    #endregion
  }
}