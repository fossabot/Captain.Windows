#pragma once
#include <Windows.h>

/// captures a frame from screen
__declspec(dllexport) BOOL CN2CaptureSingleFrame(_In_ LONG lX, _In_ LONG lY,
  _In_ LONG lCx, _In_ LONG lCy, _Out_ LPVOID lpBuf);