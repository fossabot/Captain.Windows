using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Common wrapper window for HUD components.
  /// </summary>
  internal class HudCommonWrapperWindow : Form {
    /// <summary>
    ///   Width/height for resize handles
    /// </summary>
    private const int ResizeHandleSize = 24;

    /// <inheritdoc />
    /// <summary>Gets the required creation parameters when the control handle is created.</summary>
    /// <returns>
    ///   A <see cref="T:System.Windows.Forms.CreateParams" /> that contains the required creation parameters when the
    ///   handle to the control is created.
    /// </returns>
    protected override CreateParams CreateParams {
      get {
        CreateParams createParams = base.CreateParams;

        // remove all window styles
        createParams.Style = 0;

        // WS_EX_TOOLWINDOW hides the wrapper from the Alt-Tab menu
        createParams.ExStyle = (int) User32.WindowStylesEx.WS_EX_TOOLWINDOW |
                               (int) User32.WindowStylesEx.WS_EX_COMPOSITED |
                               (int) User32.WindowStylesEx.WS_EX_LAYERED |
                               (int) User32.WindowStylesEx.WS_EX_TRANSPARENT |
                               (int) User32.WindowStylesEx.WS_EX_NOACTIVATE;
        return createParams;
      }
    }

    /// <summary>
    ///   When set to <c>true</c>, hit tests won't be made on this window
    /// </summary>
    internal bool PassThrough {
      get {
        int exStyles = User32.GetWindowLongPtr(Handle, User32.WindowLongParam.GWL_EXSTYLE).ToInt32();
        return (exStyles & (int) User32.WindowStylesEx.WS_EX_TRANSPARENT) != 0;
      }

      set {
        int exStyle = User32.GetWindowLongPtr(Handle, User32.WindowLongParam.GWL_EXSTYLE).ToInt32();

        if (value) {
          // set transparent bit
          exStyle |= (int) User32.WindowStylesEx.WS_EX_TRANSPARENT;
        } else {
          // unset transparent bit
          exStyle &= ~(int) User32.WindowStylesEx.WS_EX_TRANSPARENT;
        }

        User32.SetWindowLongPtr(Handle,
          User32.WindowLongParam.GWL_EXSTYLE,
          exStyle);
      }
    }

    /// <summary>
    ///   Allows the wrapper window to be resized and moved
    /// </summary>
    internal bool Resizable { get; set; } = true;

    /// <summary>
    ///   Client area margins
    /// </summary>
    protected virtual MARGINS Margins { get; }

    /// <summary>
    ///   Accent state for the window
    /// </summary>
    /// <remarks>
    ///   This allows for per-pixel alpha blending on Windows 10 using undocumented APIs.
    /// </remarks>
    protected virtual User32.AccentState AccentState { get; } = User32.AccentState.ACCENT_INVALID_STATE;

    /// <inheritdoc />
    public sealed override Color BackColor {
      get => base.BackColor;
      set => base.BackColor = value;
    }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    public HudCommonWrapperWindow() {
      ShowInTaskbar = ShowIcon = MinimizeBox = MaximizeBox = ControlBox = false;
      AllowTransparency = TopMost = true;
      StartPosition = FormStartPosition.Manual;
      FormBorderStyle = FormBorderStyle.None;
      BackColor = Color.Black;
      TransparencyKey = Color.Magenta;
      Location = new Point(-0x7FFF, -0x7FFF);
    }

    protected override void WndProc(ref Message msg) {
      base.WndProc(ref msg);
      switch (msg.Msg) {
        case (int) User32.WindowMessage.WM_NCHITTEST
        when Resizable:
          Point position = PointToClient(MousePosition);

          if (position.X <= ResizeHandleSize && position.Y <= ResizeHandleSize) {
            // top-left
            msg.Result = new IntPtr((int) User32.HitTestValues.HTTOPLEFT);
          } else if (position.X >= Width - ResizeHandleSize && position.Y >= Height - ResizeHandleSize) {
            // bottom right corner
            msg.Result = new IntPtr((int) User32.HitTestValues.HTBOTTOMRIGHT);
          } else if (position.X <= ResizeHandleSize && position.Y >= Height - ResizeHandleSize) {
            // bottom-left corner
            msg.Result = new IntPtr((int) User32.HitTestValues.HTBOTTOMLEFT);
          } else if (position.X >= Width - ResizeHandleSize && position.Y <= ResizeHandleSize) {
            // top-right corner
            msg.Result = new IntPtr((int) User32.HitTestValues.HTTOPRIGHT);
          } else if (position.Y <= Height - ResizeHandleSize && position.X <= ResizeHandleSize) {
            msg.Result = new IntPtr((int) User32.HitTestValues.HTLEFT);
          } else if (position.Y <= Height - ResizeHandleSize && position.X >= Width - ResizeHandleSize) {
            msg.Result = new IntPtr((int) User32.HitTestValues.HTRIGHT);
          } else if (position.Y <= ResizeHandleSize) {
            // top edge
            msg.Result = new IntPtr((int) User32.HitTestValues.HTTOP);
          } else if (position.Y >= Height - ResizeHandleSize) {
            // bottom edge
            msg.Result = new IntPtr((int) User32.HitTestValues.HTBOTTOM);
          } else {
            // screen region
            msg.Result = new IntPtr((int) User32.HitTestValues.HTCAPTION);
          }

          break;

        case (int) User32.WindowMessage.WM_SETCURSOR
        when Resizable && (msg.LParam.ToInt32() & 0x0000FFFF) == (int) User32.HitTestValues.HTCAPTION:
          Cursor.Current = Cursors.SizeAll;
          break;
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Sets up the window styling upon handle creation.
    /// </summary>
    /// <param name="eventArgs">Arguments passed to this event.</param>
    protected override void OnHandleCreated(EventArgs eventArgs) {
      // make the window actually borderless and set transparency attributes so we can use alpha blending when
      // doing the Direct2D rendering
      MARGINS margins = Margins;
      DwmApi.DwmExtendFrameIntoClientArea(Handle, ref margins);

      var attrValue = new IntPtr((int) DwmApi.DwmNcRenderingPolicy.DWMNCRP_DISABLED);
      DwmApi.DwmSetWindowAttribute(Handle,
        DwmApi.DwmWindowAttribute.DWMWA_NCRENDERING_POLICY,
        ref attrValue,
        Marshal.SizeOf(typeof(int)));

      /* Windows 10 blur */
      if (Environment.OSVersion.Version.Major >= 10) {
        try {
          // HACK(sanlyx): we're using undocumented APIs to display blur here - replace with something better
          //               when you've got nothing better to do
          var accent = new User32.AccentPolicy { AccentState = AccentState };
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

          return;
        } catch {
          /* unsupported feature? */
        }
      }

      // TODO: work out transparency for Windows < 10 and non-DWM scenarios
      base.OnHandleCreated(eventArgs);
    }
  }
}