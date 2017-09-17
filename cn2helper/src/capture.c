#include <cn2helper/capture.h>
#include <dxgi.h>
#include <stdio.h>
#include <dxgi1_2.h>
#include <d3d11.h>

BOOL CN2CaptureSingleFrame(_In_ LONG lX, _In_ LONG lY, _In_ LONG lCx, _In_ LONG lCy, _Out_ HBITMAP hBmp) {
  HRESULT hr;

  /* initialize DXGI */
  IDXGIFactory1 *pFactory;
  if (FAILED(hr = CreateDXGIFactory1(&IID_IDXGIFactory1, (void**)&pFactory))) {
    fprintf(stderr, "CreateDXGIFactory() failed; hr=0x%08x\n", hr);
    SetLastError(hr);
    return FALSE;
  }

  // enumerate outputs and capture the intersecting screen regions
  IDXGIAdapter1 *pAdapter;
  IDXGIOutput1 *pOutput;

  DXGI_ADAPTER_DESC adapterDesc = { 0 };
  DXGI_OUTPUT_DESC outputDesc = { 0 };

  BOOL bOk = FALSE;

  for (UINT uiAdapterIdx = 0; pFactory->lpVtbl->EnumAdapters1(pFactory, uiAdapterIdx, &pAdapter) != DXGI_ERROR_NOT_FOUND; uiAdapterIdx++, pAdapter->lpVtbl->Release(pAdapter)) {
    pAdapter->lpVtbl->GetDesc(pAdapter, &adapterDesc);

    for (UINT uiOutputIdx = 0; pAdapter->lpVtbl->EnumOutputs(pAdapter, uiOutputIdx, (IDXGIOutput**)&pOutput) != DXGI_ERROR_NOT_FOUND; uiOutputIdx++, pOutput->lpVtbl->Release(pOutput)) {
      pOutput->lpVtbl->GetDesc(pOutput, &outputDesc);

      if (lX >= outputDesc.DesktopCoordinates.left && lX + lCx <= outputDesc.DesktopCoordinates.right &&
        lY >= outputDesc.DesktopCoordinates.top && lY + lCy <= outputDesc.DesktopCoordinates.bottom) {
        // intersection found! Render!
        /* create D3D device */
        D3D_FEATURE_LEVEL featureLevel = D3D_FEATURE_LEVEL_11_0;
        D3D_FEATURE_LEVEL featureLevels[] = {
          D3D_FEATURE_LEVEL_11_1,
          D3D_FEATURE_LEVEL_11_0,
          D3D_FEATURE_LEVEL_10_1,
          D3D_FEATURE_LEVEL_10_0,
          D3D_FEATURE_LEVEL_9_3,
          D3D_FEATURE_LEVEL_9_2,
          D3D_FEATURE_LEVEL_9_1
        };

        ID3D11Device *pDev;
        ID3D11DeviceContext *pCtx;
        if (FAILED(hr = D3D11CreateDevice((IDXGIAdapter*)pAdapter, D3D_DRIVER_TYPE_HARDWARE, NULL,
          0, featureLevels, ARRAYSIZE(featureLevels), D3D11_SDK_VERSION, &pDev, &featureLevel, &pCtx))) {
          fprintf(stderr, "D3D11CreateDevice() failed; hr=0x%08x\n", hr);
          SetLastError(hr);
          bOk = FALSE;
          break;
        }

        IDXGIOutputDuplication *pOutputDuplication;
        DXGI_OUTDUPL_FRAME_INFO frameinfo;
        IDXGIResource *pDesktopResource;

        if (FAILED(hr = pOutput->lpVtbl->DuplicateOutput(pOutput, (IUnknown*)pDev, &pOutputDuplication))) {
          fprintf(stderr, "IDXGIOutput1::DuplicateOutput() failed; hr=0x%08x\n", hr);
          SetLastError(hr);
          bOk = FALSE;
          break;
        }

        pOutputDuplication->lpVtbl->AcquireNextFrame(pOutputDuplication, 300, &frameinfo, &pDesktopResource);
      }
    }
  }

  return bOk;
}
