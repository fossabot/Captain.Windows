#pragma once

#include <Windows.h>

#define WM_CAPTAIN (WM_APP + 0x6A6A)

#define WM_CAPTAINATTACHWINDOW WM_CAPTAIN
#define WM_CAPTAINDETACHWINDOW (WM_CAPTAIN + 1)

#define CAPTAIN_COPYDATA_SIGNATURE 0xDECADE21

typedef struct {
  /// grabber window handle
  HWND hGrabberWnd;

  /// target window handle
  HWND hTargetWnd;

  /// acceptable limits for target window position
  RECT rcAcceptableBounds;

  /// original target window bounds
  RECT rcOrgTargetBounds;
} WINATTACHINFO, *PWINATTACHINFO;