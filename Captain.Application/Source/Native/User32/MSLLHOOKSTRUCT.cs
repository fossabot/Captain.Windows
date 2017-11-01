using System;
using System.Runtime.InteropServices;

// ReSharper disable All
namespace Captain.Application.Native {
  /// <summary>
  ///   Contains information about a low-level mouse input event.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct MSLLHOOKSTRUCT {
    /// <summary>
    ///   The x- and y-coordinates of the cursor, in per-monitor-aware screen coordinates. 
    /// </summary>
    internal POINT pt;

    /// <summary>
    ///   Event-dependent mouse data.
    /// </summary>
    internal int mouseData;

    /// <summary>
    ///   The event-injected flags.
    /// </summary>
    internal int flags;

    /// <summary>
    ///   The time stamp for this message. 
    /// </summary>
    internal int time;

    /// <summary>
    ///   Additional information associated with the message. 
    /// </summary>
    internal UIntPtr dwExtraInfo;
  }
}