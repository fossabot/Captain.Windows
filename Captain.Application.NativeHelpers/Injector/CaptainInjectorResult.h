#pragma once
#include <Windows.h>

/// contains information about the injected code
typedef struct {
  DWORD dwDataSz;
  DWORD dwCodeSz;

  LPVOID lpCodeAddr;
  LPVOID lpDataAddr;

  HANDLE hProcess;
  HANDLE hThread;
} CAPTAININJECTORRESULT, *PCAPTAININJECTORRESULT;