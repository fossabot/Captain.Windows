using System;

namespace Captain.Application {
  /// <summary>
  ///   OS compatibility helpers
  /// </summary>
  internal static class CompatHelpers {
    /// <summary>
    ///   Whether the OS is at least Windows 10 Anniversary Update
    /// </summary>
    internal static bool HasAnniversaryUpdate => Environment.OSVersion.Version.Major >= 10 &&
                                                 Environment.OSVersion.Version.Build >= 14393;
  }
}