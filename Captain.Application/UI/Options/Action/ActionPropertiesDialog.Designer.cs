using System.ComponentModel;
using System.Windows.Forms;

namespace Captain.Application
{
  partial class ActionPropertiesDialog
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
      System.Windows.Forms.Panel buttonPane;
      System.Windows.Forms.Button cancelButton;
      System.Windows.Forms.Panel topButtonPaneBorder;
      System.Windows.Forms.Panel taskToolBarPanel;
      this.okButton = new System.Windows.Forms.Button();
      this.deleteTaskLinkButton = new Captain.Application.LinkButton();
      this.taskOptionsLinkButton = new Captain.Application.LinkButton();
      this.panel1 = new System.Windows.Forms.Panel();
      this.addActionLinkButton = new Captain.Application.LinkButton();
      this.streamListView = new Captain.Application.ListViewEx();
      buttonPane = new System.Windows.Forms.Panel();
      cancelButton = new System.Windows.Forms.Button();
      topButtonPaneBorder = new System.Windows.Forms.Panel();
      taskToolBarPanel = new System.Windows.Forms.Panel();
      buttonPane.SuspendLayout();
      taskToolBarPanel.SuspendLayout();
      this.SuspendLayout();
      // 
      // buttonPane
      // 
      buttonPane.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      buttonPane.Controls.Add(this.okButton);
      buttonPane.Controls.Add(cancelButton);
      buttonPane.Controls.Add(topButtonPaneBorder);
      buttonPane.Dock = System.Windows.Forms.DockStyle.Bottom;
      buttonPane.Location = new System.Drawing.Point(0, 287);
      buttonPane.Name = "buttonPane";
      buttonPane.Size = new System.Drawing.Size(360, 41);
      buttonPane.TabIndex = 4;
      // 
      // okButton
      // 
      this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
      this.okButton.Enabled = false;
      this.okButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      this.okButton.Location = new System.Drawing.Point(195, 9);
      this.okButton.Name = "okButton";
      this.okButton.Size = new System.Drawing.Size(75, 23);
      this.okButton.TabIndex = 4;
      this.okButton.Text = "&OK";
      this.okButton.UseVisualStyleBackColor = true;
      this.okButton.Click += new System.EventHandler(this.OnButtonClick);
      // 
      // cancelButton
      // 
      cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
      cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
      cancelButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
      cancelButton.Location = new System.Drawing.Point(276, 9);
      cancelButton.Name = "cancelButton";
      cancelButton.Size = new System.Drawing.Size(75, 23);
      cancelButton.TabIndex = 3;
      cancelButton.Text = "&Cancel";
      cancelButton.UseVisualStyleBackColor = true;
      cancelButton.Click += new System.EventHandler(this.OnButtonClick);
      // 
      // topButtonPaneBorder
      // 
      topButtonPaneBorder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(223)))), ((int)(((byte)(223)))), ((int)(((byte)(223)))));
      topButtonPaneBorder.Dock = System.Windows.Forms.DockStyle.Top;
      topButtonPaneBorder.Location = new System.Drawing.Point(0, 0);
      topButtonPaneBorder.Name = "topButtonPaneBorder";
      topButtonPaneBorder.Size = new System.Drawing.Size(360, 1);
      topButtonPaneBorder.TabIndex = 0;
      // 
      // taskToolBarPanel
      // 
      taskToolBarPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
      taskToolBarPanel.Controls.Add(this.deleteTaskLinkButton);
      taskToolBarPanel.Controls.Add(this.taskOptionsLinkButton);
      taskToolBarPanel.Controls.Add(this.panel1);
      taskToolBarPanel.Controls.Add(this.addActionLinkButton);
      taskToolBarPanel.Dock = System.Windows.Forms.DockStyle.Top;
      taskToolBarPanel.Location = new System.Drawing.Point(0, 1);
      taskToolBarPanel.Name = "taskToolBarPanel";
      taskToolBarPanel.Size = new System.Drawing.Size(360, 33);
      taskToolBarPanel.TabIndex = 6;
      // 
      // deleteTaskLinkButton
      // 
      this.deleteTaskLinkButton.Image = null;
      this.deleteTaskLinkButton.Location = new System.Drawing.Point(208, 4);
      this.deleteTaskLinkButton.Name = "deleteTaskLinkButton";
      this.deleteTaskLinkButton.Size = new System.Drawing.Size(96, 24);
      this.deleteTaskLinkButton.TabIndex = 7;
      this.deleteTaskLinkButton.Text = "Delete task";
      this.deleteTaskLinkButton.TintColor = System.Drawing.Color.Crimson;
      this.deleteTaskLinkButton.Visible = false;
      this.deleteTaskLinkButton.Click += new System.EventHandler(this.OnDeleteTaskClicked);
      // 
      // taskOptionsLinkButton
      // 
      this.taskOptionsLinkButton.Enabled = false;
      this.taskOptionsLinkButton.Image = null;
      this.taskOptionsLinkButton.Location = new System.Drawing.Point(106, 4);
      this.taskOptionsLinkButton.Name = "taskOptionsLinkButton";
      this.taskOptionsLinkButton.Size = new System.Drawing.Size(96, 24);
      this.taskOptionsLinkButton.TabIndex = 6;
      this.taskOptionsLinkButton.Text = "Task options";
      this.taskOptionsLinkButton.TintColor = System.Drawing.Color.SteelBlue;
      this.taskOptionsLinkButton.Visible = false;
      this.taskOptionsLinkButton.Click += new System.EventHandler(this.OnTaskOptionsClicked);
      // 
      // panel1
      // 
      this.panel1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
      this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
      this.panel1.Location = new System.Drawing.Point(0, 32);
      this.panel1.Name = "panel1";
      this.panel1.Size = new System.Drawing.Size(360, 1);
      this.panel1.TabIndex = 1;
      // 
      // addActionLinkButton
      // 
      this.addActionLinkButton.Image = null;
      this.addActionLinkButton.Location = new System.Drawing.Point(4, 4);
      this.addActionLinkButton.Name = "addActionLinkButton";
      this.addActionLinkButton.Size = new System.Drawing.Size(96, 24);
      this.addActionLinkButton.TabIndex = 5;
      this.addActionLinkButton.Text = "Add action";
      this.addActionLinkButton.TintColor = System.Drawing.Color.SeaGreen;
      this.addActionLinkButton.Click += new System.EventHandler(this.OnAddActionLinkButtonClick);
      // 
      // streamListView
      // 
      this.streamListView.BackColor = System.Drawing.Color.WhiteSmoke;
      this.streamListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
      this.streamListView.Dock = System.Windows.Forms.DockStyle.Fill;
      this.streamListView.FullRowSelect = true;
      this.streamListView.GridLines = true;
      this.streamListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
      this.streamListView.Location = new System.Drawing.Point(0, 34);
      this.streamListView.Name = "streamListView";
      this.streamListView.Size = new System.Drawing.Size(360, 253);
      this.streamListView.TabIndex = 7;
      this.streamListView.UseCompatibleStateImageBehavior = false;
      this.streamListView.View = System.Windows.Forms.View.Details;
      this.streamListView.ItemActivate += new System.EventHandler(this.OnStreamListItemActivated);
      this.streamListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.OnStreamSelectionChanged);
      // 
      // ActionPropertiesDialog
      // 
      this.AcceptButton = this.okButton;
      this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
      this.BackColor = System.Drawing.Color.WhiteSmoke;
      this.CancelButton = cancelButton;
      this.ClientSize = new System.Drawing.Size(360, 328);
      this.Controls.Add(this.streamListView);
      this.Controls.Add(taskToolBarPanel);
      this.Controls.Add(buttonPane);
      this.Font = new System.Drawing.Font("Segoe UI", 9F);
      this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
      this.KeyPreview = true;
      this.MaximizeBox = false;
      this.MinimizeBox = false;
      this.Name = "ActionPropertiesDialog";
      this.Padding = new System.Windows.Forms.Padding(0, 1, 0, 0);
      this.ShowIcon = false;
      this.ShowInTaskbar = false;
      this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
      this.Text = "Task Actions";
      buttonPane.ResumeLayout(false);
      taskToolBarPanel.ResumeLayout(false);
      this.ResumeLayout(false);

    }

    #endregion

    private Button okButton;
    private LinkButton addActionLinkButton;
    private Panel panel1;
    private ListViewEx streamListView;
    private LinkButton taskOptionsLinkButton;
    private LinkButton deleteTaskLinkButton;
  }
}