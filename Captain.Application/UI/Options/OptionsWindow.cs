using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  /// <summary>
  ///   Displays a user interface for adjusting the application settings and behavior
  /// </summary>
  internal sealed partial class OptionsWindow : Window {
    /// <summary>
    ///   Whether an About window has already been opened before
    /// </summary>
    private static bool isOpen;

    /// <summary>
    ///   Original window title
    /// </summary>
    private readonly string originalTitle;

    /// <summary>
    ///   Class constructor
    /// </summary>
    public OptionsWindow() {
      if (isOpen) { throw new InvalidOperationException("Creating multiple instances of this window is not allowed"); }
      InitializeComponent();

      // set window icon
      Icon = Resources.AppIcon;

      // format and save original window title
      this.originalTitle = Text = String.Format(Text, Application.VersionInfo.ProductName);

      // initial setup
      this.toolBar.SelectedIndex = (int)Application.Options.OptionsDialogTab;
      OnPageChanged(this, new EventArgs());
      OnSizeChanged(new EventArgs());
    }

    /// <summary>
    ///   Window procedure override for handling DWM changes
    /// </summary>
    /// <param name="msg">Window message</param>
    protected override void WndProc(ref Message msg) {
      switch (msg.Msg) {
        case (int)User32.WindowMessage.WM_DWMCOMPOSITIONCHANGED:
        case (int)User32.WindowMessage.WM_DWMCOLORIZATIONCHANGED:
          // update toolbar accent color when colorization/composition changes
          this.toolBar.UpdateAccentColor();
          Invalidate(true);
          break;
      }

      base.WndProc(ref msg);
    }

    /// <summary>
    ///   Triggered when the window size has changed
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnSizeChanged(EventArgs eventArgs) {
      this.toolBar.UpdateItemSize();
      base.OnSizeChanged(eventArgs);
    }

    /// <summary>
    ///   Triggered when the window is first shown
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnShown(EventArgs eventArgs) {
      isOpen = true;
      base.OnShown(eventArgs);
    }

    /// <summary>
    ///   Triggered when the window is closed
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnClosed(EventArgs eventArgs) {
      // save currently  selected tab index
      Application.Options.OptionsDialogTab = (uint)this.toolBar.SelectedIndex;

      isOpen = false;
      base.OnClosed(eventArgs);
    }

    /// <summary>
    ///   Triggered when the selected tab index has changed
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnPageChanged(object sender, EventArgs eventArgs) =>
      Text = this.toolBar.SelectedTab.Text.Replace("&", "") + @" – " + this.originalTitle;

    /// <summary>
    ///   Draws help tool tips
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnHelpTipDraw(object sender, DrawToolTipEventArgs eventArgs) {
      eventArgs.Graphics.Clear(Color.White);
      eventArgs.Graphics.DrawRectangle(Pens.LightGray,
                                       new Rectangle(0, 0, eventArgs.Bounds.Width - 1, eventArgs.Bounds.Height - 1));

      TextRenderer.DrawText(eventArgs.Graphics,
                            eventArgs.ToolTipText.Trim(),
                            Font,
                            Rectangle.Inflate(eventArgs.Bounds, -12, -12),
                            ForeColor,
                            TextFormatFlags.Left);
    }

    /// <summary>
    ///   Triggered before a help tool tip gets renderer
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnHelpTipPopup(object sender, PopupEventArgs eventArgs) {
      Size textSize = TextRenderer.MeasureText(this.helpTip.GetToolTip(eventArgs.AssociatedControl).Trim(), Font);
      eventArgs.ToolTipSize = new Size(textSize.Width + 24, textSize.Height + 24);
    }
  }
}
