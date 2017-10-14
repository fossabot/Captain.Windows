using System;
using System.Drawing;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Renders indicator icons on Windows Vista and 7
  /// </summary>
  internal class AeroIndicatorRenderer : IIndicatorRenderer {
    /// <summary>
    ///   Holds tray icon indicators
    /// </summary>
    private readonly Icon[] indicators;

    /// <summary>
    ///   Integer counter for animation frames
    /// </summary>
    private byte counter;

    /// <summary>
    ///   Class constructor
    /// </summary>
    internal AeroIndicatorRenderer() {
      // icon defaults
      int yOffset = 0;
      int iconSize = 32;

      using (Bitmap indicatorStrip = Resources.AeroIndicatorStrip) {
        int iconCount = indicatorStrip.Width / iconSize;

        // crop the icon strip for the current DPI setting
        if (DisplayHelper.GetScreenDpi() > 96) { /* 96 = 100% scaling */
          // use 24x24
          iconSize = 24;
          yOffset += 32; // skip 32x32 row
        } else {
          // use 16x16
          iconSize = 16;
          yOffset += 32 + 24; // skip 32x32 and 24x24 rows
        }

        var iconBounds = new Rectangle(0, 0, iconSize, iconSize);
        this.indicators = new Icon[iconCount];

        using (var currentIcon = new Bitmap(iconSize, iconSize))  // current icon bitmap
        using (var iconGraphics = Graphics.FromImage(currentIcon)) {  // create graphics for current icon
          for (int i = 0; i < iconCount; i++) {
            iconGraphics.Clear(Color.Transparent);

            // copy frame from the indicator strip to the current icon bitmap
            iconGraphics.DrawImage(indicatorStrip,
                                   iconBounds,
                                   new Rectangle(i * iconSize, yOffset, iconSize, iconSize),
                                   GraphicsUnit.Pixel);

            // create and store the icon from the current bitmap
            this.indicators[i] = Icon.FromHandle(currentIcon.GetHicon());
            Log.WriteLine(LogLevel.Debug, $"created indicator (index {i}, {iconSize}x{iconSize})");
          }
        }
      }
    }

    /// <summary>
    ///   Renders a single tray icon frame
    /// </summary>
    /// <param name="status">Icon status</param>
    /// <returns>An <see cref="Icon"/> instance</returns>
    public Icon RenderFrame(IndicatorStatus status) {
      switch (status) {
        case IndicatorStatus.Idle: // frame 0 - return application icon
        case IndicatorStatus.Recording:
        case IndicatorStatus.Success:
          return this.indicators[0];

        case IndicatorStatus.Warning: // frame 1 - return application icon with a warning badge
          return this.indicators[1];

        case IndicatorStatus.Progress: // frames [2..21] (18 frames) - increment the animation frame and return the icon
          return this.indicators[2 + (this.counter = (byte)(++this.counter % 18))];

        default: // unrecognized icon status
          throw new ArgumentOutOfRangeException(nameof(status), status, null);
      }
    }
  }
}
