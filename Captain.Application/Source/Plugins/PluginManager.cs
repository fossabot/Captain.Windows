using Captain.Common;
using Captain.Plugins.BuiltIn;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Handles Plugins
  /// </summary>
  internal class PluginManager {
    private const string CommonAssemblyName = "Captain.Common";

    internal List<PluginObject> StaticEncoders { get; }
    internal List<PluginObject> VideoEncoders { get; }
    internal List<PluginObject> OutputStreams { get; }

    /// <summary>
    ///   Initializes the Plugin manager
    /// </summary>
    internal PluginManager() {
      // get all Plugin assemblies
      var assemblyLocations = Directory.EnumerateFiles(Application.FsManager.GetSafePath(FsManager.PluginPath))
                                       .ToList();

      if (Assembly.GetExecutingAssembly().ExportedTypes.Any(type => type.Namespace == CommonAssemblyName)) {
        // the executing assembly was merged with the Captain.Common.dll assembly which contains shared types for
        // Plugins. Because it is unlikely that the DLL exists, hook the intent to load the DLL and resolve to the
        // executing module instead
        AppDomain.CurrentDomain.AssemblyResolve += (sender, resolveEventArgs) => {
          if (assemblyLocations.Contains(resolveEventArgs.RequestingAssembly.Location) &&
              resolveEventArgs.Name.Split(',')[0].StartsWith(CommonAssemblyName)) {
            return Assembly.GetExecutingAssembly();
          }

          return Assembly.Load(resolveEventArgs.Name);
        };
      }

      var assemblies = new List<Assembly>();

      try {
        assemblies.AddRange(assemblyLocations.Select(Assembly.Load));
      } catch (Exception exception) {
        Log.WriteLine(LogLevel.Error, $"could not load assemblies: {exception}");
      }

      StaticEncoders = new List<PluginObject>();
      VideoEncoders = new List<PluginObject>();
      OutputStreams = new List<PluginObject>();

      // manually add built-in encoders/handlers. We could feed the assembly and let them be discovered, but this is
      // cheaper and faster, since we already know which types are exported
      StaticEncoders.Add(new PluginObject(typeof(PngCaptureEncoder)));
      OutputStreams.AddRange(new[] {
        new PluginObject(typeof(FileOutputStream)),
        new PluginObject(typeof(ClipboardOutputStream))
      });


      foreach (Assembly assembly in assemblies) {
        try {
          Type[] types = assembly.GetExportedTypes();

          StaticEncoders.AddRange(types.Where(t => t.GetInterface("IStaticEncoder") != null)
                                       .Select(t => new PluginObject(t)));
          VideoEncoders.AddRange(types.Where(t => t.GetInterface("IVideoEncoder") != null)
                                      .Select(t => new PluginObject(t)));
          OutputStreams.AddRange(types.Where(t => t.GetInterface("IOutputStream") != null &&
                                                  t.GetNestedType("Stream") != null)
                                      .Select(t => new PluginObject(t)));
        } catch (Exception exception) {
          Log.WriteLine(LogLevel.Error, $"could not initialize plugin {assembly.FullName}: {exception}");
        }
      }

      if (!StaticEncoders.Any() && !VideoEncoders.Any() || !OutputStreams.Any()) {
        Log.WriteLine(LogLevel.Error, "no encoders or output streams found - aborting program!");
        Exit(1);
      }
    }
  }
}
