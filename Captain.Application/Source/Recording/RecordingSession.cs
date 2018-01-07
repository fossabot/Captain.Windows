using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;
using Action = Captain.Common.Action;

namespace Captain.Application {
  internal sealed class RecordingSession : IDisposable {
    /// <summary>
    ///   Video codec instance.
    /// </summary>
    private readonly IVideoCodec codec;

    /// <summary>
    ///   Effective region.
    /// </summary>
    private readonly Rectangle region;

    /// <summary>
    ///   Underlying task.
    /// </summary>
    private readonly Task task;

    /// <summary>
    ///   Actions to be performed.
    /// </summary>
    private List<Action> actions;

    /// <summary>
    ///   Whether the video provider needs its frames to be locked.
    /// </summary>
    private bool isAcceleratedEncoding;

    /// <summary>
    ///   Recording thread.
    /// </summary>
    private Thread recordingThread;

    /// <summary>
    ///   Multi-stream that contains all the action streams.
    /// </summary>
    private MultiStream stream;

    /// <summary>
    ///   Video provider.
    /// </summary>
    private IBitmapVideoProvider videoProvider;

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
      this.region = region;
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

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      State = RecordingState.None;
      this.stream?.Dispose();
      this.videoProvider?.Dispose();
      GC.Collect();
    }

    internal void Start() {
      this.videoProvider = VideoProviderFactory.Create(this.region);
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
                ?.Invoke(action, new object[] { this.codec });
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

      if (this.codec is ID3D11Codec acceleratedCodec &&
          this.videoProvider is ID3D11VideoProvider acceleratedVideoProvider &&
          acceleratedVideoProvider.SurfacePointer != IntPtr.Zero) {
        acceleratedCodec.SurfacePointer = acceleratedVideoProvider.SurfacePointer;
        this.isAcceleratedEncoding = true;
        Log.WriteLine(LogLevel.Informational, "performing hardware-assisted encoding");
      }

      this.codec?.Initialize(this.videoProvider.CaptureBounds.Size, this.stream);

      this.recordingThread = new Thread(Record) { Priority = ThreadPriority.Highest };
      this.recordingThread.SetApartmentState(ApartmentState.MTA);
      this.recordingThread.Start();

      State = RecordingState.Recording;
      Application.TrayIcon.AnimateIndicator(IndicatorStatus.Recording, 500);
    }

    /// <summary>
    ///   Starts recording.
    /// </summary>
    /// <remarks>
    ///   This method is meant to be the entry for a background thread!
    /// </remarks>
    private void Record() {
      BitmapData data = default;
      long startTime = 0;

      Kernel32.QueryPerformanceFrequency(out long freq);
      Log.WriteLine(LogLevel.Debug, $"performance frequency: {freq}");

      while (State != RecordingState.None) {
        this.videoProvider.AcquireFrame();

        // query last desktop update time
        long presentTime = this.videoProvider.LastPresentTime;

        if (State != RecordingState.Recording) {
          try {
            // wait until thread is awoken by being interrupted
            startTime = presentTime;
            Thread.Sleep(Timeout.Infinite);
          } catch (ThreadInterruptedException) {
            // recording resumed
          }
        } else {
          if (startTime == 0) { startTime = presentTime; }

          // desktop has been updated
          // TODO: query updated regions so we don't count on updates outside the selected screen region

          // lock bitmap memory so we can read from it, if hardware-assisted encoding is not available
          if (!this.isAcceleratedEncoding) { data = this.videoProvider.LockFrameBitmap(); }

          // encode frame at the frame update time, in 100-nanosecond units
          this.codec.Encode(data, (long) ((presentTime - startTime) * 10e6 / freq), this.stream);

          // on non-hardware-assisted encoding, unlock de bitmap memory
          if (!this.isAcceleratedEncoding) { this.videoProvider.UnlockFrameBitmap(data); }

          // release resources used by the video provider
          this.videoProvider.ReleaseFrame();
        }
      }
    }

    /// <summary>
    ///   Pauses the recording.
    /// </summary>
    /// <remarks>
    ///   Beware of resources created by Record(), which belong to another thread!
    /// </remarks>
    internal void Pause() {
      if (State != RecordingState.Recording) { return; }

      State = RecordingState.Paused;
      this.recordingThread.Priority = ThreadPriority.Lowest;
      Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
      GC.Collect();
    }

    /// <summary>
    ///   Resumes recording.
    /// </summary>
    internal void Resume() {
      if (State != RecordingState.Paused) { return; }

      State = RecordingState.Recording;
      this.recordingThread.Interrupt();
      this.recordingThread.Priority = ThreadPriority.Highest;
      Application.TrayIcon.AnimateIndicator(IndicatorStatus.Recording);
    }

    /// <summary>
    ///   Finalizes this recording session.
    /// </summary>
    /// <remarks>
    ///   Beware of resources created by Record(), which belong to another thread!
    /// </remarks>
    internal void Stop() {
      State = RecordingState.None;
      this.recordingThread.Interrupt();
      this.recordingThread.Join();
      Application.TrayIcon.AnimateIndicator(IndicatorStatus.Progress);
      this.codec?.Finalize(this.stream);
      Dispose();
    }
  }
}