using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  partial class OutputStreamPropertiesDialog {
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
      System.Windows.Forms.Panel buttonPane;
      System.Windows.Forms.Button cancelButton;
      System.Windows.Forms.Panel topButtonPaneBorder;
      this.okButton = new System.Windows.Forms.Button();
      this.addActionLinkButton = new Captain.Application.LinkButton();
      buttonPane = new System.Windows.Forms.Panel();
      cancelButton = new System.Windows.Forms.Button();
      topButtonPaneBorder = new System.Windows.Forms.Panel();
      buttonPane.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonPane
      // 
      buttonPane.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      buttonPane.Controls.Add(this.okButton);
      buttonPane.Controls.Add(cancelButton);
      buttonPane.Controls.Add(topButtonPaneBorder);
      buttonPane.Dock = System.Windows.Forms.DockStyle.Bottom;
      buttonPane.Location = new System.Drawing.Point(0, 220);
      buttonPane.Name = "buttonPane";
      buttonPane.Size = new System.Drawing.Size(284, 41);
      buttonPane.TabIndex = 4;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Enabled = false;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(119, 9);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      cancelButton.Location = new System.Drawing.Point(200, 9);
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new System.Drawing.Size(75, 23);
      cancelButton.TabIndex = 3;
      cancelButton.Text = "&Cancel";
      cancelButton.UseVisualStyleBackColor = true;
      // 
      // topButtonPaneBorder
      // 
      topButtonPaneBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      topButtonPaneBorder.Dock = System.Windows.Forms.DockStyle.Top;
      topButtonPaneBorder.Location = new System.Drawing.Point(0, 0);
      topButtonPaneBorder.Name = "topButtonPaneBorder";
      topButtonPaneBorder.Size = new System.Drawing.Size(284, 1);
      topButtonPaneBorder.TabIndex = 0;
      // 
      // addActionLinkButton
      // 
      this.addActionLinkButton.Image = null;
      this.addActionLinkButton.Location = new System.Drawing.Point(12, 12);
      this.addActionLinkButton.Name = "addActionLinkButton";
      this.addActionLinkButton.Size = new System.Drawing.Size(96, 24);
      this.addActionLinkButton.TabIndex = 5;
      this.addActionLinkButton.Text = "Add action";
      this.addActionLinkButton.TintColor = System.Drawing.Color.SeaGreen;
      // 
      // OutputStreamPropertiesDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.SystemColors.Window;
      this.CancelButton = cancelButton;
      this.ClientSize = new System.Drawing.Size(284, 261);
      this.Controls.Add(this.addActionLinkButton);
      this.Controls.Add(buttonPane);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "OutputStreamPropertiesDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Task Actions";
      buttonPane.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private Button okButton;
    private LinkButton addActionLinkButton;
  }
}