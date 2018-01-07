namespace Captain.Application {
  /// <summary>
  ///   Clipper UI selection modes
  /// </summary>
  internal enum ClippingMode {
    /// <summary>
    ///   Let the user pick a rectangle from the screen
    /// </summary>
    Pick,

    /// <summary>
    ///   Let the user resize and move the currently selected region
    /// </summary>
    Rescale
  }
}