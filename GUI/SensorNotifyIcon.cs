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
    private Color color;
    private Color darkColor;
    private Brush brush;
    private Brush darkBrush;
    private Pen pen;
    private Font font;

    public SensorNotifyIcon(SystemTray sensorSystemTray, ISensor sensor,
      bool balloonTip, PersistentSettings settings) 
    {
      this.sensor = sensor;
      this.notifyIcon = new NotifyIcon();

      Color defaultColor = Color.Black;
      if (sensor.SensorType == SensorType.Load) {
        defaultColor = Color.FromArgb(0xff, 0x70, 0x8c, 0xf1);
      }
      Color = settings.Get(new Identifier(sensor.Identifier, 
        "traycolor").ToString(), defaultColor);      
      
      this.pen = new Pen(Color.FromArgb(96, Color.Black));
      this.font = SystemFonts.MessageBoxFont;

      ContextMenu contextMenu = new ContextMenu();
      MenuItem hideShowItem = new MenuItem("Hide/Show");
      hideShowItem.Click += delegate(object obj, EventArgs args) {
        sensorSystemTray.SendHideShowCommand();
      };
      contextMenu.MenuItems.Add(hideShowItem);
      contextMenu.MenuItems.Add(new MenuItem("-"));
      MenuItem removeItem = new MenuItem("Remove Sensor");
      removeItem.Click += delegate(object obj, EventArgs args) {
        sensorSystemTray.Remove(this.sensor);
      };
      contextMenu.MenuItems.Add(removeItem);
      MenuItem colorItem = new MenuItem("Change Color...");
      colorItem.Click += delegate(object obj, EventArgs args) {
        ColorDialog dialog = new ColorDialog();
        dialog.Color = Color;
        if (dialog.ShowDialog() == DialogResult.OK) {
          Color = dialog.Color;
          settings.Set(new Identifier(sensor.Identifier,
            "traycolor").ToString(), Color);
        }
      };
      contextMenu.MenuItems.Add(colorItem);
      contextMenu.MenuItems.Add(new MenuItem("-"));
      MenuItem exitItem = new MenuItem("Exit");
      exitItem.Click += delegate(object obj, EventArgs args) {
        sensorSystemTray.SendExitCommand();
      };
      contextMenu.MenuItems.Add(exitItem);
      this.notifyIcon.ContextMenu = contextMenu;
      this.notifyIcon.DoubleClick += delegate(object obj, EventArgs args) {
        sensorSystemTray.SendHideShowCommand();
      };

      this.bitmap = new Bitmap(16, 16, PixelFormat.Format32bppArgb);
      this.graphics = Graphics.FromImage(this.bitmap);

      if (Environment.OSVersion.Version.Major > 5) {
        this.graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        this.graphics.SmoothingMode = SmoothingMode.HighQuality;
      }
    }

    public ISensor Sensor {
      get { return sensor; }
    }

    public Color Color {
      get { return color; }
      set { 
        this.color = value;
        this.darkColor = Color.FromArgb(255,
          this.color.R / 3,
          this.color.G / 3,
          this.color.B / 3);
        Brush brush = this.brush;
        this.brush = new SolidBrush(this.color);
        if (brush != null)
          brush.Dispose();
        Brush darkBrush = this.darkBrush;
        this.darkBrush = new SolidBrush(this.darkColor);
        if (darkBrush != null)
          darkBrush.Dispose();
      }
    }

    public void Dispose() {      
      Icon icon = notifyIcon.Icon;
      notifyIcon.Icon = null;
      if (icon != null)
        icon.Dispose();      
      notifyIcon.Dispose();

      if (brush != null)
        brush.Dispose();
      if (darkBrush != null)
        darkBrush.Dispose();
      pen.Dispose();
      graphics.Dispose();      
      bitmap.Dispose();      
    }

    private string GetString() {
      switch (sensor.SensorType) {
        case SensorType.Voltage:
          return string.Format("{0:F11}", sensor.Value);
        case SensorType.Clock:
          return string.Format("{0:F11}", 1e-3f * sensor.Value);
        case SensorType.Load: 
          return string.Format("{0:F0}", sensor.Value);
        case SensorType.Temperature: 
          return string.Format("{0:F0}", sensor.Value);
        case SensorType.Fan: 
          return string.Format("{0:F11}", 1e-3f * sensor.Value);
        case SensorType.Flow:
          return string.Format("{0:F11}", 1e-3f * sensor.Value);
        case SensorType.Control:
          return string.Format("{0:F0}", sensor.Value);
      }
      return "-";
    }

    private Icon CreateTransparentIcon() {

      graphics.Clear(Color.Black);
      TextRenderer.DrawText(graphics, GetString(), font,
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

        bytes[i] = color.B;
        bytes[i + 1] = color.G;
        bytes[i + 2] = color.R;
        bytes[i + 3] = (byte)(0.3 * red + 0.59 * green + 0.11 * blue);
      }

      return IconFactory.Create(bytes, 16, 16, PixelFormat.Format32bppArgb);
    }

    private Icon CreateLoadIcon() {      
      try {
        graphics.Clear(Color.Transparent);
      } catch (ArgumentException) {
        graphics.Clear(Color.Black);
      }
      graphics.FillRectangle(darkBrush, 0.5f, -0.5f, 14, 16);
      float y = 0.16f * (100 - sensor.Value.Value);
      graphics.FillRectangle(brush, 0.5f, -0.5f + y, 14, 16 - y);
      graphics.DrawRectangle(pen, 1, 0, 13, 15);

      BitmapData data = bitmap.LockBits(
        new Rectangle(0, 0, bitmap.Width, bitmap.Height),
        ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
      byte[] bytes = new byte[bitmap.Width * bitmap.Height * 4];
      Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
      bitmap.UnlockBits(data);

      return IconFactory.Create(bytes, 16, 16, PixelFormat.Format32bppArgb);
    }

    public void Update() {
      Icon icon = notifyIcon.Icon;

      if (sensor.SensorType == SensorType.Load) {
        notifyIcon.Icon = CreateLoadIcon();
      } else {
        notifyIcon.Icon = CreateTransparentIcon();
      }
      if (icon != null) 
        icon.Dispose();

      string format = "";
      switch (sensor.SensorType) {
        case SensorType.Voltage: format = "\n{0}: {1:F2} V"; break;
        case SensorType.Clock: format = "\n{0}: {1:F0} MHz"; break;
        case SensorType.Load: format = "\n{0}: {1:F1} %"; break;
        case SensorType.Temperature: format = "\n{0}: {1:F1} °C"; break;
        case SensorType.Fan: format = "\n{0}: {1:F0} RPM"; break;
        case SensorType.Flow: format = "\n{0}: {1:F0} L/h"; break;
        case SensorType.Control: format = "\n{0}: {1:F1} %"; break;
      }
      string formattedValue = string.Format(format, sensor.Name, sensor.Value);
      string hardwareName = sensor.Hardware.Name;
      hardwareName = hardwareName.Substring(0, 
        Math.Min(63 - formattedValue.Length, hardwareName.Length));
      string text = hardwareName + formattedValue;
      if (text.Length > 63)
        text = null;

      notifyIcon.Text = text;
      notifyIcon.Visible = true;         
    }
  }
}
