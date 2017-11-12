using System;
using System.Globalization;
using System.Linq;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Plugin object (can be a capture handler, encoder, etc.)
  /// </summary>
  [Serializable]
  internal sealed class PluginObject {
    /// <summary>
    ///   Display name
    /// </summary>
    private readonly string displayName;

    /// <summary>
    ///   Plugin object type
    /// </summary>
    internal Type Type { get; }

    /// <summary>
    ///   Whether this object is configurable
    /// </summary>
    internal bool Configurable { get; }

    /// <summary>
    ///   Instantiates a new plugin object
    /// </summary>
    /// <param name="type">Type name</param>
    internal PluginObject(Type type) {
      Type = type;
      Configurable = type.GetInterface("IHasOptions`1") != null;

      try {
        // get localized display name, preferring the one most close to the current UI locale
        this.displayName =
          type.GetCustomAttributes(typeof(DisplayName), true)
            .Cast<DisplayName>()
            .OrderBy(dn => dn.LanguageCode != null &&
                           CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.StartsWith(
                             dn.LanguageCode,
                             StringComparison.OrdinalIgnoreCase))
            .Last()
            ?.Name ??
          throw new InvalidOperationException();
      } catch (InvalidOperationException) {
        // no display name attributes
        this.displayName = type.Name;
      }

      Log.WriteLine(LogLevel.Verbose, $"initialized plugin type: {type.Name}");
    }

    /// <summary>
    ///   Gets the localized display name for this plugin object
    /// </summary>
    /// <returns>The localized display name</returns>
    public override string ToString() => this.displayName;
  }
}