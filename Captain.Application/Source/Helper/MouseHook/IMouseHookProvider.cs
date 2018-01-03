using System;
using System.Windows.Forms;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   A mouse hook provider implements the logic to capture the mouse and intercept its events. A generic interface
  ///   is provided so the logic is independent of the user interface, so it may be used when hooking on DirectInput/
  ///   XInput-enabled applications
  /// </summary>
  internal interface IMouseHookProvider : IDisposable {
    /// <summary>
    ///   Whether or not the mouse is being captured
    /// </summary>
    bool Acquired { get; }

    /// <summary>
    ///   Whether to delegate the event to the original handler.
    /// </summary>
    bool PassThrough { get; set; }

    /// <summary>
    ///   Triggered when a mouse button is held
    /// </summary>
    event MouseEventHandler OnMouseDown;

    /// <summary>
    ///   Triggered when a mouse button is released
    /// </summary>
    event MouseEventHandler OnMouseUp;

    /// <summary>
    ///   Triggered when the mouse moves
    /// </summary>
    event MouseEventHandler OnMouseMove;

    /// <summary>
    ///   Starts capturing mouse events
    /// </summary>
    void Acquire();

    /// <summary>
    ///   Releases the mouse hook
    /// </summary>
    void Release();
  }
}
