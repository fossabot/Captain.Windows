using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace Captain.Application {
  /// <summary>
  ///   macOS-like toolbar for preference ("option") dialogs
  /// </summary>
  internal sealed class ToolBarControl : TabControl {
    /// <summary>
    ///   Reference font size for 1x scale
    /// </summary>
    private float referenceFontSize;

    /// <summary>
    ///   Whether to expand tabs to fill horizontal space
    /// </summary>
    private bool extendTabs;

    /// <summary>
    ///   Index of the tab that is currently hovered (-1 for none)
    /// </summary>
    private int hoverIndex = -1;

    /// <summary>
    ///   Index of the tab that is currently being pressed (-1 for none)
    /// </summary>
    private int downIndex = -1;

    /// <summary>
    ///   Color for the tab header
    /// </summary>
    private static Brush TabHeaderBrush => new SolidBrush(Color.FromArgb(0xFF, 0xE7, 0xE7, 0xE7));

    /// <summary>
    ///   Pen for drawing tab separators
    /// </summary>
    private static Pen BorderPen => new Pen(Color.FromArgb(0x20, Color.Black));

    /// <summary>
    ///   Brush for drawing pressed tabs' background
    /// </summary>
    private static SolidBrush PressedTabBrush => new SolidBrush(Color.FromArgb(0x20, Color.Black));

    /// <summary>
    ///   Brush for drawing hovered tabs' background
    /// </summary>
    private static SolidBrush HoveredTabBrush => new SolidBrush(Color.FromArgb(0x10, Color.Black));

    /// <summary>
    ///   Color for the tab labels
    /// </summary>
    private static Color LabelColor => Color.FromArgb(0x606060);

    /// <summary>
    ///   Pen for drawing selected tab's bottom border
    /// </summary>
    private Pen SelectedTabBorderPen { get; set; }

    /// <summary>
    ///   Color for drawing selected tabs' text
    /// </summary>
    private Color SelectedTabForeColor { get; set; }

    /// <summary>
    ///   Whether to expand tabs to fill horizontal space
    /// </summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public bool ExtendTabs {
      // ReSharper disable once MemberCanBePrivate.Global
      get => this.extendTabs;
      set {
        this.extendTabs = value;
        UpdateItemSize();
      }
    }

    /// <summary>
    ///   Initializes a new instance of this control
    /// </summary>
    /// <remarks>
    ///   Public constructor ensures the Windows Forms designer generates code for this control
    /// </remarks>
    public ToolBarControl() {
      // set control styles so we have full control over the rendering and mouse handling procedures and ensure the
      // control is double buffered to reduce flicker
      SetStyle(ControlStyles.UserPaint |
               ControlStyles.UserMouse |
               ControlStyles.AllPaintingInWmPaint |
               ControlStyles.OptimizedDoubleBuffer |
               ControlStyles.ResizeRedraw,
               true);
      UpdateAccentColor();
      UpdateItemSize();

      this.referenceFontSize = Font.Size;
    }

    /// <summary>
    ///   Scale tabs accordingly when font changes
    /// </summary>
    /// <param name="eventArgs"></param>
    protected override void OnFontChanged(EventArgs eventArgs) {
      if (!DesignMode) {
        decimal scale = (decimal)Font.Size / (decimal)this.referenceFontSize;
        this.referenceFontSize = Font.Size;
        ItemSize = new Size((int)Math.Floor(ItemSize.Width * scale), (int)Math.Floor(ItemSize.Height * scale));
        UpdateItemSize();
      }

      base.OnFontChanged(eventArgs);
    }

    /// <summary>
    ///   Updates accent colors
    /// </summary>
    internal void UpdateAccentColor() {
      // set accent color
      SelectedTabBorderPen = new Pen(StyleHelper.GetAccentColor() ?? Color.FromArgb(0x40, Color.Black));
      SelectedTabForeColor = SelectedTabBorderPen.Color.Blend(Color.Black);
      Invalidate(true);
    }

    /// <summary>
    ///   Updates tab size
    /// </summary>
    internal void UpdateItemSize() {
      if (ExtendTabs) {
        ItemSize = new Size(Width / TabCount - 1, ItemSize.Height);
        Invalidate(true);
      }
    }

    /// <summary>
    ///   Gets the Rectangle that is to be occupied for the tab with the specified index
    /// </summary>
    /// <param name="index">The zero-based tab index</param>
    /// <returns>A Rectangle representing the tab bounds</returns>
    private Rectangle GetTabBounds(int index) {
      var bounds = new Rectangle(ItemSize.Width * index, 0, ItemSize.Width, ItemSize.Height);

      if (ExtendTabs && index == TabCount - 1) {
        // HACK: adjust last tab
        bounds.Width = Width - bounds.X;
      }

      return bounds;
    }

    /// <summary>
    ///   Clears background
    /// </summary>
    /// <param name="e"></param>
    protected override void OnPaintBackground(PaintEventArgs e) {
      // clear control with the default background color
      e.Graphics.Clear(Color.WhiteSmoke);

      // clear header with darker color
      e.Graphics.FillRectangle(TabHeaderBrush, new Rectangle(0, 0, Width, ItemSize.Height));

      // draw header border
      e.Graphics.DrawLine(BorderPen, Left, ItemSize.Height, Right, ItemSize.Height);
    }

    /// <summary>
    ///   Triggered when the mouse leaves the tab header
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnMouseLeave(EventArgs e) {
      if (this.hoverIndex != -1) {
        // invalidate the currently hovered tab, if any
        Invalidate(GetTabBounds(this.hoverIndex));

        // tab no longer hovered
        this.hoverIndex = -1;
      }
    }

    /// <summary>
    ///   Triggered when the mouse moves around the tab header area
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseMove(MouseEventArgs e) {
      int previousHoverIndex = this.hoverIndex; // previously hovered tab index
      this.hoverIndex = -1; // reset hover index

      // mouse is moving around the tab area, calculate the tab index from its horizontal position
      this.hoverIndex = e.X / ItemSize.Width;

      if (this.hoverIndex != previousHoverIndex) {
        // the hovered tab has changed, invalidate previous and current tab regions
        Invalidate(GetTabBounds(previousHoverIndex));
        Invalidate(GetTabBounds(this.hoverIndex));
      }
    }

    /// <summary>
    ///   Triggered when the user holds the primary mouse button
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnMouseDown(MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) {
        this.downIndex = e.X / ItemSize.Width;
        Invalidate(GetTabBounds(this.downIndex));
      }
    }

    /// <summary>
    ///   Triggered when the user releases the primary mouse button
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnMouseUp(MouseEventArgs e) {
      if (this.downIndex != -1) {
        SelectedIndex = this.downIndex;
        this.downIndex = -1;
        Invalidate();
      }
    }

    /// <summary>
    ///   Paint procedure
    /// </summary>
    /// <param name="e">Event arguments</param>
    protected override void OnPaint(PaintEventArgs e) {
      for (int i = 0; i < TabCount; i++) {
        // get bounds for the tab being rendered
        Rectangle tabBounds = GetTabBounds(i);

        if (this.downIndex == i) {
          // this tab is pressed
          e.Graphics.FillRectangle(PressedTabBrush, tabBounds);
        } else if (this.hoverIndex == i) {
          // this tab is hovered
          e.Graphics.FillRectangle(HoveredTabBrush, tabBounds);
        }

        if (i != TabCount - 1) {
          // draw tab separator except for the last tab
          e.Graphics.DrawLine(BorderPen,
                              tabBounds.Right,
                              tabBounds.Top + 4,
                              tabBounds.Right,
                              tabBounds.Bottom - 4);
        }

        // render tab label
        TextRenderer.DrawText(e.Graphics,
                              TabPages[i].Text,
                              Font,
                              new Rectangle(tabBounds.X, tabBounds.Y, tabBounds.Width, tabBounds.Height),
                              SelectedIndex == i ? SelectedTabForeColor : LabelColor,
                              TextFormatFlags.EndEllipsis |
                              TextFormatFlags.HorizontalCenter |
                              TextFormatFlags.VerticalCenter);

        if (SelectedIndex == i) {
          // display a bottom border for selected tabs
          e.Graphics.DrawLine(SelectedTabBorderPen,
                              tabBounds.Left,
                              tabBounds.Bottom,
                              tabBounds.Right,
                              tabBounds.Bottom);
        }
      }
    }
  }
}