using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application
{
  partial class JpegWicCodecOptionsWindow
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
      this.qualityTrackBar = new System.Windows.Forms.TrackBar();
      this.label1 = new System.Windows.Forms.Label();
      this.okButton = new System.Windows.Forms.Button();
      this.cancelButton = new System.Windows.Forms.Button();
      this.transformComboBox = new System.Windows.Forms.ComboBox();
      this.label2 = new System.Windows.Forms.Label();
      this.label3 = new System.Windows.Forms.Label();
      this.subsamplingOptionComboBox = new System.Windows.Forms.ComboBox();
      this.suppressApp0ComboBox = new System.Windows.Forms.CheckBox();
      ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).BeginInit();
      this.SuspendLayout();
      // 
      // qualityTrackBar
      // 
      this.qualityTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.qualityTrackBar.Location = new System.Drawing.Point(142, 9);
      this.qualityTrackBar.Maximum = 100;
      this.qualityTrackBar.Name = "qualityTrackBar";
      this.qualityTrackBar.Size = new System.Drawing.Size(204, 45);
      this.qualityTrackBar.TabIndex = 0;
      this.qualityTrackBar.TickStyle = System.Windows.Forms.TickStyle.None;
      this.qualityTrackBar.ValueChanged += new System.EventHandler(this.OnQualityTrackBarValueChanged);
      // 
      // label1
      // 
      this.label1.AutoSize = true;
      this.label1.Location = new System.Drawing.Point(12, 14);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(48, 15);
      this.label1.TabIndex = 1;
      this.label1.Text = "Quality:";
      this.label1.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(190, 140);
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
      this.cancelButton.Location = new System.Drawing.Point(271, 140);
      this.cancelButton.Name = "cancelButton";
      this.cancelButton.Size = new System.Drawing.Size(75, 23);
      this.cancelButton.TabIndex = 3;
      this.cancelButton.Text = "&Cancel";
      this.cancelButton.UseVisualStyleBackColor = true;
      // 
      // transformComboBox
      // 
      this.transformComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.transformComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.transformComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.transformComboBox.FormattingEnabled = true;
      this.transformComboBox.Items.AddRange(new object[] {
            "None",
            "Flip horizontal",
            "Flip vertical",
            "Rotate 180º",
            "Rotate 270º",
            "Rotate 90º"});
      this.transformComboBox.Location = new System.Drawing.Point(142, 40);
      this.transformComboBox.Name = "transformComboBox";
      this.transformComboBox.Size = new System.Drawing.Size(204, 23);
      this.transformComboBox.TabIndex = 4;
      this.transformComboBox.SelectedIndexChanged += new System.EventHandler(this.OnTransformOptionChanged);
      // 
      // label2
      // 
      this.label2.AutoSize = true;
      this.label2.Location = new System.Drawing.Point(12, 43);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(64, 15);
      this.label2.TabIndex = 5;
      this.label2.Text = "Transform:";
      this.label2.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // label3
      // 
      this.label3.AutoSize = true;
      this.label3.Location = new System.Drawing.Point(12, 72);
      this.label3.Name = "label3";
      this.label3.Size = new System.Drawing.Size(124, 15);
      this.label3.TabIndex = 6;
      this.label3.Text = "Chroma subsampling:";
      this.label3.TextAlign = System.Drawing.ContentAlignment.TopCenter;
      // 
      // subsamplingOptionComboBox
      // 
      this.subsamplingOptionComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
      this.subsamplingOptionComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
      this.subsamplingOptionComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.subsamplingOptionComboBox.FormattingEnabled = true;
      this.subsamplingOptionComboBox.Items.AddRange(new object[] {
            "Default",
            "Y\'CbCr 420",
            "Y\'CbCr 422",
            "Y\'CbCr 440",
            "Y\'CbCr 444"});
      this.subsamplingOptionComboBox.Location = new System.Drawing.Point(142, 69);
      this.subsamplingOptionComboBox.Name = "subsamplingOptionComboBox";
      this.subsamplingOptionComboBox.Size = new System.Drawing.Size(204, 23);
      this.subsamplingOptionComboBox.TabIndex = 7;
      this.subsamplingOptionComboBox.SelectedIndexChanged += new System.EventHandler(this.OnSubsamplingOptionChanged);
      // 
      // suppressApp0ComboBox
      // 
      this.suppressApp0ComboBox.AutoSize = true;
      this.suppressApp0ComboBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.suppressApp0ComboBox.Location = new System.Drawing.Point(142, 98);
      this.suppressApp0ComboBox.Name = "suppressApp0ComboBox";
      this.suppressApp0ComboBox.Size = new System.Drawing.Size(159, 20);
      this.suppressApp0ComboBox.TabIndex = 8;
      this.suppressApp0ComboBox.Text = "Suppress APP0 segment";
      this.suppressApp0ComboBox.UseVisualStyleBackColor = true;
      this.suppressApp0ComboBox.CheckedChanged += new System.EventHandler(this.OnApp0OptionChanged);
      // 
      // JpegWicCodecOptionsWindow
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = this.cancelButton;
      this.ClientSize = new System.Drawing.Size(358, 175);
      this.Controls.Add(this.suppressApp0ComboBox);
      this.Controls.Add(this.subsamplingOptionComboBox);
      this.Controls.Add(this.label3);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.transformComboBox);
      this.Controls.Add(this.cancelButton);
      this.Controls.Add(this.okButton);
      this.Controls.Add(this.label1);
      this.Controls.Add(this.qualityTrackBar);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "JpegWicCodecOptionsWindow";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "JPEG Options";
      ((System.ComponentModel.ISupportInitialize)(this.qualityTrackBar)).EndInit();
      this.ResumeLayout(false);
      this.PerformLayout();

    }

    #endregion

    private TrackBar qualityTrackBar;
    private Label label1;
    private Button okButton;
    private Button cancelButton;
    private ComboBox transformComboBox;
    private Label label2;
    private Label label3;
    private ComboBox subsamplingOptionComboBox;
    private CheckBox suppressApp0ComboBox;
  }
}