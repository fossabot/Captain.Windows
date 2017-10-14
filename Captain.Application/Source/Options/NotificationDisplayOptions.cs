﻿namespace Captain.Application {
  /// <summary>
  ///   Represents notification display options
  /// </summary>
  public enum NotificationDisplayOptions {
    /// <summary>
    ///   Don't ever display notifications
    /// </summary>
    Never = 0,

    /// <summary>
    ///   Displays notifications upon task completion
    /// </summary>
    OnSuccess = 1,

    /// <summary>
    ///   Displays notifications when a task fails
    /// </summary>
    OnFailure = 2,

    /// <summary>
    ///   Displays progress notifications unless they indicate progress
    /// </summary>
    ExceptProgress = 3,

    /// <summary>
    ///   Display all notifications
    /// </summary>
    Always = 4
  }
}
