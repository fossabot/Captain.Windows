using System;

namespace Captain.Common {
  /// <summary>
  ///   Defines levels for log messages, in ascendant order of precedence
  /// </summary>
  public enum LogLevel : ushort {
    /// <summary>
    ///   Error message
    /// </summary>
    Error = 0,

    /// <summary>
    ///   Warning message
    /// </summary>
    Warning = 1,

    /// <summary>
    ///   Informational message
    /// </summary>
    Informational = 2,

    /// <summary>
    ///   Verbose message
    /// </summary>
    Verbose = 3,

    /// <summary>
    ///   Message only shown when DEBUG is defined
    /// </summary>
    Debug = 4
  }

  /// <summary>
  ///   Provides extension methods to the LogLevel struct
  /// </summary>
  internal static class LogLevelExtensions {
    /// <summary>
    ///   Get the short string representing the log level associated to a message
    /// </summary>
    /// <param name="level">This log level</param>
    /// <returns>
    ///   A string of a maximum of 5 characters representing the log level, or an empty string if no such level is
    ///   implemented
    /// </returns>
    internal static string ToShortString(this LogLevel level) {
      switch (level) {
        case LogLevel.Error: return "FAIL";

        case LogLevel.Warning: return "WARN";

        case LogLevel.Informational: return "INFO";

        case LogLevel.Verbose: return "VERB";

        case LogLevel.Debug: return "DBUG";
      }

      return String.Empty;
    }

    /// <summary>
    ///   Get the <see cref="ConsoleColor" /> associated to a particular log level
    /// </summary>
    /// <param name="level">This log level</param>
    /// <returns>
    ///   The <see cref="ConsoleColor" /> associated to this log level or <c>ConsoleColor.Gray</c> if no such level is
    ///   implemented
    /// </returns>
    internal static ConsoleColor GetAssociatedConsoleColor(this LogLevel level) {
      switch (level) {
        case LogLevel.Error: return ConsoleColor.Red;

        case LogLevel.Warning: return ConsoleColor.Yellow;

        case LogLevel.Informational: return ConsoleColor.Cyan;

        case LogLevel.Verbose: return ConsoleColor.Gray;

        case LogLevel.Debug: return ConsoleColor.DarkGray;
      }

      return ConsoleColor.Gray;
    }
  }
}