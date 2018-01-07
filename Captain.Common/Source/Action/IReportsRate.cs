namespace Captain.Common {
  /// <summary>
  ///   Implemented by an <see cref="Action" /> that, during progress, reports a specific byte rate.
  /// </summary>
  public interface IReportsRate {
    /// <summary>
    ///   Calculates the current byte rate for the action progress at the current time.
    /// </summary>
    /// <value>
    ///   The value returned by this method is assumed to represent speed in terms of bytes per second unit.
    /// </value>
    /// <returns>
    ///   The capture handling speed in bytes per second.
    /// </returns>
    /// <remarks>
    ///   For performing this calculations, use the <see cref="Action.Length" /> value.
    /// </remarks>
    uint GetRate();
  }
}