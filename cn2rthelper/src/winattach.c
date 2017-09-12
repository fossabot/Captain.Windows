#include <easyhook.h>
#include <Windows.h>

#include <cn2rthelper/winattach.h>
#include <cn2/capn_wm.h>

static WINATTACHINFO g_attachinfo = { 0 };  /// window attachment information
static WNDPROC g_lpfnOrgWndProc = NULL;     /// original window procedure
static LONG g_lOrgWndStyle = 0;             /// original window style attributes

/// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam);

/// performs window attachment
void CN2AttachWindow(PWINATTACHINFO pInfo) {
  memcpy(&g_attachinfo, pInfo, sizeof(WINATTACHINFO));
  g_lOrgWndStyle = GetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE);

  // remove minimize/maximize/resize capabilities from window
  SetWindowLong(LongToPtr(pInfo->uiTargetHandle), GWL_STYLE,
    g_lOrgWndStyle & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SIZEBOX));

  // replace window procedure
  g_lpfnOrgWndProc = (WNDPROC)SetWindowLongPtr(LongToPtr(pInfo->uiTargetHandle), GWLP_WNDPROC, WndProc);

  // set toolbar window as owned window
  SetWindowLongPtr(LongToPtr(pInfo->uiToolbarHandle), GWLP_HWNDPARENT, pInfo->uiTargetHandle);
}

/// detaches the specified window
static void CN2DetachWindow(HWND hwnd) {
  // restore original style attributes
  SetWindowLong(hwnd, GWL_STYLE, g_lOrgWndStyle);

  // restore original window procedure
  SetWindowLongPtr(LongToPtr(g_attachinfo.uiTargetHandle), GWLP_WNDPROC, (LONG_PTR)g_lpfnOrgWndProc);

  // restore toolbar window parent
  SetWindowLongPtr(LongToPtr(g_attachinfo.uiToolbarHandle), GWLP_HWNDPARENT, (LONG_PTR)NULL);

  // zero-out the attach information structure
  ZeroMemory(&g_attachinfo, sizeof(WINATTACHINFO));
  g_lpfnOrgWndProc = NULL;
  g_lOrgWndStyle = 0;
}

/// replacement window procedure
static LRESULT CALLBACK WndProc(HWND hwnd, UINT uiMsg, WPARAM wParam, LPARAM lParam) {
  if (g_attachinfo.uiTargetHandle == PtrToLong(hwnd)) {
    // this window is attached
    switch (uiMsg) {
    case WM_CAPN_DETACHWND:  // detach the window
      CN2DetachWindow(hwnd);
      break;

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

    // make sure this data is targeted to us
    if (pCopydata->dwData == WM_COPYDATA_CAPNSIG && pCopydata->cbData == sizeof(WINATTACHINFO)) {
      CN2AttachWindow((PWINATTACHINFO)pCopydata->lpData);
    }
  }

  // call original window procedure
  return CallWindowProc(g_lpfnOrgWndProc, hwnd, uiMsg, wParam, lParam);
}