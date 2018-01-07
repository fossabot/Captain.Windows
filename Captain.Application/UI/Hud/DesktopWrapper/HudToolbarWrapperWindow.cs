using Captain.Application.Native;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Wrapper window for toolbar
  /// </summary>
  internal class HudToolbarWrapperWindow : HudBlurredWrapperWindow {
    /// <inheritdoc />
    /// <summary>
    ///   Sets the client area margins accordingly so that we receive the native window shadow
    /// </summary>
    protected override MARGINS Margins { get; } = new MARGINS {
      bottomWidth = -1,
      leftWidth = -1,
      rightWidth = 1,
      topWidth = -1
    };
  }
}