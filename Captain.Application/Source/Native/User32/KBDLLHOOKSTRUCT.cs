using System;
using System.Runtime.InteropServices;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Contains information about a low-level mouse input event.
  /// </summary>
  [StructLayout(LayoutKind.Sequential)]
  internal struct KBDLLHOOKSTRUCT {
    /// <summary>
    ///   A virtual-key code. The code must be a value in the range 1 to 254.
    /// </summary>
    internal readonly int vkCode;

    /// <summary>
    ///   A hardware scan code for the key.
    /// </summary>
    private readonly int scanCode;

    /// <summary>
    ///   The event-injected flags.
    /// </summary>
    internal readonly KeyFlags flags;

    /// <summary>
    ///   The time stamp for this message.
    /// </summary>
    private readonly int time;

    /// <summary>
    ///   Additional information associated with the message.
    /// </summary>
    private readonly UIntPtr dwExtraInfo;
  }
}