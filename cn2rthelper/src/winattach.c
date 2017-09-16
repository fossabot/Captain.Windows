#include <Windows.h>
#include <VersionHelpers.h>

#include <cn2rthelper/winattach.h>
#include <cn2/capn_wm.h>

#include <stdio.h>

static WINATTACHINFO g_attachinfo = { 0 };  /// window attachment information
static WNDPROC g_lpfnOrgWndProc = NULL;     /// original window procedure
static LONG g_lOrgWndStyle = 0;             /// original window style attributes

/// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam);

/// ChangeWindowMessageFilterEx()
typedef BOOL(*CWMFEPROC)(HWND, UINT, DWORD, PCHANGEFILTERSTRUCT);

/// performs window attachment
void RtAttachWindow(PWINATTACHINFO pInfo) {
  memcpy(&g_attachinfo, pInfo, sizeof(WINATTACHINFO));
  g_lOrgWndStyle = GetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE);

  // remove minimize/maximize/resize capabilities from window
  SetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE,
    g_lOrgWndStyle & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SIZEBOX));

  // replace window procedure if necessary
  if (!g_lpfnOrgWndProc) {
    g_lpfnOrgWndProc = (WNDPROC)SetWindowLongPtr(LongToPtr(pInfo->uiTargetHandle), GWLP_WNDPROC, (LONG_PTR)WndProc);
  }

  // we may need to re-attach to this window. In order to receive WM_COPYDATA messages, we need to adjust UIPI filters
  HMODULE hUser32 = NULL;
  CWMFEPROC fpChangeWindowMessageFilterEx = NULL;

  if (IsWindows7OrGreater() &&
    (hUser32 = LoadLibrary(TEXT("user32.dll")) &&
    (fpChangeWindowMessageFilterEx = (CWMFEPROC)GetProcAddress(hUser32, "ChangeWindowMessageFilterEx")))) {
    fpChangeWindowMessageFilterEx(LongToPtr(pInfo->uiTargetHandle), WM_COPYDATA, MSGFLT_ALLOW, NULL);
  }
  else {
    ChangeWindowMessageFilter(WM_COPYDATA, MSGFLT_ALLOW);
  }
#if 0
  if (IsWindows7OrGreater()) {
    ChangeWindowMessageFilterEx(LongToPtr(pInfo->uiTargetHandle), WM_COPYDATA, MSGFLT_ALLOW, NULL);
  }
  else {
    // for Windows Vista support
    ChangeWindowMessageFilter(WM_COPYDATA, MSGFLT_ALLOW);
  }
#endif


  // set toolbar window as owned window
  SetWindowLongPtr(LongToPtr(pInfo->uiToolbarHandle), GWLP_HWNDPARENT, pInfo->uiTargetHandle);
  }

/// detaches the specified window
static void DetachWindow(HWND hwnd) {
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

  // restore toolbar window parent
  SetWindowLongPtr(LongToPtr(g_attachinfo.uiToolbarHandle), GWLP_HWNDPARENT, (LONG_PTR)NULL);

  // zero-out the attach information structure
  ZeroMemory(&g_attachinfo, sizeof(WINATTACHINFO));
  g_lOrgWndStyle = 0;
}

/// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam) {
  if (g_attachinfo.uiTargetHandle == PtrToLong(hwnd)) {
    // this window is attached
    switch (uiMsg) {
    case WM_CLOSE:
    case WM_QUIT:
      SendMessage(LongToPtr(g_attachinfo.uiGrabberHandle), WM_CAPN_DETACHWND, (WPARAM)hwnd, MAKELPARAM(NULL, NULL));
      DetachWindow(hwnd);
      break;

    case WM_CAPN_DETACHWND:  // detach the window
      DetachWindow(hwnd);
      return 0;

    case WM_WINDOWPOSCHANGING: {  // window bounds are changing
      PWINDOWPOS pos = (PWINDOWPOS)lParam;

      if (!pos->x && !pos->y && !pos->cx && !pos->cy) { break; }

      // prevent the window from being resized
      // TODO: we could let this happen under some circumstances - investigate this possibility!
      if (pos->cx != g_attachinfo.rcOrgTargetBounds.right - g_attachinfo.rcOrgTargetBounds.left) {
        pos->cx = g_attachinfo.rcOrgTargetBounds.right - g_attachinfo.rcOrgTargetBounds.left;
      }

      if (pos->cy != g_attachinfo.rcOrgTargetBounds.bottom - g_attachinfo.rcOrgTargetBounds.top) {
        pos->cy = g_attachinfo.rcOrgTargetBounds.bottom - g_attachinfo.rcOrgTargetBounds.top;
      }

      // prevent the window from going off-screen
      if (pos->x < g_attachinfo.rcAcceptableBounds.left) { pos->x = g_attachinfo.rcAcceptableBounds.left; }
      if (pos->y < g_attachinfo.rcAcceptableBounds.top) { pos->y = g_attachinfo.rcAcceptableBounds.top; }
      if (pos->x + pos->cx > g_attachinfo.rcAcceptableBounds.right) {
        pos->x = g_attachinfo.rcAcceptableBounds.right - pos->cx;
      }
      if (pos->y + pos->cy > g_attachinfo.rcAcceptableBounds.bottom) {
        pos->y = g_attachinfo.rcAcceptableBounds.bottom - pos->cy;
      }

      // if we change the grabber window position, the toolbar position will be adjusted accordingly, too
      SetWindowPos(LongToPtr(g_attachinfo.uiGrabberHandle), NULL, pos->x, pos->y - 8, pos->cx, pos->cy + 8,
        SWP_NOACTIVATE);
      break;
    }
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