﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using Captain.Application.Native;
using Captain.Common;
using static Captain.Application.Application;

namespace Captain.Application {
  /// <inheritdoc />
  /// <summary>
  ///   Provides a user interface for handpicking a screen region and using additional tools
  /// </summary>
  internal sealed class Grabber : IDisposable {
    /// <summary>
    ///   Currently instantiated Grabber instance
    /// </summary>
    private static Grabber singleton;

    /// <summary>
    ///   Original grabber window bounds
    /// </summary>
    private Rectangle originalBounds;

    /// <summary>
    ///   Whether this Grabber instance is disposed or not
    /// </summary>
    private bool IsDisposed { get; set; }

    /// <summary>
    ///   Holds the currently attached window handle
    /// </summary>
    internal IntPtr AttachedWindowHandle { get; private set; }

    /// <summary>
    ///   Grabber tool bar (displays editing tools)
    /// </summary>
    internal GrabberToolBarWindow ToolBar { get; private set; }

    /// <summary>
    ///   Grabber window
    /// </summary>
    internal GrabberWindow Window { get; private set; }

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
    ///   Creates a <see cref="Grabber"/> instance
    /// </summary>
    /// <param name="acceptableActionTypes">Acceptable action types</param>
    internal static Grabber Create(ActionType acceptableActionTypes) {
      if (!singleton?.IsDisposed == true) {
        return singleton;
      }

      return singleton = new Grabber(acceptableActionTypes);
    }

    /// <summary>
    ///   Creates a new <see cref="Grabber"/> instance
    /// </summary>
    /// <param name="acceptableActionTypes">Acceptable action types</param>
    private Grabber(ActionType acceptableActionTypes) {
      Window = new GrabberWindow(this);
      ToolBar = new GrabberToolBarWindow(acceptableActionTypes);

      // TODO: restore GrabberWindow bounds
      Window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
      Window.Width = Screen.PrimaryScreen.WorkingArea.Width / 3d;
      Window.Height = Screen.PrimaryScreen.WorkingArea.Height / 3d;

      ToolBar.OnCaptureActionInitiated += OnCaptureActionInitiated;
      ToolBar.OnGrabberIntentReceived += OnGrabberIntentReceived;

      Window.Closed += OnWindowClosed;
      ToolBar.Closed += OnWindowClosed;
    }

    /// <summary>
    ///   Triggered whenever the grabber UI window or the toolbar window are closed
    /// </summary>
    /// <param name="sender">Sender object</param>
    /// <param name="eventArgs">Event args</param>
    private void OnWindowClosed(object sender, EventArgs eventArgs) => Dispose();

    /// <inheritdoc />
    /// <summary>
    ///   Disposes resources used by the grabber interface
    /// </summary>
    public void Dispose() {
      if (Window != null) {
        Window.Closed -= OnWindowClosed;
        Window.Close();
        Window = null;
      }

      if (ToolBar != null) {
        ToolBar.Closed -= OnWindowClosed;
        ToolBar.Close();
        ToolBar = null;
      }

      IsDisposed = true;
    }

    /// <summary>
    ///   Displays the grabber UI
    /// </summary>
    /// <returns>Whether or not the capture is being created</returns>
    internal void Show() => Window.Show();

    /// <summary>
    ///   Hides the grabber UI
    /// </summary>
    internal void Hide() {
      Window.Hide();
      ToolBar.Hide();
    }

