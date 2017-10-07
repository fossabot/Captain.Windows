#pragma once
#include <Windows.h>

/// <summary>
///   Information sent by Captain to remote process in order to attach the capture UI to certain window
/// </summary>
/// <remarks>
///   Please keep the size of this struct the same between 32- and 64-bit builds for cross-compatibility with structs
///   created by 32- and 64-bit processes
/// </remarks>
typedef struct {
  /// <summary>
  ///   Whether this process has loaded Direct3D libraries
  /// </summary>
  BOOL bD3DPresent;

  /// <summary>
  ///   Grabber window handle (not using HWND for compatibility with 32-bit structs)
  /// </summary>
  UINT32 uiGrabberHandle;

  /// <summary>
  ///   Toolbar window handle (not using HWND for compatibility with 32-bit structs)
  /// </summary>
  UINT32 uiToolbarHandle;

  /// <summary>
  ///   Target window handle (not using HWND for compatibility with 32-bit structs)
  /// </summary>
  UINT32 uiTargetHandle;

  /// <summary>
  ///   Original target window bounds
  /// </summary>
  RECT rcOrgTargetBounds;
} WINATTACHINFO, *PWINATTACHINFO;