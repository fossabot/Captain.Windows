using System;
using Captain.Common;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   PNG image encoder
  /// </summary>
  [DisplayName("PNG")]
  internal sealed class PngWicCodec : GenericWicCodec {
    /// <inheritdoc />
    /// <summary>
    ///   Container format GUID
    /// </summary>
    protected override Guid ContainerFormat => ContainerFormatGuids.Png;
  }
}