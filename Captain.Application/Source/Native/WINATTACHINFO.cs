// ReSharper disable All
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  /// <summary>
  ///   Contains information about window attachment requests
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct WINATTACHINFO {
    /// <summary>
    ///   Whether this process has loaded Direct3D libraries or not
    /// </summary>
    internal bool bD3DPresent;

    /// <summary>
    ///   Grabber window handle
    /// </summary>
    internal uint uiGrabberHandle;

    /// <summary>
    ///   Toolbar window handle
    /// </summary>
    internal uint uiToolbarHandle;

    /// <summary>
    ///   Target window handle
    /// </summary>
    internal uint uiTargetHandle;

    /// <summary>
    ///   Original target window bounds
    /// </summary>
    internal RECT rcOrgTargetBounds;
  }
}
