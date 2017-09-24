#include <ShlObj.h>
#include <cn2helper/shell.h>

/// reveals a path in the File Explorer window
BOOL WINAPI CN2ShellRevealInFileExplorer(LPCWSTR szPath) {
  HRESULT hr = 0;
  PIDLIST_ABSOLUTE ppidl = { 0 };

  if (FAILED(hr = SHParseDisplayName(szPath, NULL, &ppidl, 0, NULL))) {
    SetLastError(hr);
    return FALSE;
  }

  // get ID list from the file path
  LPITEMIDLIST lpItemIdList = ILCreateFromPathW(szPath);

  // open the folder in file explorer
  if (FAILED(hr = SHOpenFolderAndSelectItems(ppidl, 0, NULL, 0))) {
    SetLastError(hr);
    return FALSE;
  }

  return TRUE;
}