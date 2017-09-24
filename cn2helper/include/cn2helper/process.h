#pragma once
#include <Windows.h>

/// looks for a specific module in a remote process
__declspec(dllexport) BOOL WINAPI CN2ProcessFindModule(_In_ DWORD dwProcessId, _In_ LPCWSTR szModuleBase);

/// find the parent process ID for the specified process
__declspec(dllexport) DWORD WINAPI CN2ProcessFindParentProcessId(_In_ DWORD dwProcessId);