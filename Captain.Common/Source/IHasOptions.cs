using System.Collections.Generic;
using System.Windows.Forms;

namespace Captain.Common {
  /// <summary>
  ///   Exposes a common interface for objects which contain user-customizable options
  /// </summary>
  public interface IHasOptions {
    /// <summary>
    ///   The options.
    /// </summary>
    Dictionary<string, object> Options { get; set; }

    /// <summary>
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    DialogResult DisplayOptionsInterface(IWin32Window ownerWindow);
  }
}