    /// <summary>
    ///   Triggered when the toolbar initiates an action
    /// </summary>
    /// <param name="type">Action type</param>
    private void OnCaptureActionInitiated(ActionType type) =>
      OnIntentReceived?.Invoke(this,
                               new CaptureIntent(type) {
                                 VirtualArea = Window.Area,
                                 WindowHandle = AttachedWindowHandle
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
          if (AttachedWindowHandle != IntPtr.Zero) {
            OnGrabberIntentReceived(GrabberIntentType.DetachFromWindow, null);
          }

          Dispose();
          break;

        case GrabberIntentType.AttachToWindow:
          ToolBar.SetWindowAttachmentStatus(false, false);
          Window.PassThrough = true;

          /* get the window that contains the central point of the grabber windows */
          var point = new POINT {
            x = Window.Area.X + Window.Area.Width / 2,
            y = Window.Area.Y + Window.Area.Height / 2
          };

          IntPtr handle = User32.WindowFromPoint(point);
          Window.PassThrough = false;

          if (handle == IntPtr.Zero || handle == Window.Handle || handle == ToolBar.Handle) {
            // don't attach to desktop window!
            Log.WriteLine(LogLevel.Warning, "no window at the current area");
            ToolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // get root ancestor for this window
          IntPtr rootHandle = User32.GetAncestor(handle, User32.GetAncestorFlags.GA_ROOT);
          if (rootHandle == IntPtr.Zero) { rootHandle = handle; } // no ancestor

          long windowStyle = User32.GetWindowLongPtr(rootHandle, (int)User32.WindowLongParam.GWL_STYLE).ToInt64();
          if ((windowStyle & (int)User32.WindowStyles.WS_MAXIMIZE) != 0 ||
              (windowStyle & (int)User32.WindowStyles.WS_MINIMIZE) != 0) {
            Log.WriteLine(LogLevel.Warning, "window can not be minimized/maximized!");
            ToolBar.SetWindowAttachmentStatus(false);
            return;
          }

          // get window bounds
          RECT rect = WindowHelper.GetWindowBounds(rootHandle);

          // save original bounds
          this.originalBounds = new Rectangle((int)Window.Left,
                                              (int)Window.Top,
                                              (int)Window.Width,
                                              (int)Window.Height);

          /* this is fine - now we want to perform the actual injection. Embrace thyselves */
          const string helperLibraryName = "cn2rthelper"; // will actually match cn2rthelper32, cn2rthelperwhatever

          string GetHelperLibraryPath(bool x64 = false) => Path.Combine(Directory.GetCurrentDirectory(),
                                                                        helperLibraryName +
                                                                        (x64 ? "64" : "32") +
                                                                        ".dll");
          User32.GetWindowThreadProcessId(rootHandle, out uint pid);

          if (Environment.OSVersion.Version >= new Version(6, 1)) {
            // allow console windows to be captured on Windows >= 7 by injecting the DLL into the console host process
            // associated with the process ID. This is not supported in older platforms because csrss.exe is used
            // instead of conhost.exe and it's a privileged system process
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
          }

          // create attachment information struct
          var attachInfo = new WINATTACHINFO {
            bD3DPresent = Process.FindModule(pid, "d3d"),
            rcOrgTargetBounds = rect,
            uiGrabberHandle = (uint)Window.Handle,
            uiToolbarHandle = (uint)ToolBar.Handle,
            uiTargetHandle = (uint)rootHandle
          };

          // allocate and copy attachInfo
          IntPtr attachInfoPtr = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(WINATTACHINFO)));
          Marshal.StructureToPtr(attachInfo, attachInfoPtr, true);

          // make sure the helper library has not been already injected so we can save some memory! Yay!
          new Thread(() => {
            if (Process.FindModule(pid, helperLibraryName)) {
              // hell yeah - send re-attachment message
              Log.WriteLine(LogLevel.Debug, "no need to inject library");

              try {
                var copydata = new User32.COPYDATASTRUCT {
                  dwData = new IntPtr(WindowMessages.WM_COPYDATA_CAPNSIG),
                  cbData = new IntPtr(Marshal.SizeOf(attachInfo)),
                  lpData = attachInfoPtr
                };

                // send the window message
                User32.SendMessage(rootHandle,
                                   (uint)User32.WindowMessage.WM_COPYDATA,
                                   Window.Handle,
                                   ref copydata);
              } finally {
                // free resources
                Marshal.FreeHGlobal(attachInfoPtr);
              }
            } else {
              // fuck no
              try {
                // try to inject library
                // TODO: this should not be static. Create one InjectionHelper instance per grabber UI and "release" it
                //       safely when no longer needed (i.e. killing unresponsive helper processes on close)
                InjectionHelper.InjectLibrary(pid,
                                              GetHelperLibraryPath(),
                                              GetHelperLibraryPath(true),
                                              attachInfoPtr,
                                              Marshal.SizeOf(attachInfo));
                Log.WriteLine(LogLevel.Debug, $"injected remote helper library to process {pid}");
              } catch (Win32Exception) {
                ToolBar.Dispatcher.Invoke(() => ToolBar.SetWindowAttachmentStatus(false));
                return;
              } finally {
                Log.WriteLine(LogLevel.Debug, "releasing resources");
                Marshal.FreeHGlobal(attachInfoPtr);
              }
            }

            // everything's fine at this point
            AttachedWindowHandle = rootHandle;

            // adjust window to target bounds
            Window.Dispatcher.Invoke(() => {
              Window.Left = rect.left;
              Window.Top = rect.top;
              Window.Width = rect.right - rect.left;
              Window.Height = rect.bottom - rect.top;
              //= new Rectangle(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top);
              Window.UpdatePosition(rootHandle);
              Window.CanBeResized = false;
              Window.Opacity = 0;
            });

            ToolBar.Dispatcher.Invoke(() => ToolBar.SetWindowAttachmentStatus(true));
          }).Start();

          break;

        case GrabberIntentType.DetachFromWindow:
          ToolBar.SetWindowAttachmentStatus(true, false);

          if (AttachedWindowHandle != IntPtr.Zero) {
            // send detach message
            User32.SendMessage(AttachedWindowHandle, WindowMessages.WM_CAPN_DETACHWND, IntPtr.Zero, IntPtr.Zero);
            Log.WriteLine(LogLevel.Debug, "sent WM_CAPN_DETACHWND message");
            AttachedWindowHandle = IntPtr.Zero;
          } else {
            Log.WriteLine(LogLevel.Warning, "trying to detach from window when no window is attached");
          }

          ToolBar.SetWindowAttachmentStatus(false);

          Window.Left = this.originalBounds.Left;
          Window.Top = this.originalBounds.Top;
          Window.Width = this.originalBounds.Width;
          Window.Height = this.originalBounds.Height;
          Window.Opacity = 1;
          Window.UpdatePosition();

          // TODO: if this is a recording CanBeResized must be set to false!
          Window.CanBeResized = true;
          break;
      }
    }
  }
}
