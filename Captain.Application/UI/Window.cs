using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Contains code for abstracting application window features
  /// </summary>
  internal class Window : Form {
    /// <summary>
    ///   Last DPI value for this form
    /// </summary>
    private float lastDpi = 96;

    /// <inheritdoc />
    /// <summary>Gets the required creation parameters when the control handle is created.</summary>
    /// <returns>A <see cref="T:System.Windows.Forms.CreateParams" /> that contains the required creation parameters when the handle to the control is created.</returns>
    protected override CreateParams CreateParams {
      get {
        CreateParams createParams = base.CreateParams;
        if (!DesignMode) { createParams.ExStyle |= (int)User32.WindowStylesEx.WS_EX_COMPOSITED; }
        return createParams;
      }
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    /// <summary>
    ///   Save window position
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnClosed(EventArgs eventArgs) {
      Application.Options.WindowPositions[Name] = Location;
      Application.Options.Save();
      base.OnClosed(eventArgs);
    }

    /// <inheritdoc />
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
