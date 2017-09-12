using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using Captain.Common;
using Captain.Application.Native;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <summary>
  ///   Provides a user interface for handpicking a screen region and using additional tools
  /// </summary>
  internal class Grabber : IDisposable {
    /// <summary>
    ///   Grabber window (displays the area rectangle)
    /// </summary>
    private GrabberWindow window;

    /// <summary>
    ///   Grabber tool bar (displays editing tools)
    /// </summary>
    private GrabberToolBarWindow toolBar;

    /// <summary>
    ///   Original grabber window bounds
    /// </summary>
    private Rectangle originalBounds;

    /// <summary>
    ///   Holds the currently attached window handle
    /// </summary>
    private IntPtr attachedWindow;

    /// <summary>
    ///   Gets the area selected by the user
    /// </summary>
    internal Rectangle Area => this.window.Area;

    /// <summary>
    ///   Intent receiving delegate
    /// </summary>
    /// <param name="intent">Capture intent instance</param>
    internal delegate void IntentReceivedHandler(object sender, CaptureIntent intent);

    /// <summary>
    ///   Intent received event
    /// </summary>
    internal event IntentReceivedHandler OnIntentReceived;

    /// <summary>
    ///   Creates a new <see cref="Grabber"/> instance
    /// </summary>
    /// <param name="acceptableActionTypes">Acceptable action types</param>
    internal Grabber(ActionType acceptableActionTypes) {
      this.window = new GrabberWindow();
      this.toolBar = new GrabberToolBarWindow(acceptableActionTypes);

      // TODO: restore GrabberWindow bounds
      this.window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      this.window.Width = Screen.PrimaryScreen.WorkingArea.Width / 3d;
      this.window.Height = Screen.PrimaryScreen.WorkingArea.Height / 3d;

      this.window.SizeChanged += UpdateToolBarPosition;
      this.window.LocationChanged += UpdateToolBarPosition;

      this.toolBar.OnCaptureActionInitiated += OnCaptureActionInitiated;
      this.toolBar.OnGrabberIntentReceived += OnGrabberIntentReceived;

      this.window.Closed += (_, __) => Dispose();
      this.toolBar.Closed += (_, __) => Dispose();
    }

    /// <summary>
    ///   Disposes resources used by the grabber interface
    /// </summary>
    public void Dispose() {
      try {
        this.window.Close();
        this.toolBar.Close();
      } catch {
        // one of them was already closed - don't worry babe'
      }

      this.window = null;
      this.toolBar = null;
    }

    /// <summary>
    ///   Displays the grabber UI
    /// </summary>
    /// <returns>Whether or not the capture is being created</returns>
    internal void Show() {
      this.window.Show();
      this.toolBar.Show();
      UpdateToolBarPosition();
    }

    /// <summary>
    ///   Hides the grabber UI
    /// </summary>
    internal void Hide() {
      this.window.Hide();
      this.toolBar.Hide();
    }

    /// <summary>
    ///   Updates the position of the tool bar so it lays above/below the area selection window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="eventArgs"></param>
    private void UpdateToolBarPosition(object sender = null, EventArgs eventArgs = null) {
      if (this.window.Top + this.window.Height + 8 + this.toolBar.Height >
          SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight) {
        this.toolBar.Top = this.window.Top - this.toolBar.Height - 8;
      } else {
        this.toolBar.Top = this.window.Top + this.window.Height + 8;
      }

      this.toolBar.Left =
        Math.Min(Math.Max(SystemParameters.VirtualScreenLeft,
                          this.window.Left + (this.window.Width - this.toolBar.Width) / 2),
                 SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - this.toolBar.Width);
    }

    /// <summary>
    ///   Triggered when the toolbar initiates an action
    /// </summary>
    /// <param name="type">Action type</param>
    private void OnCaptureActionInitiated(ActionType type) =>
      OnIntentReceived?.Invoke(this,
                               new CaptureIntent(type) {
                                 VirtualArea = this.window.Area
                               });

    /// <summary>
    ///   Handles internal grabber intents
    /// </summary>
    /// <param name="intentType">Intent type</param>
    /// <param name="userData">Optional user data</param>
    private void OnGrabberIntentReceived(GrabberIntentType intentType, object userData) {
      Log.WriteLine(LogLevel.Verbose, $"grabber intent received: {intentType}");

      switch (intentType) {
        case GrabberIntentType.Close:
          if (this.attachedWindow != IntPtr.Zero) {
            OnGrabberIntentReceived(GrabberIntentType.DetachFromWindow, null);
          }

          this.window.Close();
          break;

        case GrabberIntentType.AttachToWindow:
          this.toolBar.SetWindowAttachmentStatus(false, false);
          this.window.PassThrough = true;

          /* get the window that contains the central point of the grabber windows */
          var point = new POINT { x = Area.X + Area.Width / 2, y = Area.Y + Area.Height / 2 };
          IntPtr handle = User32.WindowFromPoint(point);

          this.window.PassThrough = false;

          if (handle == IntPtr.Zero || handle == this.window.Handle || handle == this.toolBar.Handle) {
            // don't attach to desktop window!
            Log.WriteLine(LogLevel.Warning, "no window at the current area");
            this.toolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // get root ancestor for this window
          IntPtr rootHandle = User32.GetAncestor(handle, User32.GetAncestorFlags.GA_ROOT);
          if (rootHandle == IntPtr.Zero) { rootHandle = handle; } // no ancestor

          long windowStyle = User32.GetWindowLongPtr(rootHandle, (int)User32.WindowLongParam.GWL_STYLE).ToInt64();
          if ((windowStyle & (int)User32.WindowStyles.WS_MAXIMIZE) != 0 ||
              (windowStyle & (int)User32.WindowStyles.WS_MINIMIZE) != 0) {
            Log.WriteLine(LogLevel.Warning, "window can not be minimized/maximized!");
            this.toolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // get window bounds
          RECT rect;
          User32.GetWindowRect(rootHandle, out rect);

          // make sure we're on the acceptable bounds
          RECT acceptableBounds;
          Display.GetAcceptableBounds(out acceptableBounds);

          if (rect.left < acceptableBounds.left ||
              rect.right > acceptableBounds.right ||
              rect.top < acceptableBounds.top ||
              rect.bottom > acceptableBounds.bottom) {
            Log.WriteLine(LogLevel.Warning, "unacceptable target bounds");
            this.toolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // make sure there's room for the toolbar window!
          if (rect.bottom - rect.top >= acceptableBounds.bottom - acceptableBounds.top - this.toolBar.Height - 16) {
            Log.WriteLine(LogLevel.Warning, "no more room in the hotseat!");
            this.toolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // save original bounds
          this.originalBounds = new Rectangle((int)this.window.Left,
                                              (int)this.window.Top,
                                              (int)this.window.Width,

                                              (int)this.window.Height);

          /* this is fine - now we want to perform the actual injection. Embrace thyselves */
          const string helperLibraryName = "cn2rthelper"; // will actually match cn2rthelper32, cn2rthelperwhatever

          string GetHelperLibraryPath(bool x64 = false) => Path.Combine(Directory.GetCurrentDirectory(),
                                                                        helperLibraryName +
                                                                        (x64 ? "64" : "32") +
                                                                        ".dll");

          uint pid;
          User32.GetWindowThreadProcessId(rootHandle, out pid);

          // HACK: allow console windows to be captured - this is done by injecting into the conhost process for the
          //       current process ID
          const int classNameLength = 256;
          IntPtr classNamePtr = Marshal.AllocHGlobal(classNameLength);
          if (User32.GetClassName(rootHandle, classNamePtr, classNameLength) != 0) {
            if (Marshal.PtrToStringAnsi(classNamePtr) == "ConsoleWindowClass") {
              uint hostPid = InjectionHelper.GetConsoleHostProcessId(pid);

              if (hostPid != 0) {
                pid = hostPid;
                Log.WriteLine(LogLevel.Debug, $"attaching to console host process ({pid})");
              } else {
                Log.WriteLine(LogLevel.Warning, "could not find console host PID");
              }
            }
          } else {
            Log.WriteLine(LogLevel.Warning, $"GetClassName() failed with error 0x{Marshal.GetLastWin32Error():x8}");
          }

          Marshal.FreeHGlobal(classNamePtr);


          // create attachment information struct
          var attachInfo = new WINATTACHINFO();
          Display.GetAcceptableBounds(out attachInfo.rcAcceptableBounds);
          attachInfo.rcOrgTargetBounds = rect;
          attachInfo.uiGrabberHandle = (uint)this.window.Handle;
          attachInfo.uiToolbarHandle = (uint)this.toolBar.Handle;
          attachInfo.uiTargetHandle = (uint)rootHandle;

          // make sure the helper library has not been already injected
          if (Process.FindModule(pid, helperLibraryName)) {
            // hell yeah - send re-attachment message
            var copydata = new COPYDATASTRUCT {
              dwData = WindowMessages.WM_COPYDATA_CAPNSIG,
              cbData = (uint)Marshal.SizeOf(attachInfo)
            };

            Marshal.StructureToPtr(attachInfo, copydata.lpData, false);
          } else {
            // fuck no
            IntPtr data = Marshal.AllocHGlobal(Marshal.SizeOf(attachInfo));
            Marshal.StructureToPtr(attachInfo, data, false);

            try {
              // try to inject library
              InjectionHelper.InjectLibrary(pid,
                                            GetHelperLibraryPath(),
                                            GetHelperLibraryPath(true),
                                            data,
                                            Marshal.SizeOf(attachInfo));
              Log.WriteLine(LogLevel.Debug, $"injected remote helper library to process {pid}");
            } catch (Win32Exception) {
              this.toolBar.SetWindowAttachmentStatus(false);
              break;
            } finally {
              Log.WriteLine(LogLevel.Debug, "releasing resources");
              Marshal.FreeHGlobal(data);
            }
          }

          // everything's fine at this point
          this.attachedWindow = rootHandle;

          // adjust window to target bounds
          this.window.Left = rect.left;
          this.window.Top = rect.top;
          this.window.Width = rect.right - rect.left;
          this.window.Height = rect.bottom - rect.top;

          this.window.CanBeResized = false;
          this.window.Hide();

          this.toolBar.SetWindowAttachmentStatus(true);
          break;

        case GrabberIntentType.DetachFromWindow:
          this.toolBar.SetWindowAttachmentStatus(true, false);

          if (this.attachedWindow != IntPtr.Zero) {
            // send detach message
            User32.SendMessage(this.attachedWindow, WindowMessages.WM_CAPN_DETACHWND, IntPtr.Zero, IntPtr.Zero);
            Log.WriteLine(LogLevel.Debug, "sent WM_CAPN_DETACHWND message");
          } else {
            Log.WriteLine(LogLevel.Warning, "trying to detach from window when no window is attached");
          }

          this.toolBar.SetWindowAttachmentStatus(false);

          this.window.Left = this.originalBounds.Left;
          this.window.Top = this.originalBounds.Top;
          this.window.Width = this.originalBounds.Width;
          this.window.Height = this.originalBounds.Height;
          this.window.Show();

          // TODO: if this is a recording CanBeResized must be set to false!
          this.window.CanBeResized = true;
          break;
      }
    }
  }
}
