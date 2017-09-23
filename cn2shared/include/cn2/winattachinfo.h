#pragma once
#include <Windows.h>

/// NOTE: please make sure the size of this struct remains the same between 32- and 64-bit builds
typedef struct {
  /// grabber window handle (not using HWND for compatibility with 32-bit code)
  UINT32 uiGrabberHandle;

  /// toolbar window handle (not using HWND for compatibility with 32-bit code)
  UINT32 uiToolbarHandle;

  /// target window handle (not using HWND for compatibility with 32-bit code)
  UINT32 uiTargetHandle;

  /// original target window bounds
  RECT rcOrgTargetBounds;
} WINATTACHINFO, *PWINATTACHINFO;