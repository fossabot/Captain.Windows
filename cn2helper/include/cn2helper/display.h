#pragma once
#include <Windows.h>

/// gets the acceptable capture region
__declspec(dllexport) BOOL CN2DisplayGetAcceptableBounds(_Out_ PRECT prcDest);

/// registers display device notifications for the specified window
__declspec(dllexport) BOOL CN2DisplayRegisterChangeNotifications(_In_ HWND hwnd, _Out_ PHDEVNOTIFY phDevNotify);