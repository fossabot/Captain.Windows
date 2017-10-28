﻿#include <Windows.h>
#include <VersionHelpers.h>

#include <cn2rthelper/winattach.h>
#include <cn2/capn_wm.h>

#include <stdio.h>

static WINATTACHINFO g_attachinfo = { 0 };  /// window attachment information
static WNDPROC g_lpfnOrgWndProc = NULL;     /// original window procedure

static LRESULT WINAPI WndProc(_In_ const HWND hwnd, _In_ const UINT uiMsg, _In_ const WPARAM wParam,
  _In_ const LPARAM lParam);

/// ChangeWindowMessageFilterEx()
typedef BOOL(WINAPI *CWMFEPROC)(HWND, UINT, DWORD, PCHANGEFILTERSTRUCT);

/// <summary>
///   Detaches the specified window
/// </summary>
/// <param name="hwnd">Window handle</param>
static void DetachWindow(HWND hwnd) {
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
}

/// <summary>
///   Attaches a window
/// </summary>
/// <param name="pInfo">Window attachment information, usually passed by the injector process</param>
void RtAttachWindow(_In_ const PWINATTACHINFO pInfo) {
  memcpy(&g_attachinfo, pInfo, sizeof(WINATTACHINFO));

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

  // prevent toolbar window to "steal" focus of the owner window
  SetWindowLong(LongToPtr(pInfo->uiToolbarHandle), GWL_STYLE,
    GetWindowLong(LongToPtr(pInfo->uiToolbarHandle), GWL_STYLE) & ~WS_POPUP | WS_CHILD);

  // set toolbar window as owned window
  SetWindowLongPtr(LongToPtr(pInfo->uiToolbarHandle), GWLP_HWNDPARENT, pInfo->uiTargetHandle);
}

/// <summary>
///   Window procedure hook
/// </summary>
/// <param name="hwnd">Window handle</param>
/// <param name="uiMsg">Message</param>
/// <param name="wParam">Message-dependant value</param>
/// <param name="lParam">Message-dependant value</param>
/// <returns>Calls the original window procedure and returns its result</returns>
static LRESULT WINAPI WndProc(_In_ const HWND hwnd, _In_ const UINT uiMsg, _In_ const WPARAM wParam,
  _In_ const LPARAM lParam) {
  if (g_attachinfo.uiTargetHandle == PtrToLong(hwnd)) {
    // this window is attached
    switch (uiMsg) {
    case WM_CAPN_DETACHWND:  // detach the window
      DetachWindow(hwnd);
      return 0;

    case WM_WINDOWPOSCHANGED: {  // window bounds are changing
      const PWINDOWPOS pPos = (PWINDOWPOS)lParam;

      // make sure width and height are not nil
      if (pPos->cx && pPos->cy) {
        // move/resize grabber UI, then let it resize us
        SetWindowPos(LongToPtr(g_attachinfo.uiGrabberHandle), NULL, pPos->x, pPos->y, pPos->cx, pPos->cy,
          SWP_NOACTIVATE | SWP_NOZORDER | SWP_ASYNCWINDOWPOS | SWP_NOREDRAW);
      }

      break;
    }
    default:;
    }
  }
  else if (uiMsg == WM_COPYDATA) {
    const PCOPYDATASTRUCT pCopydata = (PCOPYDATASTRUCT)lParam;

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