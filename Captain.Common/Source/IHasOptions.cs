using System.Windows.Forms;

namespace Captain.Common {
  /// <summary>
  ///   Exposes a common interface for objects which contain user-customizable options
  /// </summary>
  public interface IHasOptions<T> {
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    T Options { get; set; }

    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    void DisplayOptionsInterface(IWin32Window ownerWindow);
  }
}
