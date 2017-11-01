#include <ShlObj.h>
#include <ShObjIdl.h>
#include <wrl.h>
#include <propvarutil.h>
#include <propkey.h>
#include <cn2helper/shell.h>

using namespace Microsoft::WRL;

/// <summary>
///   Reveals a path in the File Explorer window
/// </summary>
/// <param name="szPath">Path string</param>
/// <returns>Whether the operation completed successfully</returns>
HRESULT WINAPI CN2ShellRevealInFileExplorer(_In_ LPCWSTR szPath) {
  HRESULT hr;
  PIDLIST_ABSOLUTE ppidl = { nullptr };

  if (FAILED(hr = SHParseDisplayName(szPath, NULL, &ppidl, 0, NULL))) {
    return hr;
  }

  // open the folder in file explorer
  if (FAILED(hr = SHOpenFolderAndSelectItems(ppidl, 0, NULL, 0))) {
    return hr;
  }

  return hr;
}

/// <summary>
///   Creates an app shortcut in the user's start menu directory
/// </summary>
/// <param name="szProductName">Name of the shortcut to be created</param>
/// <param name="szPath">Executable path for the application to be launched</param>
/// <param name="szAppId">Application ID</param>
/// <returns>Whether the operation completed succesfully</returns>
HRESULT WINAPI CN2InstallAppShortcut(_In_ LPCWSTR szProductName, _In_ LPCWSTR szPath,
  _In_ LPCWSTR szAppId) {
  WCHAR szShortcutPath[MAX_PATH];
  const DWORD cbShortcutPath = GetEnvironmentVariable(L"APPDATA", szShortcutPath, MAX_PATH);
  HRESULT hr;

  if (SUCCEEDED(hr = cbShortcutPath > 0 ? S_OK : E_INVALIDARG)) {
    // concatenate paths
    errno_t iConcatError = wcscat_s(szShortcutPath, ARRAYSIZE(szShortcutPath),
      L"\\Microsoft\\Windows\\Start Menu\\Programs\\Captain.lnk");

    if (SUCCEEDED(hr = iConcatError == 0 ? S_OK : E_INVALIDARG)) {
      if (GetFileAttributes(szShortcutPath) < 0xFFFFFFF) {
        // file already exists
        return S_OK;
      }

      // create shortcut
      ComPtr<IShellLink> pShellLink;
      if (FAILED(hr = CoInitializeEx(nullptr, COINIT_APARTMENTTHREADED))) { return hr; }
      if (FAILED(hr = CoCreateInstance(CLSID_ShellLink, nullptr, CLSCTX_INPROC_SERVER, IID_PPV_ARGS(&pShellLink)))) {
        goto end;
      }

      if (FAILED(hr = pShellLink->SetPath(szPath))) { goto end; }
      if (FAILED(hr = pShellLink->SetArguments(L"/TASK:DEFAULT"))) { goto end; }

      // TODO: consider setting up a hotkey so the capture UI shows instantly even when the application is not running!
      //if (FAILED(hr = pShellLink->SetHotkey(/* ... */0))) { return hr; }

      ComPtr<IPropertyStore> pPropStore;
      if (FAILED(hr = pShellLink.As(&pPropStore))) { goto end; }

      // initialize app ID
      PROPVARIANT appIdPropVar;
      if (FAILED(hr = InitPropVariantFromString(szAppId, &appIdPropVar))) { goto end; }
      if (FAILED(hr = pPropStore->SetValue(PKEY_AppUserModel_ID, appIdPropVar))) { goto end; }

      // commit changes to property store
      if (FAILED(hr = pPropStore->Commit())) { goto end; }

      // save shortcut
      ComPtr<IPersistFile> pPersistFile;
      if (FAILED(hr = pShellLink.As(&pPersistFile))) { goto end; }
      if (FAILED(hr = pPersistFile->Save(szShortcutPath, TRUE))) { goto end; }
      
      // release property store values
      PropVariantClear(&appIdPropVar);
    }
  }

end:
  CoUninitialize();
  return hr;
}