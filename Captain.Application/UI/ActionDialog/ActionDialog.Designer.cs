using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  sealed partial class ActionDialog {
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
      System.Windows.Forms.Button closeButton;
      System.Windows.Forms.Panel buttonPaneTopBorder;
      this.buttonPane = new System.Windows.Forms.Panel();
      this.actionControlContainer = new System.Windows.Forms.Panel();
      closeButton = new System.Windows.Forms.Button();
      buttonPaneTopBorder = new System.Windows.Forms.Panel();
      this.buttonPane.SuspendLayout();
      this.SuspendLayout();
      // 
      // closeButton
      // 
      closeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      closeButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      closeButton.Location = new System.Drawing.Point(257, 9);
      closeButton.Name = "closeButton";
      closeButton.Size = new System.Drawing.Size(75, 23);
      closeButton.TabIndex = 3;
      closeButton.Text = "&Close";
      closeButton.UseVisualStyleBackColor = true;
      closeButton.Click += new System.EventHandler(this.OnCloseButtonClicked);
      // 
      // buttonPaneTopBorder
      // 
      buttonPaneTopBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      buttonPaneTopBorder.Dock = System.Windows.Forms.DockStyle.Top;
      buttonPaneTopBorder.Location = new System.Drawing.Point(0, 0);
      buttonPaneTopBorder.Name = "buttonPaneTopBorder";
      buttonPaneTopBorder.Size = new System.Drawing.Size(344, 1);
      buttonPaneTopBorder.TabIndex = 0;
      // 
      // buttonPane
      // 
      this.buttonPane.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      this.buttonPane.Controls.Add(closeButton);
      this.buttonPane.Controls.Add(buttonPaneTopBorder);
      this.buttonPane.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.buttonPane.Location = new System.Drawing.Point(0, 112);
      this.buttonPane.Name = "buttonPane";
      this.buttonPane.Size = new System.Drawing.Size(344, 41);
      this.buttonPane.TabIndex = 2;
      // 
      // actionControlContainer
      // 
      this.actionControlContainer.AutoScroll = true;
      this.actionControlContainer.AutoSize = true;
      this.actionControlContainer.Dock = System.Windows.Forms.DockStyle.Fill;
      this.actionControlContainer.Location = new System.Drawing.Point(0, 1);
      this.actionControlContainer.MaximumSize = new System.Drawing.Size(0, 500);
      this.actionControlContainer.MinimumSize = new System.Drawing.Size(0, 100);
      this.actionControlContainer.Name = "actionControlContainer";
      this.actionControlContainer.Size = new System.Drawing.Size(344, 111);
      this.actionControlContainer.TabIndex = 3;
      this.actionControlContainer.SizeChanged += new System.EventHandler(this.OnActionContainerSizeChanged);
      // 
      // ActionDialog
      // 
      this.AcceptButton = closeButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.AutoSize = true;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = closeButton;
      this.ClientSize = new System.Drawing.Size(344, 153);
      this.Controls.Add(this.actionControlContainer);
      this.Controls.Add(this.buttonPane);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ActionDialog";
      this.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.Text = "Results";
      this.buttonPane.ResumeLayout(false);
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private Panel actionControlContainer;
    private Panel buttonPane;
  }
}