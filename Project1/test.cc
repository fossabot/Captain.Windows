#include <cstdio>

#include <Windows.h>
#include <dxgi.h>
#include <dxgi1_2.h>
#include <d3d11.h>

#include <mfapi.h>
#include <Mfidl.h>
#include <shlwapi.h>
#include <Mfreadwrite.h>

using namespace std;

int main(int argc, char *argv[]) {
  HRESULT hr;

  IDXGIFactory1 *pFactory;
  if (FAILED(hr = CreateDXGIFactory1(__uuidof(IDXGIFactory1), reinterpret_cast<void**>(&pFactory)))) {
    fprintf(stderr, "CreateDXGIFactory() failed; hr=0x%08x\n", hr);
    return 1;
  }

  IDXGIAdapter1 *pAdapter;
  IDXGIOutput1 *pOutput;

  DXGI_ADAPTER_DESC adapterDesc = { 0 };
  DXGI_OUTPUT_DESC outputDesc = { 0 };

  for (UINT uiAdapterIdx = 0; pFactory->EnumAdapters1(uiAdapterIdx, &pAdapter) != DXGI_ERROR_NOT_FOUND;
    uiAdapterIdx++, pAdapter->Release()) {
    pAdapter->GetDesc(&adapterDesc);

    for (UINT uiOutputIdx = 0; pAdapter->EnumOutputs(uiOutputIdx, reinterpret_cast<IDXGIOutput**>(&pOutput)) != DXGI_ERROR_NOT_FOUND;
      uiOutputIdx++, pOutput->Release()) {
      pOutput->GetDesc(&outputDesc);
      printf("[dev %u.%u] %ls [%ux%u] (%u, %u)\n", uiAdapterIdx, uiOutputIdx, outputDesc.DeviceName,
        outputDesc.DesktopCoordinates.right - outputDesc.DesktopCoordinates.left,
        outputDesc.DesktopCoordinates.bottom - outputDesc.DesktopCoordinates.top,
        outputDesc.DesktopCoordinates.left,
        outputDesc.DesktopCoordinates.top);

      ID3D11Device *pDevice;
      if (FAILED(hr = D3D11CreateDevice(pAdapter, D3D_DRIVER_TYPE_UNKNOWN, NULL, D3D10_CREATE_DEVICE_DEBUG, NULL, 0,
        D3D11_SDK_VERSION, &pDevice, NULL, NULL))) {
        fprintf(stderr, "D3D11CreateDevice() failed; hr=0x%08x\n", hr);
        return 1;
      }

      /* initialize COM and MF */
      if (FAILED(hr = CoInitializeEx(NULL, COINIT_APARTMENTTHREADED))) {
        fprintf(stderr, "CoInitializeEx() failed; hr=0x%08x\n", hr);
        return 1;
      }

      if (FAILED(hr = MFStartup(MF_VERSION))) {
        fprintf(stderr, "MFStartup() failed; hr=0x%08x\n", hr);
        CoUninitialize();
        return 1;
      }

      UINT32 uiVideoWidth = abs(outputDesc.DesktopCoordinates.right - outputDesc.DesktopCoordinates.left);
      UINT32 uiVideoHeight = abs(outputDesc.DesktopCoordinates.bottom - outputDesc.DesktopCoordinates.top);
      const UINT32 uiVideoFps = 30;
      const auto uiVideoFrameDuration = 10 * 1000 * 1000 / uiVideoFps;
      const UINT32 uiVideoBitRate = 800000;
      const auto guidVideoEncodingFormat = MFVideoFormat_H264;
      const auto guidVideoInputFormat = MFVideoFormat_RGB32;
      auto uiVideoPixels = uiVideoWidth * uiVideoHeight;
      auto uiVideoFrameCount = 5 * uiVideoFps;

      /* initialize sink writer */
      IMFSinkWriter *pWriter;
      IMFMediaType *pMediaTypeOut = nullptr;
      IMFMediaType *pMediaTypeIn = nullptr;
      DWORD streamIdx;

      // set the output media type
      hr = MFCreateSinkWriterFromURL(L"asdasd.mp4", nullptr, nullptr, &pWriter);
      hr = MFCreateMediaType(&pMediaTypeOut);
      hr = pMediaTypeOut->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
      hr = pMediaTypeOut->SetGUID(MF_MT_SUBTYPE, guidVideoEncodingFormat);
      hr = pMediaTypeOut->SetUINT32(MF_MT_AVG_BITRATE, uiVideoBitRate);
      hr = pMediaTypeOut->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
      hr = MFSetAttributeSize(pMediaTypeOut, MF_MT_FRAME_SIZE, uiVideoWidth, uiVideoHeight);
      hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_FRAME_RATE, uiVideoFps, 1);
      hr = MFSetAttributeRatio(pMediaTypeOut, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
      hr = pWriter->AddStream(pMediaTypeOut, &streamIdx);

      // set the input media type
      hr = MFCreateMediaType(&pMediaTypeIn);
      hr = pMediaTypeIn->SetGUID(MF_MT_MAJOR_TYPE, MFMediaType_Video);
      hr = pMediaTypeIn->SetGUID(MF_MT_SUBTYPE, guidVideoInputFormat);
      hr = pMediaTypeIn->SetUINT32(MF_MT_INTERLACE_MODE, MFVideoInterlace_Progressive);
      hr = MFSetAttributeSize(pMediaTypeIn, MF_MT_FRAME_SIZE, uiVideoWidth, uiVideoHeight);
      hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_FRAME_RATE, uiVideoFps, 1);
      hr = MFSetAttributeRatio(pMediaTypeIn, MF_MT_PIXEL_ASPECT_RATIO, 1, 1);
      hr = pWriter->SetInputMediaType(streamIdx, pMediaTypeIn, nullptr);

      // release resources
      pMediaTypeIn->Release();
      pMediaTypeOut->Release();

      // tell the sink to start accepting data
      hr = pWriter->BeginWriting();

      /* initialize DXGI duplication */
      IDXGIOutputDuplication *pOutputDuplication;
      IDXGIDevice *pDxgiDevice;

      if (FAILED(hr = pDevice->QueryInterface(__uuidof(IDXGIDevice), reinterpret_cast<void**>(&pDxgiDevice)))) {
        fprintf(stderr, "IDXGIDevice::QueryInterface() failed; hr=0x%08x\n", hr);
        return 1;
      }

      if (FAILED(hr = pOutput->DuplicateOutput(pDxgiDevice, &pOutputDuplication))) {
        fprintf(stderr, "IDXGIOutput1::DuplicateOutput() failed; hr=0x%08x\n", hr);
        return 1;
      }

      DXGI_OUTDUPL_DESC outduplDesc;
      pOutputDuplication->GetDesc(&outduplDesc);

      DXGI_OUTDUPL_FRAME_INFO frameinfo;
      IDXGIResource *pDesktopRes = nullptr;

      /* write frame */
      LONGLONG rtStart = 0;

      for (DWORD dwIdx = 0; dwIdx < uiVideoFrameCount; dwIdx++, rtStart += uiVideoFrameDuration) {
        /* capture from screen */
        if (FAILED(hr = pOutputDuplication->AcquireNextFrame(300, &frameinfo, &pDesktopRes))) {
          fprintf(stderr, "IDXGIOutputDuplication::AcquireNextFrame() failed; hr=0x%08x\n", hr);
          return 1;
        }


        HANDLE hSharedHandle;
        if (FAILED(hr = pDesktopRes->GetSharedHandle(&hSharedHandle))) {
          fprintf(stderr, "IDXGIResource::GetSharedHandle() failed; hr=0x%08x\n", hr);
          return 1;
        }

        ID3D11Texture2D *pTexture2D;
        if (FAILED(hr = pDevice->OpenSharedResource(hSharedHandle, __uuidof(ID3D11Texture2D),
          reinterpret_cast<void**>(&pTexture2D)))) {
          fprintf(stderr, "ID3D11Device::OpenSharedResource() failed; hr=0x%08x\n", hr);
          return 1;
        }

        BYTE *lpBits = nullptr;

        ID3D11DeviceContext *pDevCtx = nullptr;
        ID3D11Texture2D *pStagingTexture2D = nullptr;

        if (outduplDesc.DesktopImageInSystemMemory) {
          // already in system memory
          DXGI_MAPPED_RECT rcLockedRect = { 0 };
          if (FAILED(hr = pOutputDuplication->MapDesktopSurface(&rcLockedRect))) {
            fprintf(stderr, "IDXGIOutputDuplication::MapDesktopSurface() failed; hr=0x%08x\n", hr);
            return 1;
          }

          lpBits = rcLockedRect.pBits;
        }
        else {
          // need to map
          pDevice->GetImmediateContext(&pDevCtx);
          D3D11_TEXTURE2D_DESC texdesc;
          D3D11_TEXTURE2D_DESC texdescorg;
          pTexture2D->GetDesc(&texdescorg);
          texdesc.Width = uiVideoWidth;
          texdesc.Height = uiVideoHeight;
          texdesc.MipLevels = 1;
          texdesc.ArraySize = 1;
          texdesc.Format = texdescorg.Format;
          texdesc.SampleDesc = { 1, 0 };
          texdesc.Usage = D3D11_USAGE_STAGING;
          texdesc.BindFlags = 0;
          texdesc.CPUAccessFlags = D3D11_CPU_ACCESS_READ | D3D11_CPU_ACCESS_WRITE;
          texdesc.MiscFlags = 0;

          pDevice->CreateTexture2D(&texdesc, nullptr, &pStagingTexture2D);
          pDevCtx->CopyResource(pStagingTexture2D, pTexture2D);

          D3D11_MAPPED_SUBRESOURCE map;
          if (FAILED(hr = pDevCtx->Map(pStagingTexture2D, 0, D3D11_MAP_READ, 0, &map))) {
            fprintf(stderr, "ID3D11DeviceContext::Map() failed; hr=0x%08x\n", hr);
            return 1;
          }

          lpBits = reinterpret_cast<BYTE*>(map.pData);
        }

        IMFSample *pSample;
        IMFMediaBuffer *pBuffer;

        const LONG cbWidth = 4 * uiVideoWidth;
        const DWORD cbBuffer = cbWidth * uiVideoHeight;

        BYTE *pData = nullptr;

        hr = MFCreateMemoryBuffer(cbBuffer, &pBuffer);
        hr = pBuffer->Lock(&pData, nullptr, nullptr);
        hr = MFCopyImage(pData, cbWidth, lpBits, cbWidth, cbWidth, uiVideoHeight);
        hr = pBuffer->Unlock();
        hr = pBuffer->SetCurrentLength(cbBuffer);
        hr = MFCreateSample(&pSample);
        hr = pSample->AddBuffer(pBuffer);
        hr = pSample->SetSampleTime(rtStart);
        hr = pSample->SetSampleDuration(uiVideoFrameDuration);

        // send the sample to the sink writer
        hr = pWriter->WriteSample(streamIdx, pSample);

        pSample->Release();
        pBuffer->Release();

        pOutputDuplication->UnMapDesktopSurface();

        if (pStagingTexture2D != nullptr) {
          pDevCtx->Unmap(pStagingTexture2D, 0);
          pStagingTexture2D->Release();
          pDevCtx->Release();
        }

        pTexture2D->Release();
      }


      pWriter->Finalize();
      pWriter->Release();

      MFShutdown();

      pDxgiDevice->Release();
      pOutputDuplication->Release();
      pDesktopRes->Release();

      pDevice->Release();
      pOutputDuplication->ReleaseFrame();

      break;
    }
  }

  return 0;
}