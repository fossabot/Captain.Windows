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
  [DisplayName("PNG")]
  internal sealed class PngWicCodec : IStillImageCodec, IHasOptions {
    /// <inheritdoc />
    /// <summary>
    ///   Extra data associated with this object that is to be saved to the user settings
    /// </summary>
    public Dictionary<string, object> Options { get; set; } = new Dictionary<string, object> {
      {"Filter", PngFilterOption.Adaptive},
      {"Interlaced", false}
    };

    /// <inheritdoc />
    /// <summary>
    ///   File extension for this codec
    /// </summary>
    public string FileExtension => ".png";

    /// <inheritdoc />
    /// <summary>
    ///   Encodes a still image
    /// </summary>
    /// <param name="data">Bitmap data</param>
    /// <param name="stream">Output stream</param>
    public void Encode(BitmapData data, Stream stream) {
      using (var factory = new ImagingFactory()) {
        using (var encoder = new BitmapEncoder(factory, ContainerFormatGuids.Png)) {
          encoder.Initialize(stream);

          using (var frame = new BitmapFrameEncode(encoder)) {
            frame.Options.FilterOption = (PngFilterOption) Convert.ToInt32(Options["Filter"]);
            frame.Options.InterlaceOption = (bool) Options["Interlaced"];
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
      using (var optionsWindow = new PngWicCodecOptionsWindow(Options ?? new Dictionary<string, object>())) {
        DialogResult result = optionsWindow.ShowDialog(ownerWindow);
        Options = optionsWindow.Options;
        return result;
      }
    }
  }
}