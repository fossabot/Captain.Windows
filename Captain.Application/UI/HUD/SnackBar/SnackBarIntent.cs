namespace Captain.Application {
  /// <summary>
  ///   Represents a snack bar action intent
  /// </summary>
  internal enum SnackBarIntent {
    /// <summary>
    ///   Take screenshot
    /// </summary>
    Screenshot,

    /// <summary>
    ///   Mute/unmute
    /// </summary>
    ToggleMute,

    /// <summary>
    ///   Record/stop
    /// </summary>
    ToggleRecord,

    /// <summary>
    ///   Displays the options dialog
    /// </summary>
    Options,

    /// <summary>
    ///   Closes the HUD
    /// </summary>
    Close
  }
}