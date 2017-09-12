using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Captain.Common;
using EasyHook;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Provides diverse utilities for performing code injection in remote processes
  /// </summary>
  internal class InjectionHelper {
    /// <summary>
    ///   Injects a dynamic library onto the specified remote process
    /// </summary>
    /// <param name="processId">Process ID</param>
    /// <param name="libraryPathX86">32-bit library file path</param>
    /// <param name="libraryPathX64">Optional 64-bit library file path</param>
    /// <param name="data">Optional pointer to the data to be passed (may be IntPtr.Zero)</param>
    /// <param name="dataLength">Length of the data to be passed</param>
    internal static void InjectLibrary(uint processId,
                                       string libraryPathX86,
                                       string libraryPathX64,
                                       IntPtr data,
                                       int dataLength) {
      int status = NativeAPI.RhInjectLibraryEx((int)processId,
                                               0,
                                               NativeAPI.EASYHOOK_INJECT_DEFAULT,
                                               libraryPathX86,
                                               libraryPathX64,
                                               data,
                                               dataLength);

      if (status == NativeAPI.STATUS_WOW_ASSERTION) {
        // can't hook through WoW64 barrier! 
        Log.WriteLine(LogLevel.Warning, "WoW64 bridge is required in order to perform injection");

        /* We'll need to invoke cn2wowbr in order to perform injection */
        // invoke WoW64 bridge for the specified process ID, waking no thread and in stealth mode 
        var startInfo = new ProcessStartInfo("cn2wowbr", $"{processId} 0 stealth {libraryPathX86}") {
          CreateNoWindow = true,
          UseShellExecute = false, // needed for redirecting process handles
          RedirectStandardInput = true // we're writing our pass-through payload to the standard input
        };

        // start process
        var process = new Process { StartInfo = startInfo };
        process.Start();

        for (var i = 0; i < dataLength; i++) {
          // copy data to stdin stream, byte by byte
          // TODO: find out a faster, cleaner way to accomplish this
          process.StandardInput.BaseStream.WriteByte(Marshal.ReadByte(data, i));
        }

        process.StandardInput.BaseStream.Flush();
        process.WaitForExit();

        if (process.ExitCode != 0) {
          Log.WriteLine(LogLevel.Error,
                        $"WoW64 bridge process exited with code 0x{process.ExitCode:x8}: " +
                        NativeAPI.RtlGetLastErrorString());
          throw new Win32Exception(status, NativeAPI.RtlGetLastErrorString());
        }
      } else if (status != NativeAPI.STATUS_SUCCESS) {
        Log.WriteLine(LogLevel.Error,
                      $"RhInjectLibraryEx() failed with error 0x{status:x8}: " +
                      NativeAPI.RtlGetLastErrorString());
        throw new Win32Exception(status, NativeAPI.RtlGetLastErrorString());
      }
    }

    /// <summary>
    ///   Gets the PID of the child console host process for the specified process ID
    /// </summary>
    /// <param name="processId">Target process ID</param>
    /// <returns>The conhost.exe PID on success; otherwise <paramref name="processId" /></returns>
    internal static uint GetConsoleHostProcessId(uint processId) {
      return (uint)Process.GetProcessesByName("conhost")
                          .First(p => Native.Process.FindParentProcessId((uint)p.Id) == (int)processId)
                          .Id;
    }
  }
}
