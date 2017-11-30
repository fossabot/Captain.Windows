using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   UI for changing PNG encoder options.
  /// </summary>
  internal partial class PngWicCodecOptionsWindow : Window {
    /// <summary>
    ///   PNG encoder options.
    /// </summary>
    internal Dictionary<string, object> Options { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a new instance of this window.
    /// </summary>
    /// <param name="options">PNG encoding options</param>
    internal PngWicCodecOptionsWindow(Dictionary<string, object> options) {
      InitializeComponent();
      Options = options;

      this.filterComboBox.SelectedIndex = Convert.ToInt32(Options["Filter"]);
      this.interlaceOptionCheckBox.Checked = (bool) Options["Interlaced"];
    }

    /// <summary>
    ///   Triggered when a button is clicked.
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnButtonClick(object sender, EventArgs eventArgs) {
      DialogResult = ((Button) sender).DialogResult;
      Close();
    }

    /// <summary>
    ///   Triggered when the filter option has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnFilterOptionChanged(object sender, EventArgs eventArgs) =>
      Options["Filter"] = (PngFilterOption) this.filterComboBox.SelectedIndex;

    /// <summary>
    ///   Triggered when the interlacing mode option has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnInterlaceOptionChanged(object sender, EventArgs eventArgs) =>
      Options["Interlaced"] = this.interlaceOptionCheckBox.Checked;
  }
}