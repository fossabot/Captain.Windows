#pragma once
#include <Windows.h>

/// reveals a path in the File Explorer window
__declspec(dllexport) BOOL WINAPI CN2ShellRevealInFileExplorer(_In_ LPCWSTR szPath);