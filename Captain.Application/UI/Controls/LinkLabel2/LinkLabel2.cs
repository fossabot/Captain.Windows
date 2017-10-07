using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Captain.Application.Native;

namespace Captain.Application {
  public sealed class LinkLabel2 : Control {
    private Font hoverFont;

    private Image image;
    private int imageRightPad = 8;

    private bool isHovered;
    private bool keyAlreadyProcessed;
    private Rectangle textRect;

    public LinkLabel2() {
      if (!DesignMode) {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.SupportsTransparentBackColor |
                 ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.ResizeRedraw |
                 ControlStyles.UserPaint |
                 ControlStyles.FixedHeight |
                 ControlStyles.FixedWidth,
                 true);

        SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, false);

        this.hoverFont = new Font(Font, FontStyle.Underline);

        ForeColor = SystemColors.HotTrack;

        UseSystemColor = true;
        HoverUnderline = true;
      }
    }

    [DefaultValue(8)]
    public int ImageRightPad {
      get => this.imageRightPad;
      set {
        this.imageRightPad = value;

        RefreshTextRect();
        Invalidate();
      }
    }

    [DefaultValue(null)]
    public Image Image {
      get => this.image;
      set {
        this.image = value;

        RefreshTextRect();
        Invalidate();
      }
    }

    [DefaultValue(true)]
    public bool HoverUnderline { get; set; }

    [DefaultValue(true)]
    public bool UseSystemColor { get; set; }

    public Color RegularColor { get; set; }
    public Color HoverColor { get; set; }

    public override string Text {
      get => base.Text;
      set {
        base.Text = value;
        RefreshTextRect();
        Invalidate();
      }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      if (e.Button == MouseButtons.Left) {
        Focus();
      }

      base.OnMouseDown(e);
    }

    protected override void OnMouseEnter(EventArgs e) {
      this.isHovered = true;
      Invalidate();

      base.OnMouseEnter(e);
    }

    protected override void OnMouseLeave(EventArgs e) {
      this.isHovered = false;
      Invalidate();

      base.OnMouseLeave(e);
    }

    protected override void OnMouseMove(MouseEventArgs mevent) {
      base.OnMouseMove(mevent);
      if (mevent.Button != MouseButtons.None) {
        if (!ClientRectangle.Contains(mevent.Location)) {
          if (this.isHovered) {
            this.isHovered = false;
            Invalidate();
          }
        } else if (!this.isHovered) {
          this.isHovered = true;
          Invalidate();
        }
      }
    }

    protected override void OnGotFocus(EventArgs e) {
      Invalidate();

      base.OnGotFocus(e);
    }

    protected override void OnLostFocus(EventArgs e) {
      this.keyAlreadyProcessed = false;
      Invalidate();

      base.OnLostFocus(e);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (!this.keyAlreadyProcessed && e.KeyCode == Keys.Enter) {
        this.keyAlreadyProcessed = true;
        OnClick(e);
      }

      base.OnKeyDown(e);
    }

    protected override void OnKeyUp(KeyEventArgs e) {
      this.keyAlreadyProcessed = false;

      base.OnKeyUp(e);
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      if (this.isHovered && e.Clicks == 1 && (e.Button == MouseButtons.Left || e.Button == MouseButtons.Middle)) {
        OnClick(e);
      }

      base.OnMouseUp(e);
    }

    protected override void OnPaint(PaintEventArgs e) {
      e.Graphics.CompositingQuality = CompositingQuality.HighSpeed;
      e.Graphics.InterpolationMode = InterpolationMode.Low;

      // image
      if (this.image != null) {
        e.Graphics.DrawImage(this.image,
                             new Rectangle(0, 0, this.image.Width, this.image.Height),
                             new Rectangle(0, 0, this.image.Width, this.image.Height),
                             GraphicsUnit.Pixel);
      }

      //text
      TextRenderer.DrawText(e.Graphics,
                            Text,
                            this.isHovered && HoverUnderline ? this.hoverFont : Font,
                            this.textRect,
                            UseSystemColor ? ForeColor : (this.isHovered ? HoverColor : RegularColor),
                            TextFormatFlags.SingleLine | TextFormatFlags.NoPrefix);

      // draw the focus rectangle.
      if (Focused && ShowFocusCues) {
        ControlPaint.DrawFocusRectangle(e.Graphics, ClientRectangle);
      }
    }

    protected override void OnFontChanged(EventArgs e) {
      this.hoverFont = new Font(Font, Font.Style | FontStyle.Underline);
      RefreshTextRect();

      base.OnFontChanged(e);
    }

    private void RefreshTextRect() {
      this.textRect = new Rectangle(Point.Empty,
                                    TextRenderer.MeasureText(Text,
                                                             Font,
                                                             Size,
                                                             TextFormatFlags.SingleLine |
                                                             TextFormatFlags.NoPrefix |
                                                             TextFormatFlags.VerticalCenter));
      int width = this.textRect.Width + 1,
          height = this.textRect.Height + 1;

      if (this.image != null) {
        width = this.textRect.Width + 1 + this.image.Width + this.imageRightPad;

        //adjust the x position of the text
        this.textRect.X += this.image.Width + this.imageRightPad;

        if (this.image.Height > this.textRect.Height) {
          height = this.image.Height + 1;

          // adjust the y-position of the text
          this.textRect.Y += (this.image.Height - this.textRect.Height) / 2;
        }
      }

      Size = new Size(width, height);
    }

    protected override void WndProc(ref Message m) {
      if (!DesignMode) {
        if (m.Msg == (int)User32.WindowMessage.WM_SETCURSOR) {
          User32.SetCursor(User32.LoadCursor(IntPtr.Zero, new IntPtr((int)User32.SystemResources.IDC_HAND)));
          m.Result = IntPtr.Zero;
          return;
        }
      }

      base.WndProc(ref m);
    }
  }
}