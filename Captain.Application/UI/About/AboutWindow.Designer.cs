namespace Captain.Application {
  sealed partial class AboutWindow {
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
      this.logoPictureBox = new System.Windows.Forms.PictureBox();
      this.versionLabel = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.licensingLinkLabel = new Captain.Application.LinkLabel2();
      this.closeButton = new System.Windows.Forms.Button();
      this.supportUriLinkLabel = new Captain.Application.LinkLabel2();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // logoPictureBox
      // 
      this.logoPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
      this.logoPictureBox.Location = new System.Drawing.Point(83, 86);
      this.logoPictureBox.Name = "logoPictureBox";
      this.logoPictureBox.Size = new System.Drawing.Size(140, 36);
      this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.logoPictureBox.TabIndex = 0;
      this.logoPictureBox.TabStop = false;
      // 
      // versionLabel
      // 
      this.versionLabel.Location = new System.Drawing.Point(62, 161);
      this.versionLabel.Name = "versionLabel";
      this.versionLabel.Size = new System.Drawing.Size(180, 15);
      this.versionLabel.TabIndex = 1;
      this.versionLabel.Tag = "Version";
      this.versionLabel.Text = "{0}";
      this.versionLabel.Paint += new System.Windows.Forms.PaintEventHandler(this.OnLabelPaint);
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
      this.panel1.Controls.Add(this.licensingLinkLabel);
      this.panel1.Controls.Add(this.closeButton);
      this.panel1.Location = new System.Drawing.Point(0, 275);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(304, 46);
      this.panel1.TabIndex = 3;
      // 
      // licensingLinkLabel
      // 
      this.licensingLinkLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.licensingLinkLabel.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(103)))), ((int)(((byte)(155)))));
      this.licensingLinkLabel.Location = new System.Drawing.Point(12, 14);
      this.licensingLinkLabel.Name = "licensingLinkLabel";
      this.licensingLinkLabel.RegularColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(103)))), ((int)(((byte)(141)))));
      this.licensingLinkLabel.Size = new System.Drawing.Size(119, 16);
      this.licensingLinkLabel.TabIndex = 5;
      this.licensingLinkLabel.Text = "Open source licenses";
      this.licensingLinkLabel.UseSystemColor = false;
      // 
      // closeButton
      // 
      this.closeButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.closeButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.closeButton.Location = new System.Drawing.Point(217, 11);
      this.closeButton.Name = "closeButton";
      this.closeButton.Size = new System.Drawing.Size(75, 23);
      this.closeButton.TabIndex = 4;
      this.closeButton.Text = "&Close";
      this.closeButton.UseVisualStyleBackColor = true;
      this.closeButton.Click += new System.EventHandler(this.OnCloseButtonClick);
      // 
      // supportUriLinkLabel
      // 
      this.supportUriLinkLabel.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.supportUriLinkLabel.HoverColor = System.Drawing.Color.Empty;
      this.supportUriLinkLabel.Location = new System.Drawing.Point(113, 128);
      this.supportUriLinkLabel.Name = "supportUriLinkLabel";
      this.supportUriLinkLabel.RegularColor = System.Drawing.Color.Empty;
      this.supportUriLinkLabel.Size = new System.Drawing.Size(22, 16);
      this.supportUriLinkLabel.TabIndex = 4;
      this.supportUriLinkLabel.Text = "{0}";
      this.supportUriLinkLabel.Click += new System.EventHandler(this.OnSupportLinkClick);
      // 
      // AboutWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = this.closeButton;
      this.ClientSize = new System.Drawing.Size(304, 321);
      this.Controls.Add(this.supportUriLinkLabel);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.versionLabel);
      this.Controls.Add(this.logoPictureBox);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(320, 360);
      this.Name = "AboutWindow";
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
      this.Text = "About {0}";
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox logoPictureBox;
    private System.Windows.Forms.Label versionLabel;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button closeButton;
    private LinkLabel2 supportUriLinkLabel;
    private LinkLabel2 licensingLinkLabel;
    private System.Windows.Forms.ToolTip toolTip;
  }
}