using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides mouse hooking logic for system UI
  /// </summary>
  internal sealed class SystemMouseHookProvider : IMouseHookProvider {
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///   The wParam and lParam parameters contain information about a mouse message.
    /// </summary>
    private const int HC_ACTION = 0;

    /// <summary>
    ///   Low-level mouse hook procedure reference so it it does not get garbage collected
    /// </summary>
    private User32.WindowsHookDelegate lowLevelMouseProc;

    /// <summary>
    ///   Handle for the system mouse hook
    /// </summary>
    private IntPtr hookHandle;

    /// <summary>
    ///   Current mouse button state
    /// </summary>
    private MouseButtons buttonState;

    /// <inheritdoc />
    /// <summary>
    ///   Triggered when a mouse button is held
    /// </summary>
    public event MouseEventHandler OnMouseDown;

    /// <inheritdoc />
    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    public event MouseEventHandler OnMouseUp;

    /// <inheritdoc />
    /// <summary>
    ///   Triggered when the mouse moves
    /// </summary>
    public event MouseEventHandler OnMouseMove;

    /// <inheritdoc />
    /// <summary>
    ///   Starts capturing mouse events
    /// </summary>
    public void Acquire() {
      if (this.hookHandle != IntPtr.Zero) {
        throw new InvalidOperationException("The previous hook must be released before capturing the mouse again.");
      }

      GC.KeepAlive(this.lowLevelMouseProc = LowLevelMouseProc);
      if ((this.hookHandle = User32.SetWindowsHookEx(User32.WindowsHookType.WH_MOUSE_LL, this.lowLevelMouseProc)) ==
          IntPtr.Zero) {
        Log.WriteLine(LogLevel.Error, $"SetWindowHookEx() failed (LE 0x{Marshal.GetLastWin32Error():x8})");
        throw new Win32Exception(Marshal.GetLastWin32Error());
      }

      Log.WriteLine(LogLevel.Debug, "low-level mouse hook has been installed");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the mouse hook
    /// </summary>
    public void Release() {
      if (this.hookHandle != IntPtr.Zero) {
        if (!User32.UnhookWindowsHookEx(this.hookHandle)) {
          Log.WriteLine(LogLevel.Warning, $"UnhookWindowsHookEx() failed (LE 0x{Marshal.GetLastWin32Error():x8}");
        }

        this.hookHandle = IntPtr.Zero; // XXX: the hook may still be present (?), in which case this is cursed
        Log.WriteLine(LogLevel.Debug, "low-level mouse hook has been uninstalled");
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() => Release();

    /// <summary>
    ///   Mouse hook procedure
    /// </summary>
    /// <param name="code">Always 0 (HC_ACTION)</param>
    /// <param name="wParam">The identifier of the mouse message.</param>
    /// <param name="lParam">A pointer to an MSLLHOOKSTRUCT structure.</param>
    /// <returns>Depending on <paramref name="code" /> and other conditions, it may yield different values.</returns>
    private int LowLevelMouseProc(int code, IntPtr wParam, IntPtr lParam) {
      if (code == HC_ACTION) {
        // this is a mouse event
        var eventInfo = (MSLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

        switch (wParam.ToInt32()) {
          case (int)User32.WindowMessage.WM_MOUSEMOVE:
            OnMouseMove?.Invoke(this, new MouseEventArgs(this.buttonState, 0, eventInfo.pt.x, eventInfo.pt.y, 0));
            break; // we want to let the mouse to be moved

          case (int)User32.WindowMessage.WM_LBUTTONDOWN:
            this.buttonState |= MouseButtons.Left;
            OnMouseDown?.Invoke(this, new MouseEventArgs(this.buttonState, 1, eventInfo.pt.x, eventInfo.pt.y, 0));
            return 1; // message processed

          case (int)User32.WindowMessage.WM_RBUTTONDOWN:
            this.buttonState |= MouseButtons.Right;
            OnMouseDown?.Invoke(this, new MouseEventArgs(this.buttonState, 1, eventInfo.pt.x, eventInfo.pt.y, 0));
            return 1; // message processed

          case (int)User32.WindowMessage.WM_LBUTTONUP:
            this.buttonState &= ~MouseButtons.Left;
            OnMouseUp?.Invoke(this, new MouseEventArgs(this.buttonState, 0, eventInfo.pt.x, eventInfo.pt.y, 0));
            return 1; // message processed

          case (int)User32.WindowMessage.WM_RBUTTONUP:
            this.buttonState &= ~MouseButtons.Right;
            OnMouseUp?.Invoke(this, new MouseEventArgs(this.buttonState, 0, eventInfo.pt.x, eventInfo.pt.y, 0));
            return 1; // message processed
        }
      }

      // pass unprocessed messages to system
      return User32.CallNextHookEx(this.hookHandle, code, wParam, lParam);
    }
  }
}