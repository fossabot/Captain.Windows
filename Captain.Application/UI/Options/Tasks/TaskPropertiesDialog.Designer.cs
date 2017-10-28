using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  sealed partial class TaskPropertiesDialog {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

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
      System.Windows.Forms.Button cancelButton;
      System.Windows.Forms.Label taskNameLabel;
      System.Windows.Forms.Label taskTypeLabel;
      System.Windows.Forms.Label hotkeyLabel;
      System.Windows.Forms.Label notificationOptionsLabel;
      System.Windows.Forms.Label afterCaptureLabel;
      System.Windows.Forms.Label regionLabel;
      System.Windows.Forms.Label encoderLabel;
      System.Windows.Forms.Panel separator2;
      System.Windows.Forms.Panel buttonPane;
      System.Windows.Forms.Panel topButtonPaneBorder;
      this.okButton = new System.Windows.Forms.Button();
      this.taskNameTextBox = new System.Windows.Forms.TextBox();
      this.taskTypeComboBox = new System.Windows.Forms.ComboBox();
      this.hotkeyComboBox = new System.Windows.Forms.TextBox();
      this.notificationPolicyComboBox = new System.Windows.Forms.ComboBox();
      this.actionsPreviewLabel = new System.Windows.Forms.Label();
      this.actionsChangeLinkLabel = new Captain.Application.LinkLabel2();
      this.separator1 = new System.Windows.Forms.Panel();
      this.regionTypeComboBox = new System.Windows.Forms.ComboBox();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.encoderOptionsLinkButton = new Captain.Application.LinkButton();
      this.encoderComboBox = new System.Windows.Forms.ComboBox();
      cancelButton = new System.Windows.Forms.Button();
      taskNameLabel = new System.Windows.Forms.Label();
      taskTypeLabel = new System.Windows.Forms.Label();
      hotkeyLabel = new System.Windows.Forms.Label();
      notificationOptionsLabel = new System.Windows.Forms.Label();
      afterCaptureLabel = new System.Windows.Forms.Label();
      regionLabel = new System.Windows.Forms.Label();
      encoderLabel = new System.Windows.Forms.Label();
      separator2 = new System.Windows.Forms.Panel();
      buttonPane = new System.Windows.Forms.Panel();
      topButtonPaneBorder = new System.Windows.Forms.Panel();
      buttonPane.SuspendLayout();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      cancelButton.Location = new System.Drawing.Point(260, 9);
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new System.Drawing.Size(75, 23);
      cancelButton.TabIndex = 3;
      cancelButton.Text = "&Cancel";
      cancelButton.UseVisualStyleBackColor = true;
      // 
      // taskNameLabel
      // 
      taskNameLabel.Location = new System.Drawing.Point(12, 12);
      taskNameLabel.Name = "taskNameLabel";
      taskNameLabel.Size = new System.Drawing.Size(111, 23);
      taskNameLabel.TabIndex = 4;
      taskNameLabel.Text = "Name:";
      taskNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // taskTypeLabel
      // 
      taskTypeLabel.Location = new System.Drawing.Point(12, 84);
      taskTypeLabel.Name = "taskTypeLabel";
      taskTypeLabel.Size = new System.Drawing.Size(111, 23);
      taskTypeLabel.TabIndex = 6;
      taskTypeLabel.Text = "Type:";
      taskTypeLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // hotkeyLabel
      // 
      hotkeyLabel.Location = new System.Drawing.Point(12, 113);
      hotkeyLabel.Name = "hotkeyLabel";
      hotkeyLabel.Size = new System.Drawing.Size(111, 23);
      hotkeyLabel.TabIndex = 8;
      hotkeyLabel.Text = "Shortcut key:";
      hotkeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // notificationOptionsLabel
      // 
      notificationOptionsLabel.Location = new System.Drawing.Point(12, 142);
      notificationOptionsLabel.Name = "notificationOptionsLabel";
      notificationOptionsLabel.Size = new System.Drawing.Size(111, 23);
      notificationOptionsLabel.TabIndex = 10;
      notificationOptionsLabel.Text = "Show notifications:";
      notificationOptionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // afterCaptureLabel
      // 
      afterCaptureLabel.Location = new System.Drawing.Point(12, 38);
      afterCaptureLabel.Name = "afterCaptureLabel";
      afterCaptureLabel.Size = new System.Drawing.Size(111, 23);
      afterCaptureLabel.TabIndex = 14;
      afterCaptureLabel.Text = "After capture:";
      afterCaptureLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // regionLabel
      // 
      regionLabel.Location = new System.Drawing.Point(12, 190);
      regionLabel.Name = "regionLabel";
      regionLabel.Size = new System.Drawing.Size(111, 23);
      regionLabel.TabIndex = 18;
      regionLabel.Text = "Region:";
      regionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // encoderLabel
      // 
      encoderLabel.Location = new System.Drawing.Point(12, 219);
      encoderLabel.Name = "encoderLabel";
      encoderLabel.Size = new System.Drawing.Size(111, 23);
      encoderLabel.TabIndex = 22;
      encoderLabel.Text = "Format:";
      encoderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // separator2
      // 
      separator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      separator2.Location = new System.Drawing.Point(0, 178);
      separator2.Name = "separator2";
      separator2.Size = new System.Drawing.Size(344, 1);
      separator2.TabIndex = 17;
      // 
      // buttonPane
      // 
      buttonPane.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      buttonPane.Controls.Add(this.okButton);
      buttonPane.Controls.Add(cancelButton);
      buttonPane.Controls.Add(topButtonPaneBorder);
      buttonPane.Dock = System.Windows.Forms.DockStyle.Bottom;
      buttonPane.Location = new System.Drawing.Point(0, 340);
      buttonPane.Name = "buttonPane";
      buttonPane.Size = new System.Drawing.Size(344, 41);
      buttonPane.TabIndex = 3;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Enabled = false;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(179, 9);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      // 
      // topButtonPaneBorder
      // 
      topButtonPaneBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      topButtonPaneBorder.Dock = System.Windows.Forms.DockStyle.Top;
      topButtonPaneBorder.Location = new System.Drawing.Point(0, 0);
      topButtonPaneBorder.Name = "topButtonPaneBorder";
      topButtonPaneBorder.Size = new System.Drawing.Size(344, 1);
      topButtonPaneBorder.TabIndex = 0;
      // 
      // taskNameTextBox
      // 
      this.taskNameTextBox.Location = new System.Drawing.Point(129, 12);
      this.taskNameTextBox.Name = "taskNameTextBox";
      this.taskNameTextBox.Size = new System.Drawing.Size(206, 23);
      this.taskNameTextBox.TabIndex = 5;
      this.taskNameTextBox.TextChanged += new System.EventHandler(this.OnTaskNameChanged);
      // 
      // taskTypeComboBox
      // 
      this.taskTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.taskTypeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.taskTypeComboBox.FormattingEnabled = true;
      this.taskTypeComboBox.Items.AddRange(new object[] {
            "Screenshot",
            "Recording"});
      this.taskTypeComboBox.Location = new System.Drawing.Point(129, 84);
      this.taskTypeComboBox.Name = "taskTypeComboBox";
      this.taskTypeComboBox.Size = new System.Drawing.Size(206, 23);
      this.taskTypeComboBox.TabIndex = 7;
      this.taskTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.OnTaskTypeChanged);
      // 
      // hotkeyComboBox
      // 
      this.hotkeyComboBox.BackColor = System.Drawing.SystemColors.Window;
      this.hotkeyComboBox.Cursor = System.Windows.Forms.Cursors.Arrow;
      this.hotkeyComboBox.Location = new System.Drawing.Point(129, 113);
      this.hotkeyComboBox.Name = "hotkeyComboBox";
      this.hotkeyComboBox.ReadOnly = true;
      this.hotkeyComboBox.Size = new System.Drawing.Size(206, 23);
      this.hotkeyComboBox.TabIndex = 9;
      // 
      // notificationPolicyComboBox
      // 
      this.notificationPolicyComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.notificationPolicyComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.notificationPolicyComboBox.FormattingEnabled = true;
      this.notificationPolicyComboBox.Items.AddRange(new object[] {
            "Never",
            "Only on success",
            "Only on failure",
            "Except progress",
            "Always",
            "Use global setting"});
      this.notificationPolicyComboBox.Location = new System.Drawing.Point(129, 142);
      this.notificationPolicyComboBox.Name = "notificationPolicyComboBox";
      this.notificationPolicyComboBox.Size = new System.Drawing.Size(206, 23);
      this.notificationPolicyComboBox.TabIndex = 11;
      this.notificationPolicyComboBox.SelectedIndexChanged += new System.EventHandler(this.OnNotificationPolicyChanged);
      // 
      // actionsPreviewLabel
      // 
      this.actionsPreviewLabel.AutoEllipsis = true;
      this.actionsPreviewLabel.Location = new System.Drawing.Point(126, 38);
      this.actionsPreviewLabel.Name = "actionsPreviewLabel";
      this.actionsPreviewLabel.Size = new System.Drawing.Size(137, 23);
      this.actionsPreviewLabel.TabIndex = 13;
      this.actionsPreviewLabel.Text = "No actions";
      this.actionsPreviewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // actionsChangeLinkLabel
      // 
      this.actionsChangeLinkLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.actionsChangeLinkLabel.HoverColor = System.Drawing.Color.Empty;
      this.actionsChangeLinkLabel.Location = new System.Drawing.Point(271, 42);
      this.actionsChangeLinkLabel.Name = "actionsChangeLinkLabel";
      this.actionsChangeLinkLabel.RegularColor = System.Drawing.Color.Empty;
      this.actionsChangeLinkLabel.Size = new System.Drawing.Size(58, 16);
      this.actionsChangeLinkLabel.TabIndex = 15;
      this.actionsChangeLinkLabel.Text = "Change...";
      this.actionsChangeLinkLabel.Click += new System.EventHandler(this.OnActionsChangeClicked);
      // 
      // separator1
      // 
      this.separator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      this.separator1.Location = new System.Drawing.Point(0, 71);
      this.separator1.Name = "separator1";
      this.separator1.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
      this.separator1.Size = new System.Drawing.Size(344, 1);
      this.separator1.TabIndex = 16;
      // 
      // regionTypeComboBox
      // 
      this.regionTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.regionTypeComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.regionTypeComboBox.FormattingEnabled = true;
      this.regionTypeComboBox.Items.AddRange(new object[] {
            "Fixed region…",
            "Full screen",
            "Pick manually"});
      this.regionTypeComboBox.Location = new System.Drawing.Point(129, 190);
      this.regionTypeComboBox.Name = "regionTypeComboBox";
      this.regionTypeComboBox.Size = new System.Drawing.Size(206, 23);
      this.regionTypeComboBox.TabIndex = 19;
      this.regionTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.OnRegionTypeChanged);
      // 
      // encoderOptionsLinkButton
      // 
      this.encoderOptionsLinkButton.Enabled = false;
      this.encoderOptionsLinkButton.Image = null;
      this.encoderOptionsLinkButton.Location = new System.Drawing.Point(311, 219);
      this.encoderOptionsLinkButton.Name = "encoderOptionsLinkButton";
      this.encoderOptionsLinkButton.Size = new System.Drawing.Size(24, 24);
      this.encoderOptionsLinkButton.TabIndex = 24;
      this.encoderOptionsLinkButton.TintColor = System.Drawing.Color.Transparent;
      this.toolTip.SetToolTip(this.encoderOptionsLinkButton, "Encoder options…");
      // 
      // encoderComboBox
      // 
      this.encoderComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.encoderComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.encoderComboBox.FormattingEnabled = true;
      this.encoderComboBox.Location = new System.Drawing.Point(129, 219);
      this.encoderComboBox.Name = "encoderComboBox";
      this.encoderComboBox.Size = new System.Drawing.Size(176, 23);
      this.encoderComboBox.TabIndex = 23;
      this.encoderComboBox.SelectedIndexChanged += new System.EventHandler(this.OnEncoderChanged);
      // 
      // TaskPropertiesDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = cancelButton;
      this.ClientSize = new System.Drawing.Size(344, 381);
      this.Controls.Add(this.encoderOptionsLinkButton);
      this.Controls.Add(this.encoderComboBox);
      this.Controls.Add(encoderLabel);
      this.Controls.Add(this.regionTypeComboBox);
      this.Controls.Add(regionLabel);
      this.Controls.Add(separator2);
      this.Controls.Add(this.separator1);
      this.Controls.Add(this.actionsChangeLinkLabel);
      this.Controls.Add(afterCaptureLabel);
      this.Controls.Add(this.notificationPolicyComboBox);
      this.Controls.Add(notificationOptionsLabel);
      this.Controls.Add(this.hotkeyComboBox);
      this.Controls.Add(hotkeyLabel);
      this.Controls.Add(this.taskTypeComboBox);
      this.Controls.Add(taskTypeLabel);
      this.Controls.Add(this.taskNameTextBox);
      this.Controls.Add(taskNameLabel);
      this.Controls.Add(buttonPane);
      this.Controls.Add(this.actionsPreviewLabel);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(360, 420);
      this.Name = "TaskPropertiesDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Task Properties";
      buttonPane.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private Button okButton;
    private TextBox taskNameTextBox;
    private ComboBox taskTypeComboBox;
    private TextBox hotkeyComboBox;
    private ComboBox notificationPolicyComboBox;
    private Label actionsPreviewLabel;
    private LinkLabel2 actionsChangeLinkLabel;
    private Panel separator1;
    private ComboBox regionTypeComboBox;
    private ToolTip toolTip;
    private LinkButton encoderOptionsLinkButton;
    private ComboBox encoderComboBox;
  }
}