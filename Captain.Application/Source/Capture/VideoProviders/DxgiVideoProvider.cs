﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using Captain.Common;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;
using Rectangle = System.Drawing.Rectangle;
using Resource = SharpDX.DXGI.Resource;
using ResultCode = SharpDX.DXGI.ResultCode;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides screen capture operations using DXGI
  /// </summary>
  internal sealed class DxgiVideoProvider : VideoProvider {
    /// <summary>
    ///   Timeout, in milliseconds, to consider a dekstop duplilcation frame lost
    /// </summary>
    private const int DuplicationFrameTimeout = 500;

    /// <summary>
    ///   Not implemented HRESULT value
    /// </summary>
    private const int NotImplementedHResult = unchecked((int) 0x80004001); // E_NOTIMPL

    /// <summary>
    ///   Enumeration of video adapters involved
    /// </summary>
    private readonly Adapter[] adapters;

    /// <summary>
    ///   Direct3D device handles
    /// </summary>
    private readonly Device[] devices;

    /// <summary>
    ///   Instances of desktop duplications for each output
    /// </summary>
    private readonly OutputDuplication[] duplications;

    /// <summary>
    ///   Enumeration of display outputs containing a region of the specified rectangle
    /// </summary>
    private readonly Output[] outputs;

    /// <summary>
    ///   Partial rectangles for each output
    /// </summary>
    private readonly Rectangle[] rects;

    /// <summary>
    ///   Regions for each texture
    /// </summary>
    private readonly ResourceRegion[] regions;

    /// <summary>
    ///   Staging textures
    /// </summary>
    internal Texture2D[] StagingTextures { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="rect">Screen region</param>
    /// <param name="windowHandle">Attached window handle (unused)</param>
    /// <exception cref="NotSupportedException">Thrown when no video adapters were found</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the capture region is empty</exception>
    internal DxgiVideoProvider(Rectangle rect, IntPtr? windowHandle = null) : base(rect, windowHandle) {
      var factory = new Factory1();
      var intersections = new List<(Adapter Adapter, Output Output, Rectangle Rectangle, Rectangle Bounds)>();

      // enumerate outputs
      if (factory.GetAdapterCount1() == 0) { throw new NotSupportedException("No suitable video adapters found"); }
      foreach (Adapter adapter in factory.Adapters) {
        intersections.AddRange(from output in adapter.Outputs
                               let outputRect = new Rectangle(output.Description.DesktopBounds.Left,
                                                              output.Description.DesktopBounds.Top,
                                                              output.Description.DesktopBounds.Right -
                                                              output.Description.DesktopBounds.Left,
                                                              output.Description.DesktopBounds.Bottom -
                                                              output.Description.DesktopBounds.Top)
                               let intersection = Rectangle.Intersect(rect, outputRect)
                               where intersection != Rectangle.Empty
                               select (adapter, output, intersection, outputRect));
      }

      // make sure we do not capture out of bounds
      //this.rect = triples.Select(t => Rectangle.Union(this.rect, t.Rectangle)).Last();
      CaptureBounds = rect;
      if (CaptureBounds.IsEmpty) { throw new ArgumentOutOfRangeException(nameof(rect)); }

      // create adapter, output arrays
      this.adapters = intersections.Select(t => t.Adapter).ToArray();
      this.outputs = intersections.Select(t => t.Output).ToArray();

      // set rectangles for each output
      this.rects = intersections.Select(t => t.Rectangle).ToArray();
      this.regions = intersections.Select(t => {
                                    var region = new ResourceRegion(t.Rectangle.X - t.Bounds.X,
                                                                    t.Rectangle.Y - t.Bounds.Y,
                                                                    0,
                                                                    t.Rectangle.Width,
                                                                    t.Rectangle.Height,
                                                                    1);

                                    region.Right += region.Left;
                                    region.Bottom += region.Top;

                                    return region;
                                  })
                                  .ToArray();

      // create devices for each adapter
      this.devices = this.adapters.Select(a => new Device(a, DeviceCreationFlags.Debug)).ToArray();

      // create the shared texture in this device
      StagingTextures = this.devices.Select((d, i) => new Texture2D(d,
                                                                    new Texture2DDescription {
                                                                      CpuAccessFlags = CpuAccessFlags.Read,
                                                                      BindFlags = BindFlags.None,
                                                                      Format = Format.B8G8R8A8_UNorm,
                                                                      Width = intersections[i].Rectangle.Width,
                                                                      Height = intersections[i].Rectangle.Height,
                                                                      OptionFlags = ResourceOptionFlags.None,
                                                                      MipLevels = 1,
                                                                      ArraySize = 1,
                                                                      SampleDescription = {
                                                                        Count = 1,
                                                                        Quality = 0
                                                                      },
                                                                      Usage = ResourceUsage.Staging
                                                                    })).ToArray();

      // duplicate desktops
      this.duplications = this.outputs.Select((o, i) => {
                                try {
                                  // attempt to use DuplicateOutput1 for improved performance and SRGB support
                                  Format[] formats = {Format.B8G8R8A8_UNorm_SRgb, Format.B8G8R8A8_UNorm};
                                  return o.QueryInterface<Output6>()
                                          .DuplicateOutput1(this.devices[i], 0, formats.Length, formats);
                                } catch (SharpDXException exception1) when (exception1.HResult ==
                                                                            ResultCode.Unsupported.Result) {
                                  try {
                                    return o.QueryInterface<Output1>().DuplicateOutput(this.devices[i]);
                                  } catch (SharpDXException exception) when ((exception.HResult ==
                                                                              ResultCode.Unsupported.Result) ||
                                                                             (exception.HResult == NotImplementedHResult
                                                                             )) {
                                    throw new NotSupportedException("Platform is not supported");
                                  }
                                }
                              })
                              .ToArray();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() {
      ReleaseFrame();

      if (StagingTextures != null) {
        foreach (Texture2D texture in StagingTextures) {
          texture.Dispose();
        }
      }

      if (this.duplications != null) {
        foreach (OutputDuplication duplication in this.duplications) {
          duplication?.Dispose();
        }
      }

      if (this.devices != null) {
        foreach (Device dev in this.devices) {
          dev?.Dispose();
        }
      }

      if (this.outputs != null) {
        foreach (Output output in this.outputs) {
          output?.Dispose();
        }
      }

      if (this.adapters != null) {
        foreach (Adapter adapter in this.adapters) {
          adapter?.Dispose();
        }
      }
    }

    /// <summary>
    ///   Acquires a frame from the desktop duplication instance with the specified index
    /// </summary>
    /// <param name="index">Index of the desktop duplication instance</param>
    private void AcquireFrame(int index) {
      OutputDuplicateFrameInformation frameInfo;
      Resource desktopResource = null;
      OutputDuplication duplication = this.duplications[index];

      do {
        // release previous frame if last capture attempt failed
        if (desktopResource != null) { duplication.ReleaseFrame(); }

        // try to capture a frame
        duplication.AcquireNextFrame(DuplicationFrameTimeout, out frameInfo, out desktopResource);
      } while (frameInfo.TotalMetadataBufferSize == 0); // make sure we got a suitable frame

      using (var tempTexture = desktopResource?.QueryInterface<Texture2D>()) {
        tempTexture?.Device.ImmediateContext.CopySubresourceRegion(tempTexture,
                                                                   0,
                                                                   this.regions[index],
                                                                   StagingTextures[index],
                                                                   0);
      }

      // release resources
      desktopResource?.Dispose();
      duplication.ReleaseFrame();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Acquires a whole frame with the current bounds
    /// </summary>
    public override void AcquireFrame() {
      for (var i = 0; i < this.duplications?.Length; i++) {
        AcquireFrame(i);
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the last captured frame
    /// </summary>
    public override void ReleaseFrame() {}

    /// <inheritdoc />
    /// <summary>
    ///   Creates a managed bitmap out of the the last captured frame
    /// </summary>
    /// <returns>A <see cref="T:System.Drawing.Bitmap" /> instance</returns>
    public override Bitmap CreateFrameBitmap() {
      // TODO: support for other pixel formats (i.e. 10-bit true color)
      var bitmap = new Bitmap(CaptureBounds.Width, CaptureBounds.Height, PixelFormat.Format32bppRgb);
      BitmapData data = bitmap.LockBits(new Rectangle(0,
                                                      0,
                                                      CaptureBounds.Width,
                                                      CaptureBounds.Height),
                                        ImageLockMode.WriteOnly,
                                        bitmap.PixelFormat);

      int minX = this.rects.Select(b => b.X).Min();
      int minY = this.rects.Select(b => b.Y).Min();

      // map textures
      for (var i = 0; i < StagingTextures.Length; i++) {
        DataBox map = StagingTextures[i]
          .Device.ImmediateContext
          .MapSubresource(StagingTextures[i], 0, MapMode.Read, MapFlags.None);

        IntPtr dstScan0 = data.Scan0,
               srcScan0 = map.DataPointer;

        int dstStride = data.Stride,
            srcStride = map.RowPitch;

        int srcWidth = this.rects[i].Width,
            srcHeight = this.rects[i].Height;

        int dstX = this.rects[i].X - minX,
            dstY = this.rects[i].Y - minY;

        for (var y = 0; y < srcHeight; y++) {
          Utilities.CopyMemory(IntPtr.Add(dstScan0, (4 * dstX) + ((y + dstY) * dstStride)),
                               IntPtr.Add(srcScan0, y * srcStride),
                               4 * srcWidth);
        }

        StagingTextures[i].Device.ImmediateContext.UnmapSubresource(StagingTextures[i], 0);
      }

      bitmap.UnlockBits(data);
      return bitmap;
    }
  }
}