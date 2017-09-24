using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Icon shown on the notification area from which the user can access diverse actions and settings
  /// </summary>
  /// <remarks>
  ///   The implementation for this class contains lots of hardcoded values.
  ///   TODO: Think of a more cleaner solution without adding much abstraction
  /// </remarks>
  internal class TrayIcon {
    /// <summary>
    ///   Square size of each indicator icon
    /// </summary>
    private const uint IndicatorIconSquare = 16;

    /// <summary>
    ///   Maximum TTL, in milliseconds, for the warning badge
    ///   Yes I'm being serious on this one
    /// </summary>
    internal const int WarningBadgeTtl = 10_000;

    /// <summary>
    ///   When true, a looping animation is being played
    /// </summary>
    private bool isLoopingAnimationPlaying;

    /// <summary>
    ///   Animation thread
    /// </summary>
    private Thread loopingAnimationThread;

    /// <summary>
    ///   Holds the zero-based index of the icon row on the indicator strip
    /// </summary>
    private readonly TrayIconVariant iconVariant;

    /// <summary>
    ///   Contains a cache of all the indicator bitmaps to be used within the tray icon
    /// </summary>
    private readonly Icon[] indicators;

    /// <summary>
    ///   Exposes underlying NotifyIcon
    /// </summary>
    internal NotifyIcon NotifyIcon { get; }

    /// <summary>
    ///   Instantiates a new TrayIcon
    /// </summary>
    internal TrayIcon() {
      var contextMenu = new ContextMenu();
      NotifyIcon = new NotifyIcon { ContextMenu = contextMenu };

      contextMenu.MenuItems.AddRange(new[] {
        new MenuItem(Resources.AppMenu_Capture,
                     (_, __) => {

                     }) {
          DefaultItem = true,
          Visible = true
        },
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Options, (_, __) => {}),  // TODO: implement this
        new MenuItem(Resources.AppMenu_About),
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Exit, (_, __) => Exit())
      });

      // get the platform-dependent indicator style variant
      if (Environment.OSVersion.Version.Major < 6) {
        this.iconVariant = TrayIconVariant.Classic;
      } else if (Environment.OSVersion.Version.Major > 6) {
        this.iconVariant = TrayIconVariant.Modern;
      } else if (Environment.OSVersion.Version.Minor > 2) {
        this.iconVariant = TrayIconVariant.Metro;
      } else {
        this.iconVariant = TrayIconVariant.Aero;
      }

      Log.WriteLine(LogLevel.Debug, $"indicator variant: {this.iconVariant}");

      // allocate array of bitmaps for indicator classes
      this.indicators = new Icon[14 + (this.iconVariant == TrayIconVariant.Modern
                                         ? 5
                                         : this.iconVariant == TrayIconVariant.Aero
                                           ? 13
                                           : 0)];
      Log.WriteLine(LogLevel.Debug, $"created {this.indicators.Length} indicator icons");

      // set initial icon
      SetIcon();

      // discover capture providers and display them as sub-menu items
      //InitializeCaptureMenu();
    }

    /// <summary>
    ///   TrayIcon instance destructor
    /// </summary>
    ~TrayIcon() {
      Log.WriteLine(LogLevel.Debug, "releasing resources");
      Hide();
      NotifyIcon.Dispose();
    }

    /// <summary>
    ///   Crops a frame from the tray indicator strip bitmap
    /// </summary>
    /// <param name="col">Colummn index</param>
    /// <param name="row">Row index</param>
    /// <returns>An Icon with the cropped frame</returns>
    private static Icon CropIconFrame(uint col, uint row) {
      var bitmap = new Bitmap((int)IndicatorIconSquare, (int)IndicatorIconSquare);

      using (var graphics = Graphics.FromImage(bitmap)) {
        var (width, height) = ((int)IndicatorIconSquare, (int)IndicatorIconSquare);
        var (x, y) = (width * (int)col, height * (int)row);

        graphics.DrawImage(Resources.TrayIconStrip, new Rectangle(0, 0, width, width),
                           new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
      }

      return Icon.FromHandle(bitmap.GetHicon());
    }

    /// <summary>
    ///   Displays the tray icon
    /// </summary>
    internal void Show() => NotifyIcon.Visible = true;

    /// <summary>
    ///   Hides the tray icon
    /// </summary>
    internal void Hide() => NotifyIcon.Visible = false;

    /// <summary>
    ///   Plays a looping animation with the specified icon class
    /// </summary>
    /// <param name="iconClass">Animation icon class</param>
    internal void PlayLoopingIconAnimation(TrayIconClass iconClass = TrayIconClass.IndeterminateProgress) {
#if false  /// XXX: possibly dead code?
      if (this.loopingAnimationThread != null && this.isLoopingAnimationPlaying) {
        Log.WriteLine(LogLevel.Warning, "you are supposed to call StopLoopingIconAnimation() - do your job!");
        StopLoopingIconAnimation(iconClass);

        this.isLoopingAnimationPlaying = false;  // we want to stop the inner thread loop
        this.loopingAnimationThread.Join();      // wait for the previous thread to end
      }
#endif

      this.isLoopingAnimationPlaying = true;
      this.loopingAnimationThread = new Thread(() => {
        uint progressValue = 0;

        while (this.isLoopingAnimationPlaying) {
          SetIcon(iconClass, progressValue = (progressValue + 5) % 100);
          Thread.Sleep(30);
        }
      });
      this.loopingAnimationThread.Start();
    }

    /// <summary>
    ///   Stops a previously started looping animation
    /// </summary>
    /// <param name="newIconClass">New icon class to be set after the animation stops</param>
    internal void StopLoopingIconAnimation(TrayIconClass newIconClass = TrayIconClass.Application) {
#if false  /// XXX: possibly dead code?
      if (!this.isLoopingAnimationPlaying) {
        Log.WriteLine(LogLevel.Warning, "tried to stop looping animation when a static indicator is set");
        return;
      }
#endif

      this.isLoopingAnimationPlaying = false;  // stop animation
      this.loopingAnimationThread.Join();      // wait for the thread to terminate
      SetIcon(newIconClass);                   // bang!
    }

    /// <summary>
    ///   Sets the current icon
    /// </summary>
    /// <param name="iconClass">Tray icon kind</param>
    /// <param name="progressValue">
    ///   Progress value for <see cref="TrayIconClass.DeterminateProgress"/> and
    ///   <see cref="TrayIconClass.IndeterminateProgress"/>, with a maximum value of <c>100</c>.
    /// </param>
    /// <remarks>
    ///   When <c>iconClass</c> parameter is set to any other value than <see cref="TrayIconClass.Warning"/>,
    ///   <c>progressValue</c> parameter must be non-null.
    /// </remarks>
    internal void SetIcon(TrayIconClass iconClass = TrayIconClass.Application, uint? progressValue = null) {
      uint index = 0;
      NotifyIcon.Text = VersionInfo.ProductName;

      switch (iconClass) {
        case TrayIconClass.Warning:
          index = 1;
          break;

        case TrayIconClass.DeterminateProgress:
          // 0.10 = 11 - 1 progress icons / 100 (maximum progressValue) on modern icon variant
          // 0.07 = 8 - 1 progress icons / 100 (maximum progressValue) on the rest of variants
          // ReSharper disable once PossibleInvalidOperationException
          index = 2 + (uint)Math.Ceiling((this.iconVariant == TrayIconVariant.Modern ? 0.10 : 0.07) *
                                         (uint)progressValue);
          NotifyIcon.Text = $@"{VersionInfo.ProductName} ({progressValue}%)";
          break;

        case TrayIconClass.IndeterminateProgress:
          // 0.03 = 4 - 1 progress icons / 100 (maximum progressValue) on classic and metro icon variants
          // 0.17 = 18 - 1 progress icons / 100 (maximum progressValue) on aero icon variants
          // 0.05 = 6 - 1 progress icons / 100 (maximum progressValue) on modern icon variant
          // ReSharper disable once PossibleInvalidOperationException
          index = 1 + (uint)((this.iconVariant == TrayIconVariant.Modern ? 13 : 9) +
                             Math.Floor((this.iconVariant == TrayIconVariant.Modern
                                           ? 0.05
                                           : this.iconVariant == TrayIconVariant.Aero
                                             ? 0.17
                                             : 0.03) * (uint)progressValue));
          break;
      }

      if (this.indicators[index] == null) {
        // not in cache (yet)
        this.indicators[index] = CropIconFrame(index, (uint)this.iconVariant);
      }

      NotifyIcon.Icon = this.indicators[index];
    }
  }
}