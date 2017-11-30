using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application
{
  partial class PngWicCodecOptionsWindow
  {
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
      if (disposing && (components != null))
      {
        components.Dispose();
      }
      base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.filterComboBox = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.interlaceOptionCheckBox = new System.Windows.Forms.CheckBox();
      this.SuspendLayout();
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(59, 79);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 2;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.OnButtonClick);
      // 
      // cancelButton
      // 
      this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      this.cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.cancelButton.Location = new System.Drawing.Point(140, 79);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 3;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // filterComboBox
      // 
      this.filterComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.filterComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.filterComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.filterComboBox.FormattingEnabled = true;
      this.filterComboBox.Items.AddRange(new object[] {
            "Unspecified",
            "None",
            "Sub",
            "Up",
            "Average",
            "Paeth",
            "Adaptive"});
      this.filterComboBox.Location = new System.Drawing.Point(61, 12);
      this.filterComboBox.Name = "filterComboBox";
      this.filterComboBox.Size = new System.Drawing.Size(154, 23);
      this.filterComboBox.TabIndex = 4;
      this.filterComboBox.SelectedIndexChanged += new System.EventHandler(this.OnFilterOptionChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 15);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(36, 15);
      this.label2.TabIndex = 5;
      this.label2.Text = "Filter:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // interlaceOptionCheckBox
      // 
      this.interlaceOptionCheckBox.AutoSize = true;
      this.interlaceOptionCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.interlaceOptionCheckBox.Location = new System.Drawing.Point(61, 41);
      this.interlaceOptionCheckBox.Name = "interlaceOptionCheckBox";
      this.interlaceOptionCheckBox.Size = new System.Drawing.Size(84, 20);
      this.interlaceOptionCheckBox.TabIndex = 8;
      this.interlaceOptionCheckBox.Text = "Interlaced";
      this.interlaceOptionCheckBox.UseVisualStyleBackColor = true;
      this.interlaceOptionCheckBox.CheckedChanged += new System.EventHandler(this.OnInterlaceOptionChanged);
      // 
      // PngWicCodecOptionsWindow
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(227, 114);
      this.Controls.Add(this.interlaceOptionCheckBox);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.filterComboBox);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "PngWicCodecOptionsWindow";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "PNG Options";
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion
    private Button okButton;
    private Button cancelButton;
    private ComboBox filterComboBox;
    private Label label2;
    private CheckBox interlaceOptionCheckBox;
  }
}