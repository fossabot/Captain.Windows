using System;
using System.Drawing;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace Captain.Application {
  /// <summary>
  ///   Implements a capture source from which frames can be acquired
  ///   TODO: implement audio support
  /// </summary>
  internal class DesktopDuplicationSource : CaptureSource {
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
    ///   Contains the currently captured desktop texture
    /// </summary>
    private Texture2D desktopTexture;

    /// <summary>
    ///   Instantiates this class
    /// </summary>
    /// <param name="initialArea">Initial screen region</param>
    /// <param name="adapterIndex">Adapter index</param>
    /// <param name="outputIndex">Output device index</param>
    public DesktopDuplicationSource(Rectangle initialArea, int adapterIndex, int outputIndex) : base(initialArea) {
      // get adapter
      Adapter1 adapter = new Factory1().GetAdapter1(adapterIndex);

      // create D3D device
      this.device = new SharpDX.Direct3D11.Device(adapter);

      // get output from adapter
      Output output = adapter.GetOutput(outputIndex);
      Output1 output1 = output.QueryInterface<Output1>();

      this.outputDescription = output1.Description;
      this.texDescription = new Texture2DDescription {
        CpuAccessFlags = CpuAccessFlags.Read,
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

      this.outputDuplication = output1.DuplicateOutput(this.device);
      this.desktopTexture = new Texture2D(this.device, this.texDescription);
    }

    /// <summary>
    ///   Acquires a single video frame
    /// </summary>
    /// <returns>The Bitmap instance</returns>
    internal override Bitmap AcquireVideoFrame() {
#if false
      SharpDX.DXGI.Resource desktopResource;
      OutputDuplicateFrameInformation frameInfo;

      this.outputDuplication.AcquireNextFrame(500, out frameInfo, out desktopResource);

      using (var tempTexture = desktopResource.QueryInterface<Texture2D>()) {
        this.device.ImmediateContext.CopyResource(tempTexture, this.desktopTexture);
      }

      desktopResource.Dispose();

      var map = this.device.ImmediateContext.MapSubresource(this.desktopTexture, 0, MapMode.Read, MapFlags.None);

      int width = this.outputDescription.DesktopBounds.Right - this.outputDescription.DesktopBounds.Left;
      int height = this.outputDescription.DesktopBounds.Bottom - this.outputDescription.DesktopBounds.Top;

      for (int y = 0; y < height; y++) {
        // copy a single line 
        Utilities.CopyMemory(ptr, map.DataPointer, 4 * width);

        // advance pointers
        ptr = IntPtr.Add(ptr, map.RowPitch);
        map.DataPointer = IntPtr.Add(map.DataPointer, map.RowPitch);
      }

      // release
      this.device.ImmediateContext.UnmapSubresource(this.desktopTexture, 0);
      return true;
#endif

      throw new NotImplementedException();
    }

    /// <summary>
    ///   Releases all resources
    /// </summary>
    public override void Dispose() {
      this.device?.Dispose();
      this.outputDuplication?.Dispose();
      this.desktopTexture?.Dispose();
    }
  }
}
