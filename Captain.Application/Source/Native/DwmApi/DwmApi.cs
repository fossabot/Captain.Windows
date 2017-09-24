// ReSharper disable All

using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Exported functions from the dwmapi.dll Windows library.
  /// </summary>
  internal static class DwmApi {
    /// <summary>
    ///   Retrieves the current value of a specified attribute applied to a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window from which the attribute data is retrieved.</param>
    /// <param name="dwAttribute">
    ///   The attribute to retrieve, specified as a <see cref="DwmWindowAttribute"/> value.
    /// </param>
    /// <param name="pvAttribute">
    ///   A pointer to a value that, when this function returns successfully, receives the current value of the
    ///   attribute. The type of the retrieved value depends on the value of the <c>dwAttribute</c> parameter.
    /// </param>
    /// <param name="cbAttribute">
    ///   The size of the <see cref="DwmWindowAttribute"/> value being retrieved. The size is dependent on the type of
    ///   the <c>pvAttribute</c> parameter.
    /// </param>
    /// <returns>
    ///   If this function succeeds, it returns <c>S_OK</c>. Otherwise, it returns an <c>HRESULT</c> error code.
    /// </returns>
    [DllImport(nameof(DwmApi))]
    internal static extern int DwmGetWindowAttribute(IntPtr hwnd,
                                                     DwmWindowAttribute dwAttribute,
                                                     out RECT pvAttribute,
                                                     int cbAttribute);
  }
}