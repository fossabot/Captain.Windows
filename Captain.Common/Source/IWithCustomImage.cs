using System.Drawing;

namespace Captain.Common {
  /// <summary>
  ///   Specifies a custom image for this plugin object
  /// </summary>
  public interface IWithCustomImage {
    /// <summary>
    ///   Retrieves a custom image to be displayed alongside this plugin
    /// </summary>
    /// <returns>An <see cref="Image"/> instance</returns>
    Image GetCustomImage();
  }
}
