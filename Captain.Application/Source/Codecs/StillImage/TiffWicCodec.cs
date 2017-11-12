using System;
using Captain.Common;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   TIFF image encoder
  /// </summary>
  [DisplayName("TIFF")]
  internal sealed class TiffWicCodec : GenericWicCodec {
    /// <inheritdoc />
    /// <summary>
    ///   Container format GUID
    /// </summary>
    protected override Guid ContainerFormat => ContainerFormatGuids.Tiff;
  }
}