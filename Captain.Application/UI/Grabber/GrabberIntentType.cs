namespace Captain.Application {
  /// <summary>
  ///   Represents a grabber UI intent
  /// </summary>
  internal enum GrabberIntentType {
    /// <summary>
    ///   Close the grabber UI
    /// </summary>
    Close,

    /// <summary>
    ///   Attach the grabber UI to a window
    /// </summary>
    AttachToWindow,

    /// <summary>
    ///   Detach the grabber UI from a window
    /// </summary>
    DetachFromWindow
  }
}
