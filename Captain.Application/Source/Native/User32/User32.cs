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
      ///   The window is intended to be used as a floating toolbar.
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
      TCM_ADJUSTRECT = 0x1328
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

    [DllImport(nameof(User32), SetLastError = true)]
    internal static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

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
    internal static extern IntPtr FindWindow([In, Optional] string lpClassName, [In, Optional] string lpWindowName);

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
      [In, Optional] IntPtr hwndParent,
      [In, Optional] IntPtr hwndChildAfter,
      [In, Optional] string lpszClass,
      [In, Optional] string lpszWindow);

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
      LWA_ALPHA = 2,

      /// <summary>
      ///   Use crKey as the transparency color.
      /// </summary>
      LWA_COLORKEY = 1
    }

    #endregion

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
    internal static extern IntPtr LoadCursor([In, Optional] IntPtr hInstance, [In] IntPtr lpCursorName);

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
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,int)" />.
    /// </summary>
    internal enum WindowsHookType {
      /// <summary>Installs a hook procedure that monitors low-level mouse input events.</summary>
      WH_MOUSE_LL = 14
    }

    /// <summary>
    ///   An application-defined or library-defined callback function used with the
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,int)" /> function.
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
      [In, Optional] IntPtr hMod,
      [In, Optional] int dwThreadId);

    /// <summary>
    ///   Removes a hook procedure installed in a hook chain by the
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,int)" /> function.
    /// </summary>
    /// <param name="hhk">
    ///   A handle to the hook to be removed. This parameter is a hook handle obtained by a previous call to
    ///   <see cref="SetWindowsHookEx(WindowsHookType,WindowsHookDelegate,IntPtr,int)" />.
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
      ACCENT_ENABLE_BLURBEHIND = 3
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct AccentPolicy {
      internal AccentState AccentState;
      private readonly int AccentFlags;
      private readonly int GradientColor;
      private readonly int AnimationId;
    }

    [DllImport(nameof(User32))]
    internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    #endregion
  }

  #endregion
}