namespace Captain.Application {
  sealed partial class OptionsWindow {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing) {
      if (disposing && (components != null)) {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      System.Windows.Forms.Panel _separator2;
      System.Windows.Forms.Panel _separator1;
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsWindow));
      this.startupTitleLabel = new System.Windows.Forms.Label();
      this.notificationsTitleLabel = new System.Windows.Forms.Label();
      this.toolBar = new Captain.Application.ToolBarControl();
      this.generalPage = new System.Windows.Forms.TabPage();
      this.updateManagerUnavailableLabel = new System.Windows.Forms.Label();
      this.checkUpdatesRadioButton = new System.Windows.Forms.RadioButton();
      this.disableUpdatesRadioButton = new System.Windows.Forms.RadioButton();
      this.automaticUpdatesRadioButton = new System.Windows.Forms.RadioButton();
      this.updatesTitleLabel = new System.Windows.Forms.Label();
      this.legacyNotificationsCheckBox = new System.Windows.Forms.CheckBox();
      this.notificationOptionsComboBox = new System.Windows.Forms.ComboBox();
      this.showNotificationsCheckBox = new System.Windows.Forms.CheckBox();
      this.displayTrayIconCheckBox = new System.Windows.Forms.CheckBox();
      this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
      this.streamsPage = new System.Windows.Forms.TabPage();
      this.encodersPage = new System.Windows.Forms.TabPage();
      this.capturePage = new System.Windows.Forms.TabPage();
      this.shortcutsPage = new System.Windows.Forms.TabPage();
      this.helpTip = new System.Windows.Forms.ToolTip(this.components);
      _separator2 = new System.Windows.Forms.Panel();
      _separator1 = new System.Windows.Forms.Panel();
      this.toolBar.SuspendLayout();
      this.generalPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // _separator2
      // 
      _separator2.BackColor = System.Drawing.Color.Gainsboro;
      _separator2.Location = new System.Drawing.Point(76, 202);
      _separator2.Name = "_separator2";
      _separator2.Size = new System.Drawing.Size(304, 1);
      _separator2.TabIndex = 9;
      // 
      // _separator1
      // 
      _separator1.BackColor = System.Drawing.Color.Gainsboro;
      _separator1.Location = new System.Drawing.Point(76, 100);
      _separator1.Name = "_separator1";
      _separator1.Size = new System.Drawing.Size(304, 1);
      _separator1.TabIndex = 5;
      // 
      // startupTitleLabel
      // 
      this.startupTitleLabel.AutoSize = true;
      this.startupTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.startupTitleLabel.Location = new System.Drawing.Point(73, 17);
      this.startupTitleLabel.Name = "startupTitleLabel";
      this.startupTitleLabel.Size = new System.Drawing.Size(49, 15);
      this.startupTitleLabel.TabIndex = 1;
      this.startupTitleLabel.Text = "Startup";
      // 
      // notificationsTitleLabel
      // 
      this.notificationsTitleLabel.AutoSize = true;
      this.notificationsTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.notificationsTitleLabel.Location = new System.Drawing.Point(73, 115);
      this.notificationsTitleLabel.Name = "notificationsTitleLabel";
      this.notificationsTitleLabel.Size = new System.Drawing.Size(78, 15);
      this.notificationsTitleLabel.TabIndex = 3;
      this.notificationsTitleLabel.Text = "Notifications";
      // 
      // toolBar
      // 
      this.toolBar.Controls.Add(this.generalPage);
      this.toolBar.Controls.Add(this.streamsPage);
      this.toolBar.Controls.Add(this.encodersPage);
      this.toolBar.Controls.Add(this.capturePage);
      this.toolBar.Controls.Add(this.shortcutsPage);
      this.toolBar.Dock = System.Windows.Forms.DockStyle.Fill;
      this.toolBar.ExtendTabs = true;
      this.toolBar.ItemSize = new System.Drawing.Size(91, 32);
      this.toolBar.Location = new System.Drawing.Point(0, 0);
      this.toolBar.Name = "toolBar";
      this.toolBar.Padding = new System.Drawing.Point(0, 0);
      this.toolBar.SelectedIndex = 0;
      this.toolBar.Size = new System.Drawing.Size(464, 381);
      this.toolBar.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
      this.toolBar.TabIndex = 0;
      this.toolBar.Selecting += new System.Windows.Forms.TabControlCancelEventHandler(this.OnSelectingPage);
      // 
      // generalPage
      // 
      this.generalPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.generalPage.Controls.Add(this.updateManagerUnavailableLabel);
      this.generalPage.Controls.Add(this.checkUpdatesRadioButton);
      this.generalPage.Controls.Add(this.disableUpdatesRadioButton);
      this.generalPage.Controls.Add(this.automaticUpdatesRadioButton);
      this.generalPage.Controls.Add(this.updatesTitleLabel);
      this.generalPage.Controls.Add(_separator2);
      this.generalPage.Controls.Add(this.legacyNotificationsCheckBox);
      this.generalPage.Controls.Add(this.notificationOptionsComboBox);
      this.generalPage.Controls.Add(this.showNotificationsCheckBox);
      this.generalPage.Controls.Add(this.notificationsTitleLabel);
      this.generalPage.Controls.Add(this.displayTrayIconCheckBox);
      this.generalPage.Controls.Add(this.startupTitleLabel);
      this.generalPage.Controls.Add(this.autoStartCheckBox);
      this.generalPage.Controls.Add(_separator1);
      this.generalPage.Location = new System.Drawing.Point(4, 36);
      this.generalPage.Name = "generalPage";
      this.generalPage.Padding = new System.Windows.Forms.Padding(3);
      this.generalPage.Size = new System.Drawing.Size(456, 341);
      this.generalPage.TabIndex = 0;
      this.generalPage.Text = "General";
      this.generalPage.Layout += new System.Windows.Forms.LayoutEventHandler(this.OnGeneralPageLayout);
      // 
      // updateManagerUnavailableLabel
      // 
      this.updateManagerUnavailableLabel.AutoSize = true;
      this.updateManagerUnavailableLabel.ForeColor = System.Drawing.SystemColors.GrayText;
      this.updateManagerUnavailableLabel.Location = new System.Drawing.Point(304, 216);
      this.updateManagerUnavailableLabel.Name = "updateManagerUnavailableLabel";
      this.updateManagerUnavailableLabel.Size = new System.Drawing.Size(76, 15);
      this.updateManagerUnavailableLabel.TabIndex = 14;
      this.updateManagerUnavailableLabel.Text = "Not available";
      this.helpTip.SetToolTip(this.updateManagerUnavailableLabel, "You can\'t change these options right now.\r\nThe update manager is currently unavai" +
        "lable, busy,\r\nor you are running the application in portable mode.");
      this.updateManagerUnavailableLabel.Visible = false;
      // 
      // checkUpdatesRadioButton
      // 
      this.checkUpdatesRadioButton.AutoSize = true;
      this.checkUpdatesRadioButton.Enabled = false;
      this.checkUpdatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkUpdatesRadioButton.Location = new System.Drawing.Point(76, 269);
      this.checkUpdatesRadioButton.Name = "checkUpdatesRadioButton";
      this.checkUpdatesRadioButton.Size = new System.Drawing.Size(304, 20);
      this.checkUpdatesRadioButton.TabIndex = 13;
      this.checkUpdatesRadioButton.TabStop = true;
      this.checkUpdatesRadioButton.Text = "Check for updates but let me choose to install them";
      this.helpTip.SetToolTip(this.checkUpdatesRadioButton, resources.GetString("checkUpdatesRadioButton.ToolTip"));
      this.checkUpdatesRadioButton.UseVisualStyleBackColor = true;
      // 
      // disableUpdatesRadioButton
      // 
      this.disableUpdatesRadioButton.AutoSize = true;
      this.disableUpdatesRadioButton.Enabled = false;
      this.disableUpdatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.disableUpdatesRadioButton.Location = new System.Drawing.Point(76, 295);
      this.disableUpdatesRadioButton.Name = "disableUpdatesRadioButton";
      this.disableUpdatesRadioButton.Size = new System.Drawing.Size(157, 20);
      this.disableUpdatesRadioButton.TabIndex = 12;
      this.disableUpdatesRadioButton.TabStop = true;
      this.disableUpdatesRadioButton.Text = "Don\'t check for updates";
      this.helpTip.SetToolTip(this.disableUpdatesRadioButton, resources.GetString("disableUpdatesRadioButton.ToolTip"));
      this.disableUpdatesRadioButton.UseVisualStyleBackColor = true;
      // 
      // automaticUpdatesRadioButton
      // 
      this.automaticUpdatesRadioButton.AutoSize = true;
      this.automaticUpdatesRadioButton.Enabled = false;
      this.automaticUpdatesRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.automaticUpdatesRadioButton.Location = new System.Drawing.Point(76, 243);
      this.automaticUpdatesRadioButton.Name = "automaticUpdatesRadioButton";
      this.automaticUpdatesRadioButton.Size = new System.Drawing.Size(182, 20);
      this.automaticUpdatesRadioButton.TabIndex = 10;
      this.automaticUpdatesRadioButton.TabStop = true;
      this.automaticUpdatesRadioButton.Text = "Install updates automatically";
      this.helpTip.SetToolTip(this.automaticUpdatesRadioButton, resources.GetString("automaticUpdatesRadioButton.ToolTip"));
      this.automaticUpdatesRadioButton.UseVisualStyleBackColor = true;
      // 
      // updatesTitleLabel
      // 
      this.updatesTitleLabel.AutoSize = true;
      this.updatesTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.updatesTitleLabel.Location = new System.Drawing.Point(73, 216);
      this.updatesTitleLabel.Name = "updatesTitleLabel";
      this.updatesTitleLabel.Size = new System.Drawing.Size(116, 15);
      this.updatesTitleLabel.TabIndex = 7;
      this.updatesTitleLabel.Text = "Application updates";
      // 
      // legacyNotificationsCheckBox
      // 
      this.legacyNotificationsCheckBox.AutoSize = true;
      this.legacyNotificationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.legacyNotificationsCheckBox.Location = new System.Drawing.Point(76, 168);
      this.legacyNotificationsCheckBox.Name = "legacyNotificationsCheckBox";
      this.legacyNotificationsCheckBox.Size = new System.Drawing.Size(199, 20);
      this.legacyNotificationsCheckBox.TabIndex = 4;
      this.legacyNotificationsCheckBox.Text = "Use legacy notification p&rovider";
      this.helpTip.SetToolTip(this.legacyNotificationsCheckBox, "On Windows 8 and later, adaptive toasts display richer notifications.\r\nWhen this " +
        "option is enabled, the application will fall back to traditional notification ti" +
        "ps.");
      this.legacyNotificationsCheckBox.UseVisualStyleBackColor = true;
      this.legacyNotificationsCheckBox.Visible = false;
      // 
      // notificationOptionsComboBox
      // 
      this.notificationOptionsComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.notificationOptionsComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.notificationOptionsComboBox.FormattingEnabled = true;
      this.notificationOptionsComboBox.IntegralHeight = false;
      this.notificationOptionsComboBox.ItemHeight = 15;
      this.notificationOptionsComboBox.Items.AddRange(new object[] {
            "on success",
            "on failure",
            "except for progress",
            "always"});
      this.notificationOptionsComboBox.Location = new System.Drawing.Point(244, 141);
      this.notificationOptionsComboBox.Name = "notificationOptionsComboBox";
      this.notificationOptionsComboBox.Size = new System.Drawing.Size(136, 23);
      this.notificationOptionsComboBox.TabIndex = 3;
      // 
      // showNotificationsCheckBox
      // 
      this.showNotificationsCheckBox.AutoSize = true;
      this.showNotificationsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.showNotificationsCheckBox.Location = new System.Drawing.Point(76, 142);
      this.showNotificationsCheckBox.Name = "showNotificationsCheckBox";
      this.showNotificationsCheckBox.Size = new System.Drawing.Size(175, 20);
      this.showNotificationsCheckBox.TabIndex = 2;
      this.showNotificationsCheckBox.Text = "Show desktop &notifications";
      this.showNotificationsCheckBox.UseVisualStyleBackColor = true;
      // 
      // displayTrayIconCheckBox
      // 
      this.displayTrayIconCheckBox.AutoSize = true;
      this.displayTrayIconCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.displayTrayIconCheckBox.Location = new System.Drawing.Point(76, 70);
      this.displayTrayIconCheckBox.Name = "displayTrayIconCheckBox";
      this.displayTrayIconCheckBox.Size = new System.Drawing.Size(198, 20);
      this.displayTrayIconCheckBox.TabIndex = 1;
      this.displayTrayIconCheckBox.Text = "Display &icon in notification area";
      this.helpTip.SetToolTip(this.displayTrayIconCheckBox, "When checked, the application will display an icon in the notification area at al" +
        "l times.\r\nIf disabled, the application can still be accessed by using one of the" +
        " configured keyboard shortcuts.");
      this.displayTrayIconCheckBox.UseVisualStyleBackColor = true;
      // 
      // autoStartCheckBox
      // 
      this.autoStartCheckBox.AutoSize = true;
      this.autoStartCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.autoStartCheckBox.Location = new System.Drawing.Point(76, 44);
      this.autoStartCheckBox.Name = "autoStartCheckBox";
      this.autoStartCheckBox.Size = new System.Drawing.Size(207, 20);
      this.autoStartCheckBox.TabIndex = 0;
      this.autoStartCheckBox.Text = "Start when you &log into Windows";
      this.autoStartCheckBox.UseVisualStyleBackColor = true;
      // 
      // streamsPage
      // 
      this.streamsPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.streamsPage.Location = new System.Drawing.Point(4, 36);
      this.streamsPage.Name = "streamsPage";
      this.streamsPage.Padding = new System.Windows.Forms.Padding(3);
      this.streamsPage.Size = new System.Drawing.Size(456, 341);
      this.streamsPage.TabIndex = 1;
      this.streamsPage.Text = "Streams";
      // 
      // encodersPage
      // 
      this.encodersPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.encodersPage.Location = new System.Drawing.Point(4, 36);
      this.encodersPage.Name = "encodersPage";
      this.encodersPage.Padding = new System.Windows.Forms.Padding(3);
      this.encodersPage.Size = new System.Drawing.Size(456, 341);
      this.encodersPage.TabIndex = 2;
      this.encodersPage.Text = "Encoders";
      // 
      // capturePage
      // 
      this.capturePage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.capturePage.Location = new System.Drawing.Point(4, 36);
      this.capturePage.Name = "capturePage";
      this.capturePage.Padding = new System.Windows.Forms.Padding(3);
      this.capturePage.Size = new System.Drawing.Size(456, 341);
      this.capturePage.TabIndex = 3;
      this.capturePage.Text = "Capture";
      // 
      // shortcutsPage
      // 
      this.shortcutsPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.shortcutsPage.Location = new System.Drawing.Point(4, 36);
      this.shortcutsPage.Name = "shortcutsPage";
      this.shortcutsPage.Padding = new System.Windows.Forms.Padding(3);
      this.shortcutsPage.Size = new System.Drawing.Size(456, 341);
      this.shortcutsPage.TabIndex = 4;
      this.shortcutsPage.Text = "Shortcuts";
      // 
      // helpTip
      // 
      this.helpTip.AutoPopDelay = 32767;
      this.helpTip.InitialDelay = 500;
      this.helpTip.OwnerDraw = true;
      this.helpTip.ReshowDelay = 100;
      this.helpTip.ShowAlways = true;
      this.helpTip.Draw += new System.Windows.Forms.DrawToolTipEventHandler(this.OnHelpTipDraw);
      this.helpTip.Popup += new System.Windows.Forms.PopupEventHandler(this.OnHelpTipPopup);
      // 
      // OptionsWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.ClientSize = new System.Drawing.Size(464, 381);
      this.Controls.Add(this.toolBar);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.MaximizeBox = false;
      this.MinimumSize = new System.Drawing.Size(480, 420);
      this.Name = "OptionsWindow";
      this.Text = "{0} Options";
      this.toolBar.ResumeLayout(false);
      this.generalPage.ResumeLayout(false);
      this.generalPage.PerformLayout();
      this.ResumeLayout(false);

    }

    #endregion

    private ToolBarControl toolBar;
    private System.Windows.Forms.TabPage generalPage;
    private System.Windows.Forms.TabPage streamsPage;
    private System.Windows.Forms.TabPage encodersPage;
    private System.Windows.Forms.TabPage capturePage;
    private System.Windows.Forms.TabPage shortcutsPage;
    private System.Windows.Forms.CheckBox autoStartCheckBox;
    private System.Windows.Forms.ToolTip helpTip;
    private System.Windows.Forms.CheckBox displayTrayIconCheckBox;
    private System.Windows.Forms.ComboBox notificationOptionsComboBox;
    private System.Windows.Forms.CheckBox showNotificationsCheckBox;
    private System.Windows.Forms.Label startupTitleLabel;
    private System.Windows.Forms.Label notificationsTitleLabel;
    private System.Windows.Forms.CheckBox legacyNotificationsCheckBox;
    private System.Windows.Forms.RadioButton disableUpdatesRadioButton;
    private System.Windows.Forms.RadioButton automaticUpdatesRadioButton;
    private System.Windows.Forms.Label updatesTitleLabel;
    private System.Windows.Forms.RadioButton checkUpdatesRadioButton;
    private System.Windows.Forms.Label updateManagerUnavailableLabel;
  }
}