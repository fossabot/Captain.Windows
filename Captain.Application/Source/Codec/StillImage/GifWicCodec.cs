using System;
using Captain.Common;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   PNG image encoder
  /// </summary>
  [DisplayName("GIF")]
  internal sealed class GifWicCodec : GenericWicCodec {
    /// <inheritdoc />
    /// <summary>
    ///   Container format GUID
    /// </summary>
    protected override Guid ContainerFormat => ContainerFormatGuids.Gif;

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public override string FileExtension => ".gif";
  }
}