using System;
using System.Drawing;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using Ookii.Dialogs.Wpf;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Displays a user interface for adjusting the application settings and behavior
  /// </summary>
  internal sealed partial class OptionsWindow : Window {
    /// <summary>
    ///   Property name for triggering page initialization on layout events
    /// </summary>
    private const string PageInitTriggerProperty = "OptionsPageInitialization";

    /// <summary>
    ///   Whether an About window has already been opened before
    /// </summary>
    private static bool isOpen;

    /// <summary>
    ///   Original window title
    /// </summary>
    private readonly string originalTitle;

    /// <summary>
    ///   Counter for Esc keystrokes.
    /// </summary>
    private int escapeStrokeCount;

    /// <summary>
    ///   Class constructor
    /// </summary>
    public OptionsWindow() {
      if (isOpen) { throw new InvalidOperationException("Creating multiple instances of this window is not allowed"); }
      InitializeComponent();

      // set window icon
      Icon = Resources.AppIcon;

      // format and save original window title
      this.originalTitle = Text = String.Format(Text, VersionInfo.ProductName);

      // initial setup
      this.toolBar.SelectedIndex = (int)Application.Options.OptionsDialogTab;
      OnSelectingPage(this,
                      new TabControlCancelEventArgs(this.toolBar.SelectedTab,
                                                    this.toolBar.SelectedIndex,
                                                    false,
                                                    TabControlAction.Selecting));
      OnSizeChanged(new EventArgs());
    }

    /// <inheritdoc />
    /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.KeyDown" /> event.</summary>
    /// <param name="eventArgs">A <see cref="T:System.Windows.Forms.KeyEventArgs" /> that contains the event data. </param>
    protected override void OnKeyDown(KeyEventArgs eventArgs) {
      if (eventArgs.KeyCode == Keys.Escape) {
        if (++this.escapeStrokeCount == 3) {
          this.escapeStrokeCount = 0;
          Log.WriteLine(LogLevel.Warning, "escape key combo triggered!");

          TaskDialogButton button = new TaskDialog {
            WindowTitle = VersionInfo.ProductName,
            MainIcon = TaskDialogIcon.Warning,
            MainInstruction = Resources.OptionsWindow_RestoreDialogInstruction,
            Content = Resources.OptionsWindow_RestoreDialogContent,
            Buttons = {
              new TaskDialogButton(Resources.OptionsWindow_RestoreDialogOptionsButton) { Default = true },
              new TaskDialogButton(Resources.OptionsWindow_RestoreDialogHardButton),
              new TaskDialogButton(ButtonType.Cancel)
            }
          }.ShowDialog();

          if (button.ButtonType == ButtonType.Custom) {
            // if the default button is pressed, only options will be reset
            Reset(!button.Default);
          }

          eventArgs.SuppressKeyPress = true;
        }
      } else {
        this.escapeStrokeCount = 0;
      }

      base.OnKeyDown(eventArgs);
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

    #region Common window logic

    /// <summary>
    ///   Processes font changes and updates some controls accordingly
    /// </summary>
    /// <param name="eventArgs">Event arguments</param>
    protected override void OnFontChanged(EventArgs eventArgs) {
      this.notificationsTitleLabel.Font =
        this.startupTitleLabel.Font =
          this.updatesTitleLabel.Font = new Font(Font, FontStyle.Bold);
      base.OnFontChanged(eventArgs);
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
    ///   Triggered before the selected tab index changes
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event arguments</param>
    private void OnSelectingPage(object sender, TabControlCancelEventArgs eventArgs) {
      Text = eventArgs.TabPage.Text.Replace("&", "") + @" – " + this.originalTitle;

      if (eventArgs.TabPage.Tag == null) {
        // page not initialized
        eventArgs.TabPage.Tag = true;

        // is this a hack? We abuse the Layout event so we don't have to implement logic for each page
        Log.WriteLine(LogLevel.Debug, $"initializing \"{eventArgs.TabPage.Name}\" page");
        eventArgs.TabPage.PerformLayout(eventArgs.TabPage, PageInitTriggerProperty);
      }
    }

    #region Help tool tip

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

    #endregion

    #endregion

    #region "General" page logic

    /// <summary>
    ///   Initializes the "General" page
    /// </summary>
    /// <param name="sender">If not the owner <see cref="ToolBarControl" />, the event is ignored.</param>
    /// <param name="eventArgs"></param>
    private void OnGeneralPageLayout(object sender, LayoutEventArgs eventArgs) {
      if (eventArgs.AffectedProperty != PageInitTriggerProperty) {
        // no need to initalize the page
        return;
      }

      // auto-start options
      var autoStartManager = new AutoStartManager();

      this.autoStartCheckBox.Enabled = autoStartManager.IsFeatureAvailable;
      this.autoStartCheckBox.Checked = autoStartManager.GetAutoStartPolicy() == AutoStartPolicy.Approved;
      this.autoStartCheckBox.CheckStateChanged += (_, __) =>
        this.autoStartCheckBox.Checked = autoStartManager.ToggleAutoStart(this.autoStartCheckBox.Checked
                                                                            ? AutoStartPolicy.Approved
                                                                            : AutoStartPolicy.Disapproved,
                                                                          (ModifierKeys & Keys.Shift) != 0)
                                                         .Equals(AutoStartPolicy.Approved);

      // tray icon display options
      // TODO: implement this feature - also, make sure the check box is disabled when no hot keys for accessing the
      //       application UI have been set
      this.displayTrayIconCheckBox.Enabled = false;
      this.displayTrayIconCheckBox.Checked = true;

      // notification options
      if (Application.Options.NotificationOptions == NotificationDisplayOptions.Never) {
        this.showNotificationsCheckBox.Checked = this.notificationOptionsComboBox.Enabled = false;
        this.notificationOptionsComboBox.Text = "";
        this.notificationOptionsComboBox.SelectedIndex = -1;
      } else {
        this.showNotificationsCheckBox.Checked = true;
        this.notificationOptionsComboBox.SelectedIndex = (int)Application.Options.NotificationOptions - 1;

        if (this.notificationOptionsComboBox.SelectedIndex == -1) {
          this.notificationOptionsComboBox.SelectedIndex += (int)NotificationDisplayOptions.Always;
        }
      }

      // only show legacy notifications check box if this platform supports any other kind of notification provider
      this.legacyNotificationsCheckBox.Visible = AreToastNotificationsSupported;
      this.legacyNotificationsCheckBox.Checked = Application.Options.UseLegacyNotificationProvider;
      this.legacyNotificationsCheckBox.CheckedChanged += (_, __) => {
        Application.Options.UseLegacyNotificationProvider = this.legacyNotificationsCheckBox.Checked;

        if (Application.Options.UseLegacyNotificationProvider && ToastProvider is ToastNotificationProvider) {
          Log.WriteLine(LogLevel.Informational, "downgrading toast provider to legacy notification provider");
          ToastProvider = new LegacyNotificationProvider();
        } else {
          Log.WriteLine(LogLevel.Informational, "upgrading legacy notification provider to toast provider");
          ToastProvider = new ToastNotificationProvider();
        }
      };

      // application update options
      if (Application.UpdateManager.Availability == UpdaterAvailability.NotSupported) {
        // unsupported (portable mode?)
        this.performInstallNoticeLabel.Text = String.Format(this.performInstallNoticeLabel.Text, VersionInfo.ProductName);
        this.upgradeToFullInstallPanel.Visible = true;
      } else {
        // updates are supported
        this.updateOptionsPanel.Visible = true;

        this.automaticUpdatesRadioButton.Checked = Application.Options.UpdatePolicy == UpdatePolicy.Automatic;
        this.checkUpdatesRadioButton.Checked = Application.Options.UpdatePolicy == UpdatePolicy.CheckOnly;
        this.disableUpdatesRadioButton.Checked = Application.Options.UpdatePolicy == UpdatePolicy.Disabled;

        this.automaticUpdatesRadioButton.CheckedChanged += (_, __) => {
          if (this.automaticUpdatesRadioButton.Checked) { Application.Options.UpdatePolicy = UpdatePolicy.Automatic; }
        };

        this.checkUpdatesRadioButton.CheckedChanged += (_, __) => {
          if (this.checkUpdatesRadioButton.Checked) { Application.Options.UpdatePolicy = UpdatePolicy.CheckOnly; }
        };

        this.disableUpdatesRadioButton.CheckedChanged += (_, __) => {
          if (this.disableUpdatesRadioButton.Checked) { Application.Options.UpdatePolicy = UpdatePolicy.Disabled; }
        };

        // make sure the feature is available
        this.updateManagerUnavailableLabel.Visible = !(this.automaticUpdatesRadioButton.Enabled =
                                                         this.checkUpdatesRadioButton.Enabled =
                                                           this.disableUpdatesRadioButton.Enabled =
                                                             Application.UpdateManager.Availability == UpdaterAvailability.FullyAvailable &&
                                                             Application.UpdateManager.Status == UpdateStatus.Idle);

        // track changes on the update manager
        Application.UpdateManager.OnUpdateStatusChanged += (m, s) => {
          this.updateManagerUnavailableLabel.Visible = !(this.automaticUpdatesRadioButton.Enabled =
                                                           this.checkUpdatesRadioButton.Enabled =
                                                             this.disableUpdatesRadioButton.Enabled =
                                                               s == UpdateStatus.Idle);
        };

        Application.UpdateManager.OnAvailabilityChanged += (m, a) => {
          this.updateManagerUnavailableLabel.Visible = !(this.automaticUpdatesRadioButton.Enabled =
                                                           this.checkUpdatesRadioButton.Enabled =
                                                             this.disableUpdatesRadioButton.Enabled =
                                                               a == UpdaterAvailability.FullyAvailable &&
                                                               m.Status == UpdateStatus.Idle);
        };
      }

    }

    #endregion

    private void generalPage_Click(object sender, EventArgs e) {

    }
  }
}
