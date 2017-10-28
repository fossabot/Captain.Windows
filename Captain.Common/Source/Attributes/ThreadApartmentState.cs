using System;
using System.Threading;

namespace Captain.Common {
  /// <inheritdoc />
  /// <summary>
  ///   Specifies a custom thread apartment state for this object
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public sealed class ThreadApartmentState : Attribute {
    /// <summary>
    ///   Thread apartment state
    /// </summary>
    public ApartmentState ApartmentState { get; }

    /// <inheritdoc />
    /// <summary>
    ///   Sets the thread apartment state for this object
    /// </summary>
    /// <param name="state"></param>
    public ThreadApartmentState(ApartmentState state) => ApartmentState = state;
  }
}