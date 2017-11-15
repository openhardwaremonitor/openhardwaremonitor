/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds <paul@werelds.net>

*/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public class GadgetWindow : NativeWindow, IDisposable
    {
        public const int WS_EX_LAYERED = 0x00080000;
        public const int WS_EX_TOOLWINDOW = 0x00000080;

        public const uint SWP_NOSIZE = 0x0001;
        public const uint SWP_NOMOVE = 0x0002;
        public const uint SWP_NOACTIVATE = 0x0010;
        public const uint SWP_FRAMECHANGED = 0x0020;
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

        public static readonly IntPtr HWND_BOTTOM = (IntPtr) 1;
        public static readonly IntPtr HWND_TOPMOST = (IntPtr) (-1);
        private bool alwaysOnTop;
        private Size bufferSize;
        private readonly MethodInfo commandDispatch;
        private Graphics graphics;
        private IntPtr handleBitmapDC;
        private Point location = new Point(100, 100);
        private byte opacity = 255;

        public PaintEventHandler Paint;
        private Size size = new Size(130, 84);

        private bool visible;

        public GadgetWindow()
        {
            var commandType =
                typeof(Form).Assembly.GetType("System.Windows.Forms.Command");
            commandDispatch = commandType.GetMethod("DispatchID",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                null, new[] {typeof(int)}, null);

            CreateHandle(CreateParams);

            // move window to the bottom
            MoveToBottom(Handle);

            // prevent window from fading to a glass sheet when peek is invoked
            try
            {
                var value = true;
                NativeMethods.DwmSetWindowAttribute(Handle,
                    WindowAttribute.DWMWA_EXCLUDED_FROM_PEEK, ref value,
                    Marshal.SizeOf(value));
            }
            catch (DllNotFoundException)
            {
            }
            catch (EntryPointNotFoundException)
            {
            }

            CreateBuffer();
        }

        protected virtual CreateParams CreateParams
        {
            get
            {
                var cp = new CreateParams();
                cp.Width = 4096;
                cp.Height = 4096;
                cp.X = location.X;
                cp.Y = location.Y;
                cp.ExStyle = WS_EX_LAYERED | WS_EX_TOOLWINDOW;
                return cp;
            }
        }

        public byte Opacity
        {
            get => opacity;
            set
            {
                if (opacity != value)
                {
                    opacity = value;
                    var blend = CreateBlendFunction();
                    NativeMethods.UpdateLayeredWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, ref blend, ULW_ALPHA);
                }
            }
        }

        public bool Visible
        {
            get => visible;
            set
            {
                if (visible != value)
                {
                    visible = value;
                    NativeMethods.SetWindowPos(Handle, IntPtr.Zero, 0, 0, 0, 0,
                        SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER |
                        (value ? SWP_SHOWWINDOW : SWP_HIDEWINDOW));
                    if (value)
                    {
                        if (!alwaysOnTop)
                            ShowDesktop.Instance.ShowDesktopChanged += ShowDesktopChanged;
                    }
                    else
                    {
                        if (!alwaysOnTop)
                            ShowDesktop.Instance.ShowDesktopChanged -= ShowDesktopChanged;
                    }
                }
            }
        }

        // if locked, the window can not be moved or resized
        public bool LockPositionAndSize { get; set; } = false;

        public bool AlwaysOnTop
        {
            get => alwaysOnTop;
            set
            {
                if (value != alwaysOnTop)
                {
                    alwaysOnTop = value;
                    if (alwaysOnTop)
                    {
                        if (visible)
                            ShowDesktop.Instance.ShowDesktopChanged -= ShowDesktopChanged;
                        MoveToTopMost(Handle);
                    }
                    else
                    {
                        MoveToBottom(Handle);
                        if (visible)
                            ShowDesktop.Instance.ShowDesktopChanged += ShowDesktopChanged;
                    }
                }
            }
        }

        public Size Size
        {
            get => size;
            set
            {
                if (size != value)
                {
                    size = value;
                    NativeMethods.UpdateLayeredWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                        ref size, IntPtr.Zero, IntPtr.Zero, 0, IntPtr.Zero, 0);
                    SizeChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public Point Location
        {
            get => location;
            set
            {
                if (location != value)
                {
                    location = value;
                    NativeMethods.SetWindowPos(Handle, IntPtr.Zero,
                        location.X, location.Y, 0, 0,
                        SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOZORDER | SWP_NOSENDCHANGING);
                    LocationChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public ContextMenu ContextMenu { get; set; } = null;

        public virtual void Dispose()
        {
            DisposeBuffer();
        }

        private void ShowDesktopChanged(bool showDesktop)
        {
            if (showDesktop) MoveToTopMost(Handle);
            else MoveToBottom(Handle);
        }

        private void MoveToBottom(IntPtr handle)
        {
            NativeMethods.SetWindowPos(handle, HWND_BOTTOM, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
        }

        private void MoveToTopMost(IntPtr handle)
        {
            NativeMethods.SetWindowPos(handle, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_NOSENDCHANGING);
        }

        private void ShowContextMenu(Point position)
        {
            NativeMethods.TrackPopupMenuEx(ContextMenu.Handle,
                TPM_RIGHTBUTTON | TPM_VERTICAL, position.X,
                position.Y, Handle, IntPtr.Zero);
        }

        protected override void WndProc(ref Message message)
        {
            switch (message.Msg)
            {
                case WM_COMMAND:
                {
                    // need to dispatch the message for the context menu
                    if (message.LParam == IntPtr.Zero)
                        commandDispatch.Invoke(null, new object[]
                        {
                            message.WParam.ToInt32() & 0xFFFF
                        });
                }
                    break;
                case WM_NCHITTEST:
                {
                    message.Result = (IntPtr) HitResult.Caption;
                    if (HitTest != null)
                    {
                        var p = new Point(
                            Macros.GET_X_LPARAM(message.LParam) - location.X,
                            Macros.GET_Y_LPARAM(message.LParam) - location.Y
                        );
                        var e = new HitTestEventArgs(p, HitResult.Caption);
                        HitTest(this, e);
                        message.Result = (IntPtr) e.HitResult;
                    }
                }
                    break;
                case WM_NCLBUTTONDBLCLK:
                {
                    MouseDoubleClick?.Invoke(this, new MouseEventArgs(MouseButtons.Left, 2,
                        Macros.GET_X_LPARAM(message.LParam) - location.X,
                        Macros.GET_Y_LPARAM(message.LParam) - location.Y, 0));
                    message.Result = IntPtr.Zero;
                }
                    break;
                case WM_NCRBUTTONDOWN:
                {
                    message.Result = IntPtr.Zero;
                }
                    break;
                case WM_NCRBUTTONUP:
                {
                    if (ContextMenu != null)
                        ShowContextMenu(new Point(
                            Macros.GET_X_LPARAM(message.LParam),
                            Macros.GET_Y_LPARAM(message.LParam)
                        ));
                    message.Result = IntPtr.Zero;
                }
                    break;
                case WM_WINDOWPOSCHANGING:
                {
                    var wp = (WindowPos) Marshal.PtrToStructure(
                        message.LParam, typeof(WindowPos));

                    if (!LockPositionAndSize)
                    {
                        // prevent the window from leaving the screen
                        if ((wp.flags & SWP_NOMOVE) == 0)
                        {
                            var rect = Screen.GetWorkingArea(
                                new Rectangle(wp.x, wp.y, wp.cx, wp.cy));
                            const int margin = 16;
                            wp.x = Math.Max(wp.x, rect.Left - wp.cx + margin);
                            wp.x = Math.Min(wp.x, rect.Right - margin);
                            wp.y = Math.Max(wp.y, rect.Top - wp.cy + margin);
                            wp.y = Math.Min(wp.y, rect.Bottom - margin);
                        }

                        // update location and fire event
                        if ((wp.flags & SWP_NOMOVE) == 0)
                            if (location.X != wp.x || location.Y != wp.y)
                            {
                                location = new Point(wp.x, wp.y);
                                LocationChanged?.Invoke(this, EventArgs.Empty);
                            }

                        // update size and fire event
                        if ((wp.flags & SWP_NOSIZE) == 0)
                            if (size.Width != wp.cx || size.Height != wp.cy)
                            {
                                size = new Size(wp.cx, wp.cy);
                                SizeChanged?.Invoke(this, EventArgs.Empty);
                            }

                        // update the size of the layered window
                        if ((wp.flags & SWP_NOSIZE) == 0)
                            NativeMethods.UpdateLayeredWindow(Handle, IntPtr.Zero,
                                IntPtr.Zero, ref size, IntPtr.Zero, IntPtr.Zero, 0,
                                IntPtr.Zero, 0);

                        // update the position of the layered window
                        if ((wp.flags & SWP_NOMOVE) == 0)
                            NativeMethods.SetWindowPos(Handle, IntPtr.Zero,
                                location.X, location.Y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE |
                                                              SWP_NOZORDER | SWP_NOSENDCHANGING);
                    }

                    // do not forward any move or size messages
                    wp.flags |= SWP_NOSIZE | SWP_NOMOVE;

                    // suppress any frame changed events
                    wp.flags &= ~SWP_FRAMECHANGED;

                    Marshal.StructureToPtr(wp, message.LParam, false);
                    message.Result = IntPtr.Zero;
                }
                    break;
                default:
                {
                    base.WndProc(ref message);
                }
                    break;
            }
        }

        private BlendFunction CreateBlendFunction()
        {
            var blend = new BlendFunction();
            blend.BlendOp = AC_SRC_OVER;
            blend.BlendFlags = 0;
            blend.SourceConstantAlpha = opacity;
            blend.AlphaFormat = AC_SRC_ALPHA;
            return blend;
        }

        private void CreateBuffer()
        {
            var handleScreenDC = NativeMethods.GetDC(IntPtr.Zero);
            handleBitmapDC = NativeMethods.CreateCompatibleDC(handleScreenDC);
            NativeMethods.ReleaseDC(IntPtr.Zero, handleScreenDC);
            bufferSize = size;

            var info = new BitmapInfo();
            info.Size = Marshal.SizeOf(info);
            info.Width = size.Width;
            info.Height = -size.Height;
            info.BitCount = 32;
            info.Planes = 1;

            var hBmp = NativeMethods.CreateDIBSection(handleBitmapDC, ref info, 0,
                out IntPtr ptr, IntPtr.Zero, 0);
            var hBmpOld = NativeMethods.SelectObject(handleBitmapDC, hBmp);
            NativeMethods.DeleteObject(hBmpOld);

            graphics = Graphics.FromHdc(handleBitmapDC);

            if (Environment.OSVersion.Version.Major > 5)
            {
                graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        private void DisposeBuffer()
        {
            graphics.Dispose();
            NativeMethods.DeleteDC(handleBitmapDC);
        }

        public void Redraw()
        {
            if (!visible || Paint == null)
                return;

            if (size != bufferSize)
            {
                DisposeBuffer();
                CreateBuffer();
            }

            Paint(this,
                new PaintEventArgs(graphics, new Rectangle(Point.Empty, size)));

            var pointSource = Point.Empty;
            var blend = CreateBlendFunction();

            NativeMethods.UpdateLayeredWindow(Handle, IntPtr.Zero, IntPtr.Zero,
                ref size, handleBitmapDC, ref pointSource, 0, ref blend, ULW_ALPHA);

            // make sure the window is at the right location
            NativeMethods.SetWindowPos(Handle, IntPtr.Zero,
                location.X, location.Y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE |
                                              SWP_NOZORDER | SWP_NOSENDCHANGING);
        }

        public event EventHandler SizeChanged;

        public event EventHandler LocationChanged;

        public event HitTestEventHandler HitTest;

        public event MouseEventHandler MouseDoubleClick;

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct BlendFunction
        {
            public byte BlendOp;
            public byte BlendFlags;
            public byte SourceConstantAlpha;
            public byte AlphaFormat;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WindowPos
        {
            public readonly IntPtr hwnd;
            public readonly IntPtr hwndInsertAfter;
            public int x;
            public int y;
            public readonly int cx;
            public readonly int cy;
            public uint flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct BitmapInfo
        {
            public int Size;
            public int Width;
            public int Height;
            public short Planes;
            public short BitCount;
            public int Compression;
            public int SizeImage;
            public int XPelsPerMeter;
            public int YPelsPerMeter;
            public int ClrUsed;
            public int ClrImportant;
            public int Colors;
        }

        private enum WindowAttribute
        {
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

        /// <summary>
        ///     Some macros imported and converted from the Windows SDK
        /// </summary>
        private static class Macros
        {
            public static ushort LOWORD(IntPtr l)
            {
                return (ushort) ((ulong) l & 0xFFFF);
            }

            public static ushort HIWORD(IntPtr l)
            {
                return (ushort) (((ulong) l >> 16) & 0xFFFF);
            }

            public static int GET_X_LPARAM(IntPtr lp)
            {
                return (short) LOWORD(lp);
            }

            public static int GET_Y_LPARAM(IntPtr lp)
            {
                return (short) HIWORD(lp);
            }
        }

        /// <summary>
        ///     Imported native methods
        /// </summary>
        private static class NativeMethods
        {
            private const string USER = "user32.dll";
            private const string GDI = "gdi32.dll";
            public const string DWMAPI = "dwmapi.dll";

            [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
                IntPtr pptDst, ref Size psize, IntPtr hdcSrc, IntPtr pprSrc,
                int crKey, IntPtr pblend, int dwFlags);

            [DllImport(USER, CallingConvention = CallingConvention.Winapi)]
            [return: MarshalAs(UnmanagedType.Bool)]
            public static extern bool UpdateLayeredWindow(IntPtr hwnd, IntPtr hdcDst,
                IntPtr pptDst, ref Size psize, IntPtr hdcSrc, ref Point pprSrc,
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
            public static extern IntPtr CreateDIBSection(IntPtr hdc,
                [In] ref BitmapInfo pbmi, uint pila, out IntPtr ppvBits,
                IntPtr hSection, uint dwOffset);

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

    public enum HitResult
    {
        Transparent = -1,
        Nowhere = 0,
        Client = 1,
        Caption = 2,
        Left = 10,
        Right = 11,
        Top = 12,
        TopLeft = 13,
        TopRight = 14,
        Bottom = 15,
        BottomLeft = 16,
        BottomRight = 17,
        Border = 18
    }

    public delegate void HitTestEventHandler(object sender, HitTestEventArgs e);

    public class HitTestEventArgs : EventArgs
    {
        public HitTestEventArgs(Point location, HitResult hitResult)
        {
            Location = location;
            HitResult = hitResult;
        }

        public Point Location { get; }
        public HitResult HitResult { get; set; }
    }
}