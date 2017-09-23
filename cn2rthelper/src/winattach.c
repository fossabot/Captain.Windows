#include <Windows.h>
#include <VersionHelpers.h>

#include <cn2rthelper/winattach.h>
#include <cn2/capn_wm.h>

#include <stdio.h>

static WINATTACHINFO g_attachinfo = { 0 };  /// window attachment information
static WNDPROC g_lpfnOrgWndProc = NULL;     /// original window procedure
static LONG g_lOrgWndStyle = 0;             /// original window style attributes

static HWINEVENTHOOK g_hweDestroyWnd;       /// window hook for window destruction

                                            /// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam);

/// ChangeWindowMessageFilterEx()
typedef BOOL(*CWMFEPROC)(HWND, UINT, DWORD, PCHANGEFILTERSTRUCT);

/// detaches the specified window
static void DetachWindow(HWND hwnd) {
  // unhook window events
  UnhookWinEvent(g_hweDestroyWnd);

  // restore original style attributes
  SetWindowLong(hwnd, GWL_STYLE, g_lOrgWndStyle);

#if 0
  // restore original window procedure
  // XXX: if we restore the original procedure we won't be able to handle WM_COPYDATA messages, meaning the library
  //      must be injected every time. So the drawback of not leaking memory each time the library is injected is to
  //      keep the overhead of handling the window procedure manually
  SetWindowLongPtr(LongToPtr(g_attachinfo.uiTargetHandle), GWLP_WNDPROC, (LONG_PTR)g_lpfnOrgWndProc);
  g_lpfnOrgWndProc = NULL;
#endif

  // restore original toolbar window style
  SetWindowLong(LongToPtr(g_attachinfo.uiToolbarHandle), GWL_STYLE,
    GetWindowLong(LongToPtr(g_attachinfo.uiToolbarHandle), GWL_STYLE) & ~WS_CHILD | WS_POPUP);

  // restore toolbar window parent
  SetWindowLongPtr(LongToPtr(g_attachinfo.uiToolbarHandle), GWLP_HWNDPARENT, (LONG_PTR)NULL);

  // zero-out the attach information structure
  ZeroMemory(&g_attachinfo, sizeof(WINATTACHINFO));
  g_lOrgWndStyle = 0;
}

/// window event hook procedure
static void CALLBACK WinEventProc(HWINEVENTHOOK hWinEventHook, DWORD dwEvent, HWND hwnd, LONG lObject, LONG lChild,
  DWORD lEventThread, DWORD dwmsEventTime) {
  DebugBreak();

  if (dwEvent == EVENT_OBJECT_DESTROY && PtrToLong(hwnd) == g_attachinfo.uiTargetHandle && lObject == OBJID_WINDOW &&
    lChild == INDEXID_CONTAINER) {
    MessageBox(NULL, "HASTA LA VISTA", "BABY", 0);

    // notify grabber UI of window destruction
    SendMessage(LongToPtr(g_attachinfo.uiGrabberHandle), WM_CAPN_DETACHWND, (WPARAM)hwnd, MAKELPARAM(NULL, NULL));

    // detach the window
    DetachWindow(hwnd);
  }
}

/// performs window attachment
void RtAttachWindow(PWINATTACHINFO pInfo) {
  memcpy(&g_attachinfo, pInfo, sizeof(WINATTACHINFO));
  g_lOrgWndStyle = GetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE);

  // remove minimize/maximize/resize capabilities from window
  SetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE,
    g_lOrgWndStyle /*& ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SIZEBOX)*/);

  // replace window procedure if necessary
  if (!g_lpfnOrgWndProc) {
    g_lpfnOrgWndProc = (WNDPROC)SetWindowLongPtr(LongToPtr(pInfo->uiTargetHandle), GWLP_WNDPROC, (LONG_PTR)WndProc);
  }

  // we may need to re-attach to this window. In order to receive WM_COPYDATA messages, we need to adjust UIPI filters
  HMODULE hUser32;
  CWMFEPROC fpChangeWindowMessageFilterEx;

  if (IsWindows7OrGreater() && ((hUser32 = LoadLibrary(TEXT("user32.dll")))) &&
    ((fpChangeWindowMessageFilterEx = (CWMFEPROC)GetProcAddress(hUser32, "ChangeWindowMessageFilterEx")))) {
    fpChangeWindowMessageFilterEx(LongToPtr(pInfo->uiTargetHandle), WM_COPYDATA, MSGFLT_ALLOW, NULL);
  }
  else {
    ChangeWindowMessageFilter(WM_COPYDATA, MSGFLT_ALLOW);
  }

  // watch this window so we get notified when it's destroyed
  DWORD dwProcessId;
  DWORD dwThreadId = GetWindowThreadProcessId(LongToPtr(pInfo->uiTargetHandle), &dwProcessId);
  DebugBreak();

  if (dwThreadId) {
    g_hweDestroyWnd = SetWinEventHook(EVENT_OBJECT_DESTROY, EVENT_OBJECT_DESTROY, NULL, WinEventProc, dwProcessId,
      dwThreadId, WINEVENT_OUTOFCONTEXT);
    DebugBreak();
  }

  // prevent toolbar window to "steal" focus of the owner window
  SetWindowLong(LongToPtr(pInfo->uiToolbarHandle), GWL_STYLE,
    GetWindowLong(LongToPtr(pInfo->uiToolbarHandle), GWL_STYLE) & ~WS_POPUP | WS_CHILD);

  // set toolbar window as owned window
  SetWindowLongPtr(LongToPtr(pInfo->uiToolbarHandle), GWLP_HWNDPARENT, pInfo->uiTargetHandle);
}

/// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam) {
  if (g_attachinfo.uiTargetHandle == PtrToLong(hwnd)) {
    // this window is attached
    switch (uiMsg) {
    case WM_CAPN_DETACHWND:  // detach the window
      DetachWindow(hwnd);
      return 0;

    case WM_WINDOWPOSCHANGED: {  // window bounds are changing
      PWINDOWPOS pos = (PWINDOWPOS)lParam;
      RECT rcGrabber;

      // make sure width and height are not nil
      if (!pos->cx || !pos->cy) { break; }

      // HACK: set grabber window bounds first - then copy the current bounds so we adjust to the actual allowed bounds
      if (MoveWindow(LongToPtr(g_attachinfo.uiGrabberHandle), pos->x, pos->y, pos->cx, pos->cy, FALSE)) {
        // get current grabber bounds
        if (GetWindowRect(LongToPtr(g_attachinfo.uiGrabberHandle), &rcGrabber) &&
          rcGrabber.right - rcGrabber.left &&
          rcGrabber.bottom - rcGrabber.top) {
          // copy position
          pos->x = rcGrabber.left;
          pos->y = rcGrabber.top;
          pos->cx = rcGrabber.right - rcGrabber.left;
          pos->cy = rcGrabber.bottom - rcGrabber.top;
        }
      }
      break;
    }

    default:;
    }
  }
  else if (uiMsg == WM_COPYDATA) {
    PCOPYDATASTRUCT pCopydata = (PCOPYDATASTRUCT)lParam;

    if (pCopydata->dwData == WM_COPYDATA_CAPNSIG && pCopydata->cbData == sizeof(WINATTACHINFO)) {
      RtAttachWindow((PWINATTACHINFO)pCopydata->lpData);
      return 0;
    }
  }

  // call original window procedure
  // TODO: apply sort of UIPI filtering here - that is, if WM_COPYDATA was not previously allowed, filter it here
  //       manually so we play nice with the application developer's intent
  return CallWindowProc(g_lpfnOrgWndProc, hwnd, uiMsg, wParam, lParam);
}