/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public class NotifyIconAdv : IDisposable
    {
        private readonly NotifyIcon genericNotifyIcon;
        private readonly NotifyIconWindowsImplementation windowsNotifyIcon;

        public NotifyIconAdv()
        {
            var p = (int) Environment.OSVersion.Platform;
            if (p == 4 || p == 128) genericNotifyIcon = new NotifyIcon();
            else windowsNotifyIcon = new NotifyIconWindowsImplementation();
        }

        public string BalloonTipText
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.BalloonTipText;
                return windowsNotifyIcon.BalloonTipText;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipText = value;
                else
                    windowsNotifyIcon.BalloonTipText = value;
            }
        }

        public ToolTipIcon BalloonTipIcon
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.BalloonTipIcon;
                return windowsNotifyIcon.BalloonTipIcon;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipIcon = value;
                else
                    windowsNotifyIcon.BalloonTipIcon = value;
            }
        }

        public string BalloonTipTitle
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.BalloonTipTitle;
                return windowsNotifyIcon.BalloonTipTitle;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipTitle = value;
                else
                    windowsNotifyIcon.BalloonTipTitle = value;
            }
        }

        public ContextMenu ContextMenu
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.ContextMenu;
                return windowsNotifyIcon.ContextMenu;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.ContextMenu = value;
                else
                    windowsNotifyIcon.ContextMenu = value;
            }
        }

        public ContextMenuStrip ContextMenuStrip
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.ContextMenuStrip;
                return windowsNotifyIcon.ContextMenuStrip;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.ContextMenuStrip = value;
                else
                    windowsNotifyIcon.ContextMenuStrip = value;
            }
        }

        public object Tag { get; set; }

        public Icon Icon
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.Icon;
                return windowsNotifyIcon.Icon;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.Icon = value;
                else
                    windowsNotifyIcon.Icon = value;
            }
        }

        public string Text
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.Text;
                return windowsNotifyIcon.Text;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.Text = value;
                else
                    windowsNotifyIcon.Text = value;
            }
        }

        public bool Visible
        {
            get
            {
                if (genericNotifyIcon != null)
                    return genericNotifyIcon.Visible;
                return windowsNotifyIcon.Visible;
            }
            set
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.Visible = value;
                else
                    windowsNotifyIcon.Visible = value;
            }
        }

        public void Dispose()
        {
            if (genericNotifyIcon != null)
                genericNotifyIcon.Dispose();
            else
                windowsNotifyIcon.Dispose();
        }

        public event EventHandler BalloonTipClicked
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipClicked += value;
                else
                    windowsNotifyIcon.BalloonTipClicked += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipClicked -= value;
                else
                    windowsNotifyIcon.BalloonTipClicked -= value;
            }
        }

        public event EventHandler BalloonTipClosed
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipClosed += value;
                else
                    windowsNotifyIcon.BalloonTipClosed += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipClosed -= value;
                else
                    windowsNotifyIcon.BalloonTipClosed -= value;
            }
        }

        public event EventHandler BalloonTipShown
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipShown += value;
                else
                    windowsNotifyIcon.BalloonTipShown += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.BalloonTipShown -= value;
                else
                    windowsNotifyIcon.BalloonTipShown -= value;
            }
        }

        public event EventHandler Click
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.Click += value;
                else
                    windowsNotifyIcon.Click += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.Click -= value;
                else
                    windowsNotifyIcon.Click -= value;
            }
        }

        public event EventHandler DoubleClick
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.DoubleClick += value;
                else
                    windowsNotifyIcon.DoubleClick += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.DoubleClick -= value;
                else
                    windowsNotifyIcon.DoubleClick -= value;
            }
        }

        public event MouseEventHandler MouseClick
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseClick += value;
                else
                    windowsNotifyIcon.MouseClick += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseClick -= value;
                else
                    windowsNotifyIcon.MouseClick -= value;
            }
        }

        public event MouseEventHandler MouseDoubleClick
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseDoubleClick += value;
                else
                    windowsNotifyIcon.MouseDoubleClick += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseDoubleClick -= value;
                else
                    windowsNotifyIcon.MouseDoubleClick -= value;
            }
        }

        public event MouseEventHandler MouseDown
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseDown += value;
                else
                    windowsNotifyIcon.MouseDown += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseDown -= value;
                else
                    windowsNotifyIcon.MouseDown -= value;
            }
        }

        public event MouseEventHandler MouseMove
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseMove += value;
                else
                    windowsNotifyIcon.MouseMove += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseMove -= value;
                else
                    windowsNotifyIcon.MouseMove -= value;
            }
        }

        public event MouseEventHandler MouseUp
        {
            add
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseUp += value;
                else
                    windowsNotifyIcon.MouseUp += value;
            }
            remove
            {
                if (genericNotifyIcon != null)
                    genericNotifyIcon.MouseUp -= value;
                else
                    windowsNotifyIcon.MouseUp -= value;
            }
        }

        public void ShowBalloonTip(int timeout)
        {
            ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText,
            ToolTipIcon tipIcon)
        {
            if (genericNotifyIcon != null)
                genericNotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
            else
                windowsNotifyIcon.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon);
        }

        private class NotifyIconWindowsImplementation : Component
        {
            private const int WM_NULL = 0x00;
            private const int WM_DESTROY = 0x02;
            private const int WM_CLOSE = 0x10;
            private const int WM_COMMAND = 0x111;
            private const int WM_INITMENUPOPUP = 0x117;
            private const int WM_MOUSEMOVE = 0x200;
            private const int WM_LBUTTONDOWN = 0x201;
            private const int WM_LBUTTONUP = 0x202;
            private const int WM_LBUTTONDBLCLK = 0x203;
            private const int WM_RBUTTONDOWN = 0x204;
            private const int WM_RBUTTONUP = 0x205;
            private const int WM_RBUTTONDBLCLK = 0x206;
            private const int WM_MBUTTONDOWN = 0x207;
            private const int WM_MBUTTONUP = 0x208;
            private const int WM_MBUTTONDBLCLK = 0x209;
            private const int WM_TRAYMOUSEMESSAGE = 0x800;

            private const int NIN_BALLOONSHOW = 0x402;
            private const int NIN_BALLOONHIDE = 0x403;
            private const int NIN_BALLOONTIMEOUT = 0x404;
            private const int NIN_BALLOONUSERCLICK = 0x405;

            private static int nextId;

            private static readonly int WM_TASKBARCREATED =
                NativeMethods.RegisterWindowMessage("TaskbarCreated");

            private readonly MethodInfo commandDispatch;
            private bool created;
            private bool doubleClickDown;
            private Icon icon;
            private readonly int id;

            private readonly object syncObj = new object();
            private string text = "";
            private bool visible;
            private NotifyIconNativeWindow window;

            public NotifyIconWindowsImplementation()
            {
                BalloonTipText = "";
                BalloonTipTitle = "";

                commandDispatch = typeof(Form).Assembly.GetType("System.Windows.Forms.Command").GetMethod("DispatchID",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public,
                    null, new[] {typeof(int)}, null);

                id = ++nextId;
                window = new NotifyIconNativeWindow(this);
                UpdateNotifyIcon(visible);
            }

            public string BalloonTipText { get; set; }
            public ToolTipIcon BalloonTipIcon { get; set; }
            public string BalloonTipTitle { get; set; }
            public ContextMenu ContextMenu { get; set; }
            public ContextMenuStrip ContextMenuStrip { get; set; }
            public object Tag { get; set; }

            public Icon Icon
            {
                get => icon;
                set
                {
                    if (icon != value)
                    {
                        icon = value;
                        UpdateNotifyIcon(visible);
                    }
                }
            }

            public string Text
            {
                get => text;
                set
                {
                    if (value == null)
                        value = "";

                    if (value.Length > 63)
                        throw new ArgumentOutOfRangeException();

                    if (!value.Equals(text))
                    {
                        text = value;

                        if (visible)
                            UpdateNotifyIcon(visible);
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
                        UpdateNotifyIcon(visible);
                    }
                }
            }

            public event EventHandler BalloonTipClicked;
            public event EventHandler BalloonTipClosed;
            public event EventHandler BalloonTipShown;
            public event EventHandler Click;
            public event EventHandler DoubleClick;
            public event MouseEventHandler MouseClick;
            public event MouseEventHandler MouseDoubleClick;
            public event MouseEventHandler MouseDown;
            public event MouseEventHandler MouseMove;
            public event MouseEventHandler MouseUp;

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (window != null)
                    {
                        icon = null;
                        text = "";
                        UpdateNotifyIcon(false);
                        window.DestroyHandle();
                        window = null;
                        ContextMenu = null;
                        ContextMenuStrip = null;
                    }
                }
                else
                {
                    if (window != null && window.Handle != IntPtr.Zero)
                    {
                        NativeMethods.PostMessage(
                            new HandleRef(window, window.Handle), WM_CLOSE, 0, 0);
                        window.ReleaseHandle();
                    }
                }
                base.Dispose(disposing);
            }

            public void ShowBalloonTip(int timeout)
            {
                ShowBalloonTip(timeout, BalloonTipTitle, BalloonTipText, BalloonTipIcon);
            }

            public void ShowBalloonTip(int timeout, string tipTitle, string tipText,
                ToolTipIcon tipIcon)
            {
                if (timeout < 0)
                    throw new ArgumentOutOfRangeException(nameof(timeout));

                if (string.IsNullOrEmpty(tipText))
                    throw new ArgumentException("tipText");

                if (DesignMode)
                    return;

                if (created)
                {
                    var data = new NativeMethods.NotifyIconData();
                    if (window.Handle == IntPtr.Zero)
                        window.CreateHandle(new CreateParams());

                    data.Window = window.Handle;
                    data.ID = id;
                    data.Flags = NativeMethods.NotifyIconDataFlags.Info;
                    data.TimeoutOrVersion = timeout;
                    data.InfoTitle = tipTitle;
                    data.Info = tipText;
                    data.InfoFlags = (int) tipIcon;

                    NativeMethods.Shell_NotifyIcon(
                        NativeMethods.NotifyIconMessage.Modify, data);
                }
            }

            private void ShowContextMenu()
            {
                if (ContextMenu == null && ContextMenuStrip == null)
                    return;

                var p = new NativeMethods.Point();
                NativeMethods.GetCursorPos(ref p);
                NativeMethods.SetForegroundWindow(
                    new HandleRef(window, window.Handle));

                if (ContextMenu != null)
                {
                    ContextMenu.GetType().InvokeMember("OnPopup",
                        BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                        BindingFlags.Instance, null, ContextMenu,
                        new object[] {EventArgs.Empty});

                    NativeMethods.TrackPopupMenuEx(
                        new HandleRef(ContextMenu, ContextMenu.Handle), 72,
                        p.x, p.y, new HandleRef(window, window.Handle),
                        IntPtr.Zero);

                    NativeMethods.PostMessage(
                        new HandleRef(window, window.Handle), WM_NULL, 0, 0);
                    return;
                }

                if (ContextMenuStrip != null)
                    ContextMenuStrip.GetType().InvokeMember("ShowInTaskbar",
                        BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                        BindingFlags.Instance, null, ContextMenuStrip,
                        new object[] {p.x, p.y});
            }

            private void UpdateNotifyIcon(bool showNotifyIcon)
            {
                if (DesignMode)
                    return;

                lock (syncObj)
                {
                    window.LockReference(showNotifyIcon);

                    var data = new NativeMethods.NotifyIconData();
                    data.CallbackMessage = WM_TRAYMOUSEMESSAGE;
                    data.Flags = NativeMethods.NotifyIconDataFlags.Message;

                    if (showNotifyIcon && window.Handle == IntPtr.Zero)
                        window.CreateHandle(new CreateParams());

                    data.Window = window.Handle;
                    data.ID = id;

                    if (icon != null)
                    {
                        data.Flags |= NativeMethods.NotifyIconDataFlags.Icon;
                        data.Icon = icon.Handle;
                    }

                    data.Flags |= NativeMethods.NotifyIconDataFlags.Tip;
                    data.Tip = text;

                    if (showNotifyIcon && icon != null)
                    {
                        if (!created)
                        {
                            var i = 0;
                            do
                            {
                                created = NativeMethods.Shell_NotifyIcon(
                                    NativeMethods.NotifyIconMessage.Add, data);
                                if (!created)
                                {
                                    Thread.Sleep(200);
                                    i++;
                                }
                            } while (!created && i < 40);
                        }
                        else
                        {
                            NativeMethods.Shell_NotifyIcon(
                                NativeMethods.NotifyIconMessage.Modify, data);
                        }
                    }
                    else
                    {
                        if (created)
                        {
                            var i = 0;
                            var deleted = false;
                            do
                            {
                                deleted = NativeMethods.Shell_NotifyIcon(
                                    NativeMethods.NotifyIconMessage.Delete, data);
                                if (!deleted)
                                {
                                    Thread.Sleep(200);
                                    i++;
                                }
                            } while (!deleted && i < 40);
                            created = false;
                        }
                    }
                }
            }

            private void ProcessMouseDown(MouseButtons button,bool doubleClick)
            {
                if (doubleClick)
                {
                    DoubleClick?.Invoke(this, new MouseEventArgs(button, 2, 0, 0, 0));

                    MouseDoubleClick?.Invoke(this, new MouseEventArgs(button, 2, 0, 0, 0));

                    doubleClickDown = true;
                }

                MouseDown?.Invoke(this,
                    new MouseEventArgs(button, doubleClick ? 2 : 1, 0, 0, 0));
            }

            private void ProcessMouseUp(MouseButtons button)
            {
                MouseUp?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));

                if (!doubleClickDown)
                {
                    Click?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));

                    MouseClick?.Invoke(this, new MouseEventArgs(button, 0, 0, 0, 0));
                }
                doubleClickDown = false;
            }

            private void ProcessInitMenuPopup(ref Message message)
            {
                if (ContextMenu != null &&
                    (bool) ContextMenu.GetType().InvokeMember("ProcessInitMenuPopup",
                        BindingFlags.NonPublic | BindingFlags.InvokeMethod |
                        BindingFlags.Instance, null, ContextMenu,
                        new object[] {message.WParam})) return;
                window.DefWndProc(ref message);
            }

            private void WndProc(ref Message message)
            {
                switch (message.Msg)
                {
                    case WM_DESTROY:
                        UpdateNotifyIcon(false);
                        return;
                    case WM_COMMAND:
                        if (message.LParam != IntPtr.Zero)
                        {
                            window.DefWndProc(ref message);
                            return;
                        }
                        commandDispatch.Invoke(null, new object[]
                        {
                            message.WParam.ToInt32() & 0xFFFF
                        });
                        return;
                    case WM_INITMENUPOPUP:
                        ProcessInitMenuPopup(ref message);
                        return;
                    case WM_TRAYMOUSEMESSAGE:
                        switch ((int) message.LParam)
                        {
                            case WM_MOUSEMOVE:
                                MouseMove?.Invoke(this,
                                    new MouseEventArgs(Control.MouseButtons, 0, 0, 0, 0));
                                return;
                            case WM_LBUTTONDOWN:
                                ProcessMouseDown(MouseButtons.Left, false);
                                return;
                            case WM_LBUTTONUP:
                                ProcessMouseUp(MouseButtons.Left);
                                return;
                            case WM_LBUTTONDBLCLK:
                                ProcessMouseDown(MouseButtons.Left, true);
                                return;
                            case WM_RBUTTONDOWN:
                                ProcessMouseDown(MouseButtons.Right, false);
                                return;
                            case WM_RBUTTONUP:
                                if (ContextMenu != null || ContextMenuStrip != null)
                                    ShowContextMenu();
                                ProcessMouseUp(MouseButtons.Right);
                                return;
                            case WM_RBUTTONDBLCLK:
                                ProcessMouseDown(MouseButtons.Right, true);
                                return;
                            case WM_MBUTTONDOWN:
                                ProcessMouseDown(MouseButtons.Middle, false);
                                return;
                            case WM_MBUTTONUP:
                                ProcessMouseUp(MouseButtons.Middle);
                                return;
                            case WM_MBUTTONDBLCLK:
                                ProcessMouseDown(MouseButtons.Middle, true);
                                return;
                            case NIN_BALLOONSHOW:
                                BalloonTipShown?.Invoke(this, EventArgs.Empty);
                                return;
                            case NIN_BALLOONHIDE:
                            case NIN_BALLOONTIMEOUT:
                                BalloonTipClosed?.Invoke(this, EventArgs.Empty);
                                return;
                            case NIN_BALLOONUSERCLICK:
                                BalloonTipClicked?.Invoke(this, EventArgs.Empty);
                                return;
                            default:
                                return;
                        }
                }

                if (message.Msg == WM_TASKBARCREATED)
                {
                    lock (syncObj)
                    {
                        created = false;
                    }
                    UpdateNotifyIcon(visible);
                }

                window.DefWndProc(ref message);
            }

            private class NotifyIconNativeWindow : NativeWindow
            {
                private readonly NotifyIconWindowsImplementation reference;
                private GCHandle referenceHandle;

                internal NotifyIconNativeWindow(NotifyIconWindowsImplementation component)
                {
                    reference = component;
                }

                ~NotifyIconNativeWindow()
                {
                    if (Handle != IntPtr.Zero)
                        NativeMethods.PostMessage(
                            new HandleRef(this, Handle), WM_CLOSE, 0, 0);
                }

                public void LockReference(bool locked)
                {
                    if (locked)
                    {
                        if (!referenceHandle.IsAllocated)
                        {
                            referenceHandle = GCHandle.Alloc(reference, GCHandleType.Normal);
                        }
                    }
                    else
                    {
                        if (referenceHandle.IsAllocated)
                            referenceHandle.Free();
                    }
                }

                protected override void OnThreadException(Exception e)
                {
                    Application.OnThreadException(e);
                }

                protected override void WndProc(ref Message m)
                {
                    reference.WndProc(ref m);
                }
            }

            private static class NativeMethods
            {
                [Flags]
                public enum NotifyIconDataFlags
                {
                    Message = 0x1,
                    Icon = 0x2,
                    Tip = 0x4,
                    State = 0x8,
                    Info = 0x10
                }

                public enum NotifyIconMessage
                {
                    Add = 0x0,
                    Modify = 0x1,
                    Delete = 0x2
                }

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                public static extern IntPtr PostMessage(HandleRef hwnd, int msg,
                    int wparam, int lparam);

                [DllImport("user32.dll", CharSet = CharSet.Auto)]
                public static extern int RegisterWindowMessage(string msg);

                [DllImport("shell32.dll", CharSet = CharSet.Auto)]
                [return: MarshalAs(UnmanagedType.Bool)]
                public static extern bool Shell_NotifyIcon(NotifyIconMessage message,
                    NotifyIconData pnid);

                [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
                public static extern bool TrackPopupMenuEx(HandleRef hmenu, int fuFlags,
                    int x, int y, HandleRef hwnd, IntPtr tpm);

                [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
                public static extern bool GetCursorPos(ref Point point);

                [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
                public static extern bool SetForegroundWindow(HandleRef hWnd);

                [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
                public class NotifyIconData
                {
                    public int CallbackMessage;
                    public NotifyIconDataFlags Flags;
                    public IntPtr Icon;
                    public int ID;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)] public string Info;

                    public int InfoFlags;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)] public string InfoTitle;

                    private int Size = Marshal.SizeOf(typeof(NotifyIconData));
                    public int State;
                    public int StateMask;
                    public int TimeoutOrVersion;

                    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)] public string Tip;

                    public IntPtr Window;
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct Point
                {
                    public readonly int x;
                    public readonly int y;
                }
            }
        }
    }
}