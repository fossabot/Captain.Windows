#include "CaptainInjector.h"
#include "CaptainInjectorResult.h"

#include <cstdio>

using namespace Captain::Common;

/// injects code to an existing process
bool CaptainInjector::InjectThreadProc(DWORD dwProcessId, FARPROC fpInjFn, DWORD dwProcLen, LPVOID lpData, SIZE_T cbData, PCAPTAININJECTORRESULT pciRes, BOOL bCopyMem) {
  Logger ^log = Logger::GetDefault();

  /* open process */
  pciRes->hProcess = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_VM_OPERATION | PROCESS_VM_WRITE, false, dwProcessId);
  pciRes->hThread = nullptr;

  if (!pciRes->hProcess) {
    log->WriteLine(LogLevel::Error, "OpenProcess() failed; LE=0x{0:x8}", GetLastError());
    return false;
  }

  /* allocate data */
  if (bCopyMem && lpData && cbData) {
    if (!(pciRes->lpDataAddr = VirtualAllocEx(pciRes->hProcess, nullptr, pciRes->dwDataSz = cbData, MEM_COMMIT, PAGE_READWRITE))) {
      log->WriteLine(LogLevel::Error, "VirtualAllocEx() failed (data); LE=0x{0:x8}", GetLastError());
      goto release;
    }
  }
  else {
    pciRes->dwDataSz = 0;
    pciRes->lpDataAddr = nullptr;
  }

  if (bCopyMem && fpInjFn && dwProcLen) {
    /* allocate code */
    if (!(pciRes->lpCodeAddr = VirtualAllocEx(pciRes->hProcess, nullptr, pciRes->dwCodeSz = dwProcLen, MEM_COMMIT, PAGE_EXECUTE_READWRITE))) {
      log->WriteLine(LogLevel::Error, "VirtualAllocEx() failed (code); LE=0x{0:x8}", GetLastError());
      goto release;
    }
  }
  else {
    pciRes->dwCodeSz = 0;
    pciRes->lpCodeAddr = nullptr;
  }

  SIZE_T szNumberOfBytesWritten = 0;

  /* copy data */
  if (pciRes->lpDataAddr) {
    if (!WriteProcessMemory(pciRes->hProcess, pciRes->lpDataAddr, lpData, cbData, &szNumberOfBytesWritten)) {
      log->WriteLine(LogLevel::Error, "WriteProcessMemory() failed (data); LE=0x{0:x8}", GetLastError());
      goto release;
    }
  }

  /* copy code */
  if (pciRes->lpCodeAddr) {
    if (!WriteProcessMemory(pciRes->hProcess, pciRes->lpCodeAddr, fpInjFn, dwProcLen, &szNumberOfBytesWritten)) {
      log->WriteLine(LogLevel::Error, "WriteProcessMemory() failed (code); LE=0x{0:x8}", GetLastError());
      goto release;
    }
  }

  /* create and execute thread */
  if (!(pciRes->hThread = CreateRemoteThread(pciRes->hProcess, nullptr, 0, (LPTHREAD_START_ROUTINE)(pciRes->lpCodeAddr ? pciRes->lpCodeAddr : fpInjFn),
    pciRes->lpDataAddr ? pciRes->lpDataAddr : lpData, 0, nullptr))) {
    log->WriteLine(LogLevel::Error, "CreateRemoteThread() failed; LE=0x{0:x8}", GetLastError());
    goto release;
  }

  log->WriteLine(LogLevel::Informational, "operation completed successfully");
  return true;

release:
  CaptainInjector::CleanInjectedThreadProc(pciRes);
  return false;
}

/// injects a library to an existing process
bool CaptainInjector::InjectThreadLibrary(DWORD dwProcessId, LPCSTR szLibraryName, PCAPTAININJECTORRESULT pciRes) {
  HMODULE hKernel32 = LoadLibrary("kernel32.dll");
  FARPROC fpLoadLibraryA = GetProcAddress(hKernel32, "LoadLibraryA");

  bool bOk = CaptainInjector::InjectThreadProc(dwProcessId, fpLoadLibraryA, 0, (LPVOID)szLibraryName, strlen(szLibraryName) + 1, pciRes, true);
  FreeLibrary(hKernel32);

  return bOk;
}

/// terminates a remote thread and releases all resources used
bool CaptainInjector::CleanInjectedThreadProc(PCAPTAININJECTORRESULT pciRes) {
  Logger ^log = Logger::GetDefault();
  bool bOk = false;

  log->WriteLine(LogLevel::Verbose, "releasing resources");

  if (pciRes->hProcess) {
    /* free data */
    if (pciRes->lpDataAddr && pciRes->dwDataSz) {
      if (!(bOk = VirtualFreeEx(pciRes->hProcess, pciRes->lpDataAddr, pciRes->dwDataSz, MEM_DECOMMIT))) {
        log->WriteLine(LogLevel::Warning, "VirtualFreeEx() failed (data); LE=0x{0:x8}", GetLastError());
      }
    }

    /* free code */
    if (pciRes->lpCodeAddr && pciRes->dwCodeSz) {
      if (!(bOk = VirtualFreeEx(pciRes->hProcess, pciRes->lpCodeAddr, pciRes->dwCodeSz, MEM_DECOMMIT))) {
        log->WriteLine(LogLevel::Warning, "VirtualFreeEx() failed (code); LE=0x{0:x8}", GetLastError());
      }
    }

    /* close thread handle */
    if (pciRes->hThread) {
      if (!(bOk = CloseHandle(pciRes->hThread))) {
        log->WriteLine(LogLevel::Warning, "CloseHandle(pciRes->hThread) failed; LE=0x{0:x8}", GetLastError());
      }
    }

    /* close process handle */
    if (!(bOk = CloseHandle(pciRes->hProcess))) {
      log->WriteLine(LogLevel::Warning, "CloseHandle(pciRes->hProcess) failed; LE=0x{0:x8}", GetLastError());
    }
  }

  return bOk;
}