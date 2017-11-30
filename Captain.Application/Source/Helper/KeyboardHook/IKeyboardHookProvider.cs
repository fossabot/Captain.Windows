﻿using System;
using System.Windows.Forms;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   A generic interface for keyboard hooks is provided so the hook logic is independent of the user interface. We
  ///   want this for capturing keyboard events on exclusive-mode applications and normal system UI using a common
  ///   interface.
  /// </summary>
  internal interface IKeyboardHookProvider : IDisposable {
    /// <summary>
    ///   Triggered when a key is held
    /// </summary>
    event KeyEventHandler OnKeyDown;

    /// <summary>
    ///   Triggered when a key is released
    /// </summary>
    event KeyEventHandler OnKeyUp;

    /// <summary>
    ///   Starts capturing keyboard events
    /// </summary>
    void Acquire();

    /// <summary>
    ///   Releases the keyboard hook
    /// </summary>
    void Release();
  }
}