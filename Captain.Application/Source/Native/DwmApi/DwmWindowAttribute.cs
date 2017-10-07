// ReSharper disable All

namespace Captain.Application.Native {
  internal static partial class DwmApi {
    /// <summary>
    ///   Flags used by the DwmGetWindowAttribute and DwmSetWindowAttribute functions to specify window attributes for
    ///   non-client rendering.
    /// </summary>
    internal enum DwmWindowAttribute {
      /// <summary>
      ///   Use with <see cref="DwmApi.DwmGetWindowAttribute"/>. Retrieves the extended frame bounds rectangle in screen
      ///   space. The retrieved value is of type <see cref="RECT"/>.
      /// </summary>
      DWMWA_EXTENDED_FRAME_BOUNDS = 9
    }
  }
}
