namespace Captain.Application {
  /// <summary>
  ///   Represents the result of the updater dialog prompt
  /// </summary>
  internal enum UpdaterDialogResult {
    /// <summary>
    ///   Proceeds to download and apply updates
    /// </summary>
    Update,
    /// <summary>
    ///   Remind on next application start
    /// </summary>
    RemindLater,

    /// <summary>
    ///   Skip the latest version
    /// </summary>
    SkipVersion
  }
}
