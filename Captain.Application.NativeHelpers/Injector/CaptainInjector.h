#pragma once

#include <Windows.h>

#include "CaptainInjectorResult.h"

/// provides a simple interface for injecting code in remote processes
public class CaptainInjector {
public:
  /// injects code to an existing process
  static bool InjectThreadProc(DWORD dwProcessId, FARPROC fpInjFn, DWORD dwProcLen, LPVOID lpData, SIZE_T cbData, PCAPTAININJECTORRESULT pciRes, BOOL bCopyMem);

  /// injects a library to an existing process
  static bool InjectThreadLibrary(DWORD dwProcessId, LPCSTR szLibraryName, PCAPTAININJECTORRESULT pciRes);

  /// terminates a remote thread and releases all resources used
  static bool CleanInjectedThreadProc(PCAPTAININJECTORRESULT pciRes);
};

