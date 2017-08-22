namespace Captain.Application {
  /// <summary>
  ///   Represents an icon kind
  /// </summary>
  internal enum TrayIconClass {
    /// <summary>
    ///   Represents the application icon
    /// </summary>
    Application = 0,

    /// <summary>
    ///   Represents the application icon with a warning badge
    /// </summary>
    Warning = 1,

    /// <summary>
    ///   Represents a determinate progress indicator
    /// </summary>
    DeterminateProgress = 2,

    /// <summary>
    ///   Represents an indeterminate progress indicator
    /// </summary>
    IndeterminateProgress = 3
  }
}