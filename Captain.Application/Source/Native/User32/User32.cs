using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Exported functions from the user32.dll Windows library.
  /// </summary>
  internal static partial class User32 {
    #region System resources

    internal enum SystemResources {
      /// <summary>
      ///   Hand cursor
      /// </summary>
      IDC_HAND = 32649
    }

    #endregion

    #region Windows

    #region Window styles

    [Flags]
    internal enum WindowStylesEx : uint {
      /// <summary>
      ///   The window should not be painted until siblings beneath the window (that were created by the same thread)
      ///   have been painted.
      /// </summary>
      WS_EX_TRANSPARENT = 0x00000020,

      /// <summary>
      ///   The window is intended to be used as a floating toolbar.
      /// </summary>
      WS_EX_TOOLWINDOW = 0x00000080,

      /// <summary>
      ///   Paints via double-buffering, which reduces flicker. This extended style also enables alpha-blended marquee
      ///   selection on systems where it is supported.
      /// </summary>
      LVS_EX_DOUBLEBUFFER = 0x00010000,

      /// <summary>
      ///   This window is a layered window.
      /// </summary>
      WS_EX_LAYERED = 0x00080000,

      /// <summary>
      ///   Paints all descendants of a window in bottom-to-top painting order using double-buffering.
      /// </summary>
      WS_EX_COMPOSITED = 0x02000000,

      /// <summary>
      ///   A  top-level window created with this style does not become the foreground window when the user clicks it.
      /// </summary>
      WS_EX_NOACTIVATE = 0x08000000
    }

    #endregion

    #region Window procedures/messages

    /// <summary>
    ///   Represents UI state flags
    /// </summary>
    internal enum UIStateFlags {
      /// <summary>
      ///   The UI state flags specified by the high-order word should be set.
      /// </summary>
      UIS_SET = 1,

      /// <summary>
      ///   Focus indicators are hidden.
      /// </summary>
      UISF_HIDEFOCUS = 1
    }

    /// <summary>
    ///   Windows Messages
    ///   Defined in winuser.h from Windows SDK v6.1
    ///   Documentation pulled from MSDN.
    /// </summary>
    internal enum WindowMessage {
      /// <summary>
      ///   Sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured.
      /// </summary>
      WM_SETCURSOR = 0x0020,

      /// <summary>
      ///   Sent by a common control to its parent window when an event has occurred or the control requires some
      ///   information.
      /// </summary>
      WM_NOTIFY = 0x004E,

      /// <summary>
      ///   Sent to a window in order to determine what part of the window corresponds to a particular screen coordinate.
      /// </summary>
      WM_NCHITTEST = 0x0084,

      /// <summary>
      ///   Posted to the window with the keyboard focus when a nonsystem key is pressed.
      ///   A nonsystem key is a key that is pressed when the ALT key is not pressed.
      /// </summary>
      WM_KEYDOWN = 0x0100,

      /// <summary>
      ///   Posted to the window with the keyboard focus when a nonsystem key is released.
      ///   A nonsystem key is a key that is pressed when the ALT key is not pressed, or a keyboard key that is pressed
      ///   when a window has the keyboard focus.
      /// </summary>
      WM_KEYUP = 0x0101,

      /// <summary>
      ///   Posted to the window with the keyboard focus when the user presses the F10 key (which activates the menu
      ///   bar) or holds down the ALT key and then presses another key. It also occurs when no window currently has
      ///   the keyboard focus.
      /// </summary>
      WM_SYSKEYDOWN = 0x104,

      /// <summary>
      ///   Posted to the window with the keyboard focus when the user releases a key that was pressed while the ALT
      ///   key was held down. 
      /// </summary>
      WM_SYSKEYUP = 0x105,

      /// <summary>
      ///   An application sends the WM_CHANGEUISTATE message to indicate that the UI state should be changed.
      /// </summary>
      WM_CHANGEUISTATE = 0x0127,

      #region Mouse events

      /// <summary>
      ///   Posted to a window when the cursor moves.
      /// </summary>
      WM_MOUSEMOVE = 0x0200,

      /// <summary>
      ///   Posted when the user presses the left mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_LBUTTONDOWN = 0x0201,

      /// <summary>
      ///   Posted when the user releases the left mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_LBUTTONUP = 0x0202,

      /// <summary>
      ///   Posted when the user presses the right mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_RBUTTONDOWN = 0x0204,

      /// <summary>
      ///   Posted when the user releases the right mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_RBUTTONUP = 0x0205,

      #endregion

      /// <summary>
      ///   Sent when the effective dots per inch (dpi) for a window has changed.
      /// </summary>
      WM_DPICHANGED = 0x02E0,

      /// <summary>
      ///   Informs all top-level windows that Desktop Window Manager (DWM) composition has been enabled or disabled.
      /// </summary>
      WM_DWMCOMPOSITIONCHANGED = 0x031E,

      /// <summary>
      ///   Informs all top-level windows that the colorization color has changed.
      /// </summary>
      WM_DWMCOLORIZATIONCHANGED = 0x0320,

      /// <summary>
      ///   Sets extended styles in list-view controls.
      /// </summary>
      LVM_SETEXTENDEDLISTVIEWSTYLE = 0x1036,

      /// <summary>
      ///   Sets the HCURSOR value that the list-view control uses when the pointer is over an item while hot tracking
      ///   is enabled.
      /// </summary>
      LVM_SETHOTCURSOR = 0x103E,

      /// <summary>
      ///   Calculates a tab control's display area given a window rectangle, or calculates the window rectangle that
      ///   would correspond to a specified display area.
      /// </summary>
      TCM_ADJUSTRECT = 0x1328
    }

    /// <summary>
    ///   Return values for WM_NCHITTEST window messages.
    /// </summary>
    internal enum HitTestValues {
      /// <summary>
      ///   In a title bar
      /// </summary>
      HTCAPTION = 2,

      /// <summary>
      ///   In the left border of a resizable window
      /// </summary>
      HTLEFT = 10,

      /// <summary>
      ///   In the right border of a resizable window
      /// </summary>
      HTRIGHT = 11,

      /// <summary>
      ///   In the top border of a resizable window
      /// </summary>
      HTTOP = 12,

      /// <summary>
      ///   In the upper-left corner of a border of a resizable window 
      /// </summary>
      HTTOPLEFT = 13,

      /// <summary>
      ///   In the upper-resizable corner of a border of a resizable window 
      /// </summary>
      HTTOPRIGHT = 14,

      /// <summary>
      ///   In the bottom border of a resizable window
      /// </summary>
      HTBOTTOM = 15,

      /// <summary>
      ///   In the lower-left corner of a border of a resizable window 
      /// </summary>
      HTBOTTOMLEFT = 16,

      /// <summary>
      ///   In the lower-right corner of a border of a resizable window 
      /// </summary>
      HTBOTTOMRIGHT = 17
    }

    /// <summary>
    ///   Sends the specified message to a window or windows. The SendMessage function calls the window procedure for
    ///   the specified window and does not return until the window procedure has processed the message.
    ///   To send a message and return immediately, use the SendMessageCallback or SendNotifyMessage function. To post
    ///   a message to a thread's message queue and return immediately, use the PostMessage or PostThreadMessage
    ///   function.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window whose window procedure will receive the message. If this parameter is HWND_BROADCAST
    ///   ((HWND)0xffff), the message is sent to all top-level windows in the system, including disabled or invisible
    ///   unowned windows, overlapped windows, and pop-up windows; but the message is not sent to child windows.
    ///   Message sending is subject to UIPI. The thread of a process can send messages only to message queues of
    ///   threads in processes of lesser or equal integrity level.
    /// </param>
    /// <param name="uiMsg">The message to be sent.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>
    ///   The return value specifies the result of the message processing; it depends on the message sent.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern IntPtr SendMessage(
      [In] IntPtr hWnd,
      [In] uint uiMsg,
      [In] IntPtr wParam,
      [In] IntPtr lParam);

    #region CallWindowProc

    #endregion

    #endregion

    #region GetWindowLong(Ptr)/SetWindowLong(Ptr)

    internal enum WindowLongParam {
      /// <summary>Sets a new window style.</summary>
      GWL_STYLE = -16,

      /// <summary>Sets a new extended window style.</summary>
      GWL_EXSTYLE = -20
    }

    /// <summary>
    ///   Retrieves information about the specified window. The function also retrieves the value at a specified offset
    ///   into the extra window memory.
    /// </summary>
    /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
    /// <param name="nIndex">
    ///   The zero-based offset to the value to be retrieved. Valid values are in the range zero
    ///   through the number of bytes of extra window memory, minus the size of a LONG_PTR. To retrieve any other value,
    ///   specify one of the following values.</param>
    /// <returns>If the function succeeds, the return value is the requested value.</returns>
