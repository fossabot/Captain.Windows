using System;

// ReSharper disable InconsistentNaming

namespace Captain.Application.Native {
  /// <summary>
  ///   Flags used by the <see cref="DWM_BLURBEHIND"/> structure to indicate which of its members contain valid
  ///   information
  /// </summary>
  [Flags]
  internal enum DwmBbFlags {
    /// <summary>
    ///   A value for the <c>fEnable</c> member has been specified
    /// </summary>
    DWM_BB_ENABLE = 1,

    /// <summary>
    ///   A value for the <c>hRgnBlur</c> member has been specified
    /// </summary>
    DWM_BB_BLURREGION = 2,

    /// <summary>
    ///   A value for the fTransitionOnMaximized member has been specified
    /// </summary>
    DWM_BB_TRANSITIONONMAXIMIZED = 4
  }
}