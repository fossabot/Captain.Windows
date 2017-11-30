using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Captain.Common;
using SharpDX;
using SharpDX.WIC;

namespace Captain.Application {
  /// <inheritdoc cref="IStillImageCodec" />
  /// <summary>
  ///   JPEG image encoder
  /// </summary>
  [DisplayName("JPEG")]
  internal sealed class JpegWicCodec : IStillImageCodec, IHasOptions {
    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object> {
      {"Quality", 0.9},
      {"Transform", 0},
      {"ChromaSubsampling", JpegYCrCbSubsamplingOption.Default},
      {"NoApp0", false}
    };

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public string FileExtension => ".jpg";

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a still image
    /// </summary>
    /// <param name="data">Bitmap data</param>
    /// <param name="stream">Output stream</param>
    public void Encode(BitmapData data, Stream stream) {
      using (var factory = new ImagingFactory()) {
        using (var encoder = new BitmapEncoder(factory, ContainerFormatGuids.Jpeg)) {
          encoder.Initialize(stream);

          using (var frame = new BitmapFrameEncode(encoder)) {
            frame.Options.ImageQuality = (float) (double) Options["Quality"];
            frame.Options.BitmapTransform =
              ((BitmapTransformOptions[]) Enum.GetValues(typeof(BitmapTransformOptions)))[
                Convert.ToInt32(Options["Transform"])];
            frame.Options.JpegYCrCbSubsampling = (JpegYCrCbSubsamplingOption) Convert.ToInt32(Options["ChromaSubsampling"]);
            frame.Options.SuppressApp0 = (bool) Options["NoApp0"];
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
    ///   Displays an interface for letting the user configure this object
    /// </summary>
    /// <param name="ownerWindow">If the interface makes use of dialogs, an instance of the owner window</param>
    public DialogResult DisplayOptionsInterface(IWin32Window ownerWindow) {
      using (var optionsWindow = new JpegWicCodecOptionsWindow(Options ?? new Dictionary<string, object>())) {
        DialogResult result = optionsWindow.ShowDialog(ownerWindow);
        Options = optionsWindow.Options;
        return result;
      }
    }
  }
}