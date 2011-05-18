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
  Portions created by the Initial Developer are Copyright (C) 2009-2010
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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI {
  public class SplitContainerAdv : SplitContainer {

    private int delta = 0;
    private Border3DStyle border3DStyle = Border3DStyle.Raised;
    private Color color = SystemColors.Control;

    public SplitContainerAdv()
      : base() {
      SetStyle(ControlStyles.ResizeRedraw, true);
      SetStyle(ControlStyles.AllPaintingInWmPaint, true);
      SetStyle(ControlStyles.UserPaint, true);
      SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
      SetStyle(ControlStyles.ContainerControl, true);
      UpdateStyles();
    }

    protected override void OnPaint(PaintEventArgs e) {
      base.OnPaint(e);

      Graphics g = e.Graphics;
      Rectangle r = SplitterRectangle;
      using (SolidBrush brush = new SolidBrush(color))
        g.FillRectangle(brush, r);
      ControlPaint.DrawBorder3D(g, r, border3DStyle);
    }

    protected override void OnKeyDown(KeyEventArgs e) {
      if (!base.IsSplitterFixed) {
        if (e.KeyData == Keys.Right || e.KeyData == Keys.Down) {
          SplitterDistance += SplitterIncrement;
        } else if (e.KeyData == Keys.Left || e.KeyData == Keys.Up) {
          SplitterDistance -= SplitterIncrement;
        }
        Invalidate();
      }
    }

    protected override void OnMouseDown(MouseEventArgs e) {
      if (Orientation == Orientation.Vertical) {
        delta = this.SplitterDistance - e.X;
        Cursor.Current = Cursors.VSplit;
      } else {
        delta = this.SplitterDistance - e.Y;
        Cursor.Current = Cursors.HSplit;
      }
      base.IsSplitterFixed = true;
    }

    protected override void OnMouseMove(MouseEventArgs e) {
      if (base.IsSplitterFixed) {
        if (e.Button == MouseButtons.Left) {
          if (Orientation == Orientation.Vertical) {
            if (e.X > 0 && e.X < Width) {
              SplitterDistance = e.X + delta < 0 ? 0 : e.X + delta;
            }
          } else {
            if (e.Y > 0 && e.Y < Height) {
              SplitterDistance = e.Y + delta < 0 ? 0 : e.Y + delta;
            }
          }
        } else {
          base.IsSplitterFixed = false;
        }
        Invalidate();
      } else {
        if (SplitterRectangle.Contains(e.Location)) {
          Cursor = Orientation == Orientation.Vertical ?
            Cursors.VSplit : Cursors.HSplit;
        }
      }
    }

    protected override void OnMouseLeave(EventArgs e) {
      base.OnMouseLeave(e);
      Cursor = Cursors.Default;
    }

    protected override void OnMouseUp(MouseEventArgs e) {
      delta = 0;
      base.IsSplitterFixed = false;
      Cursor.Current = Cursors.Default;
    }

    public Border3DStyle Border3DStyle {
      get { return border3DStyle; }
      set {
        border3DStyle = value;
        Invalidate(false);
      }
    }

    public Color Color {
      get { return color; }
      set {
        color = value;
        Invalidate(false);
      }
    }

    public new bool IsSplitterFixed {
      get {
        return false;
      }
    }

  }
}
