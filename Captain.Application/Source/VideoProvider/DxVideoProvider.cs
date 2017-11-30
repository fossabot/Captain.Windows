using System;
using System.Collections.Generic;
using System.Linq;
using Captain.Application.Native;
using Captain.Common;
using SharpDX;
using SharpDX.Direct3D9;
using SharpDX.WIC;
using static Captain.Application.Application;
using Bitmap = SharpDX.WIC.Bitmap;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides screen capture operations using DirectX
  /// </summary>
  internal sealed class DxVideoProvider : VideoProvider {
    /// <summary>
    ///   Direct3D instance.
    /// </summary>
    private readonly Direct3D direct3d;

    /// <summary>
    ///   Enumeration of video adapters involved.
    /// </summary>
    private readonly AdapterInformation[] adapters;

    /// <summary>
    ///   Direct3D device handles
    /// </summary>
    private readonly Device[] devices;

    /// <summary>
    ///   Partial rectangles for each output
    /// </summary>
    private readonly Rectangle[] rects;

    /// <summary>
    ///   Regions for each texture
    /// </summary>
    private readonly Rectangle[] regions;

    /// <summary>
    ///   Staging textures
    /// </summary>
    internal Surface[] Surfaces { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor
    /// </summary>
    /// <param name="rect">Screen region</param>
    /// <param name="windowHandle">Attached window handle (unused)</param>
    /// <exception cref="NotSupportedException">Thrown when no video adapters were found</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the capture region is empty</exception>
    internal DxVideoProvider(Rectangle rect, IntPtr? windowHandle = null) : base(rect, windowHandle) {
      throw new NotImplementedException();

      Log.WriteLine(LogLevel.Debug, "creating DirectX video provider");
      this.direct3d = new Direct3D();

      // enumerate adapters
      List<(AdapterInformation Adapter, Rectangle Rectangle, Rectangle Bounds)> intersections = (
        from adapter in this.direct3d.Adapters
        let info = new MONITORINFO()
        where User32.GetMonitorInfo(adapter.Monitor, info)
        let outputRect = Rectangle.FromLTRB(info.rcMonitor.left,
          info.rcMonitor.top,
          info.rcMonitor.right,
          info.rcMonitor.bottom)
        let intersection = Rectangle.Intersect(rect, outputRect)
        where intersection != Rectangle.Empty
        select (adapter, intersection, outputRect)).ToList();

      // make sure we do not capture out of bounds
      CaptureBounds = rect;
      if (CaptureBounds.IsEmpty) { throw new ArgumentOutOfRangeException(nameof(rect)); }

      // set rectangles for each output
      this.rects = intersections.Select(t => t.Rectangle).ToArray();
      this.regions = intersections.Select(t => t.Bounds).ToArray();

      // create devices for each adapter
      this.devices = intersections.Select(i =>
          new Device(this.direct3d,
            i.Adapter.Adapter,
            DeviceType.Hardware,
            User32.GetDesktopWindow(),
            CreateFlags.HardwareVertexProcessing,
            new PresentParameters(i.Bounds.Width, i.Bounds.Height)))
        .ToArray();

      // create adapter and surface arrays
      this.adapters = intersections.Select(t => t.Adapter).ToArray();
      Surfaces = new Surface[this.devices.Length];
    }

    /// <summary>
    ///   Acquires a frame from the desktop duplication instance with the specified index
    /// </summary>
    /// <param name="index">Index of the desktop duplication instance</param>
    private void AcquireFrame(int index) {
      Surfaces[index] = Surface.CreateOffscreenPlain(this.devices[index],
        this.regions[index].Width,
        this.regions[index].Height,
        this.adapters[index].CurrentDisplayMode.Format,
        Pool.Scratch);
      
        this.devices[index].GetFrontBufferData(0, Surfaces[index]);
      //Surfaces[index].RenderTarget.Device.GetRenderTargetData(Surfaces[index].RenderTarget, Surfaces[index].DestTarget);
    }

    /// <inheritdoc />
    /// <summary>
    ///   Acquires a whole frame with the current bounds
    /// </summary>
    public override void AcquireFrame() {
      for (int i = 0; i < Surfaces?.Length; i++) { AcquireFrame(i); }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the last captured frame
    /// </summary>
    public override void ReleaseFrame() {
      for (int i = 0; i < Surfaces?.Length; i++) { Surfaces[i].Dispose(); }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Creates a single bitmap from the captured frames and returns an object with its information
    /// </summary>
    /// <returns>A <see cref="BitmapData" /> containing raw bitmap information</returns>
    public override BitmapData LockFrameBitmap() {
      using (var factory = new ImagingFactory2()) {
        var bmp = new Bitmap(factory,
          CaptureBounds.Width,
          CaptureBounds.Height,
          PixelFormat.Format32bppBGRA,
          BitmapCreateCacheOption.CacheOnDemand);

        // caller is responsible for disposing BitmapLock
        BitmapLock data = bmp.Lock(BitmapLockFlags.Write);
        int minX = this.rects.Select(b => b.X).Min();
        int minY = this.rects.Select(b => b.Y).Min();

        // map textures
        for (int i = 0; i < Surfaces.Length; i++) {
          DataRectangle map = Surfaces[i].LockRectangle(LockFlags.ReadOnly);
          IntPtr dstScan0 = data.Data.DataPointer,
            srcScan0 = map.DataPointer;

          int dstStride = data.Stride,
            srcStride = map.Pitch;
          int srcWidth = this.regions[i].Width,
            srcHeight = this.regions[i].Height;
          int dstPixelSize = dstStride / data.Size.Width,
            srcPixelSize = srcStride / srcWidth;
          int dstX = this.rects[i].X - minX,
            dstY = this.rects[i].Y - minY;

          for (int y = 0; y < srcHeight; y++) {
            Utilities.CopyMemory(IntPtr.Add(dstScan0,
                dstPixelSize * dstX + (y + dstY) * dstStride),
              IntPtr.Add(srcScan0, y * srcStride),
              srcPixelSize * srcWidth);
          }

          Surfaces[i].UnlockRectangle();
        }

        return new BitmapData {
          Width = CaptureBounds.Width,
          Height = CaptureBounds.Height,
          PixelFormat = bmp.PixelFormat,
          Scan0 = data.Data.DataPointer,
          Stride = data.Stride,
          LockPointer = data.NativePointer
        };
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases the bitmap created for the last frame
    /// </summary>
    /// <param name="data"></param>
    public override void UnlockFrameBitmap(BitmapData data) =>
      new BitmapLock(data.LockPointer).Dispose();

    /// <inheritdoc />
    /// <summary>
    ///   Updates the video provider capture position
    /// </summary>
    /// <remarks>
    ///   TODO: Implement this feature - it may seem trivial but when moving *across* different display devices,
    ///   textures may have to be created each time the position is updated (!) Find a way to performantly
    ///   achieve this (e.g. find a way to use a global, shared texture the size of the actual region instead of
    ///   splitting this into different textures for each output device)
    /// </remarks>
    /// <param name="x">New X axis value</param>
    /// <param name="y">New Y axis value</param>
    public override void UpdatePosition(int x, int y) => throw new NotImplementedException();

    /// <inheritdoc />
    /// <summary>
    ///   Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public override void Dispose() => this.direct3d?.Dispose();
  }
}