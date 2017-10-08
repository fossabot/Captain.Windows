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
      System.Windows.Forms.Label startupTitleLabel;
      System.Windows.Forms.Label notificationsTitleLabel;
      this.toolBar = new Captain.Application.ToolBarControl();
      this.generalPage = new System.Windows.Forms.TabPage();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.displayTrayIconCheckBox = new System.Windows.Forms.CheckBox();
      this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
      this.streamsPage = new System.Windows.Forms.TabPage();
      this.encodersPage = new System.Windows.Forms.TabPage();
      this.capturePage = new System.Windows.Forms.TabPage();
      this.shortcutsPage = new System.Windows.Forms.TabPage();
      this.helpTip = new System.Windows.Forms.ToolTip(this.components);
      startupTitleLabel = new System.Windows.Forms.Label();
      notificationsTitleLabel = new System.Windows.Forms.Label();
      this.toolBar.SuspendLayout();
      this.generalPage.SuspendLayout();
      this.SuspendLayout();
      // 
      // startupTitleLabel
      // 
      startupTitleLabel.AutoSize = true;
      startupTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      startupTitleLabel.Location = new System.Drawing.Point(17, 17);
      startupTitleLabel.Name = "startupTitleLabel";
      startupTitleLabel.Size = new System.Drawing.Size(49, 15);
      startupTitleLabel.TabIndex = 1;
      startupTitleLabel.Text = "Startup";
      // 
      // notificationsTitleLabel
      // 
      notificationsTitleLabel.AutoSize = true;
      notificationsTitleLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      notificationsTitleLabel.Location = new System.Drawing.Point(17, 102);
      notificationsTitleLabel.Name = "notificationsTitleLabel";
      notificationsTitleLabel.Size = new System.Drawing.Size(78, 15);
      notificationsTitleLabel.TabIndex = 3;
      notificationsTitleLabel.Text = "Notifications";
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
      this.toolBar.ItemSize = new System.Drawing.Size(84, 32);
      this.toolBar.Location = new System.Drawing.Point(0, 0);
      this.toolBar.Name = "toolBar";
      this.toolBar.Padding = new System.Drawing.Point(0, 0);
      this.toolBar.SelectedIndex = 0;
      this.toolBar.Size = new System.Drawing.Size(464, 381);
      this.toolBar.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
      this.toolBar.TabIndex = 0;
      this.toolBar.SelectedIndexChanged += new System.EventHandler(this.OnPageChanged);
      // 
      // generalPage
      // 
      this.generalPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.generalPage.Controls.Add(this.comboBox1);
      this.generalPage.Controls.Add(this.checkBox1);
      this.generalPage.Controls.Add(notificationsTitleLabel);
      this.generalPage.Controls.Add(this.displayTrayIconCheckBox);
      this.generalPage.Controls.Add(startupTitleLabel);
      this.generalPage.Controls.Add(this.autoStartCheckBox);
      this.generalPage.Location = new System.Drawing.Point(4, 36);
      this.generalPage.Name = "generalPage";
      this.generalPage.Padding = new System.Windows.Forms.Padding(3);
      this.generalPage.Size = new System.Drawing.Size(456, 341);
      this.generalPage.TabIndex = 0;
      this.generalPage.Text = "General";
      // 
      // comboBox1
      // 
      this.comboBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.comboBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.comboBox1.FormattingEnabled = true;
      this.comboBox1.IntegralHeight = false;
      this.comboBox1.ItemHeight = 15;
      this.comboBox1.Items.AddRange(new object[] {
            "on failure",
            "on success",
            "always",
            "Custom..."});
      this.comboBox1.Location = new System.Drawing.Point(188, 128);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(90, 23);
      this.comboBox1.TabIndex = 3;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox1.Location = new System.Drawing.Point(20, 129);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(175, 20);
      this.checkBox1.TabIndex = 2;
      this.checkBox1.Text = "Show desktop &notifications";
      this.helpTip.SetToolTip(this.checkBox1, "When checked, the application will display an icon in the notification area at al" +
        "l times.\r\nIf disabled, the application can still be accessed by using one of the" +
        " configured keyboard shortcuts.");
      this.checkBox1.UseVisualStyleBackColor = true;
      // 
      // displayTrayIconCheckBox
      // 
      this.displayTrayIconCheckBox.AutoSize = true;
      this.displayTrayIconCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.displayTrayIconCheckBox.Location = new System.Drawing.Point(20, 70);
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
      this.autoStartCheckBox.Location = new System.Drawing.Point(20, 44);
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
    private System.Windows.Forms.ComboBox comboBox1;
    private System.Windows.Forms.CheckBox checkBox1;
  }
}