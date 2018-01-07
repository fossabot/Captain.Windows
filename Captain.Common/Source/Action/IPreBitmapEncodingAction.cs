namespace Captain.Common {
  /// <summary>
  ///   <see cref="Action" />s implementing this interface won't have any data written, but instead will receive a
  ///   <see cref="BitmapData" /> structure from which the original bitmap can be safely created.
  /// </summary>
  /// <remarks>
  ///   Only still image captures can be copied using this method. If a video capture is copied to the action stream,
  ///   the data will be encoded and written via the standard <see cref="Action.Write" /> method.
  /// </remarks>
  public interface IPreBitmapEncodingAction {
    /// <summary>
    ///   Sets this action's bitmap data.
    /// </summary>
    /// <param name="data">An instance of <see cref="BitmapData" /> containing capture information.</param>
    void SetBitmapData(BitmapData data);
  }
}