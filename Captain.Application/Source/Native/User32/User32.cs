// ReSharper disable All

using System;
using System.Runtime.InteropServices;

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
      IDC_HAND = 32649,

      /// <summary>
      ///   Default application icon
      /// </summary>
      IDI_APPLICATION = 32512,
    }

    #endregion

    #region Hit test values

    internal enum HitTestValues {
      /// <summary>
      ///   In a window currently covered by another window in the same thread (the message will be sent to underlying
      ///   windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
      /// </summary>
      HTTRANSPARENT = -1
    }

    #endregion

    #region Windows

    #region Window styles

    /// <summary>
    ///   Window Styles. The following styles can be specified wherever a window style is required.
    ///   After the control has been created, these styles cannot be modified, except as noted.
    /// </summary>
    [Flags]
    internal enum WindowStyles {
      /// <summary>
      ///   The window is a control that can receive the keyboard focus when the user presses the TAB
      ///   key. Pressing the TAB key changes the keyboard focus to the next control with the
      ///   WS_TABSTOP style. You can turn this style on and off to change dialog box navigation. To
      ///   change this style after a window has been created, use the SetWindowLong function. For
      ///   user-created windows and modeless dialogs to work with tab stops, alter the message loop
      ///   to call the IsDialogMessage function.
      /// </summary>
      WS_TABSTOP = 0x10000,

      /// <summary>
      ///   The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style.
      ///   The WS_SYSMENU style must also be specified.
      /// </summary>
      WS_MAXIMIZEBOX = 0x10000,

      /// <summary>
      ///   The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style.
      ///   The WS_SYSMENU style must also be specified.
      /// </summary>
      WS_MINIMIZEBOX = 0x20000,

      /// <summary>
      ///   The window is initially maximized.
      /// </summary>
      WS_MAXIMIZE = 0x1000000,

      /// <summary>
      ///   Excludes the area occupied by child windows when drawing occurs within the parent window. This style is
      ///   used when creating the parent window.
      /// </summary>
      WS_CLIPCHILDREN = 0x2000000,

      /// <summary>
      ///   The window is initially visible. This style can be turned on and off by using the
      ///   ShowWindow or SetWindowPos function.
      /// </summary>
      WS_VISIBLE = 0x10000000,

      /// <summary>
      ///   The window is initially minimized.
      /// </summary>
      WS_MINIMIZE = 0x20000000,

      /// <summary>
      ///   The window is a child window. A window with this style cannot have a menu bar. This style
      ///   cannot be used with the WS_POPUP style.
      /// </summary>
      WS_CHILD = 0x40000000
    }

    [Flags]
    internal enum WindowStylesEx : uint {
      /// <summary>
      ///   The window should be placed above all non-topmost windows and should stay above them, even when the window
      ///   is deactivated.
      /// </summary>
      WS_EX_TOPMOST = 0x00000008,

      /// <summary>
      ///   Specifies a window that is intended to be used as a floating toolbar. A tool window has a
      ///   title bar that is shorter than a normal title bar, and the window title is drawn using a
      ///   smaller font. A tool window does not appear in the taskbar or in the dialog that appears
      ///   when the user presses ALT+TAB. If a tool window has a system menu, its icon is not
      ///   displayed on the title bar. However, you can display the system menu by right-clicking or
      ///   by typing ALT+SPACE.
      /// </summary>
      WS_EX_TOOLWINDOW = 0x00000080,

      /// <summary>
      ///   Paints via double-buffering, which reduces flicker. This extended style also enables alpha-blended marquee
      ///   selection on systems where it is supported.
      /// </summary>
      LVS_EX_DOUBLEBUFFER = 0x00010000,

      /// <summary>
      ///   Paints all descendants of a window in bottom-to-top painting order using double-buffering.
      /// </summary>
      WS_EX_COMPOSITED = 0x02000000
    }

    #endregion

    #region Window procedures/messages

    /// <summary>
    ///   Represents UI state flags
    /// </summary>
    internal enum UIStateFlags : int {
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
    internal enum WindowMessage : int {
      /// <summary>
      ///   Sent to a window if the mouse causes the cursor to move within a window and mouse input is not captured.
      /// </summary>
      WM_SETCURSOR = 0x0020,

      /// <summary>
      ///   The WM_WINDOWPOSCHANGING message is sent to a window whose size, position, or place in the Z order is about
      ///   to change as a result of a call to the SetWindowPos function or another window-management function.
      /// </summary>
      WM_WINDOWPOSCHANGING = 0x0046,

      /// <summary>
      ///   An application sends the WM_COPYDATA message to pass data to another application.
      /// </summary>
      WM_COPYDATA = 0x004A,

      /// <summary>
      ///   Sent by a common control to its parent window when an event has occurred or the control requires some
      ///   information.
      /// </summary>
      WM_NOTIFY = 0x004E,

      /// <summary>
      ///   The WM_DISPLAYCHANGE message is sent to all windows when the display resolution has changed.
      /// </summary>
      WM_DISPLAYCHANGE = 0x007E,

      /// <summary>
      ///   Sent to a window in order to determine what part of the window corresponds to a particular screen
      ///   coordinate.
      /// </summary>
      WM_NCHITTEST = 0x0084,

      /// <summary>
      ///   An application sends the WM_CHANGEUISTATE message to indicate that the UI state should be changed.
      /// </summary>
      WM_CHANGEUISTATE = 0x0127,

      #region Mouse events
      /// <summary>
      ///   Posted to a window when the cursor moves.
      /// </summary>
      WM_MOUSEMOVE = 0x200,
      
      /// <summary>
      ///   Posted when the user presses the left mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_LBUTTONDOWN = 0x201,

      /// <summary>
      ///   Posted when the user releases the left mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_LBUTTONUP = 0x202,

      /// <summary>
      ///   Posted when the user presses the right mouse button while the cursor is in the client area of a window. 
      /// </summary>
      WM_RBUTTONDOWN = 0x204,

      /// <summary>
      ///   Posted when the user releases the right mouse button while the cursor is in the client area of a window.
      /// </summary>
      WM_RBUTTONUP = 0x205,
      #endregion

      /// <summary>
      ///   Notifies an application of a change to the hardware configuration of a device or the computer.
      /// </summary>
      WM_DEVICECHANGE = 0x0219,

      /// <summary>
      ///   Sent when the effective dots per inch (dpi) for a window has changed.
      /// </summary>
      WM_DPICHANGED = 0x02E0,

      /// <summary>
      ///   Informs all top-level windows that Desktop Window Manager (DWM) composition has been enabled or disabled.
      /// </summary>
      WM_DWMCOMPOSITIONCHANGED = 0x31E,

      /// <summary>
      ///   Informs all top-level windows that the colorization color has changed.
      /// </summary>
      WM_DWMCOLORIZATIONCHANGED = 0x320,

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
      TCM_ADJUSTRECT = 0x1328,

      /// <summary>
      ///   The WM_APP constant is used by applications to help define private messages, usually of the form WM_APP+X,
      ///   where X is an integer value.
      /// </summary>
      WM_APP = 0x8000
    }

    /// <summary>
    ///   Window procedure delegate
    /// </summary>
    /// <param name="hWnd">Window handle</param>
    /// <param name="msg">Window message</param>
    /// <param name="wParam">Reserved</param>
    /// <param name="lParam">Reserved</param>
    /// <returns>Reserved</returns>
    internal delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

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
    internal static extern IntPtr SendMessage(IntPtr hWnd, uint uiMsg, IntPtr wParam, IntPtr lParam);

    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern IntPtr SendMessage(
      IntPtr hWnd,
      uint uiMsg,
      IntPtr wParam,
      ref COPYDATASTRUCT lParam);

    #region CallWindowProc

    /// <summary>
    ///   Passes message information to the specified window procedure.
    /// </summary>
    /// <param name="lpPrevWndFunc">Previous WndProc delegate function</param>
    /// <param name="hWnd">A handle to the window procedure to receive the message.</param>
    /// <param name="uiMsg">The message.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>The return value specifies the result of the processing and depends on the message sent.</returns>
    [DllImport("user32.dll",
      CallingConvention = CallingConvention.Winapi,
      CharSet = CharSet.Ansi,
      SetLastError = true)]
    internal static extern IntPtr CallWindowProcA(
      WndProcDelegate lpPrevWndFunc,
      IntPtr hWnd,
      uint Msg,
      IntPtr wParam,
      IntPtr lParam);

    /// <summary>
    ///   Passes message information to the specified window procedure.
    /// </summary>
    /// <param name="lpPrevWndFunc">Previous WndProc delegate function</param>
    /// <param name="hWnd">A handle to the window procedure to receive the message.</param>
    /// <param name="uiMsg">The message.</param>
    /// <param name="wParam">Additional message-specific information.</param>
    /// <param name="lParam">Additional message-specific information.</param>
    /// <returns>The return value specifies the result of the processing and depends on the message sent.</returns>
    [DllImport("user32.dll",
      CallingConvention = CallingConvention.Winapi,
      CharSet = CharSet.Unicode,
      SetLastError = true)]
    internal static extern IntPtr CallWindowProcW(
      WndProcDelegate lpPrevWndFunc,
      IntPtr hWnd,
      uint Msg,
      IntPtr wParam,
      IntPtr lParam);

    #endregion

    #endregion

    #region GetWindowProc(Ptr)/SetWindowProc(Ptr)

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
    ///   specify one of the following values.
    /// </param>
    /// <returns>If the function succeeds, the return value is the requested value.</returns>
#if WIN32
    [DllImport(nameof(User32), EntryPoint = "GetWindowLong")]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#elif WIN64
    [DllImport(nameof(User32))]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#endif

    /// <summary>
    ///   Changes an attribute of the specified window. The function also sets a value at the specified offset in the
    ///   extra window memory.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window and, indirectly, the class to which the window belongs.
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
    internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
      new IntPtr(SetWindowLongPtr(hWnd, nIndex, dwNewLong.ToInt32()));

    [DllImport(nameof(User32), EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);
#elif WIN64
    [DllImport(nameof(User32))]
    internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif

    #endregion

    [Flags]
    internal enum SetWindowPosFlags {
      /// <summary>
      ///   Retains the current size (ignores the cx and cy parameters).
      /// </summary>
      SWP_NOSIZE = 0x0001,

      /// <summary>
      ///   Retains the current position (ignores X and Y parameters).
      /// </summary>
      SWP_NOMOVE = 0x0002,

      /// <summary>
      ///   Does not activate the window. If this flag is not set, the window is activated and moved to the top of
      ///   either the topmost or non-topmost group (depending on the setting of the hwndInsertAfter member).
      /// </summary>
      SWP_NOACTIVATE = 0x0010
    }

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

    #region GetAncestor

    /// <summary>The ancestor to be retrieved by <see cref="GetAncestor" />.</summary>
    internal enum GetAncestorFlags {
      /// <summary>Retrieves the root window by walking the chain of parent windows.</summary>
      GA_ROOT = 2
    }

    /// <summary>Retrieves the handle to the ancestor of the specified window.</summary>
    /// <param name="hWnd">
    ///   A handle to the window whose ancestor is to be retrieved. If this parameter is the desktop window,
    ///   the function returns <see cref="IntPtr.Zero" />.
    /// </param>
    /// <param name="gaFlags">The ancestor to be retrieved.</param>
    /// <returns>The handle to the ancestor window.</returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern IntPtr GetAncestor(IntPtr hWnd, GetAncestorFlags gaFlags);

    #endregion

    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern bool MoveWindow(
      IntPtr hWnd,
      int X,
      int Y,
      int nWidth,
      int nHeight,
      [MarshalAs(UnmanagedType.Bool)] bool bRepaint);

    /// <summary>
    ///   Retrieves the identifier of the thread that created the specified window and, optionally, the identifier of
    ///   the process that created the window.
    /// </summary>
    /// <param name="hWnd">A handle to the window.</param>
    /// <param name="lpdwProcessId">
    ///   A pointer to a variable that receives the process identifier. If this parameter is not NULL,
    ///   GetWindowThreadProcessId copies the identifier of the process to the variable; otherwise, it does not.
    /// </param>
    /// <returns>The return value is the identifier of the thread that created the window. </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern int GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    /// <summary>
    ///   Retrieves the name of the class to which the specified window belongs.
    /// </summary>
    /// <param name="hWnd">A handle to the window and, indirectly, the class to which the window belongs.</param>
    /// <param name="lpClassName">The class name string.</param>
    /// <param name="nMaxCount">
    ///   The length of the <paramref name="lpClassName" /> buffer, in characters. The buffer must be large enough to
    ///   include the terminating null character; otherwise, the class name string is truncated to
    ///   <paramref name="nMaxCount" />-1 characters.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is the number of characters copied to the buffer, not including
    ///   the terminating null character.
    ///   If the function fails, the return value is zero. To get extended error information, call GetLastError.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern int GetClassName(IntPtr hWnd, IntPtr lpClassName, int nMaxCount);

    /// <summary>
    ///   Retrieves a handle to the desktop window. The desktop window covers the entire screen. The desktop window is
    ///   the area on top of which other windows are painted.
    /// </summary>
    /// <returns>The return value is a handle to the desktop window.</returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr GetDesktopWindow();

    /// <summary>
    ///   Retrieves a handle to the top-level window whose class name and window name match the specified strings.
    ///   This function does not search child windows. This function does not perform a case-sensitive search.
    /// </summary>
    /// <param name="lpClassName">
    ///   The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function.
    /// </param>
    /// <param name="lpWindowName">
    ///   The window name (the window's title). If this parameter is NULL, all window names match.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the window that has the specified class name and
    ///   window name.
    /// </returns>
    [DllImport(nameof(User32), CharSet = CharSet.Unicode)]
    internal static extern IntPtr FindWindow([In] [Optional] string lpClassName, [In] [Optional] string lpWindowName);

    /// <summary>
    ///   Retrieves a handle to a window whose class name and window name match the specified strings.
    ///   The function searches child windows, beginning with the one following the specified child window.
    ///   This function does not perform a case-sensitive search.
    /// </summary>
    /// <param name="hwndParent">A handle to the parent window whose child windows are to be searched.</param>
    /// <param name="hwndChildAfter">
    ///   A handle to a child window. The search begins with the next child window in the Z order.
    ///   The child window must be a direct child window of hwndParent, not just a descendant window.
    /// </param>
    /// <param name="lpszClass">
    ///   The class name or a class atom created by a previous call to the RegisterClass or RegisterClassEx function.
    /// </param>
    /// <param name="lpszWindow">
    ///   The window name (the window's title). If this parameter is NULL, all window names match.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the window that has the specified class and window
    ///   names.
    /// </returns>
    [DllImport(nameof(User32), CharSet = CharSet.Unicode)]
    internal static extern IntPtr FindWindowEx(
      [In] [Optional] IntPtr hwndParent,
      [In] [Optional] IntPtr hwndChildAfter,
      [In] [Optional] string lpszClass,
      [In] [Optional] string lpszWindow);

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

    #endregion

    #region Drawing contexts

    /// <summary>
    ///   The ReleaseDC function releases a device context (DC), freeing it for use by other applications. The effect
    ///   of the ReleaseDC function depends on the type of DC. It frees only common and window DCs. It has no effect on
    ///   class or private DCs.
    /// </summary>
    /// <param name="hWnd">A handle to the window whose DC is to be released.</param>
    /// <param name="hDC">A handle to the DC to be released.</param>
    /// <returns>
    ///   The return value indicates whether the DC was released. If the DC was released, the return value is 1.
    ///   If the DC was not released, the return value is zero.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern bool ReleaseDC([In] IntPtr hWnd, [In] IntPtr hDC);

    /// <summary>
    ///   The GetWindowDC function retrieves the device context (DC) for the entire window, including title bar, menus,
    ///   and scroll bars. A window device context permits painting anywhere in a window, because the origin of the
    ///   device context is the upper-left corner of the window instead of the client area.
    ///   GetWindowDC assigns default attributes to the window device context each time it retrieves the device
    ///   context. Previous attributes are lost.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window with a device context that is to be retrieved. If this value is NULL, GetWindowDC
    ///   retrieves the device context for the entire screen.
    ///   If this parameter is NULL, GetWindowDC retrieves the device context for the primary display monitor. To get
    ///   the device context for other display monitors, use the EnumDisplayMonitors and CreateDC functions.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to a device context for the specified window.
    ///   If the function fails, the return value is NULL, indicating an error or an invalid hWnd parameter.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr GetWindowDC([In] IntPtr hWnd);

    /// <summary>
    ///   The <see cref="GetDC" /> function retrieves a handle to a device context (DC) for the client area of a
    ///   specified window or for the entire screen. You can use the returned handle in subsequent GDI functions to
    ///   draw in the DC. The device context is an opaque data structure, whose values are used internally by GDI.
    ///   The GetDCEx function is an extension to <see cref="GetDC" />, which gives an application more control over
    ///   how and whether clipping occurs in the client area.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window whose DC is to be retrieved. If this value is NULL, <see cref="GetDC" /> retrieves the
    ///   DC for the entire screen.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the DC for the specified window's client area.
    ///   If the function fails, the return value is <see cref="IntPtr.Zero" />.
    /// </returns>
    [DllImport(nameof(User32), SetLastError = false)]
    internal static extern IntPtr GetDC(IntPtr hWnd);

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
    ///   Values for the <c>nIndex</c> parameter in <see cref="GetSystemMetrics" />
    /// </summary>
    internal enum SystemMetrics {
      /// <summary>
      ///   The recommended height of a small icon, in pixels.
      /// </summary>
      SM_CYSMICON = 50
    }

    /// <summary>
    ///   Retrieves te specified system metric or system configuration setting.
    /// </summary>
    /// <param name="nIndex">The system metric or configuration setting to be retrieved.</param>
    /// <returns>If the function succeeds, the return value is nonzero.</returns>
    [DllImport(nameof(User32))]
    internal static extern int GetSystemMetrics([In] int nIndex);

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

    [DllImport(nameof(User32))]
    internal static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

    [DllImport(nameof(User32))]
    internal static extern IntPtr SetCursor(IntPtr hCursor);

    #endregion

    #region ListView Empty Markup

    internal const uint LVN_FIRST = unchecked(0u - 100u);
    internal const uint LVN_GETEMPTYMARKUP = LVN_FIRST - 87;
    internal const uint L_MAX_URL_LENGTH = 2084;

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
      internal IntPtr idFrom;

      /// <summary>
      ///   A notification code.
      /// </summary>
      internal int code;
    }

    /// <summary>
    ///   Contains information used with the LVN_GETEMPTYMARKUP notification code.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct NMLVEMPTYMARKUP {
      /// <summary>
      ///   Info on the notification message.
      /// </summary>
      internal NMHDR hdr;

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
    ///   <see cref="SetWindowsHookEx(WindowsHookType, IntPtr, IntPtr, int)" />.
    /// </summary>
    internal enum WindowsHookType {
      /// <summary>
      ///   Installs a hook procedure that monitors messages before the system sends them to the destination window
      ///   procedure.
      /// </summary>
      WH_CALLWNDPROC = 4,
      
      /// <summary>Installs a hook procedure that monitors low-level keyboard input events.</summary>
      WH_KEYBOARD_LL = 13,

      /// <summary>Installs a hook procedure that monitors low-level mouse input events.</summary>
      WH_MOUSE_LL = 14
    }

    /// <summary>
    ///   An application-defined or library-defined callback function used with the
    ///   <see cref="SetWindowsHookEx(WindowsHookType, IntPtr, IntPtr, int)" /> function.
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
    internal delegate int WindowsHookDelegate(int nCode, IntPtr wParam, IntPtr lParam);

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
      [In, Optional] IntPtr hMod,
      [In, Optional] int dwThreadId);

    /// <summary>
    ///   Removes a hook procedure installed in a hook chain by the
    ///   <see cref="SetWindowsHookEx(WindowsHookType, IntPtr, IntPtr, int)" /> function.
    /// </summary>
    /// <param name="hhk">
    ///   A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
    ///   <see cref="SetWindowsHookEx(WindowsHookType, IntPtr, IntPtr, int)" />.
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

  }

  #endregion
}