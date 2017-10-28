#pragma once
#include <Windows.h>

extern "C" {
  /// <summary>
  ///   Reveals a path in the File Explorer window
  /// </summary>
  /// <param name="szPath">Path string</param>
  /// <returns>A value representing the operation status</returns>
  __declspec(dllexport) HRESULT WINAPI CN2ShellRevealInFileExplorer(_In_ LPCWSTR szPath);

  /// <summary>
  ///   Creates an app shortcut in the user's start menu directory
  /// </summary>
  /// <param name="szProductName">Name of the shortcut to be created</param>
  /// <param name="szPath">Executable path for the application to be launched</param>
  /// <param name="szAppId">Application ID</param>
  /// <returns>A value representing the operation result</returns>
  __declspec(dllexport) HRESULT WINAPI CN2InstallAppShortcut(
    _In_ LPCWSTR szProductName,
    _In_ LPCWSTR szPath,
    _In_ LPCWSTR szAppId);
}