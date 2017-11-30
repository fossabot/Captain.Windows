using System;
using Captain.Common;
using SharpDX;
using Rectangle = System.Drawing.Rectangle;

namespace Captain.Application {
  internal static class VideoProviderFactory {
    /// <summary>
    ///   Creates a generic <see cref="VideoProvider"/> instance using the most suitable capture method.
    /// </summary>
    /// <param name="bounds">Capture bounds</param>
    /// <param name="windowHandle">Attached window handle</param>
    /// <param name="createSharedTexture">True to let specific video providers create a single texture for the capture.</param>
    /// <returns>An instance of <see cref="VideoProvider"/></returns>
    internal static VideoProvider Create(Rectangle bounds, IntPtr? windowHandle = null, bool createSharedTexture = false) {
      if (Environment.OSVersion.Version >= new Version(6, 2)) {
        try {
          // use DXGI desktop duplication on Windows 8 and greater
          return new DxgiVideoProvider(bounds, windowHandle, createSharedTexture);
        } catch (NotSupportedException) { } catch (SharpDXException) { }
      }

      // fallback to DirectX
      return new DxVideoProvider(bounds, windowHandle);
    }
  }
}