using System.Drawing;
using System.Windows.Forms;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps the HUD snack bar in a composited window
  /// </summary>
  internal sealed class SnackBarWrapper : Form {
    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal SnackBarWrapper() {
      MinimumSize = MaximumSize = ClientSize = new Size(192, 32);
      ShowInTaskbar = false;
      ShowIcon = false;
      ControlBox = false;
      Opacity = .96;
      TopMost = true;
      StartPosition = FormStartPosition.CenterScreen;
      FormBorderStyle = FormBorderStyle.None;

      // some kind of hack?
      // There's some delay between the form display and the Direct2D rendering - this makes the window invisible
      // until something's actually been rendered
      TransparencyKey = Color.Black;
      AllowTransparency = true;
      BackColor = Color.Black;
    }
  }
}
