// ReSharper disable All

using System;
using System.Runtime.InteropServices;
using Windows.UI.Composition.Interactions;

namespace Captain.Application.Native {
  /// <summary>
  ///   Exported functions from the user32.dll Windows library.
  /// </summary>
  internal static partial class User32 {
    internal enum SystemResources {
      /// <summary>
      ///   Hand cursor
      /// </summary>
      IDC_HAND = 32649
    }

    internal enum HitTestValues {
      /// <summary>
      ///   In a window currently covered by another window in the same thread (the message will be sent to underlying
      ///   windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
      /// </summary>
      HTTRANSPARENT = -1
    }

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
      WS_EX_TOOLWINDOW = 0x00000080
    }

    #endregion

    #region Window procedures/messages

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
      ///   Sent to a window in order to determine what part of the window corresponds to a particular screen
      ///   coordinate.
      /// </summary>
      WM_NCHITTEST = 0x0084,

      /// <summary>
      ///   Notifies an application of a change to the hardware configuration of a device or the computer.
      /// </summary>
      WM_DEVICECHANGE = 0x0219,

      /// <summary>
      ///   Sent when the effective dots per inch (dpi) for a window has changed.
      /// </summary>
      WM_DPICHANGED = 0x02E0,

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
    internal static extern IntPtr SendMessage(IntPtr hWnd,
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
    internal static extern IntPtr CallWindowProcA(WndProcDelegate lpPrevWndFunc,
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
    internal static extern IntPtr CallWindowProcW(WndProcDelegate lpPrevWndFunc,
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
    ///   specify one of the following values.</param>
    /// <returns>If the function succeeds, the return value is the requested value.</returns>
#if WIN32
    [DllImport(nameof(User32), EntryPoint = "GetWindowLong")]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
#elif WIN64
    [DllImport(nameof(User32), EntryPoint = "GetWindowLongPtr")]
    internal static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
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
    internal static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong) =>
      new IntPtr(SetWindowLongPtr(hWnd, nIndex, dwNewLong.ToInt32()));

    [DllImport(nameof(User32), EntryPoint = "SetWindowLong")]
    private static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);
#elif WIN64
    [DllImport(nameof(User32))]
    internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif

    #endregion

    /// <summary>
    /// Retrieves a handle to the window that contains the specified point.
    /// </summary>
    /// <param name="Point">The point to be checked.</param>
    /// <returns>
    ///   The return value is a handle to the window that contains the point. If no window exists at the given point,
    ///   the return value is <see cref="IntPtr.Zero"/>. If the point is over a static text control, the return value
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
    ///   The length of the <paramref name="lpClassName"/> buffer, in characters. The buffer must be large enough to
    ///   include the terminating null character; otherwise, the class name string is truncated to
    ///   <paramref name="nMaxCount"/>-1 characters.
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
    /// The window name (the window's title). If this parameter is NULL, all window names match.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the window that has the specified class name and
    ///   window name.
    /// </returns>
    [DllImport(nameof(User32))]
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
    /// The window name (the window's title). If this parameter is NULL, all window names match.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the window that has the specified class and window
    ///   names.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern IntPtr FindWindowEx([In] [Optional] IntPtr hwndParent,
                                               [In] [Optional] IntPtr hwndChildAfter,
                                               [In] [Optional] string lpszClass,
                                               [In] [Optional] string lpszWindow);
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
    ///   The <see cref="GetDC"/> function retrieves a handle to a device context (DC) for the client area of a
    ///   specified window or for the entire screen. You can use the returned handle in subsequent GDI functions to
    ///   draw in the DC. The device context is an opaque data structure, whose values are used internally by GDI.
    ///   The GetDCEx function is an extension to <see cref="GetDC"/>, which gives an application more control over
    ///   how and whether clipping occurs in the client area.
    /// </summary>
    /// <param name="hWnd">
    ///   A handle to the window whose DC is to be retrieved. If this value is NULL, <see cref="GetDC"/> retrieves the
    ///   DC for the entire screen.
    /// </param>
    /// <returns>
    ///   If the function succeeds, the return value is a handle to the DC for the specified window's client area.
    ///   If the function fails, the return value is <see cref="IntPtr.Zero"/>.
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
    ///   The DPI for the window which depends on the DPI_AWARENESS of the window. See the Remarks for more
    ///   information. An invalid hwnd value will result in a return value of 0.
    /// </returns>
    [DllImport(nameof(User32))]
    internal static extern int GetDpiForWindow(IntPtr hWnd);

    /// <summary>
    ///   Returns the system DPI.
    /// </summary>
    /// <returns>The return value will be dependent based upon the calling context.</returns>
    [DllImport(nameof(User32))]
    internal static extern int GetDpiForSystem();
    #endregion

    #region Cursors
    [DllImport(nameof(User32))]
    internal static extern IntPtr LoadCursor(IntPtr hInstance, IntPtr lpCursorName);

    [DllImport(nameof(User32))]
    internal static extern IntPtr SetCursor(IntPtr hCursor);
    #endregion
  }
}
