#include <easyhook.h>
#include <Windows.h>

#include <cn2/winattachinfo.h>
#include <cn2rthelper/winattach.h>

/// <summary>
///   Thread entry procedure for the injected DLL
/// </summary>
/// <param name="inRemoteInfo">Information passed by the injector process</param>
__declspec(dllexport) void WINAPI NativeInjectionEntryPoint(REMOTE_ENTRY_INFO *inRemoteInfo) {
  if (inRemoteInfo && inRemoteInfo->UserData && inRemoteInfo->UserDataSize == sizeof(WINATTACHINFO)) {
    // attach window with the given information
    RtAttachWindow((PWINATTACHINFO)inRemoteInfo->UserData);
  }
}
