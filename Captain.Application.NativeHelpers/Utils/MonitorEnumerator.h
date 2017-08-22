#include <Windows.h>
#include <vector>

using namespace std;

struct MonitorEnumerator {
  vector<RECT> vrcMonitors;

  static BOOL CALLBACK MonitorEnumProc(HMONITOR hMon, HDC hdcMon, LPRECT lprcMon, LPARAM lpData) {
    MonitorEnumerator *pMonEnum = reinterpret_cast<MonitorEnumerator*>(lpData);
    pMonEnum->vrcMonitors.push_back(*lprcMon);

    return true;
  }

  MonitorEnumerator() { EnumDisplayMonitors(nullptr, nullptr, MonitorEnumProc, (LPARAM)this); }
};