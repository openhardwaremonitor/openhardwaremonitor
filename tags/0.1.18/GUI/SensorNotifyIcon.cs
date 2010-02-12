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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public class SensorNotifyIcon : IDisposable {

    private ISensor sensor;
    private NotifyIcon notifyIcon;
    private Bitmap bitmap;
    private Graphics graphics;
    private int majorVersion;

    public SensorNotifyIcon(SensorSystemTray sensorSystemTray, ISensor sensor) {
      this.sensor = sensor;
      this.notifyIcon = new NotifyIcon();
      this.majorVersion = Environment.OSVersion.Version.Major;      
      
      ContextMenuStrip contextMenuStrip = new ContextMenuStrip();
      ToolStripMenuItem item = new ToolStripMenuItem("Remove");
      item.Click += delegate(object obj, EventArgs args) {
        sensorSystemTray.Remove(sensor);
      };
      contextMenuStrip.Items.Add(item);
      this.notifyIcon.ContextMenuStrip = contextMenuStrip;

      this.bitmap = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
      this.graphics = Graphics.FromImage(this.bitmap);
      this.graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
      this.graphics.SmoothingMode = SmoothingMode.HighQuality;
    }

    public ISensor Sensor {
      get { return sensor; }
    }

    public void Dispose() {      
      Icon icon = notifyIcon.Icon;
      notifyIcon.Icon = null;
      if (icon != null)
        icon.Dispose();      
      notifyIcon.Dispose();
      notifyIcon = null;

      graphics.Dispose();
      graphics = null;
      bitmap.Dispose();
      bitmap = null;
    }

    private string GetString() {
      switch (sensor.SensorType) {
        case SensorType.Voltage:
          return string.Format("{0:F11}", sensor.Value);
        case SensorType.Clock:
          return string.Format("{0:F11}", 1e-3f * sensor.Value);
        case SensorType.Load: 
          return string.Format("{0:F0}", sensor.Value < 99 ? sensor.Value : 99);
        case SensorType.Temperature: 
          return string.Format("{0:F0}", sensor.Value);
        case SensorType.Fan: 
          return string.Format("{0:F11}", 1e-3f * sensor.Value);
      }
      return "-";
    }

    private Icon CreateSimpleIcon() {

      graphics.Clear(SystemColors.ButtonFace);
      TextRenderer.DrawText(graphics, GetString(), SystemFonts.StatusFont,
        new Point(-2, 0), Color.Blue, SystemColors.ButtonFace);

      BitmapData data = bitmap.LockBits(
        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

      int stride = data.Stride;
      IntPtr Scan0 = data.Scan0;

      int numBytes = bitmap.Width * bitmap.Height * 4;
      byte[] bytes = new byte[numBytes];
      Marshal.Copy(Scan0, bytes, 0, numBytes);
      bitmap.UnlockBits(data);

      return IconFactory.Create(bytes, 16, 16, PixelFormat.Format32bppArgb);
    }

    private Icon CreateTransparentIcon() {

      graphics.Clear(Color.Black);
      TextRenderer.DrawText(graphics, GetString(), SystemFonts.StatusFont,
        new Point(-2, 0), Color.White, Color.Black);

      BitmapData data = bitmap.LockBits(
        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

      int stride = data.Stride;
      IntPtr Scan0 = data.Scan0;

      int numBytes = bitmap.Width * bitmap.Height * 4;
      byte[] bytes = new byte[numBytes];
      Marshal.Copy(Scan0, bytes, 0, numBytes);
      bitmap.UnlockBits(data);

      byte red, green, blue;
      for (int i = 0; i < bytes.Length; i += 4) {
        blue = bytes[i];
        green = bytes[i + 1];
        red = bytes[i + 2];

        bytes[i] = 255;
        bytes[i + 1] = 255;
        bytes[i + 2] = 255;
        bytes[i + 3] = (byte)(0.3 * red + 0.59 * green + 0.11 * blue);
      }

      return IconFactory.Create(bytes, 16, 16, PixelFormat.Format32bppArgb);
    }

    public void Update() {
      Icon icon = notifyIcon.Icon;

      if (majorVersion < 6) {
        notifyIcon.Icon = CreateSimpleIcon();
      } else {
        notifyIcon.Icon = CreateTransparentIcon();
      }
      
      if (icon != null) 
        icon.Dispose();

      string format = "";
      switch (sensor.SensorType) {
        case SensorType.Voltage: format = "{0}\n{1}: {2:F2} V"; break;
        case SensorType.Clock: format = "{0}\n{1}: {2:F0} MHz"; break;
        case SensorType.Load: format = "{0}\n{1}: {2:F1} %"; break;
        case SensorType.Temperature: format = "{0}\n{1}: {2:F1} °C"; break;
        case SensorType.Fan: format = "{0}\n{1}: {2:F0} RPM"; break;
      }

      notifyIcon.Text = string.Format(format, 
        sensor.Hardware.Name, sensor.Name, sensor.Value);
      notifyIcon.Visible = true;
    }
  }
}
