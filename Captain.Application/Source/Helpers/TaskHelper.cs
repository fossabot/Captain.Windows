using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using SharpDX;
using static Captain.Application.Application;
using Action = Captain.Common.Action;
using BitmapData = Captain.Common.BitmapData;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;
using Task = Captain.Common.Task;

namespace Captain.Application {
  /// <summary>
  ///   Helper class for interacting with tasks
  /// </summary>
  internal static class TaskHelper {
    /// <summary>
    ///   Starts a given task.
    /// </summary>
    /// <param name="task">The task instance to be started.</param>
    /// <param name="effectiveRegion">Optional reset region for this task</param>
    internal static async void StartTask(Task task, Rectangle? effectiveRegion = null) {
      Application.Hud.Display(false);

      if (!effectiveRegion.HasValue) {
        switch (task.RegionType) {
          case RegionType.AllScreens:
            effectiveRegion = new Rectangle(
              Screen.AllScreens.Min(s => s.Bounds.X),
              Screen.AllScreens.Min(s => s.Bounds.Y),
              Screen.AllScreens.Min(s => s.Bounds.Width),
              Screen.AllScreens.Min(s => s.Bounds.Height));
            break;

          case RegionType.CurrentScreen:
            effectiveRegion = Screen.FromPoint(Control.MousePosition).Bounds;
            break;

          case RegionType.Fixed:
            effectiveRegion = task.Region;
            break;

          case RegionType.UserSelected:
            Log.WriteLine(LogLevel.Debug, "waiting for HUD to yield screen region");
            Application.Hud.Display();

            // create completion source for crop event and bind handler
            var completionSource = new TaskCompletionSource<Rectangle>();
            void OnScreenCrop(object sender, Rectangle bounds) { completionSource.SetResult(bounds); }
            Application.Hud.OnScreenCrop += OnScreenCrop;

            // wait for a screen region to be selected
            effectiveRegion = await completionSource.Task;

            // unbind event handler so it does not get called once again on next screenshot (remember the HUD is
            // unlikely to be disposed!)
            Application.Hud.OnScreenCrop -= OnScreenCrop;
            Log.WriteLine(LogLevel.Debug,
              $"got rectangle: {effectiveRegion.Value.Width}x{effectiveRegion.Value.Height} " +
              $"at ({effectiveRegion.Value.X}, {effectiveRegion.Value.Y})");
            break;
        }
      }

      // sanity check
      if (!effectiveRegion.HasValue) { return; }

      // perform capture
      try {
        switch (task.TaskType) {
          case TaskType.StillImage:
            // set tray icon animation
            Application.TrayIcon.AnimateIndicator(IndicatorStatus.Progress);

            // start the actual task
            var actions = StartScreenshotTask(task, effectiveRegion.Value).ToList();
            actions.ForEach(a => a.OnStatusChanged += OnActionStatusChanged);

            // HACK: force refresh
            OnActionStatusChanged(null, ActionStatus.Ongoing);

            void OnActionStatusChanged(object o, ActionStatus actionStatus) {
              if (actions.Any(a => a.Status == ActionStatus.Paused)) {
                Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
                // not all finished
              } else if (actions.All(a => a.Status != ActionStatus.Ongoing)) {
                if (actions.Any(a => a.Status == ActionStatus.Failed)) {
                  Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
                  Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);
                  // some/all failed
                } else {
                  Application.TrayIcon.SetIndicator(IndicatorStatus.Idle);
                  Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Success);
                  // all succeeded
                }
              }
            }

            break;

          case TaskType.Video: throw new NotImplementedException();
        }
      } catch (TaskException exception) {
        Application.TrayIcon.SetTimedIndicator(IndicatorStatus.Warning);
        LegacyNotificationProvider.PushMessage(exception.ShortMessage,
          exception.Message,
          ToolTipIcon.Error,
          null,
          (s, a) => {
            TaskDialogButton button = DisplayTaskExceptionDialog(exception);

            if (button.ButtonType == ButtonType.Retry) {
              Log.WriteLine(LogLevel.Verbose, "user requested retry");
              StartTask(task, effectiveRegion);
            }
          });
      }
    }

    /// <summary>
    ///   Displays a generic task exception dialog
    /// </summary>
    /// <remarks>
    ///   TODO: consider moving this to a UI helper class
    /// </remarks>
    /// <param name="exception">An instance of <see cref="TaskException"/>.</param>
    /// <returns>The <see cref="TaskDialogButton"/> that had been activated.</returns>
    private static TaskDialogButton DisplayTaskExceptionDialog(TaskException exception) => new TaskDialog {
      WindowTitle = System.Windows.Forms.Application.ProductName,
      MainIcon = TaskDialogIcon.Error,
      WindowIcon = Resources.AppIcon,
      AllowDialogCancellation = true,
      Width = 200,
      Buttons = {
        new TaskDialogButton(ButtonType.Close) {Default = true},
        new TaskDialogButton(ButtonType.Retry),
        new TaskDialogButton("&Report…") {Enabled = false} // TODO: implement bug reporter helper
      },
      MainInstruction = exception.ShortMessage,
      Content = exception.Message,
      ExpandFooterArea = true,
      ExpandedInformation = exception.InnerException?.ToString() ?? exception.ToString()
    }.ShowDialog();

    /// <summary>
    ///   Captures an still image, encodes it and executes each bound action.
    /// </summary>
    /// <param name="task">Task to be run.</param>
    /// <param name="rect">Rectangle to be captured.</param>
    /// <returns>An enumeration of the <see cref="Action"/>s running.</returns>
    private static IEnumerable<Action> StartScreenshotTask(Task task, Rectangle rect) {
      VideoProvider provider = null;
      BitmapData bmpData = default;
      Bitmap thumbnail;

      try {
        // create video provider
        provider = VideoProviderFactory.Create(rect);

        // acquire frame and lock frame bits for reading so that codecs can work over it
        provider.AcquireFrame();
        bmpData = provider.LockFrameBitmap();

        // generate thumbnail
        thumbnail = new Bitmap(bmpData.Width, bmpData.Height);
        System.Drawing.Imaging.BitmapData data = thumbnail.LockBits(new Rectangle(Point.Empty, thumbnail.Size),
          ImageLockMode.WriteOnly,
          (PixelFormat) bmpData.PixelFormatId);
        Utilities.CopyMemory(data.Scan0, bmpData.Scan0, bmpData.Height * bmpData.Stride);
        thumbnail.UnlockBits(data);

        using (var tempBmp = new Bitmap(Resources.NeutralResultOverlay.Width - 48,
          Resources.NeutralResultOverlay.Height - 48)) {
          using (var graphics = Graphics.FromImage(tempBmp)) {
            graphics.DrawImage(thumbnail,
              new Rectangle(Point.Empty, tempBmp.Size),
              new Rectangle(Point.Empty, thumbnail.Size),
              GraphicsUnit.Pixel);
          }

          thumbnail.Dispose();
          thumbnail = tempBmp.Clone() as Bitmap;
        }
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"video provider exception: ${exception}");
        if (bmpData.Scan0 == default) { provider?.UnlockFrameBitmap(bmpData); }
        provider?.ReleaseFrame();
        provider?.Dispose();
        throw new TaskException(Resources.TaskHelper_CaptureFailedCaption,
          Resources.TaskHelper_CaptureFailedContent,
          exception);
      }

      // initialize codec
      IStillImageCodec codec;

      try {
        // get uninitialized object in case we need to set Options property first
        codec = FormatterServices.GetSafeUninitializedObject(
          Type.GetType(task.Codec.CodecType, true, true) ??
          throw new InvalidOperationException("No such codec loaded.")) as IStillImageCodec;

        if (task.Codec.Options != null &&
            Application.PluginManager.StillImageCodecs.First(c => c.Type.Name == task.Codec.CodecType).Configurable) {
          // set Options property
          task.Codec.GetType().GetProperty("Options")?.SetValue(codec, task.Codec.Options);
        }

        // call parameterless constructor
        task.Codec.GetType().GetConstructor(new Type[] { })?.Invoke(codec, new object[] { });
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"error initializing codec {task.Codec.CodecType}: {exception}");
        throw new TaskException(Resources.TaskHelper_EncodingFailedCaption,
          Resources.TaskHelper_EncodingInitializationFailedContent,
          exception);
      }

      // create temporary memory stream for holding the encoded data
      var stream = new MemoryStream();
      try {
        // encode the still image
        codec?.Encode(bmpData, stream);
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"error encoding still image: {exception}");

        stream.Dispose();
        provider.UnlockFrameBitmap(bmpData);
        provider.ReleaseFrame();
        provider.Dispose();
        thumbnail?.Dispose();

        throw new TaskException(Resources.TaskHelper_EncodingFailedCaption,
          Resources.TaskHelper_EncodingFailedContent,
          exception);
      }

      var actions = task.Actions.Select(a => {
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
                ?.Invoke(action, new object[] { });
            }

            return Activator.CreateInstance(Type.GetType(a.ActionType) ??
                                            throw new InvalidOperationException(
                                              Resources.TaskHelper_NoSuchActionMessage)) as Action;
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"error initializing action {a.ActionType}: {exception}");

            // create dummy action for displaying error
            var action = new Action();
            action.SetStatus(ActionStatus.Failed,
              new Exception(Resources.TaskHelper_ActionInitializationFailedCaption, exception));
            return action;
          }
        })
        .Where(a => a != null)
        .ToList();

      actions.ForEach(a => {
        void Release() {
          if (a == actions.Last()) {
            // this was the last action, release the temporary stream. It's not disposed unless the last stream
            // fails to initialize
            // ReSharper disable once AccessToDisposedClosure
            Log.WriteLine(LogLevel.Debug, "releasing resources");
            stream.Dispose();
            provider.UnlockFrameBitmap(bmpData);
            provider.ReleaseFrame();
            provider.Dispose();
            GC.Collect();
          }
        }

        // set preview bitmap
        a.Thumbnail = thumbnail;

        try {
          if (a is IFiltered filteredAction) {
            // action applies media type filters to captures
            if (!filteredAction.GetMediaAcceptance(task.TaskType, codec, task.Codec.Options as ICodecParameters)) {
              Log.WriteLine(LogLevel.Warning, "media filter did not accept this capture");
              throw new Exception(Resources.TaskHelper_UnsupportedMediaMessage);
            }
          }

          if (a is IPreBitmapEncodingAction preEncodingAction) {
            // set bitmap data instead of copying to the stream
            Log.WriteLine(LogLevel.Debug, $"setting bitmap data for {a.GetType().Name}");
            preEncodingAction.SetBitmapData(bmpData);
            Release();
            a.SetStatus(ActionStatus.Success);
            Log.WriteLine(LogLevel.Informational, $"action {a.GetType().Name} is done");
          } else {
            // copy data to the action stream
            Log.WriteLine(LogLevel.Debug, $"writing {stream.Length} bytes to {a.GetType().Name}");
            a.SetLength(stream.Length);

            new Thread(() => {
              using (var target = new BufferedStream(a, a.BufferSize)) { stream.WriteTo(target); }

              a.Flush();
              a.Dispose();
              Release();
              Log.WriteLine(LogLevel.Informational, $"action {a.GetType().Name} is ready");
            }).Start();
          }
        } catch (Exception exception) {
          Release();
          Log.WriteLine(LogLevel.Warning, $"error copying to action stream {a.GetType().Name}: {exception}");
          a.SetStatus(ActionStatus.Failed, new Exception(Resources.TaskHelper_ActionFailedCaption, exception));
        }
      });

      return actions;
    }
  }
}