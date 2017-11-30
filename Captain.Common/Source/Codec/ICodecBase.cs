namespace Captain.Common {
  /// <summary>
  ///   Base interface for codecs
  /// </summary>
  public interface ICodecBase {
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    string FileExtension { get; }
  }
}