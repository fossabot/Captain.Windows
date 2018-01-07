using System;
using System.Runtime.InteropServices;
using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Common wrapper window for HUD components.
  /// </summary>
  internal class HudBlurredWrapperWindow : HudCommonWrapperWindow {
    /// <inheritdoc />
    /// <summary>
    ///   Sets the accent state accordingly so that we get blur effect on Windows 10
    /// </summary>
    protected override User32.AccentState AccentState { get; } = User32.AccentState.ACCENT_ENABLE_BLURBEHIND;

    /// <inheritdoc />
    /// <summary>
    ///   Sets the client area margins accordingly so that we receive the native window shadow
    /// </summary>
    protected override MARGINS Margins { get; } = new MARGINS {
      bottomWidth = 0,
      leftWidth = 0,
      rightWidth = 0,
      topWidth = 1
    };
  }
}