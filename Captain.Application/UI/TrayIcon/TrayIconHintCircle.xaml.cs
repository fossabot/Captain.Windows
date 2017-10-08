using System.Windows.Input;

namespace Captain.Application {
  /// <summary>
  /// Interaction logic for TrayIconHintCircle.xaml
  /// </summary>
  public partial class TrayIconHintCircle {
    /// <summary>
    ///   Class constructor
    /// </summary>
    public TrayIconHintCircle() => InitializeComponent();

    /// <summary>
    ///   Triggered when the mouse moves across the window
    /// </summary>
    /// <param name="sender">Window instance object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnMouseMove(object sender, MouseEventArgs eventArgs) => Close();
  }
}
