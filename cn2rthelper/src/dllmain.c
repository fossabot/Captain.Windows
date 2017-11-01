#include <Windows.h>

/// <summary>
///   Dummy DLL entry point
/// </summary>
/// <param name="hInstance">Module handle. Unused</param>
/// <param name="dwReason">DLL load/unload reason. Unused</param>
/// <param name="lpReserved">Reserved</param>
/// <returns>Always returns <c>TRUE</c></returns>
BOOL WINAPI DllMain(HINSTANCE hInstance, DWORD dwReason, LPVOID lpReserved) { return TRUE; }