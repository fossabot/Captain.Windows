using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Handles Plugins
  /// </summary>
  internal sealed class PluginManager {
    /// <summary>
    ///   Full name of the Captain Common Library assembly
    /// </summary>
    private const string CommonAssemblyName = "Captain.Common";

    /// <summary>
    ///   Static capture encoders
    /// </summary>
    internal List<PluginObject> StillImageCodecs { get; }

    /// <summary>
    ///   Video capture encoders
    /// </summary>
    internal List<PluginObject> VideoCodecs { get; }

    /// <summary>
    ///   Output streams
    /// </summary>
    internal List<PluginObject> Actions { get; }

    /// <summary>
    ///   Initializes the Plugin manager
    /// </summary>
    internal PluginManager() {
      // get all Plugin assemblies
      var assemblyLocations = Directory.EnumerateFiles(Application.FsManager.GetSafePath(FsManager.PluginPath))
        .ToList();

      // XXX: dead code?
      // TODO: investigate whether loading plugins from the local application path will work without hooking the
      //       AssemblyResolve event
      if (Assembly.GetExecutingAssembly().ExportedTypes.Any(type => type.Namespace == CommonAssemblyName)) {
        // the executing assembly was merged with the Captain.Common.dll assembly which contains shared types for
        // Plugins. Because it is unlikely that the DLL exists, hook the intent to load the DLL and resolve to the
        // executing module instead
        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveEventArgs) => {
          if (assemblyLocations.Contains(resolveEventArgs.RequestingAssembly.Location) &&
              resolveEventArgs.Name.Split(',')[0].StartsWith(CommonAssemblyName, StringComparison.OrdinalIgnoreCase)) {
            return Assembly.GetExecutingAssembly();
          }

          return Assembly.Load(resolveEventArgs.Name);
        };
      }

      var assemblies = new List<Assembly>();
      try { assemblies.AddRange(assemblyLocations.Select(Assembly.Load)); } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not load assemblies: {exception}");
      }

      StillImageCodecs = new List<PluginObject>();
      VideoCodecs = new List<PluginObject>();
      Actions = new List<PluginObject>();

      StillImageCodecs.AddRange(new[] {
        new PluginObject(typeof(BmpWicCodec)),
        new PluginObject(typeof(GifWicCodec)),
        new PluginObject(typeof(JpegWicCodec)),
        new PluginObject(typeof(PngWicCodec)),
        new PluginObject(typeof(TiffWicCodec))
      });

      Actions.AddRange(new[] {
        new PluginObject(typeof(SaveToFileAction)),
        new PluginObject(typeof(CopyToClipboardAction))
      });

      foreach (Assembly assembly in assemblies) {
        try {
          Type[] types = assembly.GetExportedTypes();

          StillImageCodecs.AddRange(
            types.Where(t => t.GetInterface("IStillImageCodec") != null).Select(t => new PluginObject(t)));
          VideoCodecs.AddRange(types.Where(t => t.GetInterface("IVideoCodec") != null)
            .Select(t => new PluginObject(t)));
          Actions.AddRange(types.Where(t => t.GetNestedType("Action") != null).Select(t => new PluginObject(t)));
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"could not initialize plugin {assembly.FullName}: {exception}");
        }
      }
    }
  }
}