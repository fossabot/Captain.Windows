using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <summary>
  ///   Displays information about the application
  /// </summary>
  public partial class AboutWindow : Form {
    private float lastDpi = 96.0f;

    /// <summary>
    ///   Class constructor
    /// </summary>
    public AboutWindow() {
      InitializeComponent();

      Icon = Resources.AppIcon;
      this.logoPictureBox.Image = Resources.Logo;
    }

    /// <summary>
    ///   Updates the DPI setting for the current form
    /// </summary>
    /// <param name="dpi"></param>
    private void UpdateDpi(float dpi) {
      Font = new Font(Font.FontFamily, Font.Size * (dpi / this.lastDpi));
      this.lastDpi = dpi;
    }

    /// <summary>
    ///   Triggered when the form is loaded
    /// </summary>
    /// <param name="eventArgs">Arguments passed to this event</param>
    protected override void OnLoad(EventArgs eventArgs) {
      base.OnLoad(eventArgs);
      UpdateDpi(DisplayHelper.GetScreenDpi(Handle));
    }

    /// <summary>
    ///   Window procedure
    /// </summary>
    /// <param name="msg">Message</param>
    protected override void WndProc(ref Message msg) {
      base.WndProc(ref msg);

      if (msg.Msg == (int)User32.WindowMessage.WM_DPICHANGED) {
        float dpi = msg.WParam.ToInt64() >> 16 & 0xFFFF;
        UpdateDpi(dpi);
      }
    }

    /// <summary>
    ///   Paints information labels
    /// </summary>
    /// <param name="sender">Control to be painted</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnLabelPaint(object sender, PaintEventArgs eventArgs) {
      var label = (Label)sender;

      eventArgs.Graphics.Clear(label.BackColor);

      // label parameters
      var labelRect = new Rectangle(0, 0, label.Width / 2, label.Height);
      const TextFormatFlags labelFlags = TextFormatFlags.EndEllipsis;

      // value parameters
      var valueRect = new Rectangle(label.Width / 2, 0, label.Width / 2, label.Height);
      const TextFormatFlags valueFlags = TextFormatFlags.EndEllipsis |
                                         TextFormatFlags.Right |
                                         TextFormatFlags.LeftAndRightPadding;

      // size of the label rectangle
      Size labelSize = TextRenderer.MeasureText(eventArgs.Graphics,
                                                (string)label.Tag,
                                                label.Font,
                                                labelRect.Size,
                                                labelFlags);

      // size of the value rectangle
      Size valueSize = TextRenderer.MeasureText(eventArgs.Graphics, label.Text, label.Font, valueRect.Size, valueFlags);

      // render label and value
      TextRenderer.DrawText(eventArgs.Graphics,
                            (string)label.Tag,
                            label.Font,
                            labelRect,
                            Color.FromArgb(0x666666),
                            labelFlags);
      TextRenderer.DrawText(eventArgs.Graphics,
                            label.Text,
                            label.Font,
                            valueRect,
                            Color.FromArgb(0x333333),
                            valueFlags);

      // draw separator
      eventArgs.Graphics.DrawLine(new Pen(Color.FromArgb(0x30, label.ForeColor)),
                                  labelSize.Width,
                                  1 + label.Height / 2,
                                  label.Width - valueSize.Width,
                                  1 + label.Height / 2);
    }

    /// <summary>
    ///   Triggered when the Close button gets clicked
    /// </summary>
    /// <param name="sender">Button object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnCloseButtonClick(object sender, EventArgs eventArgs) => Close();
  }
}
