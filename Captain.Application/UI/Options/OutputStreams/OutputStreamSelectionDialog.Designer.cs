using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application {
  partial class OutputStreamSelectionDialog {
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
      System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputStreamSelectionDialog));
      this.okButton = new System.Windows.Forms.Button();
      this.streamIconList = new System.Windows.Forms.ImageList(this.components);
      this.panel1 = new System.Windows.Forms.Panel();
      this.panel2 = new System.Windows.Forms.Panel();
      this.streamListView = new Captain.Application.ListViewEx();
      cancelButton = new System.Windows.Forms.Button();
      this.panel1.SuspendLayout();
      this.SuspendLayout();
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      cancelButton.Location = new System.Drawing.Point(257, 9);
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new System.Drawing.Size(75, 23);
      cancelButton.TabIndex = 3;
      cancelButton.Text = "&Cancel";
      cancelButton.UseVisualStyleBackColor = true;
      cancelButton.Click += new System.EventHandler(this.OnButtonClick);
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Enabled = false;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(176, 9);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.OnButtonClick);
      // 
      // streamIconList
      // 
      this.streamIconList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("streamIconList.ImageStream")));
      this.streamIconList.TransparentColor = System.Drawing.Color.Transparent;
      this.streamIconList.Images.SetKeyName(0, "");
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      this.panel1.Controls.Add(this.okButton);
      this.panel1.Controls.Add(cancelButton);
      this.panel1.Controls.Add(this.panel2);
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 240);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(344, 41);
      this.panel1.TabIndex = 2;
      // 
      // panel2
      // 
      this.panel2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
      this.panel2.Location = new System.Drawing.Point(0, 0);
      this.panel2.Name = "panel2";
      this.panel2.Size = new System.Drawing.Size(344, 1);
      this.panel2.TabIndex = 0;
      // 
      // streamListView
      // 
      this.streamListView.Activation = System.Windows.Forms.ItemActivation.TwoClick;
      this.streamListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.streamListView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.streamListView.FullRowSelect = true;
      this.streamListView.HideSelection = false;
      this.streamListView.LargeImageList = this.streamIconList;
      this.streamListView.Location = new System.Drawing.Point(0, 0);
      this.streamListView.Name = "streamListView";
      this.streamListView.ShowGroups = false;
      this.streamListView.ShowItemToolTips = true;
      this.streamListView.Size = new System.Drawing.Size(344, 240);
      this.streamListView.TabIndex = 0;
      this.streamListView.TileSize = new System.Drawing.Size(326, 56);
      this.streamListView.UseCompatibleStateImageBehavior = false;
      this.streamListView.View = System.Windows.Forms.View.Tile;
      this.streamListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnStreamSelectionChanged);
      // 
      // OutputStreamSelectionDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.White;
      this.CancelButton = cancelButton;
      this.ClientSize = new System.Drawing.Size(344, 281);
      this.Controls.Add(this.streamListView);
      this.Controls.Add(this.panel1);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.MinimumSize = new System.Drawing.Size(360, 320);
      this.Name = "OutputStreamSelectionDialog";
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Add Stream";
      this.panel1.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private ListViewEx streamListView;
    private ImageList streamIconList;
    private Panel panel1;
    private Panel panel2;
    private Button okButton;
  }
}