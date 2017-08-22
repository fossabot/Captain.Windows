namespace Captain.Application {
  /// <summary>
  ///   Enumerates the different style variants for the tray icon indicators
  /// </summary>
  internal enum TrayIconVariant : uint {
    /// <summary>
    ///   Classic style (prior to Windows Vista)
    /// </summary>
    Classic = 0,

    /// <summary>
    ///   Aero style (Windows Vista and 7)
    /// </summary>
    Aero = 1,

    /// <summary>
    ///   Alternative Aero style (Windows 8 and 8.1)
    /// </summary>
    Metro = 2,

    /// <summary>
    ///   Modern style (Windows 10 and upwards)
    /// </summary>
    Modern = 3
  }
}