﻿namespace Captain.Application {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsWindow));
      this.startupTitleLabel = new System.Windows.Forms.Label();
      this.notificationsTitleLabel = new System.Windows.Forms.Label();
      this.toolBar = new Captain.Application.ToolBarControl();
      this.generalPage = new System.Windows.Forms.TabPage();
      this.radioButton2 = new System.Windows.Forms.RadioButton();
      this.radioButton3 = new System.Windows.Forms.RadioButton();
      this.radioButton1 = new System.Windows.Forms.RadioButton();
      this.label1 = new System.Windows.Forms.Label();
      this.panel2 = new System.Windows.Forms.Panel();
      this.checkBox2 = new System.Windows.Forms.CheckBox();
      this.comboBox1 = new System.Windows.Forms.ComboBox();
      this.checkBox1 = new System.Windows.Forms.CheckBox();
      this.displayTrayIconCheckBox = new System.Windows.Forms.CheckBox();
      this.autoStartCheckBox = new System.Windows.Forms.CheckBox();
      this.panel1 = new System.Windows.Forms.Panel();
      this.streamsPage = new System.Windows.Forms.TabPage();
      this.encodersPage = new System.Windows.Forms.TabPage();
      this.capturePage = new System.Windows.Forms.TabPage();
      this.shortcutsPage = new System.Windows.Forms.TabPage();
      this.helpTip = new System.Windows.Forms.ToolTip(this.components);
      this.toolBar.SuspendLayout();
      this.generalPage.SuspendLayout();
      this.SuspendLayout();
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
      this.toolBar.SelectedIndexChanged += new System.EventHandler(this.OnPageChanged);
      // 
      // generalPage
      // 
      this.generalPage.BackColor = System.Drawing.Color.WhiteSmoke;
      this.generalPage.Controls.Add(this.radioButton2);
      this.generalPage.Controls.Add(this.radioButton3);
      this.generalPage.Controls.Add(this.radioButton1);
      this.generalPage.Controls.Add(this.label1);
      this.generalPage.Controls.Add(this.panel2);
      this.generalPage.Controls.Add(this.checkBox2);
      this.generalPage.Controls.Add(this.comboBox1);
      this.generalPage.Controls.Add(this.checkBox1);
      this.generalPage.Controls.Add(this.notificationsTitleLabel);
      this.generalPage.Controls.Add(this.displayTrayIconCheckBox);
      this.generalPage.Controls.Add(this.startupTitleLabel);
      this.generalPage.Controls.Add(this.autoStartCheckBox);
      this.generalPage.Controls.Add(this.panel1);
      this.generalPage.Location = new System.Drawing.Point(4, 36);
      this.generalPage.Name = "generalPage";
      this.generalPage.Padding = new System.Windows.Forms.Padding(3);
      this.generalPage.Size = new System.Drawing.Size(456, 341);
      this.generalPage.TabIndex = 0;
      this.generalPage.Text = "General";
      // 
      // radioButton2
      // 
      this.radioButton2.AutoSize = true;
      this.radioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton2.Location = new System.Drawing.Point(76, 269);
      this.radioButton2.Name = "radioButton2";
      this.radioButton2.Size = new System.Drawing.Size(304, 20);
      this.radioButton2.TabIndex = 13;
      this.radioButton2.TabStop = true;
      this.radioButton2.Text = "Check for updates but let me choose to install them";
      this.helpTip.SetToolTip(this.radioButton2, resources.GetString("radioButton2.ToolTip"));
      this.radioButton2.UseVisualStyleBackColor = true;
      // 
      // radioButton3
      // 
      this.radioButton3.AutoSize = true;
      this.radioButton3.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton3.Location = new System.Drawing.Point(76, 295);
      this.radioButton3.Name = "radioButton3";
      this.radioButton3.Size = new System.Drawing.Size(157, 20);
      this.radioButton3.TabIndex = 12;
      this.radioButton3.TabStop = true;
      this.radioButton3.Text = "Don\'t check for updates";
      this.helpTip.SetToolTip(this.radioButton3, resources.GetString("radioButton3.ToolTip"));
      this.radioButton3.UseVisualStyleBackColor = true;
      // 
      // radioButton1
      // 
      this.radioButton1.AutoSize = true;
      this.radioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.radioButton1.Location = new System.Drawing.Point(76, 243);
      this.radioButton1.Name = "radioButton1";
      this.radioButton1.Size = new System.Drawing.Size(182, 20);
      this.radioButton1.TabIndex = 10;
      this.radioButton1.TabStop = true;
      this.radioButton1.Text = "Install updates automatically";
      this.helpTip.SetToolTip(this.radioButton1, resources.GetString("radioButton1.ToolTip"));
      this.radioButton1.UseVisualStyleBackColor = true;
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
      this.label1.Location = new System.Drawing.Point(73, 216);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(116, 15);
      this.label1.TabIndex = 7;
      this.label1.Text = "Application updates";
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.Color.Gainsboro;
      this.panel2.Location = new System.Drawing.Point(76, 202);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(304, 1);
      this.panel2.TabIndex = 9;
      // 
      // checkBox2
      // 
      this.checkBox2.AutoSize = true;
      this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox2.Location = new System.Drawing.Point(76, 168);
      this.checkBox2.Name = "checkBox2";
      this.checkBox2.Size = new System.Drawing.Size(199, 20);
      this.checkBox2.TabIndex = 4;
      this.checkBox2.Text = "Use legacy notification p&rovider";
      this.helpTip.SetToolTip(this.checkBox2, "On Windows 8 and later, adaptive toasts display richer notifications.\r\nWhen this " +
        "option is enabled, the application will fall back to traditional notification ti" +
        "ps.");
      this.checkBox2.UseVisualStyleBackColor = true;
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
      this.comboBox1.Location = new System.Drawing.Point(244, 141);
      this.comboBox1.Name = "comboBox1";
      this.comboBox1.Size = new System.Drawing.Size(90, 23);
      this.comboBox1.TabIndex = 3;
      // 
      // checkBox1
      // 
      this.checkBox1.AutoSize = true;
      this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.checkBox1.Location = new System.Drawing.Point(76, 142);
      this.checkBox1.Name = "checkBox1";
      this.checkBox1.Size = new System.Drawing.Size(175, 20);
      this.checkBox1.TabIndex = 2;
      this.checkBox1.Text = "Show desktop &notifications";
      this.checkBox1.UseVisualStyleBackColor = true;
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
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.Gainsboro;
      this.panel1.Location = new System.Drawing.Point(76, 100);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(304, 1);
      this.panel1.TabIndex = 5;
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
    private System.Windows.Forms.Label startupTitleLabel;
    private System.Windows.Forms.Label notificationsTitleLabel;
    private System.Windows.Forms.CheckBox checkBox2;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.RadioButton radioButton3;
    private System.Windows.Forms.RadioButton radioButton1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Panel panel2;
    private System.Windows.Forms.RadioButton radioButton2;
  }
}