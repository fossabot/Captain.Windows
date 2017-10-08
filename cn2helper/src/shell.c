#include <ShlObj.h>
#include <cn2helper/shell.h>

/// <summary>
///   Reveals a path in the File Explorer window
/// </summary>
/// <param name="szPath">Path string</param>
/// <returns>Whether the operation completed successfully</returns>
BOOL WINAPI CN2ShellRevealInFileExplorer(_In_ LPCWSTR szPath) {
  HRESULT hr;
  PIDLIST_ABSOLUTE ppidl = { 0 };

  if (FAILED(hr = SHParseDisplayName(szPath, NULL, &ppidl, 0, NULL))) {
    SetLastError(hr);
    return FALSE;
  }

  // open the folder in file explorer
  if (FAILED(hr = SHOpenFolderAndSelectItems(ppidl, 0, NULL, 0))) {
    SetLastError(hr);
    return FALSE;
  }

  return TRUE;
}