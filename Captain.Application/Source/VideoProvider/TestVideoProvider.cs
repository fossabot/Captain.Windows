using System;
using System.Drawing;
using System.Drawing.Imaging;
using Captain.Common;
using BitmapData = Captain.Common.BitmapData;

namespace Captain.Application {
  internal class TestVideoProvider : VideoProvider {
    private Bitmap bmp;
    private int i = 200;

    public TestVideoProvider(Rectangle captureBounds, IntPtr? windowHandle = null) :
      base(captureBounds, windowHandle) { }

    public override void AcquireFrame() {
      this.bmp = new Bitmap(CaptureBounds.Width, CaptureBounds.Height);
      using (var graphics = Graphics.FromImage(this.bmp)) {
        this.i = (this.i + 1) % Byte.MaxValue;
        graphics.Clear(Color.FromArgb(0, this.i, this.i));
      }
    }

    public override void ReleaseFrame() => this.bmp.Dispose();

    public override BitmapData LockFrameBitmap() {
      System.Drawing.Imaging.BitmapData data = this.bmp.LockBits(new Rectangle(Point.Empty, this.bmp.Size),
        ImageLockMode.ReadOnly,
        PixelFormat.Format32bppArgb);

      return new BitmapData {
        Width = data.Width,
        Height = data.Height,
        LockPointer = new IntPtr(data.Reserved),
        PixelFormat = SharpDX.WIC.PixelFormat.Format32bppBGRA,//new Guid((int) data.PixelFormat, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0),
        Scan0 = data.Scan0,
        Stride = data.Stride
      };
    }

    public override void UnlockFrameBitmap(BitmapData data) {
      var gdiData = new System.Drawing.Imaging.BitmapData {
        Width = data.Width,
        Height = data.Height,
        Reserved = data.LockPointer.ToInt32(),
        PixelFormat = PixelFormat.Format32bppArgb,
        Scan0 = data.Scan0,
        Stride = data.Stride
      };

      this.bmp.UnlockBits(gdiData);
    }

    public override void Dispose() => ReleaseFrame();
  }
}