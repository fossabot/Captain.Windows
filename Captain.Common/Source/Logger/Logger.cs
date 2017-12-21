﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

namespace Captain.Common {
  /// <summary>
  ///   Provides basic logging facilities to applications
  /// </summary>
  public sealed class Logger {
    /// <summary>
    ///   Number of stack frames to skip for retrieving calling method on log messages
    /// </summary>
    private const uint SkipFrames = 1;

    /// <summary>
    ///   Number of ticks transcurred before the current log message
    /// </summary>
    private long previousTicks = -1;

    /// <summary>
    ///   Provides a list of streams where each log message is to be forwarded
    /// </summary>
    public List<Stream> Streams { get; } = new List<Stream>();
    
    /// <summary>
    ///   Writes a message to the logger
    /// </summary>
    /// <remarks>
    ///   TODO: revise ALL application logging and make sure we output *useful* information for real problem diagnosis
    /// </remarks>
    /// <param name="level">Logging level</param>
    /// <param name="format">Message format, compatible with String.Format()</param>
    /// <param name="args">Arguments used to format the message</param>
    public void WriteLine(LogLevel level, object format, params object[] args) {
      /*#if !DEBUG
            if (level == LogLevel.Debug) {
              // do not print debug messages on non-debug configurations
              return;
            }
      #endif*/

      // ticks to be compared for displaying time diff
      long ticks = DateTime.Now.Ticks;

      // get calling method
      MethodBase method = new StackFrame((int)SkipFrames).GetMethod();
      string methodName;

      // format method as "<enclosing type>::<method name>"
      if (method is null || method.DeclaringType is null) {
        methodName = "????";
      } else {
        methodName = $"{method.DeclaringType.Name}.{method.Name}";
      }

      // format message as "[<milliseconds diff> <level> <method>] <message>"
      // ReSharper disable once UseStringInterpolation
      string msg = String.Format("{0:00.0000} {1} {2}",
                                 TimeSpan.FromTicks(this.previousTicks == -1 ? 0 : ticks - this.previousTicks).TotalSeconds,  // zero if no previous tick count is set, otherwise the difference of ticks
                                 level.ToShortString(),
                                 methodName);
      string body = String.Format(format?.ToString() ?? "<null>", args) + Environment.NewLine;
      Console.ForegroundColor = level.GetAssociatedConsoleColor();
      Console.Write(msg = $"[{msg}] {body}");

      // update ticks for next message
      this.previousTicks = ticks;

      // write message to each stream
      Streams.ForEach(stream => {
        // TODO: queue stream writing so that logger does not slow down overall application performance
        stream.Write(Encoding.UTF8.GetBytes(msg), 0, msg.Length);
        stream.FlushAsync();
      });
    }
  }
}
