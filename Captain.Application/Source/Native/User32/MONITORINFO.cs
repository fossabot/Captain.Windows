using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 4)]
  public class MONITORINFO {
    internal int cbSize = Marshal.SizeOf(typeof(MONITORINFO));
    internal RECT rcMonitor = new RECT();
    internal RECT rcWork = new RECT();
    internal int dwFlags = 0;
  }
}