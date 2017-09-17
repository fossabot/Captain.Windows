#include <cn2helper/display.h>
#include <dxgi.h>
#include <initguid.h>
#include <Ntddvdeo.h>
#include <dbt.h>

/// gets the acceptable capture region
BOOL CN2DisplayGetAcceptableBounds(_Out_ PRECT prcDest) {
  // initialize destination rectangle
  ZeroMemory(prcDest, sizeof(RECT));
  HRESULT hr;

  /* initialize DXGI */
  IDXGIFactory *pFactory;
  if (FAILED(hr = CreateDXGIFactory(&IID_IDXGIFactory, (void**)&pFactory))) {
    OutputDebugStringW(L"CreateDXGIFactory() failed");
    SetLastError(hr);
    return FALSE;
  }

  // enumerate outputs
  IDXGIAdapter *pAdapter;
  IDXGIOutput *pOutput;

  DXGI_ADAPTER_DESC adapterDesc = { 0 };
  DXGI_OUTPUT_DESC outputDesc = { 0 };
  POINT outputTopLeftEdge;
  RECT outputWindowRect = { 0 };
  HWND outputHwnd;

  for (UINT uiAdapterIdx = 0; pFactory->lpVtbl->EnumAdapters(pFactory, uiAdapterIdx, &pAdapter) != DXGI_ERROR_NOT_FOUND; uiAdapterIdx++, pAdapter->lpVtbl->Release(pAdapter)) {
    pAdapter->lpVtbl->GetDesc(pAdapter, &adapterDesc);

    for (UINT uiOutputIdx = 0; pAdapter->lpVtbl->EnumOutputs(pAdapter, uiOutputIdx, &pOutput) != DXGI_ERROR_NOT_FOUND; uiOutputIdx++, pOutput->lpVtbl->Release(pOutput)) {
      pOutput->lpVtbl->GetDesc(pOutput, &outputDesc);


      if (!outputDesc.AttachedToDesktop) {
        // not attached to desktop - exclude output device
        continue;
      }

      // construct the point that is on the top-left edge of the output device area
      outputTopLeftEdge.x = outputDesc.DesktopCoordinates.left;
      outputTopLeftEdge.y = outputDesc.DesktopCoordinates.top;

      // try to find a window on that position
      if ((outputHwnd = WindowFromPoint(outputTopLeftEdge))) {
        // found window on top-left edge of output device
        // get window bounds
        if (GetWindowRect(outputHwnd, &outputWindowRect)) {
          // compare the window bounds to those of the output device area - if they match the window is on full-screen mode.
          // If this is the case, weIDXGI may not be able to capture that screen region
          if (!memcmp(&outputWindowRect, &outputDesc.DesktopCoordinates, sizeof(RECT))) {
            if (GetWindowLong(outputHwnd, GWL_EXSTYLE) & WS_EX_TOPMOST) {
              // now we are certain this window does not intend to be captured under normal methods!
              continue;
            }
          }
        }
      }

      prcDest->left = min(prcDest->left, outputDesc.DesktopCoordinates.left);
      prcDest->top = min(prcDest->top, outputDesc.DesktopCoordinates.top);
      prcDest->right = max(prcDest->right, outputDesc.DesktopCoordinates.right);
      prcDest->bottom = max(prcDest->bottom, outputDesc.DesktopCoordinates.bottom);
    }
  }

  return TRUE;
}

/// registers display device notifications for the specified window
__declspec(dllexport) BOOL CN2DisplayRegisterChangeNotifications(_In_ HWND hwnd, _Out_ PHDEVNOTIFY phDevNotify) {
  DEV_BROADCAST_DEVICEINTERFACE devBroadcastFilter = { 0 };
  devBroadcastFilter.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
  devBroadcastFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
  devBroadcastFilter.dbcc_classguid = GUID_DEVINTERFACE_MONITOR;

  if (!(*phDevNotify = RegisterDeviceNotification(hwnd, &devBroadcastFilter, DEVICE_NOTIFY_WINDOW_HANDLE))) {
    OutputDebugStringW("RegisterDeviceNotification() failed");
    return FALSE;
  }

  return TRUE;
}