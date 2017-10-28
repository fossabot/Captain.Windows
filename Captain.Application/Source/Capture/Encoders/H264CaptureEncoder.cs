using System;
using System.Drawing;
using System.IO;
using Captain.Common;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides a generic H.264 video encoder
  /// </summary>
  internal sealed class H264CaptureEncoder : IVideoEncoder {
    /// <summary>
    ///   Byte stream for writing data
    /// </summary>
    private ByteStream byteStream;

    /// <summary>
    ///   MediaFoundation sink writer
    /// </summary>
    private SinkWriter sinkWriter;

    /// <summary>
    ///   Stream index
    /// </summary>
    private int streamIdx;

    /// <summary>
    ///   Time counter
    /// </summary>
    private long time;

    /// <inheritdoc />
    /// <summary>
    ///   Contains information about the encoder
    /// </summary>
    public EncoderInfo EncoderInfo => new EncoderInfo {
      Extension = "mp4",
      MediaType = "video/mp4"
    };

    /// <inheritdoc />
    /// <summary>
    ///   Begins encoding the video
    /// </summary>
    /// <param name="frameSize">Frame size</param>
    /// <param name="outputStream">Destination stream</param>
    public void Start(Size frameSize, Stream outputStream) {
      MediaFactory.Startup(MediaFactory.Version, 0);

      // enable hardware encoders (transforms)
      var attrs = new MediaAttributes();
      attrs.Set(TranscodeAttributeKeys.TranscodeContainertype, TranscodeContainerTypeGuids.Mpeg4);
      attrs.Set(SinkWriterAttributeKeys.ReadwriteEnableHardwareTransforms, 1);

      // create byte stream and sink writer
      this.byteStream = new ByteStream(outputStream);
      this.sinkWriter = MediaFactory.CreateSinkWriterFromURL(null, this.byteStream.NativePointer, attrs);

      var outMediaType = new MediaType();
      outMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
      outMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.FromFourCC(new FourCC("H264")));
      outMediaType.Set(MediaTypeAttributeKeys.AvgBitrate, 3000000);
      outMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
      outMediaType.Set(MediaTypeAttributeKeys.FrameSize,
                       ((long) frameSize.Width << 32) | (uint) frameSize.Height);
      outMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) 30 << 32) | 1);
      outMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | (uint) 1);

      this.sinkWriter.AddStream(outMediaType, out this.streamIdx);

      var inMediaType = new MediaType();
      inMediaType.Set(MediaTypeAttributeKeys.MajorType, MediaTypeGuids.Video);
      inMediaType.Set(MediaTypeAttributeKeys.Subtype, VideoFormatGuids.Rgb32);
      inMediaType.Set(MediaTypeAttributeKeys.InterlaceMode, (int) VideoInterlaceMode.Progressive);
      inMediaType.Set(MediaTypeAttributeKeys.FrameSize,
                      ((long) frameSize.Width << 32) | (uint) frameSize.Height);
      inMediaType.Set(MediaTypeAttributeKeys.FrameRate, ((long) 30 << 32) | 1);
      inMediaType.Set(MediaTypeAttributeKeys.PixelAspectRatio, (1 << 32) | (uint) 1);

      this.sinkWriter.SetInputMediaType(this.streamIdx, inMediaType, null);
      this.sinkWriter.BeginWriting();

      inMediaType.Dispose();
      outMediaType.Dispose();
      attrs.Dispose();
    }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a single frame
    ///   TODO: Implement some sort of (optional) AudioProvider support
    /// </summary>
    /// <param name="videoProvider">Video capture provider</param>
    /// <param name="outputStream">Destination stream</param>
    public void Encode(VideoProvider videoProvider, Stream outputStream) {
      if (videoProvider is DxgiVideoProvider dxgiProvider) {
        dxgiProvider.AcquireFrame();
        Sample sample = MediaFactory.CreateSample();

        // TODO: support for multiple monitors
        MediaFactory.CreateDXGISurfaceBuffer(Utilities.GetGuidFromType(typeof(Texture2D)),
                                             dxgiProvider.StagingTextures[0],
                                             0,
                                             false,
                                             out MediaBuffer buf);

        // map textures to system memory
        DataBox map = dxgiProvider.StagingTextures[0]
                                  .Device.ImmediateContext
                                  .MapSubresource(dxgiProvider.StagingTextures[0], 0, MapMode.Read, MapFlags.None);

        sample.SampleDuration = (10 * 1000 * 1000) / 30;
        sample.SampleTime = this.time += sample.SampleDuration;
        sample.AddBuffer(buf);

        Utilities.CopyMemory(buf.Lock(out int _, out int _),
                             map.DataPointer,
                             map.RowPitch);

        buf.CurrentLength = map.RowPitch;
        buf.Unlock();

        dxgiProvider.StagingTextures[0].Device.ImmediateContext.UnmapSubresource(dxgiProvider.StagingTextures[0], 0);
        dxgiProvider.ReleaseFrame();

        this.sinkWriter.WriteSample(this.streamIdx, sample);

        buf.Dispose();
        sample.Dispose();
      } else {
        // TODO: create buffer from Bitmap
        throw new NotImplementedException();
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Terminates the capture encoding
    /// </summary>
    /// <param name="outputStream">Destination stream</param>
    public void End(Stream outputStream) {
      this.sinkWriter?.Flush(this.streamIdx);
      this.sinkWriter?.Finalize();
      this.byteStream?.Flush();
      this.byteStream?.Dispose();
      this.sinkWriter?.Dispose();
      MediaFactory.Shutdown();
    }
  }
}