using System;

namespace Captain.Application {
  /// <summary>
  ///   Enumerates action types
  /// </summary>
  [Flags]
  internal enum ActionType {
    /// <summary>
    ///   No type
    /// </summary>
    None,

    /// <summary>
    ///   Static capture
    /// </summary>
    Screenshot,

    /// <summary>
    ///   Video capture
    /// </summary>
    Record
  }
}