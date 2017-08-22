using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Represents a list of saved window positions
  /// </summary>
  public class WindowPositionMap : SerializableDictionary<string, Point> {
    protected override string ItemName => "WindowPosition";
    protected override string KeyName => "TypeFullName";
    protected override string ValueName => "Location";
  }
}
