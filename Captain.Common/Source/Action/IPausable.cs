namespace Captain.Common {
  /// <summary>
  ///   Implemented by an <see cref="Action" /> that supports being paused while ongoing.
  /// </summary>
  public interface IPausable {
    /// <summary>
    ///   Pauses the action progress
    /// </summary>
    void Pause();

    /// <summary>
    ///   Resumes the action operation.
    /// </summary>
    void Resume();
  }
}