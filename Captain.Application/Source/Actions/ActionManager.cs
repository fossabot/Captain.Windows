using System;
using System.Linq;

namespace Captain.Application {
  internal class ActionManager {
    /// <summary>
    ///   Creates an action with the default parameterrs
    /// </summary>
    /// <returns>An <see cref="Action"/> instance, guaranteed</returns>
    internal static Action CreateDefault() {
      PluginObject staticEncoder = null, videoEncoder = null;

      // it is not guaranteed that we have at least one static encoder and one video encoder, we may have one of both
      // so we have to handle it accordingly
      try { staticEncoder = Application.PluginManager.StaticEncoders.First(); } catch (InvalidOperationException) { }
      try { videoEncoder = Application.PluginManager.VideoEncoders.First(); } catch (InvalidOperationException) { }

      var action = new Action(staticEncoder, videoEncoder);

      // in the other hand, it is guaranteed that at least on capture handler exists
      // action.AddHandler(Application.PluginManager.CaptureHandlers.First());
      Application.PluginManager.OutputStreams.ForEach(action.AddOutputStream);
      return action;
    }
  }
}
