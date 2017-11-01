using System;

namespace Captain.Common {
  /// <summary>
  ///   OS compatibility helpers
  /// </summary>
  public static class CompatHelpers {
    /// <summary>
    ///   Whether the OS is at least Windows 10 Anniversary Update
    /// </summary>
    public static bool HasAnniversaryUpdate => Environment.OSVersion.Version.Major >= 10 &&
                                               Environment.OSVersion.Version.Build >= 14393;
  }
}
