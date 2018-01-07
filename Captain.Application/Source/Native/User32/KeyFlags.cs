using System;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Flags for the <see cref="KBDLLHOOKSTRUCT"/> structure
  /// </summary>
  [Flags]
  internal enum KeyFlags {
    /// <summary>
    ///   Manipulates the ALT key flag, which indicates whether the ALT key is pressed
    /// </summary>
    KF_ALTDOWN = 0x2000,
  }
}