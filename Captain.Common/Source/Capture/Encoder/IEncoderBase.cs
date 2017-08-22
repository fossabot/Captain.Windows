namespace Captain.Common {
  /// <summary>
  ///   Basic encoder capabilities
  /// </summary>
  public interface IEncoderBase {
    /// <summary>
    ///   Get the file extension for captures saved to the file system
    /// </summary>
    /// <returns>A string containing the file extension (i.e. "png" or "mp4")</returns>
    string GetFileExtension();
  }
}
