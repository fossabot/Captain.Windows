using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   The MONITORINFO structure contains information about a display monitor.
  /// </summary>
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
  internal class MONITORINFO {
    /// <summary>
    ///   The size of the structure, in bytes.
    /// </summary>
    internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));

    /// <summary>
    ///   A set of flags that represent attributes of the display monitor.
    /// </summary>
    internal int dwFlags = 0;

    /// <summary>
    ///   A <see cref="RECT" /> structure that specifies the display monitor rectangle, expressed in virtual-screen
    ///   coordinates.
    /// </summary>
    internal RECT rcMonitor = new RECT();

    /// <summary>
    ///   A <see cref="RECT" /> structure that specifies the work area rectangle of the display monitor, expressed in
    ///   virtual-screen coordinates.
    /// </summary>
    internal RECT rcWork = new RECT();
  }
}