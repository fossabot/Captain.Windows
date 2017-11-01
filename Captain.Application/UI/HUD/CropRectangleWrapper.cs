using System.Drawing;
using System.Windows.Forms;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps the HUD region selection UI in a desktop window
  /// </summary>
  internal sealed class CropRectangleWrapper : Form {
    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    internal CropRectangleWrapper() {
      ShowInTaskbar = false;
      ShowIcon = false;
      ControlBox = false;
      Opacity = .25;
      TopMost = true;
      StartPosition = FormStartPosition.Manual;
      Size = new Size(0, 0);
      FormBorderStyle = FormBorderStyle.None;
      BackColor = Color.Black;
      Cursor = Cursors.No;  // replace by actual area selection cursor
    }
  }
}
