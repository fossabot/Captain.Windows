using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps the HUD region selection UI in a desktop window
  /// </summary>
  internal sealed class CropRectangleWrapper : Form {
    /// <summary>
    ///   Valid selection border pen.
    /// </summary>
    private readonly Pen validBorderPen = new Pen(Color.FromArgb(0, 204, 136));

    /// <summary>
    ///   Invalid selection border pen.
    /// </summary>
    private readonly Pen invalidBorderPen = new Pen(Color.FromArgb(204, 189, 36));

    /// <summary>
    ///   Current border pen.
    /// </summary>
    private Pen borderPen = new Pen(Color.FromArgb(0x08, 0x08, 0x08));

    /// <summary>
    ///   Whether to display the area selection hint
    /// </summary>
    private bool displayHelpLabel = true;

    /// <summary>
    ///   Whether to display the area selection hint
    /// </summary>
    internal bool DisplayHelpLabel {
      set {
        // ReSharper disable once AssignmentInConditionalExpression
        if (this.displayHelpLabel = value) { Opacity = 0.75; } else { Opacity = 0.5; }
      }
    }

    /// <inheritdoc />
    /// <summary>Gets the required creation parameters when the control handle is created.</summary>
    /// <returns>
    ///   A <see cref="T:System.Windows.Forms.CreateParams" /> that contains the required creation parameters when the
    ///   handle to the control is created.
    /// </returns>
    protected override CreateParams CreateParams {
      get {
        CreateParams createParams = base.CreateParams;

        // WS_EX_TOOLWINDOW hides the wrapper from the Alt-Tab menu
        createParams.ExStyle |= (int) User32.WindowStylesEx.WS_EX_TOOLWINDOW;
        return createParams;
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal CropRectangleWrapper() {
      ShowInTaskbar = false;
      ShowIcon = false;
      ControlBox = false;
      Visible = false;
      TopMost = true;
      StartPosition = FormStartPosition.Manual;
      Size = new Size(1, 1);
      FormBorderStyle = FormBorderStyle.None;
      BackColor = Color.FromArgb(0x10, 0x10, 0x10);
      Opacity = 0.5;
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize" /> event.</summary>
    /// <param name="eventArgs">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnResize(EventArgs eventArgs) {
      Refresh();
      if (!this.displayHelpLabel) {
        this.borderPen = Size.Width >= 42 && Size.Height >= 42 ? this.validBorderPen : this.invalidBorderPen;
      }

      base.OnResize(eventArgs);
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
    /// <param name="eventArgs">
    ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnPaint(PaintEventArgs eventArgs) {
      eventArgs.Graphics.DrawRectangle(borderPen, new Rectangle(0, 0, Width - 1, Height - 1));

      if (this.displayHelpLabel) {
        eventArgs.Graphics.DrawString("Drag your mouse to select a region",
          new Font(SystemFonts.MessageBoxFont.FontFamily, 12.0f),
          new SolidBrush(Color.White),
          new Rectangle(Point.Empty, Size),
          new StringFormat {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
          });
      }

      base.OnPaint(eventArgs);
    }
  }
}