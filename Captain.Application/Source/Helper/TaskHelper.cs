using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using SharpDX;
using static Captain.Application.Application;
using Action = Captain.Common.Action;
using BitmapData = Captain.Common.BitmapData;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

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

          case RegionType.UserSelected: throw new NotImplementedException();
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
            StartScreenshotTask(task, effectiveRegion.Value);
            break;

          case TaskType.Video: throw new NotImplementedException();
        }
      } catch (TaskException exception) { throw new NotImplementedException(); }
    }

    /// <summary>
    ///   Displays a generic task exception dialog
    /// </summary>
    /// <remarks>
    ///   TODO: consider moving this to a UI helper class
    /// </remarks>
    /// <param name="exception">An instance of <see cref="TaskException" />.</param>
    /// <returns>The <see cref="TaskDialogButton" /> that had been activated.</returns>
    private static TaskDialogButton DisplayTaskExceptionDialog(TaskException exception) => new TaskDialog {
      WindowTitle = System.Windows.Forms.Application.ProductName,
      MainIcon = TaskDialogIcon.Error,
      WindowIcon = Resources.AppIcon,
      AllowDialogCancellation = true,
      Width = 200,
      Buttons = {
        new TaskDialogButton(ButtonType.Close) { Default = true },
        new TaskDialogButton(ButtonType.Retry),
        new TaskDialogButton(Resources.TaskHelper_DisplayTaskExceptionDialog_ReportButtonCaption) {
          Enabled = false
        } // TODO: implement bug reporter helper
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
    /// <returns>An enumeration of the <see cref="Common.Action" />s running.</returns>
    private static void StartScreenshotTask(Task task, Rectangle rect) {
      IBitmapVideoProvider provider = null;
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
        // TODO: debug this
        System.Drawing.Imaging.BitmapData data = thumbnail.LockBits(new Rectangle(Point.Empty, thumbnail.Size),
          ImageLockMode.WriteOnly,
          PixelFormat.Format32bppArgb);
        Utilities.CopyMemory(data.Scan0, bmpData.Scan0, bmpData.Height * bmpData.Stride);
        thumbnail.UnlockBits(data);

        using (var tempBmp = new Bitmap(4 * (Resources.NeutralResultOverlay.Width - 48),
          4 * (Resources.NeutralResultOverlay.Height - 48))) {
          using (Graphics graphics = Graphics.FromImage(tempBmp)) {
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
        codec = Activator.CreateInstance(Type.GetType(task.Codec.CodecType, true, true) ??
                                         throw new InvalidOperationException("No such codec loaded.")) as
          IStillImageCodec;

        if (task.Codec.Options is Dictionary<string, object> userOptions &&
            codec is IHasOptions configurableObject) {
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

      List<Action> actions = task.Actions.Select(a => {
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
                ?.Invoke(action, new object[] { codec });
            }

            return Activator.CreateInstance(Type.GetType(a.ActionType) ??
                                            throw new InvalidOperationException(
                                              Resources.TaskHelper_NoSuchActionMessage),
              codec) as Action;
          } catch (Exception exception) {
            Log.WriteLine(LogLevel.Warning, $"error initializing action {a.ActionType}: {exception}");

            // create dummy action for displaying error
            var action = new Action(codec);
            action.SetStatus(ActionStatus.Failed,
              new Exception(Resources.TaskHelper_ActionInitializationFailedCaption, exception));
            return action;
          }
        })
        .Where(a => a != null)
        .ToList();

      actions.ForEach(a => {
        Application.ActionManager.AddAction(a);

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
    }
  }
}