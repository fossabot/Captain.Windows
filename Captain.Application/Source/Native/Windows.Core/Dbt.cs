// ReSharper disable All

namespace Captain.Application.Native {
  internal static class Dbt {
    /// <summary>
    ///   The system broadcasts the DBT_DEVNODES_CHANGED device event when a device
    ///   has been added to or removed from the system. Applications that maintain list
    ///   of devices in the system should refresh their lists.
    /// </summary>
    internal const int DBT_DEVNODES_CHANGED = 0x07;
  }
}
