// ReSharper disable All

namespace Captain.Application.Native {
  internal static partial class DwmApi {
    /// <summary>
    ///   Flags used by the DwmGetWindowAttribute and DwmSetWindowAttribute functions to specify window attributes for
    ///   non-client rendering.
    /// </summary>
    internal enum DwmWindowAttribute {
      /// <summary>
      ///   Use with <see cref="DwmSetWindowAttribute" />. Sets the non-client rendering policy. The
      ///   <paramref name="pvAttribute" /> parameter points to a value from the DWMNCRENDERINGPOLICY enumeration.
      /// </summary>
      DWMWA_NCRENDERING_POLICY = 2,

      /// <summary>
      ///   Use with <see cref="DwmGetWindowAttribute" />. Retrieves the extended frame bounds rectangle in screen
      ///   space. The retrieved value is of type <see cref="RECT" />.
      /// </summary>
      DWMWA_EXTENDED_FRAME_BOUNDS = 9
    }
  }
}