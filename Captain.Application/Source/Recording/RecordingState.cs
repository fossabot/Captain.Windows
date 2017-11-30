namespace Captain.Application {
  /// <summary>
  ///   Defines the different states for a recording session.
  /// </summary>
  internal enum RecordingState {
    /// <summary>
    ///   Not recording.
    /// </summary>
    None,

    /// <summary>
    ///   Recording.
    /// </summary>
    Recording,

    /// <summary>
    ///   Paused.
    /// </summary>
    Paused
  }
}