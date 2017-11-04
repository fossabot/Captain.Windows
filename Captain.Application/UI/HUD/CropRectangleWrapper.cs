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
    ///   Whether to display the area selection hint
    /// </summary>
    private bool displayHelpLabel = true;

    /// <summary>
    ///   Whether to display the area selection hint
    /// </summary>
    internal bool DisplayHelpLabel {
      set {
        // ReSharper disable once AssignmentInConditionalExpression
        if (this.displayHelpLabel = value) {
          Opacity = 0.5;
        } else {
          Opacity = 0.25;
        }
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
      BackColor = Color.Black;
      Opacity = 0.5;
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint" /> event.</summary>
    /// <param name="eventArgs">
    ///   A <see cref="T:System.Windows.Forms.PaintEventArgs" /> that contains the event data.
    /// </param>
    protected override void OnPaint(PaintEventArgs eventArgs) {
      if (this.displayHelpLabel) {
        eventArgs.Graphics.DrawString("Select an area from the screen",
          new Font(SystemFonts.MessageBoxFont.FontFamily, 12.0f),
          Brushes.White,
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