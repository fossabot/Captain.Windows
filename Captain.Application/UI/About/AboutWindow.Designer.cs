namespace Captain.Application {
  partial class AboutWindow {
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
      this.logoPictureBox = new System.Windows.Forms.PictureBox();
      this.label1 = new System.Windows.Forms.Label();
      this.label2 = new System.Windows.Forms.Label();
      this.panel1 = new System.Windows.Forms.Panel();
      this.closeButton = new System.Windows.Forms.Button();
      this.linkLabel21 = new Captain.Application.LinkLabel2();
      this.linkLabel22 = new Captain.Application.LinkLabel2();
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
      // label1
      // 
      this.label1.Location = new System.Drawing.Point(62, 161);
      this.label1.Name = "label1";
      this.label1.Size = new System.Drawing.Size(180, 15);
      this.label1.TabIndex = 1;
      this.label1.Tag = "Version";
      this.label1.Text = "0.6";
      this.label1.Paint += new System.Windows.Forms.PaintEventHandler(this.OnLabelPaint);
      // 
      // label2
      // 
      this.label2.Location = new System.Drawing.Point(62, 185);
      this.label2.Name = "label2";
      this.label2.Size = new System.Drawing.Size(180, 15);
      this.label2.TabIndex = 2;
      this.label2.Tag = "License";
      this.label2.Text = "Free";
      this.label2.Paint += new System.Windows.Forms.PaintEventHandler(this.OnLabelPaint);
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(231)))), ((int)(((byte)(231)))), ((int)(((byte)(231)))));
      this.panel1.Controls.Add(this.linkLabel22);
      this.panel1.Controls.Add(this.closeButton);
      this.panel1.Location = new System.Drawing.Point(0, 275);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(304, 46);
      this.panel1.TabIndex = 3;
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
      // linkLabel21
      // 
      this.linkLabel21.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.linkLabel21.HoverColor = System.Drawing.Color.Empty;
      this.linkLabel21.Location = new System.Drawing.Point(113, 128);
      this.linkLabel21.Name = "linkLabel21";
      this.linkLabel21.RegularColor = System.Drawing.Color.Empty;
      this.linkLabel21.Size = new System.Drawing.Size(79, 16);
      this.linkLabel21.TabIndex = 4;
      this.linkLabel21.Text = "git.io/captain";
      // 
      // linkLabel22
      // 
      this.linkLabel22.ForeColor = System.Drawing.SystemColors.HotTrack;
      this.linkLabel22.HoverColor = System.Drawing.Color.FromArgb(((int)(((byte)(51)))), ((int)(((byte)(103)))), ((int)(((byte)(155)))));
      this.linkLabel22.Location = new System.Drawing.Point(12, 14);
      this.linkLabel22.Name = "linkLabel22";
      this.linkLabel22.RegularColor = System.Drawing.Color.FromArgb(((int)(((byte)(65)))), ((int)(((byte)(103)))), ((int)(((byte)(141)))));
      this.linkLabel22.Size = new System.Drawing.Size(119, 16);
      this.linkLabel22.TabIndex = 5;
      this.linkLabel22.Text = "Open source licenses";
      this.linkLabel22.UseSystemColor = false;
      // 
      // AboutWindow
      // 
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = this.closeButton;
      this.ClientSize = new System.Drawing.Size(304, 321);
      this.Controls.Add(this.linkLabel21);
      this.Controls.Add(this.panel1);
      this.Controls.Add(this.label2);
      this.Controls.Add(this.label1);
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
      ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private System.Windows.Forms.PictureBox logoPictureBox;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.Label label2;
    private System.Windows.Forms.Panel panel1;
    private System.Windows.Forms.Button closeButton;
    private LinkLabel2 linkLabel21;
    private LinkLabel2 linkLabel22;
  }
}