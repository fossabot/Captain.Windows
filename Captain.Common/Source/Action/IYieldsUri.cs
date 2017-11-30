using System;

namespace Captain.Common {
  /// <summary>
  ///   Implemented by an <see cref="Action"/> which yields an URI for the processed capture.
  /// </summary>
  public interface IYieldsUri {
    /// <summary>
    ///   Retrieves the Universal Resource Identifier (URI) for the processed capture.
    /// </summary>
    /// <remarks>
    ///   This method is called by Captain if and only if the <see cref="Action.Status"/> property has
    ///   been set to <see cref="ActionStatus.Success"/>.
    /// </remarks>
    /// <returns>An instance of <see cref="Uri"/>, representing the capture.</returns>
    Uri GetUri();
  }
}
