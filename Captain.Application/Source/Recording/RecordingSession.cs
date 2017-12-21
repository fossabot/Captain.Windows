using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading;
using Captain.Common;
using static Captain.Application.Application;
using Action = Captain.Common.Action;

namespace Captain.Application {
  internal sealed class RecordingSession {
    /// <summary>
    ///   Video codec instance.
    /// </summary>
    private readonly IVideoCodec codec;

    /// <summary>
    ///   Underlying task.
    /// </summary>
    private readonly Task task;

    /// <summary>
    ///   Effective region.
    /// </summary>
    private readonly Rectangle region;

    /// <summary>
    ///   Video provider.
    /// </summary>
    private VideoProvider videoProvider;

    /// <summary>
    ///   Multi-stream that contains all the action streams.
    /// </summary>
    private MultiStream stream;

    /// <summary>
    ///   Actions to be performed.
    /// </summary>
    private List<Action> actions;

    /// <summary>
    ///   Recording thread.
    /// </summary>
    private Thread recordingThread;

    /// <summary>
    ///   True if only textures are being used by the encoder.
    /// </summary>
    private bool dxgiEncoding;

    /// <summary>
    ///   Recording state.
    /// </summary>
    internal RecordingState State { get; private set; } = RecordingState.None;

    /// <summary>
    ///   Creates a new instance of this class.
    /// </summary>
    /// <param name="task">Task associated with this recording session.</param>
    /// <param name="region">Effective region.</param>
    internal RecordingSession(Task task, Rectangle region) {
      //this.region = region;
      this.region = new Rectangle(0, 0, 1920, 1080);
      this.task = task;

      try {
        // get uninitialized object in case we need to set Options property first
        this.codec = Activator.CreateInstance(Type.GetType(task.Codec.CodecType, true, true) ??
                                              throw new InvalidOperationException("No such codec loaded.")) as
          IVideoCodec;

        if (task.Codec.Options is Dictionary<string, object> userOptions &&
            this.codec is IHasOptions configurableObject) {
          // set user options
          configurableObject.Options = configurableObject.Options ?? new Dictionary<string, object>();

          foreach (KeyValuePair<string, object> pair in userOptions) {
            configurableObject.Options[pair.Key] = pair.Value;
          }
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"error initializing codec {task.Codec.CodecType}: {exception}");
        throw new TaskException(Resources.TaskHelper_EncodingFailedCaption,
          Resources.TaskHelper_EncodingInitializationFailedContent,
          exception);
      }
    }

    internal void Start() {
      this.videoProvider = VideoProviderFactory.Create(this.region, null, true);
      this.actions = this.task.Actions.Select(a => {
          try {
            // set options if needed
            if (a.Options != null &&
                Application.PluginManager.Actions.First(a2 => a2.Type.Name == a.ActionType).Configurable) {
              // create uninitialized instance
              var action = FormatterServices.GetSafeUninitializedObject(
                Type.GetType(a.ActionType, true, true) ??
                throw new InvalidOperationException(Resources.TaskHelper_NoSuchActionMessage)) as Action;

              // set options property
              a.GetType().GetProperty("Options")?.SetValue(action, a.Options);

              // call parameterless constructor
              action?.GetType()
                .GetConstructor(
                  BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                  null,
                  Type.EmptyTypes,
                  null)
                ?.Invoke(action, new object[] {this.codec});
            }

            return Activator.CreateInstance(Type.GetType(a.ActionType) ??
                                            throw new InvalidOperationException(
                                              Resources.TaskHelper_NoSuchActionMessage),
              this.codec) as Action;
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"error initializing action {a.ActionType}: {exception}");

            // create dummy action for displaying error
            var action = new Action(this.codec);
            action.SetStatus(ActionStatus.Failed,
              new Exception(Resources.TaskHelper_ActionInitializationFailedCaption, exception));
            return action;
          }
        })
        .Where(a => a != null)
        .ToList();

      this.stream = new MultiStream();
      this.actions.ForEach(a => {
        this.stream.Add(a);
        Application.ActionManager.AddAction(a);
      });

      if (this.codec is GenericMfCodec mediaFoundationCodec &&
          this.videoProvider is DxgiVideoProvider dxgiProvider) {
        // accelerate MediaFoundation encoding if we could get the whole screen in a single texture
        mediaFoundationCodec.SourceTexture = dxgiProvider.SharedTexture;
        this.dxgiEncoding = true;
      }

      this.dxgiEncoding = true;
      this.codec?.Initialize(this.videoProvider.CaptureBounds.Size, this.stream);

      this.recordingThread = new Thread(Record) {Priority = ThreadPriority.Highest};
      this.recordingThread.SetApartmentState(ApartmentState.MTA);
      this.recordingThread.Start();

      Application.TrayIcon.AnimateIndicator(IndicatorStatus.Recording, 500);
      Application.Hud.SnackBar.MorphRecordButton(true);
    }

    [DllImport("kernel32.dll", SetLastError = true)]
    private static extern bool QueryPerformanceFrequency(out long frequency);

    /// <summary>
    ///   Starts recording.
    /// </summary>
    /// <remarks>
    ///   This method is meant to be the entry for a background thread!
    /// </remarks>
    private void Record() {
      BitmapData data = default;
      long startTime = 0,
           lastPresentTime = 0;

      QueryPerformanceFrequency(out long freq);
      Log.WriteLine(LogLevel.Debug, $"performance frequency: {freq}");
      State = RecordingState.Recording;

      while (State == RecordingState.Recording) {
        this.videoProvider.AcquireFrame();

        // query last desktop update time
        long presentTime = ((DxgiVideoProvider)this.videoProvider).LastPresentTime;

        if (startTime == 0) {
          // begin counting time at the first frame
          startTime = presentTime;
        }

        if (lastPresentTime != presentTime) {
          // desktop has been updated
          // TODO: query updated regions so we don't count on updates outside the selected screen region

          // lock bitmap memory so we can read from it, if hardware-assisted encoding is not available
          if (!this.dxgiEncoding) { data = this.videoProvider.LockFrameBitmap(); }

          // encode frame at the frame update time, in 100-nanosecond units
          this.codec.Encode(data, (long) ((presentTime - startTime) * 10e6 / freq), this.stream);

          // on non-hardware-assisted encoding, unlock de bitmap memory
          if (!this.dxgiEncoding) { this.videoProvider.UnlockFrameBitmap(data); }

          // release resources used by the video provider
          this.videoProvider.ReleaseFrame();
        } else {
          // desktop unchanged
          lastPresentTime = presentTime;
        }
      }
    }

    /// <summary>
    ///   Pauses the recording.
    /// </summary>
    /// <remarks>
    ///   Beware of resources created by Record(), which belong to another thread!
    /// </remarks>
    internal void Pause() { }

    /// <summary>
    ///   Finalizes this recording session.
    /// </summary>
    /// <remarks>
    ///   Beware of resources created by Record(), which belong to another thread!
    /// </remarks>
    internal void Stop() {
      State = RecordingState.None;
      this.recordingThread.Join();
      Application.Hud.SnackBar.MorphRecordButton();
      Application.TrayIcon.AnimateIndicator(IndicatorStatus.Progress);
      this.codec.Finalize(this.stream);
      this.stream.Dispose();
      this.videoProvider.Dispose();
      GC.Collect();
    }
  }
}