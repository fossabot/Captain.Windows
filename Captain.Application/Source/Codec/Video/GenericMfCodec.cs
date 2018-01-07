using System;
using System.Drawing;
using System.IO;
using Captain.Common;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace Captain.Application {
  /// <inheritdoc cref="IVideoCodec" />
  /// <summary>
  ///   Provides a dummy Media Foundation wrapper.
  /// </summary>
  internal abstract class GenericMfCodec : IVideoCodec, ID3D11Codec {
    /// <summary>
    ///   MFSTARTUP_NOSOCKET: flag for MediaFoundation.Startup()
    /// </summary>
    private const int NoSocket = 1;

    /// <summary>
    ///   Byte stream for writing data.
    /// </summary>
    private ByteStream byteStream;

    /// <summary>
    ///   DXGI device manager instance.
    /// </summary>
    private DXGIDeviceManager dxgiManager;

    /// <summary>
    ///   MF sink writer.
    /// </summary>
    private SinkWriter sinkWriter;

    /// <summary>
    ///   Stream index.
    /// </summary>
    private int streamIdx;

    /// <summary>
    ///   Source D3D11 surface.
    /// </summary>
    private Texture2D surface;

    /// <summary>
    ///   Value for the TranscodeContainertype media attribute.
    /// </summary>
    protected abstract Guid ContainerType { get; }

    /// <summary>
    ///   Video format GUID.
    /// </summary>
    protected abstract Guid VideoFormat { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Frame rate, in frames per second.
    /// </summary>
    public abstract int FrameRate { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Frame rate, in frames per second.
    /// </summary>
    public abstract int BitRate { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Gets or sets the Direct3D surface pointer to be passed to the underlying encoder.
    /// </summary>
    public IntPtr SurfacePointer { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public abstract string FileExtension { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Begins encoding the video.
    /// </summary>
    /// <param name="frameSize">Frame size.</param>
    /// <param name="stream">Output stream.</param>
    public void Initialize(Size frameSize, Stream stream) {
      MediaFactory.Startup(MediaFactory.Version, NoSocket);

      using (var attrs = new MediaAttributes()) {
        attrs.Set(TranscodeAttributeKeys.TranscodeContainertype, ContainerType);
        attrs.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);
        attrs.Set(SinkWriterAttributeKeys.DisableThrottling, 1);
        attrs.Set(SinkWriterAttributeKeys.LowLatency, true);

        if (SurfacePointer != IntPtr.Zero) {
          // get the source surface
          this.surface = new Texture2D(SurfacePointer);

          // create and bind a DXGI device manager
          this.dxgiManager = new DXGIDeviceManager();
          this.dxgiManager.ResetDevice(this.surface.Device);

          attrs.Set(SinkWriterAttributeKeys.D3DManager, this.dxgiManager);
        }

        // create byte stream and sink writer
        this.byteStream = new ByteStream(stream);
        this.sinkWriter = MediaFactory.CreateSinkWriterFromURL(null, this.byteStream.NativePointer, attrs);

        // create output media type
        using (var outMediaType = new MediaType()) {
          outMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
          outMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormat);
          outMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, BitRate);
          outMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
          outMediaType.Set(MediaTypeAttributeKeys.FrameSize,
            ((long) frameSize.Width << 32) | (uint) frameSize.Height);
          outMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) FrameRate << 32) | 1);
          outMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | (uint) 1);

          this.sinkWriter.AddStream(outMediaType, out this.streamIdx);
        }

        // create input media type
        using (var inMediaType = new MediaType()) {
          inMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
          inMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32);
          inMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
          inMediaType.Set(MediaTypeAttributeKeys.FrameSize,
            ((long) frameSize.Width << 32) | (uint) frameSize.Height);
          inMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) FrameRate << 32) | 1);
          inMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | (uint) 1);

          this.sinkWriter.SetInputMediaType(this.streamIdx, inMediaType, null);
          this.sinkWriter.BeginWriting();
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a video frame.
    /// </summary>
    /// <param name="data">Bitmap data.</param>
    /// <param name="time">Time in ticks.</param>
    /// <param name="stream">Output stream.</param>
    public void Encode(BitmapData data, long time, Stream stream) {
      // create sample
      Sample sample = MediaFactory.CreateSample();
      MediaBuffer buffer;

      if (this.surface == null) {
        // create buffer
        MediaFactory.Create2DMediaBuffer(
          data.Width,
          data.Height,
          new FourCC((int) Format.X8R8G8B8),
          new RawBool(false),
          out buffer);

        // calculate length
        buffer.CurrentLength = data.Stride * data.Height;

        // copy data
        Utilities.CopyMemory(buffer.Lock(out _, out _), data.Scan0, buffer.CurrentLength);

        // unlock bits
        buffer.Unlock();
      } else {
        // create buffer
        MediaFactory.CreateDXGISurfaceBuffer(typeof(Texture2D).GUID,
          this.surface,
          0,
          new RawBool(false),
          out buffer);

        // set buffer length
        buffer.CurrentLength = buffer.QueryInterface<Buffer2D>().ContiguousLength;
      }

      // add buffer to sample
      sample.AddBuffer(buffer);
      sample.SampleTime = time;

      try { this.sinkWriter.WriteSample(this.streamIdx, sample); } catch (SharpDXException) { } finally {
        buffer.Dispose();
        sample.Dispose();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Finalizes video encoding.
    /// </summary>
    /// <param name="stream">Output stream.</param>
    public void Finalize(Stream stream) {
      this.sinkWriter?.Finalize();
      this.byteStream?.Flush();
      stream.Flush();
      Dispose();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources.
    /// </summary>
    public void Dispose() {
      this.byteStream?.Dispose();
      this.sinkWriter?.Dispose();
      this.dxgiManager?.Dispose();
      MediaFactory.Shutdown();

      this.sinkWriter = null;
      this.byteStream = null;
      this.dxgiManager = null;
    }
  }
}