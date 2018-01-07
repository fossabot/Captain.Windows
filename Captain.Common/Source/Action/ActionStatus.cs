namespace Captain.Common {
  /// <summary>
  ///   Represents the status of an <see cref="Action" />
  /// </summary>
  public enum ActionStatus {
    /// <summary>
    ///   The action failed
    /// </summary>
    Failed,

    /// <summary>
    ///   The action succeeded
    /// </summary>
    Success,

    /// <summary>
    ///   The action is paused
    /// </summary>
    Paused,

    /// <summary>
    ///   The action is being performed
    /// </summary>
    Ongoing
  }
}