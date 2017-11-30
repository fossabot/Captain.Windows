using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   UI for changing JPEG encoder options.
  /// </summary>
  internal partial class JpegWicCodecOptionsWindow : Window {
    /// <summary>
    ///   JPEG encoder options.
    /// </summary>
    internal Dictionary<string, object> Options { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a new instance of this window.
    /// </summary>
    /// <param name="options">JPEG encoding options</param>
    internal JpegWicCodecOptionsWindow(Dictionary<string, object> options) {
      InitializeComponent();
      Options = options;

      this.qualityTrackBar.Value = (int) ((double) Options["Quality"] * 100);
      this.transformComboBox.SelectedIndex = (int) Options["Transform"];
      this.subsamplingOptionComboBox.SelectedIndex = (int) Options["ChromaSubsampling"];
      this.suppressApp0ComboBox.Checked = (bool) Options["NoApp0"];
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
    ///   Triggered when the quality track bar value has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnQualityTrackBarValueChanged(object sender, EventArgs eventArgs) =>
      Options["Quality"] = this.qualityTrackBar.Value / 100.0;

    /// <summary>
    ///   Triggered when the bitmap transform option has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnTransformOptionChanged(object sender, EventArgs eventArgs) =>
      Options["Transform"] = this.transformComboBox.SelectedIndex;

    /// <summary>
    ///   Triggered when the Y'CrCb subsampling option has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnSubsamplingOptionChanged(object sender, EventArgs eventArgs) =>
      Options["ChromaSubsampling"] = (JpegYCrCbSubsamplingOption) this.subsamplingOptionComboBox.SelectedIndex;

    /// <summary>
    ///   Triggered when the App0 suppression option has changed.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="eventArgs">Event arguments.</param>
    private void OnApp0OptionChanged(object sender, EventArgs eventArgs) =>
      Options["NoApp0"] = this.suppressApp0ComboBox.Checked;
  }
}