#include <easyhook.h>
#include <Windows.h>
#include <stdio.h>

/// <summary>
///   Program entry point
/// </summary>
/// <param name="hInstance">Current module instance</param>
/// <param name="hPrevInstance">Unused, always <c>NULL</c></param>
/// <param name="lpCmdLine">Command line argument string</param>
/// <param name="nCmdShow">Initial window display state</param>
/// <returns>0 on success, otherwise the injection error code</returns>
int WINAPI wWinMain(_In_ HINSTANCE hInstance, _In_opt_ HINSTANCE hPrevInstance, _In_ LPWSTR lpCmdLine,
  _In_ int nCmdShow) {
  // convert command line argument string to array
  int nArgCount = 0;
  LPWSTR *lpArgList = CommandLineToArgvW(GetCommandLineW(), &nArgCount);

  if (nArgCount < 6) {
    fwprintf(stderr, L"Usage: cn2wowbr [PID] [THREAD ID] [MODE] [PATHX86] [PATHX64]\n");
    return ERROR_INVALID_PARAMETER;
  }

  // command line arguments
  DWORD dwProcessId = wcstoul(lpArgList[1], NULL, 10);
  DWORD dwThreadId = wcstoul(lpArgList[2], NULL, 10);
  ULONG ulInjectionOptions = wcscmp(lpArgList[3], L"stealth")
    ? EASYHOOK_INJECT_DEFAULT
    : EASYHOOK_INJECT_STEALTH;

  // pass data from stdin to injection entry point
  const HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);
  if (hStdin == INVALID_HANDLE_VALUE) { return ERROR_INVALID_HANDLE; }

  // allocate data buffer
  BYTE rgbBuf[0x100];
  DWORD cbBuf = 0;

  // read data
  if (!ReadFile(hStdin, rgbBuf, sizeof(rgbBuf), &cbBuf, NULL)) { return GetLastError(); }

inject:;
  const long lStatus = RhInjectLibrary(dwProcessId, dwThreadId, ulInjectionOptions, lpArgList[4], lpArgList[5],
    rgbBuf, cbBuf);

  if (lStatus != ERROR_SUCCESS) {
    if (ulInjectionOptions == EASYHOOK_INJECT_STEALTH) {
      // retry in default injection mode
      ulInjectionOptions = EASYHOOK_INJECT_DEFAULT;
      goto inject;
    }

    return lStatus;
  }

  return 0;
}