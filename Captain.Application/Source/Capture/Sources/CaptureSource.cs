using System;
using System.Drawing;

namespace Captain.Application {
  /// <summary>
  ///   Implements a capture source from which frames can be acquired
  ///   TODO: implement audio support
  /// </summary>
  internal abstract class CaptureSource : IDisposable {
    /// <summary>
    ///   Current screen area
    /// </summary>
    internal Rectangle Area { get; }

    /// <summary>
    ///   Instantiates this class
    /// </summary>
    /// <param name="initialArea">Initial screen region</param>
    internal CaptureSource(Rectangle initialArea) => Area = initialArea;

    /// <summary>
    ///   Acquires a single video frame
    /// </summary>
    /// <returns>The frame Bitmap</returns>
    internal abstract Bitmap AcquireVideoFrame();

    /// <summary>
    ///   Releases resources
    /// </summary>
    public abstract void Dispose();
  }
}
