using System.Collections.Generic;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Represents a dialog that lets the user pick one or more output streams and change their configurations
  /// </summary>
  internal sealed partial class OutputStreamPropertiesDialog : Window {
    /// <summary>
    ///   Contains all the output streams alongside their configurations
    /// </summary>
    internal List<SerializableDictionary<string, SerializableDictionary<object, object>>> OutputStreams { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    public OutputStreamPropertiesDialog(
      List<SerializableDictionary<string, SerializableDictionary<object, object>>> outputStreams = null) {
      InitializeComponent();

      this.addActionLinkButton.Image = Resources.TaskAdd;
      OutputStreams = outputStreams ??
                      new List<SerializableDictionary<string, SerializableDictionary<object, object>>>();
    }
  }
}
