using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  internal sealed partial class ActionControl {
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

    #region Component Designer generated code

    /// <summary> 
    /// Required method for Designer support - do not modify 
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent() {
      this.components = new System.ComponentModel.Container();
      this.previewOverlay = new System.Windows.Forms.PictureBox();
      this.actionNameLabel = new System.Windows.Forms.Label();
      this.statusLabel = new System.Windows.Forms.Label();
      this.uriLinkButton = new Captain.Application.LinkButton();
      this.errorDetailsLinkButton = new Captain.Application.LinkButton();
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.timer1 = new System.Windows.Forms.Timer(this.components);
      ((System.ComponentModel.ISupportInitialize)(this.previewOverlay)).BeginInit();
      this.SuspendLayout();
      // 
      // previewOverlay
      // 
      this.previewOverlay.BackColor = System.Drawing.Color.Transparent;
      this.previewOverlay.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
      this.previewOverlay.Location = new System.Drawing.Point(3, 3);
      this.previewOverlay.Name = "previewOverlay";
      this.previewOverlay.Size = new System.Drawing.Size(82, 66);
      this.previewOverlay.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
      this.previewOverlay.TabIndex = 0;
      this.previewOverlay.TabStop = false;
      this.previewOverlay.SizeChanged += new System.EventHandler(this.OnPreviewOverlaySizeChanged);
      // 
      // actionNameLabel
      // 
      this.actionNameLabel.AutoEllipsis = true;
      this.actionNameLabel.BackColor = System.Drawing.Color.Transparent;
      this.actionNameLabel.Location = new System.Drawing.Point(91, 16);
      this.actionNameLabel.Name = "actionNameLabel";
      this.actionNameLabel.Size = new System.Drawing.Size(185, 15);
      this.actionNameLabel.TabIndex = 1;
      // 
      // statusLabel
      // 
      this.statusLabel.AutoEllipsis = true;
      this.statusLabel.BackColor = System.Drawing.Color.Transparent;
      this.statusLabel.ForeColor = System.Drawing.Color.Gray;
      this.statusLabel.Location = new System.Drawing.Point(91, 40);
      this.statusLabel.Name = "statusLabel";
      this.statusLabel.Size = new System.Drawing.Size(185, 15);
      this.statusLabel.TabIndex = 2;
      // 
      // uriLinkButton
      // 
      this.uriLinkButton.BackColor = System.Drawing.Color.Transparent;
      this.uriLinkButton.Image = null;
      this.uriLinkButton.Location = new System.Drawing.Point(312, 24);
      this.uriLinkButton.Name = "uriLinkButton";
      this.uriLinkButton.Size = new System.Drawing.Size(24, 24);
      this.uriLinkButton.TabIndex = 3;
      this.uriLinkButton.TintColor = System.Drawing.Color.Transparent;
      this.toolTip.SetToolTip(this.uriLinkButton, "Open");
      // 
      // errorDetailsLinkButton
      // 
      this.errorDetailsLinkButton.BackColor = System.Drawing.Color.Transparent;
      this.errorDetailsLinkButton.Image = null;
      this.errorDetailsLinkButton.Location = new System.Drawing.Point(312, 24);
      this.errorDetailsLinkButton.Name = "errorDetailsLinkButton";
      this.errorDetailsLinkButton.Size = new System.Drawing.Size(24, 24);
      this.errorDetailsLinkButton.TabIndex = 4;
      this.errorDetailsLinkButton.TintColor = System.Drawing.Color.Transparent;
      this.toolTip.SetToolTip(this.errorDetailsLinkButton, "Display error details");
      // 
      // toolTip
      // 
      this.toolTip.BackColor = System.Drawing.Color.Transparent;
      // 
      // timer1
      // 
      this.timer1.Enabled = true;
      this.timer1.Interval = 50;
      this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
      // 
      // ActionControl
      // 
      this.AutoSize = true;
      this.BackColor = System.Drawing.SystemColors.Control;
      this.Controls.Add(this.errorDetailsLinkButton);
      this.Controls.Add(this.uriLinkButton);
      this.Controls.Add(this.statusLabel);
      this.Controls.Add(this.actionNameLabel);
      this.Controls.Add(this.previewOverlay);
      this.DoubleBuffered = true;
      this.MinimumSize = new System.Drawing.Size(360, 72);
      this.Name = "ActionControl";
      this.Size = new System.Drawing.Size(360, 72);
      this.MouseLeave += new System.EventHandler(this.OnControlMouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnControlMouseMove);
      ((System.ComponentModel.ISupportInitialize)(this.previewOverlay)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private PictureBox previewOverlay;
    private Label actionNameLabel;
    private Label statusLabel;
    private LinkButton uriLinkButton;
    private LinkButton errorDetailsLinkButton;
    private ToolTip toolTip;
    private Timer timer1;
  }
}
