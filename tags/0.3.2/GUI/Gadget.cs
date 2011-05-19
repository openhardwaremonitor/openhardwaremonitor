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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI {
  public abstract class Gadget : IDisposable {

    private GadgetWindow window;
    private Bitmap buffer;
    private Graphics graphics;

    public Gadget() {
      this.window = new GadgetWindow();
      CreateBuffer();
    }

    public virtual void Dispose() {
      DisposeBuffer();
    }

    public Point Location {
      get {
        return window.Location;
      }
      set {
        window.Location = value;
      }
    }

    public event EventHandler LocationChanged {
      add {
        window.LocationChanged += value;
      }
      remove {
        window.LocationChanged -= value;
      }
    }

    public virtual Size Size {
      get {
        return window.Size; 
      }
      set {        
        this.window.Size = value;
      }
    }

    public event EventHandler SizeChanged {
      add {
        window.SizeChanged += value;
      }
      remove {
        window.SizeChanged -= value;
      }
    }

    public byte Opacity {
      get {
        return window.Opacity;
      }
      set {
        window.Opacity = value;
      }
    }

    public bool LockPositionAndSize {
      get {
        return window.LockPositionAndSize;
      }
      set {
        window.LockPositionAndSize = value;
      }
    }

    public bool AlwaysOnTop {
      get {
        return window.AlwaysOnTop;
      }
      set {
        window.AlwaysOnTop = value;
      }
    }

    public ContextMenu ContextMenu {
      get {
        return window.ContextMenu;
      }
      set {
        window.ContextMenu = value;
      }
    }

    public event HitTestEventHandler HitTest {
      add {
        window.HitTest += value;
      }
      remove {
        window.HitTest -= value;
      }
    }

    public event MouseEventHandler MouseDoubleClick {
      add {
        window.MouseDoubleClick += value;
      }
      remove {
        window.MouseDoubleClick -= value;
      }
    }

    private void CreateBuffer() {
      this.buffer = new Bitmap(window.Size.Width, window.Size.Height, 
        PixelFormat.Format32bppArgb);
      this.graphics = Graphics.FromImage(this.buffer);
      if (Environment.OSVersion.Version.Major > 5) {
        this.graphics.TextRenderingHint = TextRenderingHint.SystemDefault;
        this.graphics.SmoothingMode = SmoothingMode.HighQuality;
      }
    }

    private void DisposeBuffer() {
      if (buffer != null) {
        this.buffer.Dispose();
        this.buffer = null;
      }
      if (graphics != null) {
        this.graphics.Dispose();
        this.graphics = null;
      }
    }

    public bool Visible {
      get {
        return window.Visible;
      }
      set {
        if (value != window.Visible) {
          window.Visible = value;
          if (VisibleChanged != null)
            VisibleChanged(this, EventArgs.Empty);
          if (value)
            Redraw();          
        }
      }
    }

    public event EventHandler VisibleChanged;

    public void Redraw() {
      if (!window.Visible)
        return;
      
      if (window.Size != buffer.Size) {
        DisposeBuffer();
        CreateBuffer();
      }

      OnPaint(new PaintEventArgs(graphics, 
        new Rectangle(Point.Empty, window.Size)));
      window.Update(buffer);
    }

    protected abstract void OnPaint(PaintEventArgs e);
  
  }
}
