using System;
using System.Collections.Generic;
using System.Linq;

namespace Captain.Application {
  /// <summary>
  ///   Performs common variable substitutions on a string
  /// </summary>
  internal static class TemplateHelper {
    /// <summary>
    ///   Common substitution templates
    /// </summary>
    private static readonly Dictionary<CommonVariable, Func<object>> Templates =
      new Dictionary<CommonVariable, Func<object>> {
        {CommonVariable.Year, () => DateTime.Now.Year},
        {CommonVariable.ShortYear, () => DateTime.Now.Year.ToString().Substring(2)},
        {CommonVariable.Month, () => DateTime.Now.Month.ToString("D2")},
        {CommonVariable.Day, () => DateTime.Now.Day.ToString("D2")},
        {CommonVariable.Hour, () => DateTime.Now.Hour.ToString("D2")},
        {CommonVariable.Minute, () => DateTime.Now.Minute.ToString("D2")},
        {CommonVariable.Second, () => DateTime.Now.Second.ToString("D2")}
      };

    /// <summary>
    ///   Normalizes a localized template string
    /// </summary>
    /// <param name="localizedInputTemplate">Input template string</param>
    /// <returns>The normalized template string</returns>
    internal static string Normalize(string localizedInputTemplate) =>
      Templates.Aggregate(localizedInputTemplate,
        (current, template) =>
          current.Replace("(" + Resources.ResourceManager.GetString("CommonVariable_" + template.Key) + ")",
            "{" + template.Key.ToString("d") + "}"));

    /// <summary>
    ///   Localizes a normalized template string
    /// </summary>
    /// <param name="normalizedInputTemplate">Input template string</param>
    /// <returns>The localized template string</returns>
    internal static string Localize(string normalizedInputTemplate) =>
      Templates.Aggregate(normalizedInputTemplate,
        (current, template) =>
          current.Replace("{" + template.Key.ToString("d") + "}",
            "{" + Resources.ResourceManager.GetString("CommonVariable_" + template.Key) + "}"));

    /// <summary>
    ///   Performs replacements in the input template string
    /// </summary>
    /// <param name="inputTemplate">Normalized template string</param>
    /// <returns>The substituted string</returns>
    internal static string GetString(string inputTemplate) =>
      Templates.Aggregate(inputTemplate,
        (current, template) =>
          current.Replace("{" + template.Key.ToString("d") + "}", template.Value().ToString()));
  }
}