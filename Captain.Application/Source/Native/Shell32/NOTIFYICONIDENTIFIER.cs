// ReSharper disable All

using System;
using System.Runtime.InteropServices;

namespace Captain.Application.Native {
  [StructLayout(LayoutKind.Sequential)]
  internal struct NOTIFYICONIDENTIFIER {
    internal int cbSize;
    internal IntPtr hWnd;
    internal int uID;
    internal Guid guidItem;
  }
}
