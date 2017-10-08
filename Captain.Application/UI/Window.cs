using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <summary>
  ///   Contains code for abstracting application window features
  /// </summary>
  internal class Window : Form {
    /// <summary>
    ///   Last DPI value for this form
    /// </summary>
    private float lastDpi = 96;

    /// <summary>
    ///   Class constructor
    /// </summary>
    internal Window() {
      AutoScaleMode = AutoScaleMode.Font;
      StartPosition = FormStartPosition.CenterScreen;
    }

    /// <summary>
    ///   Updates the DPI setting for this form
    /// </summary>
    /// <param name="dpi">New DPI value</param>
    private void UpdateDpi(float dpi) {
      Font = new Font(Font.FontFamily, Font.Size * (dpi / this.lastDpi));
      this.lastDpi = dpi;
    }

    /// <summary>
    ///   Sets the initial DPI for this form
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnLoad(EventArgs eventArgs) {
      if (!DesignMode) {
        UpdateDpi(DisplayHelper.GetScreenDpi(Handle));

        // restore saved window position, if any
        if (Application.Options.WindowPositions.ContainsKey(Name)) {
          // got saved position
          Point position = Application.Options.WindowPositions[Name];

          if (DisplayHelper.GetOutputInfoFromRect(new Rectangle(position, Size)).Length > 0) {
            // the window would be visible - no problem
            Location = position;
          }
        }
      }

      base.OnLoad(eventArgs);
    }

    /// <summary>
    ///   Save window position
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnClosed(EventArgs eventArgs) {
      Application.Options.WindowPositions[Name] = Location;
      Application.Options.Save();
      base.OnClosed(eventArgs);
    }

    /// <summary>
    ///   Window procedure override for handling DPI changes
    /// </summary>
    /// <param name="msg">Window message</param>
    protected override void WndProc(ref Message msg) {
      if (msg.Msg == (int)User32.WindowMessage.WM_DPICHANGED) {
        UpdateDpi(msg.WParam.ToInt64() >> 16 & 0xFFFF);
      }

      base.WndProc(ref msg);
    }
  }
}
