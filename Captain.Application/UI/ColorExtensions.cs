using System.Drawing;

namespace Captain.Application {
  internal static class ColorExtensions {
    /// <summary>Blends the specified colors together.</summary>
    /// <param name="color">Color to blend onto the background color.</param>
    /// <param name="backColor">Color to blend the other color onto.</param>
    /// <param name="amount">
    ///   How much of <paramref name="color"/> to keep,
    ///   “on top of” <paramref name="backColor"/>.
    /// </param>
    /// <returns>The blended colors.</returns>
    /// <remarks>
    ///   This extension method is taken from https://stackoverflow.com/a/3722337
    /// </remarks>
    internal static Color Blend(this Color color, Color backColor, double amount) =>
      Color.FromArgb((byte)((color.R * amount) + backColor.R * (1 - amount)),
                     (byte)((color.G * amount) + backColor.G * (1 - amount)),
                     (byte)((color.B * amount) + backColor.B * (1 - amount)));
  }
}
