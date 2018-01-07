using System;
using Captain.Common;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   BMP image encoder
  /// </summary>
  [DisplayName("BMP")]
  internal sealed class BmpWicCodec : GenericWicCodec {
    /// <inheritdoc />
    /// <summary>
    ///   Container format GUID
    /// </summary>
    protected override Guid ContainerFormat => ContainerFormatGuids.Bmp;

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public override string FileExtension => ".bmp";
  }
}