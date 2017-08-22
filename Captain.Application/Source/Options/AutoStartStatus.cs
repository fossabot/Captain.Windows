namespace Captain.Application {
  /// <summary>
  ///   Represents auto-start approval status on the StartupApproved registry key
  /// </summary>
  internal enum AutoStartStatus {
    /// <summary>
    ///   Auto-start is enabled for this application
    /// </summary>
    Approved = 0x02,

    /// <summary>
    ///   Auto-start is disabled for this application
    /// </summary>
    Disapproved = 0x03
  }
}