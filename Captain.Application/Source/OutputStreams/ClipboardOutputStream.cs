﻿using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using Captain.Common;

namespace Captain.Application {
  [DisplayName("Copy to clipboard"), ThreadApartmentState(ApartmentState.STA)]
  internal sealed class ClipboardOutputStream : MemoryStream, IOutputStream, IWithCustomImage {
    /// <inheritdoc />
    /// <summary>
    ///   Encoder information passed to this output stream
    /// </summary>
    public EncoderInfo EncoderInfo { get; set; }

    /// <inheritdoc />
    /// <summary>
    ///   Class constructor.
    ///   Makes sure the media is supported
    /// </summary>
    public ClipboardOutputStream() {
      if (!EncoderInfo.MediaType.StartsWith("image/", StringComparison.OrdinalIgnoreCase)) {
        throw new NotSupportedException("This media type is not supported.");
      }
    }

    /// <inheritdoc />
    /// <summary>
    ///   Called when the data has been successfully copied to this output stream
    /// </summary>
    /// <returns>A <see cref="T:Captain.Common.CaptureResult" /> instance containing result information</returns>
    public CaptureResult Commit() {
      Clipboard.SetImage(Image.FromStream(this));
      return new CaptureResult();
    }

    /// <summary>
    ///   Retrieves a custom image to be displayed alongside this plugin
    /// </summary>
    /// <returns>An <see cref="Image"/> instance</returns>
    public Image GetCustomImage() => Resources.CopyToClipboard;
  }
}