using System;
using System.Drawing;
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
    private readonly Pen fixedBorderPen = new Pen(Color.FromArgb(0x08, 0x08, 0x08));

    /// <summary>
    ///   Current border pen.
    /// </summary>
    private Pen borderPen = new Pen(Color.FromArgb(0x08, 0x08, 0x08));

    /// <summary>
    ///   Whether the screen region has been fixed.
    /// </summary>
    private bool isFixed;

    /// <summary>
    ///   Whether the screen region has been fixed.
    /// </summary>
    internal bool Fixed {
      get => this.isFixed;
      set {
        this.isFixed = value;
        //BackColor = this.isFixed ? Color.Black : Color.FromArgb(0x08, 0x08, 0x08);
        Invalidate();
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
      TransparencyKey = Color.Black;
      BackColor = Color.FromArgb(0x08, 0x08, 0x08);
      Opacity = 0.3333;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Processes window messages for moving the snack bar when the grip is held.
    /// </summary>
    /// <param name="msg">Window message.</param>
    protected override void WndProc(ref Message msg) {
      base.WndProc(ref msg);
      switch (msg.Msg) {
        case (int) User32.WindowMessage.WM_NCHITTEST:
          msg.Result = new IntPtr((int) User32.HitTestValues.HTCAPTION);
          break;

        case (int) User32.WindowMessage.WM_SETCURSOR when Fixed:
          // set move cursor
          Cursor.Current = Cursors.SizeAll;
          break;
      }
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize" /> event.</summary>
    /// <param name="eventArgs">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnResize(EventArgs eventArgs) {
      this.borderPen = Hud.IsValidRegion(Bounds) ? this.validBorderPen : this.invalidBorderPen;
      Invalidate();
      base.OnResize(eventArgs);
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
    /// <param name="eventArgs">
    ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnPaint(PaintEventArgs eventArgs) {
      eventArgs.Graphics.DrawRectangle(Fixed ? this.fixedBorderPen : this.borderPen,
        new Rectangle(0, 0, Width - 1, Height - 1));

      base.OnPaint(eventArgs);
    }
  }
}