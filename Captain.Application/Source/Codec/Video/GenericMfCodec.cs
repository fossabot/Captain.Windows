using System;
using System.Drawing;
using System.IO;
using Captain.Application.Native;
using Captain.Common;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Direct3D9;
using SharpDX.Mathematics.Interop;
using SharpDX.MediaFoundation;
using FourCC = SharpDX.Multimedia.FourCC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides a dummy Media Foundation wrapper.
  /// </summary>
  internal abstract class GenericMfCodec : IVideoCodec {
    /// <summary>
    ///   MFSTARTUP_NOSOCKET: flag for MediaFoundation.Startup()
    /// </summary>
    private const int NoSocket = 1;

    /// <summary>
    ///   Byte stream for writing data.
    /// </summary>
    private ByteStream byteStream;

    /// <summary>
    ///   MF sink writer.
    /// </summary>
    private SinkWriter sinkWriter;

    /// <summary>
    ///   Media buffer
    /// </summary>
    private MediaBuffer buffer;

    /// <summary>
    ///   Current MF sample.
    /// </summary>
    private Sample sample;

    /// <summary>
    ///   Straem index.
    /// </summary>
    private int streamIdx;

    /// <summary>
    ///   DXGI device manager instance.
    /// </summary>
    private DXGIDeviceManager dxgiManager;

    /// <summary>
    ///   Current frame index.
    /// </summary>
    private long frameIdx;

    /// <summary>
    ///   Value for the TranscodeContainertype media attribute.
    /// </summary>
    public abstract Guid ContainerType { get; }

    /// <summary>
    ///   Video format GUID.
    /// </summary>
    public abstract Guid VideoFormat { get; }

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public abstract string FileExtension { get; }

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

    /// <summary>
    ///   Source DXGI texture.
    /// </summary>
    public Texture2D SourceTexture { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Begins encoding the video.
    /// </summary>
    /// <param name="frameSize">Frame size.</param>
    /// <param name="stream">Output stream.</param>
    public void Initialize(Size frameSize, Stream stream) {
      MediaFactory.Startup(MediaFactory.Version, NoSocket);

      using (var attrs = new MediaAttributes()) {
        // enable hardware transforms
        attrs.Set(TranscodeAttributeKeys.TranscodeContainertype, ContainerType);
        attrs.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);
        attrs.Set(SinkWriterAttributeKeys.LowLatency, true);

        if (SourceTexture != null) {
          // create and bind a DXGI device manager
          this.dxgiManager = new DXGIDeviceManager();
          this.dxgiManager.ResetDevice(SourceTexture.Device);

          attrs.Set(SinkWriterAttributeKeys.D3DManager, this.dxgiManager);
        }

        // create byte stream and sink writer
        this.byteStream = new ByteStream(stream);
        this.sinkWriter = MediaFactory.CreateSinkWriterFromURL(null, this.byteStream.NativePointer, attrs);

        // create media types
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

      if (SourceTexture != null) {
        // DXGI buffer
        MediaFactory.CreateDXGISurfaceBuffer(SourceTexture.GetType().GUID,
          SourceTexture,
          0,
          new RawBool(false),
          out this.buffer);

        this.buffer.CurrentLength = this.buffer.QueryInterface<Buffer2D>().ContiguousLength;
        Evr.CreateVideoSampleFromSurface(SourceTexture, out this.sample);
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
      if (SourceTexture == null) {
        this.sample?.Dispose();
        this.buffer?.Dispose();

        MediaFactory.Create2DMediaBuffer(data.Width,
          data.Height,
          new FourCC((int) Format.A8R8G8B8),
          new RawBool(false),
          out this.buffer);

        this.sample = MediaFactory.CreateSample();
        this.sample.SampleTime = time;

        int length = data.Stride * data.Height;

        // copy data
        Utilities.CopyMemory(this.buffer.Lock(out _, out _), data.Scan0, length);

        // buffer is full
        this.buffer.CurrentLength = length;

        // unlock bits
        this.buffer.Unlock();

        // add buffer to the sample
        this.sample?.AddBuffer(this.buffer);
      } else {
        this.sample.SampleTime = time;
      }

      // write the sample to the output stream
      this.sinkWriter.WriteSample(this.streamIdx, this.sample);
      this.frameIdx++;
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
      this.byteStream?.Dispose();
      this.sinkWriter?.Dispose();
      this.dxgiManager?.Dispose();
      this.buffer?.Dispose();

      MediaFactory.Shutdown();
    }
  }
}