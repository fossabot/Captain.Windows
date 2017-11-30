﻿using System;
using Captain.Common;
using SharpDX.MediaFoundation;
using SharpDX.Multimedia;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   H.264 codec wrapper
  /// </summary>
  [DisplayName("H.265/MPEG-4 HEVC")]
  internal class H265MfCodec : GenericMfCodec {
    /// <inheritdoc />
    /// <summary>
    ///   Value for the TranscodeContainertype media attribute.
    /// </summary>
    public override Guid ContainerType => TranscodeContainerTypeGuids.Mpeg4;

    /// <inheritdoc />
    /// <summary>
    ///   Video format GUID.
    /// </summary>
    public override Guid VideoFormat => VideoFormatGuids.FromFourCC(new FourCC("HEVC"));

    /// <inheritdoc />
    /// <summary>
    ///   Frame rate, in frames per second.
    /// </summary>
    public override int FrameRate => 60;

    /// <inheritdoc />
    /// <summary>
    ///   Frame rate, in frames per second.
    /// </summary>
    public override int BitRate => 8_000_000;

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public override string FileExtension => ".mp4";
  }
}