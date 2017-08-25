using System;

namespace Captain.Common {
  /// <summary>
  ///   Must be thrown by encoder and output stream implementations that do not support the media they are given.
  ///   For instance, an output stream that streams live video won't support still screenshots; throwing this exception
  ///   will let Captain know the given media is not supported and won't display an error dialog
  /// </summary>
  public class UnsupportedMediaException : Exception { }
}
