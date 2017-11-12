namespace Captain.Common {
  /// <summary>
  ///   Implemented by <see cref="Action" />s to filter the media they are given as input
  /// </summary>
  public interface IFiltered {
    /// <summary>
    ///   Determines whether the specified input media parameters are acceptable for this action.
    /// </summary>
    /// <param name="type">Task type</param>
    /// <param name="codec">Codec instance</param>
    /// <param name="codecParams">Codec parameters</param>
    /// <returns>A boolean value indicating whether the specified media is supported by this action.</returns>
    bool GetMediaAcceptance(TaskType type, ICodecBase codec, ICodecParameters codecParams);
  }
}