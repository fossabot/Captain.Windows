using System;

namespace Captain.Common {
  /// <summary>
  ///   Base interface for codecs
  /// </summary>
  public interface ICodecBase : IDisposable {
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    string FileExtension { get; }
  }
}