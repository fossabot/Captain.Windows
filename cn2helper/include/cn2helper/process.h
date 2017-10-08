#pragma once
#include <Windows.h>

/// <summary>
///   Looks for a specific module in a remote process
/// </summary>
/// <param name="dwProcessId">Process ID</param>
/// <param name="szModuleBase">Module base name</param>
/// <returns><c>TRUE</c> if the process loaded a module which a base name starting with the specified string</returns>
__declspec(dllexport) BOOL WINAPI CN2ProcessFindModule(_In_ DWORD dwProcessId, _In_ LPCWSTR szModuleBase);

/// <summary>
///   Find the parent process ID for the specified process
/// </summary>
/// <param name="dwProcessId">Process ID</param>
/// <returns>0 on error, otherwise the process ID</returns>
__declspec(dllexport) DWORD WINAPI CN2ProcessFindParentProcessId(_In_ DWORD dwProcessId);