using System.Drawing;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a list of saved window positions
  /// </summary>
  public class WindowPositionMap : SerializableDictionary<string, Point> {
    protected override string ItemName => "WindowPosition";
    protected override string KeyName => "Name";
    protected override string ValueName => "Location";
  }
}
