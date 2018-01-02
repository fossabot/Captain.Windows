using System;
using System.IO;
using Captain.Common;
using SharpDX;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Generic WIC image encoder
  /// </summary>
  /// <remarks>
  ///   TODO: implement ID3D11Codec and perform encoding on GPU?
  /// </remarks>
  internal abstract class GenericWicCodec : IStillImageCodec {
    /// <summary>
    ///   Container format GUID
    /// </summary>
    protected abstract Guid ContainerFormat { get; }

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public abstract string FileExtension { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a still image
    /// </summary>
    /// <param name="data">Bitmap data</param>
    /// <param name="stream">Output stream</param>
    public virtual void Encode(BitmapData data, Stream stream) {
      using (var factory = new ImagingFactory()) {
        using (var encoder = new BitmapEncoder(factory, ContainerFormat)) {
          encoder.Initialize(stream);

          using (var frame = new BitmapFrameEncode(encoder)) {
            frame.Initialize();

            using (var bitmap = new Bitmap(factory,
              data.Width,
              data.Height,
              data.PixelFormat,
              new DataRectangle(data.Scan0, data.Stride),
              data.Height * data.Stride)) {
              frame.WriteSource(bitmap);
              frame.Commit();
            }

            encoder.Commit();
          }
        }
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Releases resources.
    /// </summary>
    public void Dispose() { }
  }
}