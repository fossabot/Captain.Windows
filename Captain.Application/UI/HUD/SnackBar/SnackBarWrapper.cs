using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Wraps the HUD snack bar in a desktop window
  /// </summary>
  internal sealed class SnackBarWrapper : Form {
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
    internal SnackBarWrapper() {
      MinimumSize = MaximumSize = ClientSize = new Size(208, 32);
      MinimizeBox = false;
      MaximizeBox = false;
      ControlBox = false;
      ShowInTaskbar = false;
      ShowIcon = false;
      TopMost = true;
      StartPosition = FormStartPosition.Manual;
      FormBorderStyle = FormBorderStyle.None;
      BackColor = Color.Black;
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
          Point position = PointToClient(MousePosition);
          if (position.X <= 16 && position.Y <= 32) {
            // this is the snack bar grip
            msg.Result = new IntPtr((int) User32.HitTestValues.HTCAPTION);
          }
          break;

        case (int) User32.WindowMessage.WM_SETCURSOR
        when (msg.LParam.ToInt32() & 0x0000FFFF) == (int) User32.HitTestValues.HTCAPTION:
          // set move cursor when the snack bar grip is hovered
          Cursor.Current = Cursors.SizeAll;
          break;
      }
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.HandleCreated" /> event.</summary>
    /// <param name="eventArgs">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
    protected override void OnHandleCreated(EventArgs eventArgs) {
      // make the window actually borderless and set transparency attributes so we can use alpha blending when
      // doing the Direct2D rendering
      var margins = new MARGINS {bottomWidth = 1, leftWidth = 1, rightWidth = 1, topWidth = 2};
      DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);

      var attrValue = new IntPtr((int) DwmApi.DwmNcRenderingPolicy.DWMNCRP_DISABLED);
      DwmApi.DwmSetWindowAttribute(Handle,
        DwmApi.DwmWindowAttribute.DWMWA_NCRENDERING_POLICY,
        ref attrValue,
        Marshal.SizeOf(typeof(int)));

      User32.SetLayeredWindowAttributes(Handle,
        0,
        0xFF,
        User32.LayeredWindowActions.LWA_ALPHA);

      /* Windows 10 blur */
      if (Environment.OSVersion.Version.Major >= 10) {
        try {
          // HACK(sanlyx): we're using undocumented APIs to display blur here - replace with something better
          //               when you've got nothing better to do
          var accent = new User32.AccentPolicy {AccentState = User32.AccentState.ACCENT_ENABLE_BLURBEHIND};
          int accentStructSize = Marshal.SizeOf(accent);

          // allocate space for the struct
          IntPtr accentPtr = Marshal.AllocHGlobal(accentStructSize);
          Marshal.StructureToPtr(accent, accentPtr, false);

          // set composition data
          var data = new User32.WindowCompositionAttributeData {
            Attribute = User32.WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr
          };

          // change window composition attributes and release resources
          User32.SetWindowCompositionAttribute(Handle, ref data);
          Marshal.FreeHGlobal(accentPtr);
        } catch {
          /* unsupported feature? */
        }
      }

      base.OnHandleCreated(eventArgs);
    }
  }
}