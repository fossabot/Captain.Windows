#include <easyhook.h>
#include <Windows.h>
#include <stdio.h>

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow) {
  int nArgCount = 0;
  LPWSTR *lpArgList = CommandLineToArgvW(GetCommandLineW(), &nArgCount);

  if (nArgCount < 6) {
    fwprintf(stderr, L"Usage: cn2wowbr [PID] [THREAD ID] [MODE] [PATHX86] [PATHX64]\n");
    return ERROR_INVALID_PARAMETER;
  }

  DWORD dwProcessId = wcstoul(lpArgList[1], NULL, 10);
  DWORD dwThreadId = wcstoul(lpArgList[2], NULL, 10);
  ULONG ulInjectionOptions = wcscmp(lpArgList[3], L"stealth")
    ? EASYHOOK_INJECT_DEFAULT
    : EASYHOOK_INJECT_STEALTH;

  // read binary data from standard input
  HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);

  if (hStdin == INVALID_HANDLE_VALUE) {
    return ERROR_INVALID_HANDLE;
  }

  BYTE rgbBuf[0x100];
  DWORD cbBuf = 0;

  if (!ReadFile(hStdin, rgbBuf, sizeof(rgbBuf), &cbBuf, NULL)) {
    return GetLastError();
  }


inject:;
  NTSTATUS status = RhInjectLibrary(dwProcessId, dwThreadId, ulInjectionOptions, lpArgList[4], lpArgList[5], rgbBuf,
    cbBuf);

  if (status != ERROR_SUCCESS) {
    if (ulInjectionOptions == EASYHOOK_INJECT_STEALTH) {
      // retry in default injection mode
      ulInjectionOptions = EASYHOOK_INJECT_DEFAULT;
      goto inject;
    }

    return status;
  }

  return 0;
}