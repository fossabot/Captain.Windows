using System;

namespace Captain.Application {
  /// <summary>
  ///   Contains the task region types
  /// </summary>
  [Serializable]
  public enum TaskRegionType {
    /// <summary>
    ///   Capture a fixed region of the virtual desktop
    /// </summary>
    Fixed = 0,

    /// <summary>
    ///   Capture one or more screens
    /// </summary>
    FullScreen = 1,

    /// <summary>
    ///   Select a custom area each time
    /// </summary>
    Grab = 2
  }
}
