#pragma once
#include <Windows.h>

/// registers display device notifications for the specified window
__declspec(dllexport) BOOL WINAPI CN2DisplayRegisterChangeNotifications(_In_ HWND hwnd, _Out_ PHDEVNOTIFY phDevNotify);