using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using Captain.Common;
using SharpDX.Direct3D;
using static Captain.Application.Application;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace Captain.Application {
  internal class DxgiSource : CaptureSource {
    /// <summary>
    ///   Direct3D device for the selected output
    /// </summary>
    private SharpDX.Direct3D11.Device device;

    /// <summary>
    ///   Contains information about the current output device
    /// </summary>
    private OutputDescription outputDescription;

    /// <summary>
    ///   Contains information about the desktop textire
    /// </summary>
    private Texture2DDescription texDescription;

    /// <summary>
    ///   Contains information about the Desktop Duplication session
    /// </summary>
    private OutputDuplication outputDuplication;

    /// <summary>
    ///   Current adapter
    /// </summary>
    private Adapter1 adapter;

    /// <summary>
    ///   Output device from adapter
    /// </summary>
    private Output output;

    /// <summary>
    ///   Contains the currently captured desktop texture
    /// </summary>
    private Texture2D desktopTexture;

    /// <summary>
    ///   Instantiates this class
    /// </summary>
    /// <param name="handle">Window handle</param>
    /// <param name="initialArea">Initial screen region</param>
    /// <param name="adapterIndex">Adapter index</param>
    /// <param name="outputIndex">Output index</param>
    public DxgiSource(IntPtr handle, System.Drawing.Rectangle initialArea, int adapterIndex, int outputIndex) : base(initialArea) {
      Log.WriteLine(LogLevel.Debug, "trying to instantiate DXGI desktop duplication source");

      // get adapter
      this.adapter = new Factory1().GetAdapter1(adapterIndex);
      Log.WriteLine(LogLevel.Debug, $"selected adapter {adapterIndex}");

      // create D3D device
      this.device = new SharpDX.Direct3D11.Device(this.adapter, DeviceCreationFlags.Debug);

      // get output from adapter
      this.output = this.adapter.GetOutput(outputIndex);
      var output1 = this.output.QueryInterface<Output1>();

      this.outputDescription = output1.Description;
      Log.WriteLine(LogLevel.Debug, $"found output {adapterIndex}.{outputIndex} ({this.outputDescription.DeviceName})");

      this.texDescription = new Texture2DDescription {
        CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
        BindFlags = BindFlags.None,
        Format = Format.B8G8R8A8_UNorm,
        Width = this.outputDescription.DesktopBounds.Right - this.outputDescription.DesktopBounds.Left,
        Height = this.outputDescription.DesktopBounds.Bottom - this.outputDescription.DesktopBounds.Top,
        OptionFlags = ResourceOptionFlags.None,
        MipLevels = 1,
        ArraySize = 1,
        SampleDescription = { Count = 1, Quality = 0 },
        Usage = ResourceUsage.Staging
      };

      //this.outputDuplication = output1.DuplicateOutput1(this.device, 0, 1, new[] { Format.B8G8R8A8_UNorm });
      this.outputDuplication = output1.DuplicateOutput(this.device);
      Log.WriteLine(LogLevel.Debug, "desktop duplicated successfully");
    }

    /// <summary>
    ///   Acquires a single video frame
    /// </summary>
    /// <returns>The frame Bitmap</returns>
    internal override Bitmap AcquireVideoFrame() {
      if (this.desktopTexture is null || this.desktopTexture.IsDisposed) {
        Log.WriteLine(LogLevel.Debug, "creating texture for video frame");
        this.desktopTexture = new Texture2D(this.device, this.texDescription);
      }

      // TODO: use MapDesktopSurface when possible
      OutputDuplicateFrameInformation frameInfo;
      SharpDX.DXGI.Resource desktopResource;

      while (true) {  // HACK !!!
        this.outputDuplication.AcquireNextFrame(500,
                                                out frameInfo,
                                                out desktopResource);

        if (frameInfo.TotalMetadataBufferSize == 0) {
          this.outputDuplication.ReleaseFrame();
        } else {
          break;
        }
      }

      using (var tempTexture = desktopResource.QueryInterface<Texture2D>()) {
        this.device.ImmediateContext.CopyResource(tempTexture, this.desktopTexture);
      }

      desktopResource.Dispose();
      DataBox map = this.device.ImmediateContext.MapSubresource(this.desktopTexture, 0, MapMode.Read, MapFlags.None);

      var bitmap = new Bitmap(Area.Width, Area.Height, PixelFormat.Format32bppRgb);
      BitmapData bitmapData = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, Area.Width, Area.Height),
                                              ImageLockMode.WriteOnly,
                                              bitmap.PixelFormat);

      // start copying from the specified position
      map.DataPointer = IntPtr.Add(map.DataPointer,
                                   4 * Area.X + // X offset in bytes
                                   map.RowPitch * Area.Y);  // Y offset in pixels (actually scanlines!)

      for (int y = 0; y < Area.Height; y++) {
        // copy a single line 
        Utilities.CopyMemory(bitmapData.Scan0, map.DataPointer, 4 * Area.Width);

        // advance pointers
        bitmapData.Scan0 += bitmapData.Stride;
        map.DataPointer += map.RowPitch;
      }

      Log.WriteLine(LogLevel.Debug, $"copied {Area.Height * map.RowPitch} bytes to bitmapData");

      // release
      bitmap.UnlockBits(bitmapData);
      this.device.ImmediateContext.UnmapSubresource(this.desktopTexture, 0);
      this.outputDuplication.ReleaseFrame();

      return bitmap;
    }

    /// <summary>
    ///   Releases resources
    /// </summary>
    public override void Dispose() {
      Log.WriteLine(LogLevel.Debug, "releasing resources");

      this.device?.Dispose();
      this.output?.ReleaseOwnership();
      this.output?.Dispose();
      this.adapter?.Dispose();
      this.outputDuplication?.Dispose();
      this.desktopTexture?.Dispose();
    }
  }
}