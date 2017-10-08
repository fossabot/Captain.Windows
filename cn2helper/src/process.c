#define PSAPI_VERSION 1  // link to psapi.lib for compatibility with Windows < 7

#include <cn2helper/process.h>
#include <Psapi.h>
#include <TlHelp32.h>

/// <summary>
///   Looks for a specific module in a remote process
/// </summary>
/// <param name="dwProcessId">Process ID</param>
/// <param name="szModuleBase">Module base name</param>
/// <returns><c>TRUE</c> if the process loaded a module which a base name starting with the specified string</returns>
BOOL WINAPI CN2ProcessFindModule(_In_ DWORD dwProcessId, _In_ LPCWSTR szModuleBase) {
  HANDLE hProcess = OpenProcess(PROCESS_QUERY_INFORMATION | PROCESS_VM_READ, FALSE, dwProcessId);
  if (!hProcess) {
    OutputDebugStringW(L"OpenProcess() failed\n");
    return FALSE;
  }

  HMODULE hModules[1024];
  WCHAR szCurModuleBase[MAX_PATH + 1];
  DWORD cbNeeded;

  // enumerate modules loaded by process
  if (EnumProcessModules(hProcess, hModules, sizeof(hModules), &cbNeeded)) {
    for (DWORD dwIdx = 0; dwIdx < cbNeeded / sizeof(HMODULE); dwIdx++) {
      if (GetModuleBaseNameW(hProcess, hModules[dwIdx], szCurModuleBase, sizeof(szCurModuleBase) / sizeof(WCHAR))) {
        // if the module starts with this name - go ahead
        if (wcsstr(szCurModuleBase, szModuleBase) == szCurModuleBase) {
          CloseHandle(hProcess);
          return TRUE;
        }
      }
    }
  }
  else {
    OutputDebugStringW(L"EnumProcessModules() failed\n");
  }

  CloseHandle(hProcess);
  return FALSE;
}

/// <summary>
///   Find the parent process ID for the specified process
/// </summary>
/// <param name="dwProcessId">Process ID</param>
/// <returns>0 on error, otherwise the process ID</returns>
DWORD WINAPI CN2ProcessFindParentProcessId(_In_ DWORD dwProcessId) {
  HANDLE hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, dwProcessId);
  if (hSnapshot == INVALID_HANDLE_VALUE) {
    OutputDebugStringW(L"CreateToolHelp32Snapshot() failed\n");
    return 0;
  }

  PROCESSENTRY32 entry = { 0 };
  entry.dwSize = sizeof(PROCESSENTRY32);

  if (!Process32First(hSnapshot, &entry)) {
    OutputDebugStringW(L"Process32First() failed\n");
    CloseHandle(hSnapshot);
    return 0;
  }

  do {
    if (entry.th32ProcessID == dwProcessId) {
      return entry.th32ParentProcessID;
    }
  } while (Process32Next(hSnapshot, &entry));

  return 0;
}