namespace Captain.Common {
  /// <summary>
  ///   Output streams receive the raw data from the encoder in real-time and handle it
  ///   accordingly
  /// </summary>
  public interface IOutputStream {
    /// <summary>
    ///   Method called by the application after the last chunk of data has been written
    ///   to this output stream
    /// </summary>
    /// <returns>Optionally, this method may return information to be displayed on the UI</returns>
    CaptureResult Commit();
  }
}