#if WIN32
    [DllImport(nameof(User32), EntryPoint = "GetWindowLong")]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongParam nIndex);
#elif WIN64
    [DllImport(nameof(User32), EntryPoint = "GetWindowLongPtr")]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, WindowLongParam nIndex);
#endif

    /// <summary>
    ///   Changes an attribute of the specified window. The function also sets a value at the specified offset in the
    ///   extra window memory.
    /// </summary>
    /// <param name="hWnd">
    /// A handle to the window and, indirectly, the class to which the window belongs.
    ///   The SetWindowLongPtr function fails if the process that owns the window specified by the hWnd parameter is
    ///   at a higher process privilege in the UIPI hierarchy than the process the calling thread resides in.
    /// </param>
    /// <param name="nIndex">
    ///   The zero-based offset to the value to be set. Valid values are in the range zero through
    ///   the number of bytes of extra window memory, minus the size of a LONG_PTR.
    /// </param>
    /// <param name="dwNewLong">The replacement value.</param>
    /// <returns>If the function succeeds, the return value is the previous value of the specified offset.</returns>
#if WIN32
    [DllImport(nameof(User32), EntryPoint = "SetWindowLong")]
    internal static extern int SetWindowLongPtr(IntPtr hWnd, WindowLongParam nIndex, int dwNewLong);
