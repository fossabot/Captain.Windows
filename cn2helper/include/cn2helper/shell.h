#pragma once
#include <Windows.h>

/// <summary>
///   Reveals a path in the File Explorer window
/// </summary>
/// <param name="szPath">Path string</param>
/// <returns>Whether the operation completed successfully</returns>
__declspec(dllexport) BOOL WINAPI CN2ShellRevealInFileExplorer(_In_ LPCWSTR szPath);