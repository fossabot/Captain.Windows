using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc cref="Behaviour" />
  /// <summary>
  ///   Provides keyboard hooking logic for system UI
  /// </summary>
  internal sealed class DesktopKeyboardHook : Behaviour, IKeyboardHookProvider {
    // ReSharper disable once InconsistentNaming
    /// <summary>
    ///   The wParam and lParam parameters contain information about a keyboard message.
    /// </summary>
    private const int HC_ACTION = 0;

    /// <summary>
    ///   Handle for the system keyboard hook
    /// </summary>
    private IntPtr hookHandle;

    /// <summary>
    ///   Current keyboard state
    /// </summary>
    private Keys keys;

    /// <summary>
    ///   Low-level keyboard hook procedure reference so it it does not get garbage collected
    /// </summary>
    private User32.WindowsHookDelegate lowLevelKeyboardHook;

    /// <summary>
    ///   Modifier keys.
    /// </summary>
    private Keys modifiers;

    /// <inheritdoc />
    /// <summary>
    ///   Triggered when a key is held
    /// </summary>
    public event KeyEventHandler OnKeyDown;

    /// <inheritdoc />
    /// <summary>
    ///   Triggered when a key is released
    /// </summary>
    public event KeyEventHandler OnKeyUp;

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose() {
      Unlock();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Starts capturing keyboard events
    /// </summary>
    protected override void Lock() {
      if (this.hookHandle != IntPtr.Zero) {
        throw new InvalidOperationException("The previous hook must be released before capturing the keyboard again.");
      }

      GC.KeepAlive(this.lowLevelKeyboardHook = LowLevelKeyboardProc);
      if ((this.hookHandle =
            User32.SetWindowsHookEx(User32.WindowsHookType.WH_KEYBOARD_LL, this.lowLevelKeyboardHook)) ==
          IntPtr.Zero) {
        Log.WriteLine(LogLevel.Error, $"SetWindowHookEx() failed (LE 0x{Marshal.GetLastWin32Error():x8})");
        throw new Win32Exception(Marshal.GetLastWin32Error());
      }

      Log.WriteLine(LogLevel.Debug, "low-level keyboard hook has been installed");
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the keyboard hook
    /// </summary>
    protected override void Unlock() {
      if (this.hookHandle != IntPtr.Zero) {
        if (!User32.UnhookWindowsHookEx(this.hookHandle)) {
          Log.WriteLine(LogLevel.Warning, $"UnhookWindowsHookEx() failed (LE 0x{Marshal.GetLastWin32Error():x8}");
        }

        this.hookHandle = IntPtr.Zero; // XXX: the hook may still be present (?), in which case this is cursed
        Log.WriteLine(LogLevel.Debug, "low-level mouse hook has been uninstalled");
      }
    }

    /// <summary>
    ///   Keyboard hook procedure
    /// </summary>
    /// <param name="code">Always 0 (HC_ACTION)</param>
    /// <param name="wParam">The identifier of the keyboard message.</param>
    /// <param name="lParam">A pointer to an KBDLLHOOKSTRUCT structure.</param>
    /// <returns>Depending on <paramref name="code" /> and other conditions, it may yield different values.</returns>
    private int LowLevelKeyboardProc(int code, IntPtr wParam, IntPtr lParam) {
      bool handled = false;

      if (code == HC_ACTION) {
        // this is a keyboard event
        var eventInfo = (KBDLLHOOKSTRUCT) Marshal.PtrToStructure(lParam, typeof(KBDLLHOOKSTRUCT));

        switch (wParam.ToInt32()) {
          // a key is held
          case (int) User32.WindowMessage.WM_SYSKEYDOWN:
          case (int) User32.WindowMessage.WM_KEYDOWN:
            /*if (eventInfo.vkCode == (int) Keys.LShiftKey || eventInfo.vkCode == (int) Keys.RShiftKey) {
              this.modifiers |= Keys.Shift;
            }

            if ((eventInfo.flags & KeyFlags.KF_ALTDOWN) != 0 || eventInfo.vkCode == (int) Keys.LMenu || eventInfo.vkCode == (int) Keys.RMenu) {
              this.modifiers |= Keys.Alt;
            }

            if (eventInfo.vkCode == (int) Keys.LControlKey || eventInfo.vkCode == (int) Keys.RControlKey) {
              this.modifiers |= Keys.Control;
            }

            if (eventInfo.vkCode == (int) Keys.LWin || eventInfo.vkCode == (int) Keys.RWin) {
              this.modifiers |= Keys.LWin;
            }*/

            this.keys = (Keys) eventInfo.vkCode;

            var keyDownEventArgs = new KeyEventArgs(this.keys | this.modifiers);
            OnKeyDown?.Invoke(this, keyDownEventArgs);
            handled = keyDownEventArgs.Handled;

            break;

          // a key is released
          case (int) User32.WindowMessage.WM_SYSKEYUP:
          case (int) User32.WindowMessage.WM_KEYUP:
            var keyUpEventArgs = new KeyEventArgs(this.keys | this.modifiers);
            OnKeyUp?.Invoke(this, keyUpEventArgs);
            handled = keyUpEventArgs.Handled;

            this.keys = Keys.None;

            /*if (eventInfo.vkCode == (int) Keys.LShiftKey || eventInfo.vkCode == (int) Keys.RShiftKey) {
              this.modifiers &= ~Keys.Shift;
            }

            if ((eventInfo.flags & KeyFlags.KF_ALTDOWN) != 0 || eventInfo.vkCode == (int) Keys.LMenu || eventInfo.vkCode == (int) Keys.RMenu) {
              this.modifiers &= ~Keys.Alt;
            }

            if (eventInfo.vkCode == (int) Keys.LControlKey || eventInfo.vkCode == (int) Keys.RControlKey) {
              this.modifiers &= ~Keys.Control;
            }

            if (eventInfo.vkCode == (int) Keys.LWin || eventInfo.vkCode == (int) Keys.RWin) {
              this.modifiers &= ~Keys.LWin;
            }*/

            break;
        }
      }

      // pass unprocessed messages to system
      return handled ? 0 : User32.CallNextHookEx(this.hookHandle, code, wParam, lParam);
    }
  }
}