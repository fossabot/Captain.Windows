#include <initguid.h>
#include <cn2helper/display.h>
#include <Ntddvdeo.h>
#include <dbt.h>

/// registers display device notifications for the specified window
BOOL WINAPI CN2DisplayRegisterChangeNotifications(_In_ HWND hwnd, _Out_ PHDEVNOTIFY phDevNotify) {
  DEV_BROADCAST_DEVICEINTERFACE devBroadcastFilter = { 0 };
  devBroadcastFilter.dbcc_size = sizeof(DEV_BROADCAST_DEVICEINTERFACE);
  devBroadcastFilter.dbcc_devicetype = DBT_DEVTYP_DEVICEINTERFACE;
  devBroadcastFilter.dbcc_classguid = GUID_DEVINTERFACE_MONITOR;

  if (!((*phDevNotify = RegisterDeviceNotification(hwnd, &devBroadcastFilter, DEVICE_NOTIFY_WINDOW_HANDLE)))) {
    return FALSE;
  }

  return TRUE;
}