/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI {
  public class ShowDesktop {
    private static ShowDesktop instance = new ShowDesktop();

    public delegate void ShowDesktopChangedEventHandler(bool showDesktop);
    
    private event ShowDesktopChangedEventHandler ShowDesktopChangedEvent;

    private System.Threading.Timer timer;
    private bool showDesktop = false;   
    private NativeWindow referenceWindow;
    private string referenceWindowCaption =
      "OpenHardwareMonitorShowDesktopReferenceWindow";

    private ShowDesktop() {
      // create a reference window to detect show desktop
      referenceWindow = new NativeWindow();
      CreateParams cp = new CreateParams();
      cp.ExStyle = GadgetWindow.WS_EX_TOOLWINDOW;
      cp.Caption = referenceWindowCaption;
      referenceWindow.CreateHandle(cp);
      NativeMethods.SetWindowPos(referenceWindow.Handle, 
        GadgetWindow.HWND_BOTTOM, 0, 0, 0, 0, GadgetWindow.SWP_NOMOVE | 
        GadgetWindow.SWP_NOSIZE | GadgetWindow.SWP_NOACTIVATE | 
        GadgetWindow.SWP_NOSENDCHANGING);

      // start a repeated timer to detect "Show Desktop" events 
      timer = new System.Threading.Timer(OnTimer, null,
        System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
    }

    private void StartTimer() {
      timer.Change(0, 200);
    }

    private void StopTimer() {
      timer.Change(System.Threading.Timeout.Infinite,
        System.Threading.Timeout.Infinite);
    }

    // the desktop worker window (if available) can hide the reference window
    private IntPtr GetDesktopWorkerWindow() {
      IntPtr shellWindow = NativeMethods.GetShellWindow();
      if (shellWindow == IntPtr.Zero)
        return IntPtr.Zero;

      int shellId;
      NativeMethods.GetWindowThreadProcessId(shellWindow, out shellId);

      IntPtr workerWindow = IntPtr.Zero;
      while ((workerWindow = NativeMethods.FindWindowEx(
          IntPtr.Zero, workerWindow, "WorkerW", null)) != IntPtr.Zero) {

        int workerId;
        NativeMethods.GetWindowThreadProcessId(workerWindow, out workerId);
        if (workerId == shellId) {
          IntPtr window = NativeMethods.FindWindowEx(
            workerWindow, IntPtr.Zero, "SHELLDLL_DefView", null);
          if (window != IntPtr.Zero) {
            IntPtr desktopWindow = NativeMethods.FindWindowEx(
              window, IntPtr.Zero, "SysListView32", null);
            if (desktopWindow != IntPtr.Zero)
              return workerWindow;
          }
        }
      }
      return IntPtr.Zero;
    }

    private void OnTimer(Object state) {
      bool showDesktopDetected;

      IntPtr workerWindow = GetDesktopWorkerWindow();
      if (workerWindow != IntPtr.Zero) {
        // search if the reference window is behind the worker window
        IntPtr reference = NativeMethods.FindWindowEx(
          IntPtr.Zero, workerWindow, null, referenceWindowCaption);
        showDesktopDetected = reference == referenceWindow.Handle;
      } else {
        // if there is no worker window, then nothing can hide the reference
        showDesktopDetected = false;
      }

      if (showDesktop != showDesktopDetected) {
        showDesktop = showDesktopDetected;
        if (ShowDesktopChangedEvent != null) {
          ShowDesktopChangedEvent(showDesktop);
        }
      }
    }

    public static ShowDesktop Instance {
      get { return instance; }
    }

    // notify when the "show desktop" mode is changed
    public event ShowDesktopChangedEventHandler ShowDesktopChanged {
      add {
        // start the monitor timer when someone is listening
        if (ShowDesktopChangedEvent == null)           
          StartTimer();
        ShowDesktopChangedEvent += value;
      }
      remove {
        ShowDesktopChangedEvent -= value;
        // stop the monitor timer if nobody is interested
        if (ShowDesktopChangedEvent == null)
          StopTimer();
      }
    }

    private static class NativeMethods {
      private const string USER = "user32.dll";

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern bool SetWindowPos(IntPtr hWnd,
        IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr FindWindowEx(IntPtr hwndParent,
        IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetShellWindow();

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern int GetWindowThreadProcessId(IntPtr hWnd,
        out int processId);
    }  
  }
}
