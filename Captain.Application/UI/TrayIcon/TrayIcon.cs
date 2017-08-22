using System;
using System.Collections;
using System.Drawing;
using System.Windows.Forms;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Icon shown on the notification area from which the user can access diverse actions and settings
  /// </summary>
  internal class TrayIcon {
    /// <summary>
    ///   Square size of each indicator icon
    /// </summary>
    private const uint IndicatorIconSquare = 16;

    /// <summary>
    ///   Tray icon context menu
    /// </summary>
    private ContextMenu contextMenu;

    /// <summary>
    ///   Holds the zero-based index of the icon row on the indicator strip
    /// </summary>
    private TrayIconVariant iconVariant;

    /// <summary>
    ///   Contains a cache of all the indicator bitmaps to be used within the tray icon
    /// </summary>
    private Icon[] indicators;

    /// <summary>
    ///   Exposes underlying NotifyIcon
    /// </summary>
    internal NotifyIcon NotifyIcon { get; }

    /// <summary>
    ///   Instantiates a new TrayIcon
    /// </summary>
    internal TrayIcon() {
      this.contextMenu = new ContextMenu();
      NotifyIcon = new NotifyIcon { ContextMenu = this.contextMenu };
      NotifyIcon.MouseClick += OnTrayIconClick;

      this.contextMenu.MenuItems.AddRange(new[] {
        new MenuItem(Resources.AppMenu_Capture,
                     (_, __) => {

                     }) {
          DefaultItem = true,
          Visible = true
        },
        new MenuItem("-"),
        new MenuItem(Resources.AppMenu_Options, (_, __) => throw new NotImplementedException()),
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
      Log.WriteLine(LogLevel.Debug, $"allocated {this.indicators.Length} indicator icons");

      // set initial icon
      SetIcon();

      // discover capture providers and display them as sub-menu items
      //InitializeCaptureMenu();
    }

    /// <summary>
    ///   TrayIcon instance destructor
    /// </summary>
    ~TrayIcon() {
      Log.WriteLine(LogLevel.Debug, "disposing tray icon");
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
    ///   Builds the "Capture" menu item by discovering the available capture providers
    /// </summary>
    private void InitializeCaptureMenu() {
      var captureMenu = this.contextMenu.MenuItems[0];
      captureMenu.MenuItems.Clear();

      if (this.contextMenu.MenuItems[1].Text != @"-") {  // HACK
        // there's a provider menu item "superseding" the capture menu item - remove it
        this.contextMenu.MenuItems.RemoveAt(1);

        // and bring the capture menu item back
        captureMenu.Visible = true;
      }

      /*foreach (CaptureProvider provider in Application.PluginManager.CaptureProviders) {
        captureMenu.MenuItems.Add(provider.ToString(), (_, __) => Action.GetDefault(provider).Perform());
      }

      if (Application.PluginManager.CaptureProviders.Count == 0) {
        captureMenu.Enabled = false;
      } else if (
        Application.PluginManager.CaptureProviders.Count == 1) {
        // there's a single capture provider - "replace" the "Capture" menu item
        captureMenu.Visible = false;
        captureMenu.MenuItems[0].DefaultItem = true;
        (this.contextMenu.MenuItems as IList).Insert(1, captureMenu.MenuItems[0]);
      */
    }

    /// <summary>
    ///   Event handler triggered when the tray icon is clicked
    /// </summary>
    /// <param name="sender">Event sender</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnTrayIconClick(object sender, MouseEventArgs eventArgs) {
      if (eventArgs.Button == MouseButtons.Left) {
        // execute default action
        //Action.GetDefault().Perform();
      }
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
    ///   Sets the current icon
    /// </summary>
    /// <param name="iconClass">Tray icon kind</param>
    /// <param name="progressValue">Progress value for <see cref="TrayIconClass.DeterminateProgress"/> and <see cref="TrayIconClass.IndeterminateProgress"/>, with a maximum value of <c>100</c>.</param>
    internal void SetIcon(TrayIconClass iconClass = TrayIconClass.Application, uint? progressValue = null) {
      uint index = 0;
      NotifyIcon.Text = VersionInfo.ProductName;

      switch (iconClass) {
        case TrayIconClass.Warning:
          index = 1;
          break;

        case TrayIconClass.DeterminateProgress:
          // XXX: progressValue MUST be non-null when iconClass == TrayIconClass.DeterminateProgress!
          // 0.10 = 11 - 1 progress icons / 100 (maximum progressValue) on modern icon variant
          // 0.07 = 8 - 1 progress icons / 100 (maximum progressValue) on the rest of variants
          // ReSharper disable once PossibleInvalidOperationException
          index = 2 + (uint)Math.Ceiling((this.iconVariant == TrayIconVariant.Modern ? 0.10 : 0.07) *
                                         (uint)progressValue);
          NotifyIcon.Text = $@"{VersionInfo.ProductName} ({progressValue}%)";
          break;

        case TrayIconClass.IndeterminateProgress:
          // XXX: progressValue MUST be non-null when iconClass == TrayIconClass.IndeterminateProgress
          // 0.03 = 4 - 1 progress icons / 100 (maximum progressValue) on classic and metro icon variants
          // 0.17 = 18 - 1 progress icons / 100 (maximum progressValue) on aero icon variants
          // 0.05 = 6 - 1 progress icons / 100 (maximum progressValue) on modern icon variant
          // ReSharper disable once PossibleInvalidOperationException
          index = 2 + (uint)(this.iconVariant == TrayIconVariant.Modern
                               ? 13
                               : 10 + Math.Ceiling((this.iconVariant == TrayIconVariant.Modern
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