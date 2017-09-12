// ReSharper disable All

using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <content>
  /// Contains the Windows Messages constants.
  /// </content>
  internal static class User32 {
    #region Window styles

    /// <summary>
    ///   Window Styles. The following styles can be specified wherever a window style is required.
    ///   After the control has been created, these styles cannot be modified, except as noted.
    /// </summary>
    [Flags]
    internal enum WindowStyles {
      /// <summary>
      ///   The window is initially maximized.
      /// </summary>
      WS_MAXIMIZE = 0x1000000,

      /// <summary>
      ///   The window has a maximize button. Cannot be combined with the WS_EX_CONTEXTHELP style.
      ///   The WS_SYSMENU style must also be specified.
      /// </summary>
      WS_MAXIMIZEBOX = 0x10000,

      /// <summary>
      ///   The window is initially minimized.
      /// </summary>
      WS_MINIMIZE = 0x20000000,

      /// <summary>
      ///   The window has a minimize button. Cannot be combined with the WS_EX_CONTEXTHELP style.
      ///   The WS_SYSMENU style must also be specified.
      /// </summary>
      WS_MINIMIZEBOX = 0x20000,
    }

    [Flags]
    internal enum WindowStylesEx : uint {
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
    /// Windows Messages
    /// Defined in winuser.h from Windows SDK v6.1
    /// Documentation pulled from MSDN.
    /// </summary>
    internal enum WindowMessage : int {
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
    [DllImport(nameof(User32), EntryPoint = "SetWindowLong")]
    internal static extern int SetWindowLongPtr(IntPtr hWnd, int nIndex, int dwNewLong);
#elif WIN64
    [DllImport(nameof(User32))]
    internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
#endif

    #endregion

    internal enum HitTestValues {
      /// <summary>
      ///   In a window currently covered by another window in the same thread (the message will be sent to underlying
      ///   windows in the same thread until one of them returns a code that is not HTTRANSPARENT).
      /// </summary>
      HTTRANSPARENT = -1
    }

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
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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
  }
}
