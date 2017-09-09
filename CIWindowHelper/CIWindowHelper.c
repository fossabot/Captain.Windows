#include <Windows.h>
#include <stdio.h>
#include <easyhook.h>

#include "../Captain.Application.NativeHelpers/UI/WindowAttachInfo.h"

/// internal CIWindowHelper data structure
typedef struct {
  WNDPROC lpfnOrgWndProc;  /// original WndProc function
  LONG lWindowStyle;       /// original window style attributes
} CIWNDPROCDATA, *PCIWNDPROCDATA;

static CIWNDPROCDATA g_data = { 0 };
static WINATTACHINFO32 g_winAttachInfo = { 0 };
static BOOL g_bAttached = FALSE;

static void PerformWindowAttachment(void);

/// WndProc hook
static LRESULT CALLBACK WndProcHook(HWND hWnd, UINT uiMsg, WPARAM wParam, LPARAM lParam) {
  if (g_bAttached) {
    switch (uiMsg) {
    case WM_CAPTAINDETACHWINDOW:
      SetWindowLong(hWnd, GWL_STYLE, g_data.lWindowStyle);
      g_bAttached = FALSE;
      break;

    case WM_WINDOWPOSCHANGING: {
      PWINDOWPOS pos = (PWINDOWPOS)lParam;

      if (!pos->x && !pos->y && !pos->cx && !pos->cy) { break; }

      // prevent the window from being resized
      // TODO: we could let this happen under some circumstances - investigate this possibility!
      if (pos->cx != g_winAttachInfo.rcOrgTargetBounds.right - g_winAttachInfo.rcOrgTargetBounds.left) { pos->cx = g_winAttachInfo.rcOrgTargetBounds.right - g_winAttachInfo.rcOrgTargetBounds.left; }
      if (pos->cy != g_winAttachInfo.rcOrgTargetBounds.bottom - g_winAttachInfo.rcOrgTargetBounds.top) { pos->cy = g_winAttachInfo.rcOrgTargetBounds.bottom - g_winAttachInfo.rcOrgTargetBounds.top; }

      // prevent the window from going off-screen
      if (pos->x < g_winAttachInfo.rcAcceptableBounds.left) { pos->x = g_winAttachInfo.rcAcceptableBounds.left; }
      if (pos->y < g_winAttachInfo.rcAcceptableBounds.top) { pos->y = g_winAttachInfo.rcAcceptableBounds.top; }
      if (pos->x + pos->cx > g_winAttachInfo.rcAcceptableBounds.right) { pos->x = g_winAttachInfo.rcAcceptableBounds.right - pos->cx; }
      if (pos->y + pos->cy > g_winAttachInfo.rcAcceptableBounds.bottom) { pos->y = g_winAttachInfo.rcAcceptableBounds.bottom - pos->cy; }

      // if we change the grabber window position, the toolbar position will be adjusted accordingly, too
      SetWindowPos(LongToPtr(g_winAttachInfo.hGrabberWndLong), NULL, pos->x, pos->y - 8, pos->cx, pos->cy + 8, SWP_NOACTIVATE);
      break;
    }
    }
  }
  else if (uiMsg == WM_COPYDATA) {  // we're not attached - but may be re-attached so we'll need fresh window attachment information that will be provided through WM_COPYDATA
    PCOPYDATASTRUCT pCopydata = (PCOPYDATASTRUCT)lParam;

    // make sure these are the droids we're looking for
    if (pCopydata->dwData == CAPTAIN_COPYDATA_SIGNATURE) {
      // copy the window attachment information
      memcpy(&g_winAttachInfo, pCopydata->lpData, sizeof(WINATTACHINFO32));
      PerformWindowAttachment();
    }
  }

  // call original window procedure
  return CallWindowProc(g_data.lpfnOrgWndProc, hWnd, uiMsg, wParam, lParam);
}

/// adjusts the window to perform attachment
static void PerformWindowAttachment(void) {
  if (!g_data.lpfnOrgWndProc) {
    // save original window procedure
    g_data.lpfnOrgWndProc = (WNDPROC)GetWindowLongPtr(LongToPtr(g_winAttachInfo.hTargetWndLong), GWLP_WNDPROC);

    // hook window procedure
    if (!SetWindowLongPtr(LongToPtr(g_winAttachInfo.hTargetWndLong), GWLP_WNDPROC, WndProcHook)) {
      MessageBox(LongToPtr(g_winAttachInfo.hTargetWndLong), TEXT("Could not hook window procedure!"), TEXT("Captain Window Helper"), MB_OK | MB_ICONERROR);
      fprintf(stderr, "SetWindowLongPtr() failed: 0x%08x\n", GetLastError());
    }
  }

  // remove maximize/minimize and resize capabilities from the target window
  SetWindowLong(LongToPtr(g_winAttachInfo.hTargetWndLong), GWL_STYLE, (g_data.lWindowStyle = GetWindowLong(LongToPtr(g_winAttachInfo.hTargetWndLong), GWL_STYLE)) & ~(WS_MAXIMIZEBOX | WS_MINIMIZEBOX | WS_SIZEBOX));
  g_bAttached = TRUE;
}

/// actual entry point
__declspec(dllexport) void WINAPI NativeInjectionEntryPoint(REMOTE_ENTRY_INFO *inRemoteInfo) {
  if (inRemoteInfo && inRemoteInfo->UserData && inRemoteInfo->UserDataSize == sizeof(WINATTACHINFO32)) {
    g_winAttachInfo = *(PWINATTACHINFO32)inRemoteInfo->UserData;
    PerformWindowAttachment();
  }
}

/// DLL entry point
BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved) { return TRUE; }