#elif WIN64
    [DllImport(nameof(User32))]
    internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, WindowLongParam nIndex, IntPtr dwNewLong);
#endif

    #endregion

    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    /// <summary>
    ///   Retrieves a handle to the window that contains the specified point.
    /// </summary>
    /// <param name="Point">The point to be checked.</param>
    /// <returns>
    ///   The return value is a handle to the window that contains the point. If no window exists at the given point,
    ///   the return value is <see cref="IntPtr.Zero" />. If the point is over a static text control, the return value
    ///   is a handle to the window under the static text control.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr WindowFromPoint(POINT Point);

    /// <summary>
    ///   Retrieves a handle to the desktop window. The desktop window covers the entire screen. The desktop window is
    ///   the area on top of which other windows are painted.
    /// </summary>
    /// <returns>The return value is a handle to the desktop window.</returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr GetDesktopWindow();

    /// <summary>
    ///   Destroys the specified window. The function sends WM_DESTROY and WM_NCDESTROY messages to the window to
    ///   deactivate it and remove the keyboard focus from it. The function also destroys the window's menu, flushe
    ///   the thread message queue, destroys timers, removes clipboard ownership, and breaks the clipboard viewer chain
    ///   (if the window is at the top of the viewer chain).
    /// </summary>
    /// <param name="hWnd">A handle to the window to be destroyed.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(nameof(User32))]
    internal static extern bool DestroyWindow([In] IntPtr hWnd);

    #region Layered windows

    /// <summary>
    ///   Sets the opacity and transparency color key of a layered window.
    /// </summary>
    /// <param name="hwnd">A handle to the layered window.</param>
    /// <param name="crKey">
    ///   A COLORREF structure that specifies the transparency color key to be used when composing the layered window.
    /// </param>
    /// <param name="bAlpha">Alpha value used to describe the opacity of the layered window.</param>
    /// <param name="dwFlags">An action to be taken.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(nameof(User32))]
    internal static extern bool SetLayeredWindowAttributes(
      [In] IntPtr hwnd,
      [In] int crKey,
      [In] byte bAlpha,
      [In] LayeredWindowActions dwFlags);

    /// <summary>
    ///   Layered window action for using as <c>dwFlags</c> in <see cref="SetLayeredWindowAttributes" />
    /// </summary>
    [Flags]
    internal enum LayeredWindowActions {
      /// <summary>
      ///   Use bAlpha to determine the opacity of the layered window.
      /// </summary>
      LWA_ALPHA = 2
    }

    #endregion

    #endregion

    #region DPI

    /// <summary>
    ///   Returns the dots per inch (dpi) value for the associated window.
    /// </summary>
    /// <param name="hWnd">The window you want to get information about.</param>
    /// <returns>
    ///   The DPI for the window which depends on the DPI_AWARENESS of the window.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern int GetDpiForWindow([In] IntPtr hWnd);

    /// <summary>
    ///   Returns the dots per inch (dpi) value for the system.
    /// </summary>
    /// <returns>The system DPI value.</returns>
    [DllImport(nameof(User32))]
    internal static extern int GetDpiForSystem();

    /// <summary>
    ///   Values for the <c>nIndex</c> parameter in <see cref="GetSystemMetricsForDpi" />
    /// </summary>
    internal enum SystemMetrics {
      /// <summary>
      ///   The recommended height of a small icon, in pixels.
      /// </summary>
      SM_CYSMICON = 50
    }

    /// <summary>
    ///   Retrieves the specified system metric or system configuration setting taking into account a provided DPI.
    /// </summary>
    /// <param name="nIndex">The system metric or configuration setting to be retrieved.</param>
    /// <param name="dpi">The DPI to use for scaling the metric.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(nameof(User32))]
    internal static extern int GetSystemMetricsForDpi([In] int nIndex, [In] uint dpi);

    #endregion

    #region Cursors

    /// <summary>
    ///   Loads the specified cursor resource from the executable (.EXE) file associated with an application instance.
    /// </summary>
    /// <param name="hInstance">
    ///   A handle to an instance of the module whose executable file contains the cursor to be loaded.
    /// </param>
    /// <param name="lpCursorName">
    ///   The name of the cursor resource to be loaded. Alternatively, this parameter can consist of the resource
    ///   identifier in the low-order word and zero in the high-order word.
    /// </param>
    /// <returns>If the function succeeds, the return value is the handle to the newly loaded cursor.</returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr LoadCursor([In] [Optional] IntPtr hInstance, [In] IntPtr lpCursorName);

    /// <summary>
    ///   Sets the cursor shape.
    /// </summary>
    /// <param name="hCursor">
    ///   A handle to the cursor. The cursor must have been created by the CreateCursor function or loaded by the
    ///   <see cref="LoadCursor(IntPtr,IntPtr)" /> or LoadImage function.
    /// </param>
    /// <returns>The return value is the handle to the previous cursor, if there was one.</returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr SetCursor([In] IntPtr hCursor);

    #endregion

    #region ListView Empty Markup

    private const uint LVN_FIRST = unchecked(0u - 100u);
    private const uint L_MAX_URL_LENGTH = 2084;
    internal const uint LVN_GETEMPTYMARKUP = LVN_FIRST - 87;

    /// <summary>
    ///   Render markup centered in the listview area.
    /// </summary>
    internal const uint EMF_CENTERED = 1;

    /// <summary>
    ///   Contains information about a notification message.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct NMHDR {
      /// <summary>
      ///   A window handle to the control sending the message.
      /// </summary>
      internal IntPtr hwndFrom;

      /// <summary>
      ///   An identifier of the control sending the message.
      /// </summary>
      private readonly IntPtr idFrom;

      /// <summary>
      ///   A notification code.
      /// </summary>
      internal readonly int code;
    }

    /// <summary>
    ///   Contains information used with the LVN_GETEMPTYMARKUP notification code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NMLVEMPTYMARKUP {
      /// <summary>
      ///   Info on the notification message.
      /// </summary>
      private readonly NMHDR hdr;

      /// <summary>
      ///   If NULL, markup is rendered left-justified in the listview area.
      /// </summary>
      internal int dwFlags;

      /// <summary>
      ///   Markup to display.
      /// </summary>
      [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) L_MAX_URL_LENGTH)] public string szMarkup;
    }

    #endregion

    #region Hooks

    /// <summary>
    ///   The type of hook procedure to be installed by
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,Int32)" />.
    /// </summary>
    internal enum WindowsHookType {
      /// <summary>Installs a hook procedure that monitors low-level keyboard input events.</summary>
      WH_KEYBOARD_LL = 13,

      /// <summary>Installs a hook procedure that monitors low-level mouse input events.</summary>
      WH_MOUSE_LL = 14
    }

    /// <summary>
    ///   An application-defined or library-defined callback function used with the
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,Int32)" /> function.
    ///   This is a generic function to Hook callbacks. For specific callback functions see this
    ///   <see href="https://msdn.microsoft.com/en-us/library/windows/desktop/ms632589(v=vs.85).aspx">
    ///     API documentation
    ///     on MSDN
    ///   </see>
    ///   .
    /// </summary>
    /// <param name="nCode">
    ///   An action code for the callback. Can be used to indicate if the hook procedure must process the message or
    ///   not.
    /// </param>
    /// <param name="wParam">First message parameter</param>
    /// <param name="lParam">Second message parameter</param>
    /// <returns>
    ///   An LRESULT. Usually if nCode is less than zero, the hook procedure must return the value returned by
    ///   CallNextHookEx.
    ///   If nCode is greater than or equal to zero, it is highly recommended that you call CallNextHookEx and return
    ///   the value it returns;
    ///   otherwise, other applications that have installed hooks will not receive hook notifications and may behave
    ///   incorrectly as a result.
    ///   If the hook procedure does not call CallNextHookEx, the return value should be zero.
    /// </returns>
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    internal delegate int WindowsHookDelegate([In] int nCode, [In] IntPtr wParam, [In] IntPtr lParam);

    /// <summary>
    ///   Installs an application-defined hook procedure into a hook chain. You would install a hook procedure to
    ///   monitor the system for certain types of events. These events are associated either with a specific thread or
    ///   with all threads in the same desktop as the calling thread.
    /// </summary>
    /// <param name="idHook">The type of hook procedure to be installed.</param>
    /// <param name="lpfn">
    ///   A pointer to the hook procedure. If the <paramref name="dwThreadId" /> parameter is zero or
    ///   specifies the identifier of a thread created by a different process, the <paramref name="lpfn" /> parameter
    ///   must point to a hook procedure in a DLL. Otherwise, <paramref name="lpfn" /> can point to a hook procedure in
    ///   the code associated with the current process.
    /// </param>
    /// <param name="hMod">
    ///   A handle to the DLL containing the hook procedure pointed to by the <paramref name="lpfn" />
    ///   parameter. The <paramref name="hMod" /> parameter must be set to <see cref="IntPtr.Zero" /> if the
    ///   <paramref name="dwThreadId" /> parameter specifies a thread created by the current process and if the hook
    ///   procedure is within the code associated with the current process.
    /// </param>
    /// <param name="dwThreadId">
    ///   The identifier of the thread with which the hook procedure is to be associated. For desktop
    ///   apps, if this parameter is zero, the hook procedure is associated with all existing threads running in the
    ///   same desktop as the calling thread. For Windows Store apps, see the Remarks section.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is the handle to the hook procedure.
    ///   <para>
    ///     If the function fails, the return value is an invalid handle. To get extended error information, call
    ///     GetLastError.
    ///   </para>
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern IntPtr SetWindowsHookEx(
      [In] WindowsHookType idHook,
      [In] WindowsHookDelegate lpfn,
      [In] [Optional] IntPtr hMod,
      [In] [Optional] int dwThreadId);

    /// <summary>
    ///   Removes a hook procedure installed in a hook chain by the
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,Int32)" /> function.
    /// </summary>
    /// <param name="hhk">
    ///   A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,Int32)" />.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is true.
    ///   <para>
    ///     If the function fails, the return value is false. To get extended error information, call
    ///     GetLastError.
    ///   </para>
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool UnhookWindowsHookEx([In] IntPtr hhk);

    /// <summary>
    ///   Passes the hook information to the next hook procedure in the current hook chain. A hook procedure can call
    ///   this function either before or after processing the hook information.
    /// </summary>
    /// <param name="hhk">This parameter is ignored.</param>
    /// <param name="nCode">
    ///   The hook code passed to the current hook procedure. The next hook procedure uses this code to determine how
    ///   to process the hook information.
    /// </param>
    /// <param name="wParam">
    ///   The wParam value passed to the current hook procedure. The meaning of this parameter depends on the type of
    ///   hook associated with the current hook chain.
    /// </param>
    /// <param name="lParam">
    ///   The lParam value passed to the current hook procedure. The meaning of this parameter depends on the type of
    ///   hook associated with the current hook chain.
    /// </param>
    /// <returns>
    ///   This value is returned by the next hook procedure in the chain. The current hook procedure must also return
    ///   this value. The meaning of the return value depends on the hook type. For more information, see the
    ///   descriptions of the individual hook procedures.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern int CallNextHookEx(
      [In] IntPtr hhk,
      [In] int nCode,
      [In] IntPtr wParam,
      [In] IntPtr lParam);

    #region Undocumented Windows 10 composition features

    [StructLayout(LayoutKind.Sequential)]
    internal struct WindowCompositionAttributeData {
      internal WindowCompositionAttribute Attribute;
      internal IntPtr Data;
      internal int SizeOfData;
    }

    internal enum WindowCompositionAttribute {
      WCA_ACCENT_POLICY = 19
    }

    internal enum AccentState {
      ACCENT_ENABLE_BLURBEHIND = 3,
      ACCENT_INVALID_STATE = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy {
      internal AccentState AccentState;
      internal int AccentFlags;
      internal int GradientColor;
      internal int AnimationId;
    }

    [DllImport(nameof(User32))]
    internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    #endregion
  }

  #endregion
}