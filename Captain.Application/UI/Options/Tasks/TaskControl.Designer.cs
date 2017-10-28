using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  internal sealed partial class TaskControl {
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
      this.toolTip = new System.Windows.Forms.ToolTip(this.components);
      this.taskType = new System.Windows.Forms.PictureBox();
      this.taskRegionType = new System.Windows.Forms.PictureBox();
      this.nameLabel = new System.Windows.Forms.Label();
      this.hotKeyLabel = new System.Windows.Forms.Label();
      this.editButton = new Captain.Application.LinkButton();
      this.deleteButton = new Captain.Application.LinkButton();
      ((System.ComponentModel.ISupportInitialize)(this.taskType)).BeginInit();
      ((System.ComponentModel.ISupportInitialize)(this.taskRegionType)).BeginInit();
      this.SuspendLayout();
      // 
      // taskType
      // 
      this.taskType.BackColor = System.Drawing.Color.Transparent;
      this.taskType.Location = new System.Drawing.Point(0, 0);
      this.taskType.MinimumSize = new System.Drawing.Size(32, 32);
      this.taskType.Name = "taskType";
      this.taskType.Size = new System.Drawing.Size(32, 32);
      this.taskType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.taskType.TabIndex = 2;
      this.taskType.TabStop = false;
      // 
      // taskRegionType
      // 
      this.taskRegionType.BackColor = System.Drawing.Color.Transparent;
      this.taskRegionType.Location = new System.Drawing.Point(32, 0);
      this.taskRegionType.MinimumSize = new System.Drawing.Size(32, 32);
      this.taskRegionType.Name = "taskRegionType";
      this.taskRegionType.Size = new System.Drawing.Size(32, 32);
      this.taskRegionType.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
      this.taskRegionType.TabIndex = 3;
      this.taskRegionType.TabStop = false;
      // 
      // nameLabel
      // 
      this.nameLabel.AutoEllipsis = true;
      this.nameLabel.Location = new System.Drawing.Point(72, 0);
      this.nameLabel.Name = "nameLabel";
      this.nameLabel.Size = new System.Drawing.Size(144, 32);
      this.nameLabel.TabIndex = 4;
      this.nameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // hotKeyLabel
      // 
      this.hotKeyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.hotKeyLabel.AutoEllipsis = true;
      this.hotKeyLabel.ForeColor = System.Drawing.SystemColors.GrayText;
      this.hotKeyLabel.Location = new System.Drawing.Point(222, 0);
      this.hotKeyLabel.Name = "hotKeyLabel";
      this.hotKeyLabel.Size = new System.Drawing.Size(128, 32);
      this.hotKeyLabel.TabIndex = 5;
      this.hotKeyLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
      // 
      // editButton
      // 
      this.editButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.editButton.Image = null;
      this.editButton.Location = new System.Drawing.Point(356, 4);
      this.editButton.MinimumSize = new System.Drawing.Size(24, 24);
      this.editButton.Name = "editButton";
      this.editButton.Size = new System.Drawing.Size(24, 24);
      this.editButton.TabIndex = 1;
      this.editButton.TintColor = System.Drawing.Color.Blue;
      this.toolTip.SetToolTip(this.editButton, "Edit task");
      // 
      // deleteButton
      // 
      this.deleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.deleteButton.Image = null;
      this.deleteButton.Location = new System.Drawing.Point(388, 4);
      this.deleteButton.MinimumSize = new System.Drawing.Size(24, 24);
      this.deleteButton.Name = "deleteButton";
      this.deleteButton.Size = new System.Drawing.Size(24, 24);
      this.deleteButton.TabIndex = 0;
      this.deleteButton.TintColor = System.Drawing.Color.Red;
      this.toolTip.SetToolTip(this.deleteButton, "Delete task");
      // 
      // TaskControl
      // 
      this.BackColor = System.Drawing.Color.Transparent;
      this.Controls.Add(this.hotKeyLabel);
      this.Controls.Add(this.nameLabel);
      this.Controls.Add(this.taskRegionType);
      this.Controls.Add(this.taskType);
      this.Controls.Add(this.editButton);
      this.Controls.Add(this.deleteButton);
      this.DoubleBuffered = true;
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.MinimumSize = new System.Drawing.Size(416, 33);
      this.Name = "TaskControl";
      this.Size = new System.Drawing.Size(416, 33);
      this.MouseLeave += new System.EventHandler(this.OnControlMouseLeave);
      this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.OnControlMouseMove);
      ((System.ComponentModel.ISupportInitialize)(this.taskType)).EndInit();
      ((System.ComponentModel.ISupportInitialize)(this.taskRegionType)).EndInit();
      this.ResumeLayout(false);

    }

    #endregion

    private LinkButton deleteButton;
    private ToolTip toolTip;
    private LinkButton editButton;
    private PictureBox taskType;
    private PictureBox taskRegionType;
    private Label nameLabel;
    private Label hotKeyLabel;
  }
}
