// ReSharper disable All

namespace Captain.Application.Native {
  internal static class WindowMessages {
    /// <summary>
    ///   Base window message number for Captain-specific messages
    /// </summary>
    private const int WM_CAPN = (int)User32.WindowMessage.WM_APP + 0x6A6A;

    /// <summary>
    ///   Sent to a remote process' window to notify re-attachment
    /// </summary>
    internal const int WM_CAPN_ATTACHWND = WM_CAPN;

    /// <summary>
    ///   Sent to a remote process' window to notify detachment
    /// </summary>
    internal const int WM_CAPN_DETACHWND = WM_CAPN + 1;

    /// <summary>
    ///   Signature for WM_COPYDATA messages sent to c2rthelper
    /// </summary>
    internal const uint WM_COPYDATA_CAPNSIG = 0xDECADE21;
  }
}
