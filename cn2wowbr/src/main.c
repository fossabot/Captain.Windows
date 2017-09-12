#include <easyhook.h>
#include <Windows.h>
#include <stdio.h>

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nCmdShow) {
  int nArgCount = 0;
  LPWSTR *lpArgList = CommandLineToArgvW(GetCommandLineW(), &nArgCount);

  if (nArgCount < 5) {
    fwprintf(stderr, L"Usage: cn2wowbr [PID] [THREAD ID] [MODE] [PATH]\n");
    return 1;
  }

  DWORD dwProcessId = wcstoul(lpArgList[1], NULL, 10);
  DWORD dwThreadId = wcstoul(lpArgList[2], NULL, 10);
  ULONG ulInjectionOptions = wcscmp(lpArgList[3], L"stealth")
    ? EASYHOOK_INJECT_DEFAULT
    : EASYHOOK_INJECT_STEALTH;

  // read binary data from standard input
  HANDLE hStdin = GetStdHandle(STD_INPUT_HANDLE);

  if (hStdin == INVALID_HANDLE_VALUE) {
    fwprintf(stderr, L"invalid input handle\n");
    return 3;
  }

  BYTE rgbBuf[0x100];
  DWORD cbBuf = 0;

  if (!ReadFile(hStdin, rgbBuf, sizeof(rgbBuf), &cbBuf, NULL)) {
    fwprintf(stderr, L"ReadFile() failed with error 0x%08x\n", GetLastError());
    return 4;
  }


inject:;
  NTSTATUS status = RhInjectLibrary(dwProcessId, dwThreadId, ulInjectionOptions, lpArgList[4], NULL, rgbBuf, cbBuf);

  if (status != ERROR_SUCCESS) {
    fwprintf(stderr, L"RhInjectLibrary() failed with error 0x%08x: %s\n", status, RtlGetLastErrorString());

    if (ulInjectionOptions == EASYHOOK_INJECT_STEALTH) {
      // retry in default injection mode
      ulInjectionOptions = EASYHOOK_INJECT_DEFAULT;
      goto inject;
    }

    return status;
  }

  return 0;
}