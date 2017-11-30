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
using SharpDX.Multimedia;

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
    ///   Straem index.
    /// </summary>
    private int streamIdx;

    /// <summary>
    ///   Current frame index.
    /// </summary>
    private long frameIdx;

    /// <summary>
    ///   DXGI device manager instance.
    /// </summary>
    private DXGIDeviceManager dxgiManager;

    /// <summary>
    ///   Recording start time.
    /// </summary>
    private DateTime startTime;

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
          this.startTime = DateTime.Now;
        }
      }

      if (SourceTexture != null) {
        // DXGI buffer
        MediaFactory.CreateDXGISurfaceBuffer(SourceTexture.GetType().GUID,
          SourceTexture,
          0,
          new RawBool(false),
          out this.buffer);
      } else {
        // generic 2D media buffer
        MediaFactory.Create2DMediaBuffer(frameSize.Width,
          frameSize.Height,
          new FourCC((int) Format.A8R8G8B8),
          new RawBool(false),
          out this.buffer);
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a video frame.
    /// </summary>
    /// <param name="data">Bitmap data.</param>
    /// <param name="stream">Output stream.</param>
    public void Encode(BitmapData data, Stream stream) {
      Sample sample = null;
      DateTime now = DateTime.Now;

      if (SourceTexture is null) {
        sample = MediaFactory.CreateSample();
        sample.SampleTime = (long) (DateTime.Now - this.startTime).TotalMilliseconds * 10_000;

        int length = data.Stride * data.Height;

        // copy data
        Utilities.CopyMemory(this.buffer.Lock(out _, out _), data.Scan0, length);

        // buffer is full
        this.buffer.CurrentLength = length;

        // unlock bits
        this.buffer.Unlock();
      } else if (this.buffer.QueryInterfaceOrNull<Buffer2D>() is Buffer2D mediaBuffer2D) {
        //MediaFactory.CreateVideoSampleFromSurface(SourceTexture, out sample);
        Evr.CreateVideoSampleFromSurface(SourceTexture, out sample);

        // remove buffers added by MFCreateVideoSampleFromSurface (???)
        sample.RemoveAllBuffers();
        sample.SampleTime = (long) (DateTime.Now - this.startTime).TotalMilliseconds * 10_000;

        // buffer now has data
        this.buffer.CurrentLength = mediaBuffer2D.ContiguousLength;
      }

      // add buffer to the sample and write it
      sample?.AddBuffer(this.buffer);
      this.sinkWriter.WriteSample(this.streamIdx, sample);

      sample?.Dispose();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Finalizes video encoding.
    /// </summary>
    /// <param name="stream">Output stream.</param>
    public void Finalize(Stream stream) {
      this.frameIdx = 0;
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