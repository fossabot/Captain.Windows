namespace Captain.Common {
  /// <summary>
  ///   Capture region types.
  /// </summary>
  public enum RegionType {
    /// <summary>
    ///   Fixed screen region.
    /// </summary>
    Fixed = 0,

    /// <summary>
    ///   Screen in which the mouse is currently positioned.
    /// </summary>
    CurrentScreen = 1,

    /// <summary>
    ///   Captures all the system screens.
    /// </summary>
    AllScreens = 2,

    /// <summary>
    ///   Display an UI for letting the user pick the screen region.
    /// </summary>
    UserSelected = 3
  }
}