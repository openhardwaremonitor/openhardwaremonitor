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
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI {
  public class GadgetWindow : NativeWindow {

    private bool visible = false;
    private bool lockPosition = false;
    private bool alwaysOnTop = false;
    private byte opacity = 255;
    private Point location = new Point(100, 100);
    private Size size = new Size(130, 84);
    private ContextMenu contextMenu = null;
    private MethodInfo commandDispatch;

    public GadgetWindow() {
      Type commandType = 
        typeof(Form).Assembly.GetType("System.Windows.Forms.Command");
      commandDispatch = commandType.GetMethod("DispatchID", 
        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public, 
        null, new Type[]{ typeof(int) }, null);

      this.CreateHandle(CreateParams);

      // move window to the bottom
      MoveToBottom(Handle);

      // prevent window from fading to a glass sheet when peek is invoked
      try {
        bool value = true;
        int r = NativeMethods.DwmSetWindowAttribute(Handle,
          WindowAttribute.DWMWA_EXCLUDED_FROM_PEEK, ref value,
          Marshal.SizeOf(value));
      } catch (DllNotFoundException) { } catch (EntryPointNotFoundException) { }
    }

    private void ShowDesktopChanged(bool showDesktop) {
      if (showDesktop) {
        MoveToTopMost(Handle);
      } else {
        MoveToBottom(Handle);
      }
    }

    private void MoveToBottom(IntPtr handle) {
      NativeMethods.SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0,
        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
    }

    private void MoveToTopMost(IntPtr handle) {
      NativeMethods.SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0,
        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
    }

    private void ShowContextMenu(Point position) {
      NativeMethods.TrackPopupMenuEx(contextMenu.Handle, 
        TPM_RIGHTBUTTON | TPM_VERTICAL, position.X,
        position.Y, Handle, IntPtr.Zero);
    }

    protected virtual CreateParams CreateParams {
      get {
        CreateParams cp = new CreateParams();
        cp.Width = size.Width;
        cp.Height = size.Height;
        cp.X = location.X;
        cp.Y = location.Y;
        cp.ExStyle = WS_EX_LAYERED | WS_EX_TOOLWINDOW;
        return cp;
      }
    }

    protected override void WndProc(ref Message message) {
      switch (message.Msg) {
        case WM_COMMAND:
          // need to dispatch the message for the context menu
          if (message.LParam == IntPtr.Zero) 
            commandDispatch.Invoke(null, new object[] { 
              message.WParam.ToInt32() & 0xFFFF });          
          break;
        case WM_NCHITTEST:           
          // all pixels of the form belong to the caption
          message.Result = HTCAPTION; 
          break;
        case WM_NCLBUTTONDBLCLK:  
          message.Result = IntPtr.Zero; break;
        case WM_NCRBUTTONDOWN:
          message.Result = IntPtr.Zero; break;
        case WM_NCRBUTTONUP:
          if (contextMenu != null)
            ShowContextMenu(new Point(
              (int)((uint)message.LParam & 0xFFFF), 
              (int)(((uint)message.LParam >>16) & 0xFFFF)));          
          message.Result = IntPtr.Zero; 
          break;
        case WM_WINDOWPOSCHANGING:         
          WindowPos wp = (WindowPos)Marshal.PtrToStructure(
            message.LParam, typeof(WindowPos));

          // add the nomove flag if position is locked
          if (lockPosition)
            wp.flags |= SWP_NOMOVE;

          // prevent the window from leaving the screen
          if ((wp.flags & SWP_NOMOVE) == 0) {            
            Rectangle rect = Screen.GetWorkingArea(new Point(wp.x, wp.y));
            const int margin = 20;
            wp.x = Math.Max(wp.x, rect.Left - wp.cx + margin);
            wp.x = Math.Min(wp.x, rect.Right - margin);
            wp.y = Math.Max(wp.y, rect.Top - wp.cy + margin);
            wp.y = Math.Min(wp.y, rect.Bottom - margin);

            // raise the event if location changed
            if (location.X != wp.x || location.Y != wp.y) {
              location = new Point(wp.x, wp.y);
              if (LocationChanged != null)
                LocationChanged(this, EventArgs.Empty);
            }
          }          

          Marshal.StructureToPtr(wp, message.LParam, false);
          message.Result = IntPtr.Zero;
          break;           
        default:
          base.WndProc(ref message); break;
      }      
    }

    private BlendFunction CreateBlendFunction() {
      BlendFunction blend = new BlendFunction();
      blend.BlendOp = AC_SRC_OVER;
      blend.BlendFlags = 0;
      blend.SourceConstantAlpha = opacity;
      blend.AlphaFormat = AC_SRC_ALPHA;
      return blend;
    }

    public void Update(Bitmap bitmap) {
      IntPtr screen = NativeMethods.GetDC(IntPtr.Zero);
      IntPtr memory = NativeMethods.CreateCompatibleDC(screen);
      IntPtr newHBitmap = IntPtr.Zero;
      IntPtr oldHBitmap = IntPtr.Zero;

      try {
        newHBitmap = bitmap.GetHbitmap(Color.Black);
        oldHBitmap = NativeMethods.SelectObject(memory, newHBitmap);

        Size size = bitmap.Size;
        Point pointSource = Point.Empty;
        Point topPos = Location;

        BlendFunction blend = CreateBlendFunction();
        NativeMethods.UpdateLayeredWindow(Handle, screen, ref topPos,
          ref size, memory, ref pointSource, 0, ref blend, ULW_ALPHA);
      } finally {
        NativeMethods.ReleaseDC(IntPtr.Zero, screen);
        if (newHBitmap != IntPtr.Zero) {
          NativeMethods.SelectObject(memory, oldHBitmap);
          NativeMethods.DeleteObject(newHBitmap);
        }
        NativeMethods.DeleteDC(memory);
      }
    }

    public byte Opacity {
      get {
        return opacity;
      }
      set {
        if (opacity != value) {
          opacity = value;
          BlendFunction blend = CreateBlendFunction();
          NativeMethods.UpdateLayeredWindow(Handle, IntPtr.Zero, IntPtr.Zero,
            IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref blend, ULW_ALPHA);
        }
      }
    }

    public bool Visible {
      get {
        return visible;
      }
      set {
        if (visible != value) {
          visible = value;
          NativeMethods.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER |
            (value ? SWP_SHOWWINDOW : SWP_HIDEWINDOW));
          if (value)
            ShowDesktop.Instance.ShowDesktopChanged += ShowDesktopChanged;
          else
            ShowDesktop.Instance.ShowDesktopChanged -= ShowDesktopChanged;         
        }
      }
    }

    // if locked, the window can not be moved
    public bool LockPosition {
      get {
        return lockPosition;
      }
      set {
        lockPosition = value;
      }
    }

    public bool AlwaysOnTop {
      get {
        return alwaysOnTop;
      }
      set {
        if (value != alwaysOnTop) {
          alwaysOnTop = value;
          if (alwaysOnTop) {
            ShowDesktop.Instance.ShowDesktopChanged -= ShowDesktopChanged;
            MoveToTopMost(Handle);            
          } else {
            MoveToBottom(Handle);
            ShowDesktop.Instance.ShowDesktopChanged += ShowDesktopChanged;
          }
        }
      }
    }

    public Size Size {
      get {
        return size; 
      }
      set {
        if (size != value) {
          size = value;
          NativeMethods.SetWindowPos(Handle, IntPtr.Zero, 0, 0, size.Width,
            size.Height, SWP_NOMOVE | SWP_NOACTIVATE | SWP_NOZORDER | 
            SWP_NOSENDCHANGING);
        }
      }
    }

    public Point Location {
      get {
        return location;
      }
      set {
        NativeMethods.SetWindowPos(Handle, IntPtr.Zero, value.X, value.Y, 0,
          0, SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER | SWP_NOSENDCHANGING);
        location = value;
        if (LocationChanged != null)
          LocationChanged(this, EventArgs.Empty);
      }
    }

    public event EventHandler LocationChanged;

    public ContextMenu ContextMenu {
      get {
        return contextMenu;
      }
      set {
        this.contextMenu = value;
      }
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct BlendFunction {
      public byte BlendOp;
      public byte BlendFlags;
      public byte SourceConstantAlpha;
      public byte AlphaFormat;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WindowPos {
      public IntPtr hwnd;
      public IntPtr hwndInsertAfter;
      public int x;
      public int y;
      public int cx;
      public int cy;
      public uint flags;
    }

    public static readonly IntPtr HWND_BOTTOM = (IntPtr)1;
    public static readonly IntPtr HWND_TOPMOST = (IntPtr)(-1);

    public const int WS_EX_LAYERED = 0x00080000;
    public const int WS_EX_TOOLWINDOW = 0x00000080;

    public const uint SWP_NOSIZE = 0x0001;
    public const uint SWP_NOMOVE = 0x0002;
    public const uint SWP_NOACTIVATE = 0x0010;
    public const uint SWP_HIDEWINDOW = 0x0080;
    public const uint SWP_SHOWWINDOW = 0x0040;
    public const uint SWP_NOZORDER = 0x0004;
    public const uint SWP_NOSENDCHANGING = 0x0400;

    public const int ULW_COLORKEY = 0x00000001;
    public const int ULW_ALPHA = 0x00000002;
    public const int ULW_OPAQUE = 0x00000004;

    public const byte AC_SRC_OVER = 0x00;
    public const byte AC_SRC_ALPHA = 0x01;

    public const int WM_NCHITTEST = 0x0084;
    public const int WM_NCLBUTTONDBLCLK = 0x00A3;
    public const int WM_NCLBUTTONDOWN = 0x00A1;
    public const int WM_NCLBUTTONUP = 0x00A2;
    public const int WM_NCRBUTTONDOWN = 0x00A4;
    public const int WM_NCRBUTTONUP = 0x00A5;
    public const int WM_WINDOWPOSCHANGING = 0x0046;
    public const int WM_COMMAND = 0x0111;

    public const int TPM_RIGHTBUTTON = 0x0002;
    public const int TPM_VERTICAL = 0x0040;

    public readonly IntPtr HTCAPTION = (IntPtr)2;

    private enum WindowAttribute : int {
      DWMWA_NCRENDERING_ENABLED = 1,
      DWMWA_NCRENDERING_POLICY,
      DWMWA_TRANSITIONS_FORCEDISABLED,
      DWMWA_ALLOW_NCPAINT,
      DWMWA_CAPTION_BUTTON_BOUNDS,
      DWMWA_NONCLIENT_RTL_LAYOUT,
      DWMWA_FORCE_ICONIC_REPRESENTATION,
      DWMWA_FLIP3D_POLICY,
      DWMWA_EXTENDED_FRAME_BOUNDS,
      DWMWA_HAS_ICONIC_BITMAP,
      DWMWA_DISALLOW_PEEK,
      DWMWA_EXCLUDED_FROM_PEEK,
      DWMWA_LAST
    }

    private static class NativeMethods {
      private const string USER = "user32.dll";
      private const string GDI = "gdi32.dll";
      public const string DWMAPI = "dwmapi.dll";

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst, 
        ref Point pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc, 
        int crKey, ref BlendFunction pblend, int dwFlags);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
        IntPtr pptDst, IntPtr psize, IntPtr hdcSrc, IntPtr pprSrc,
        int crKey, ref BlendFunction pblend, int dwFlags);  

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetDC(IntPtr hWnd);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern bool SetWindowPos(IntPtr hWnd,
        IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

      [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
      public static extern bool TrackPopupMenuEx(IntPtr hMenu, uint uFlags, 
        int x, int y, IntPtr hWnd, IntPtr tpmParams);

      [DllImport(GDI, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr CreateCompatibleDC(IntPtr hDC);

      [DllImport(GDI, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool DeleteDC(IntPtr hdc);
      
      [DllImport(GDI, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

      [DllImport(GDI, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAs(UnmanagedType.Bool)]
      public static extern bool DeleteObject(IntPtr hObject);

      [DllImport(DWMAPI, CallingConvention = CallingConvention.Winapi)]
      public static extern int DwmSetWindowAttribute(IntPtr hwnd,
        WindowAttribute dwAttribute, ref bool pvAttribute, int cbAttribute);
    }    
  }
}
