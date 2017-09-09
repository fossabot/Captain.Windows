#pragma once

#include <Windows.h>

#define WM_CAPTAIN (WM_APP + 0x6A6A)

#define WM_CAPTAINATTACHWINDOW WM_CAPTAIN
#define WM_CAPTAINDETACHWINDOW (WM_CAPTAIN + 1)

#define CAPTAIN_COPYDATA_SIGNATURE 0xDECADE21

typedef struct {
  UINT32 hGrabberWndLong;
  UINT32 hTargetWndLong;

  RECT rcAcceptableBounds;
  RECT rcOrgTargetBounds;
} WINATTACHINFO32, *PWINATTACHINFO32;