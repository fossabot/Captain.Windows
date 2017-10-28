using System;
using System.Runtime.InteropServices;

// ReSharper disable All
namespace Captain.Application.Native {
  [StructLayout(LayoutKind.Sequential)]
  internal struct NOTIFYICONIDENTIFIER {
    /// <summary>
    ///   The size of this structure, in bytes.
    /// </summary>
    internal int cbSize;

    /// <summary>
    ///   A handle to the parent window use by the notification's callback function.
    /// </summary>
    internal IntPtr hWnd;

    /// <summary>
    ///   The application-defined identifier of the notification icon.
    /// </summary>
    internal int uID;

    /// <summary>
    ///   A registered GUID that identifies the icon.
    /// </summary>
    internal Guid guidItem;
  }
}
