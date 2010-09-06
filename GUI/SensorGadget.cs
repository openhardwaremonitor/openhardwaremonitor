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
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI {
  public class SensorGadget : Gadget {

    private UnitManager unitManager;

    private Image back = Utilities.EmbeddedResources.GetImage("gadget.png");
    private Image barBack = Utilities.EmbeddedResources.GetImage("barback.png");
    private Image barblue = Utilities.EmbeddedResources.GetImage("barblue.png");
    private const int topBorder = 4;
    private const int bottomBorder = 6;
    private const int leftBorder = 6;
    private const int rightBorder = 6;
    private const int iconSize = 11;
    private const int hardwareLineHeight = 13;
    private const int sensorLineHeight = 11;

    private IDictionary<IHardware, IList<ISensor>> sensors =
      new SortedDictionary<IHardware, IList<ISensor>>(new HardwareComparer());

    private PersistentSettings settings;
    private UserOption alwaysOnTop;
    private UserOption lockPosition;

    private Font largeFont;
    private Font smallFont;
    private Brush darkWhite = new SolidBrush(Color.FromArgb(0xF0, 0xF0, 0xF0));

    public SensorGadget(IComputer computer, PersistentSettings settings, 
      UnitManager unitManager) 
    {
      this.unitManager = unitManager;
      this.settings = settings;
      computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
      computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);

      this.largeFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 7.5f, 
        FontStyle.Bold); 
      this.smallFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 6.5f);      

      this.Location = new Point(
        settings.GetValue("sensorGadget.Location.X", 100),
        settings.GetValue("sensorGadget.Location.Y", 100)); 
      LocationChanged += delegate(object sender, EventArgs e) {
        settings.SetValue("sensorGadget.Location.X", Location.X);
        settings.SetValue("sensorGadget.Location.Y", Location.Y);
      };
      
      ContextMenu contextMenu = new ContextMenu();
      MenuItem lockItem = new MenuItem("Lock Position");
      contextMenu.MenuItems.Add(lockItem);
      contextMenu.MenuItems.Add(new MenuItem("-"));
      MenuItem alwaysOnTopItem = new MenuItem("Always on Top");
      contextMenu.MenuItems.Add(alwaysOnTopItem);
      MenuItem opacityMenu = new MenuItem("Opacity");
      contextMenu.MenuItems.Add(opacityMenu);
      Opacity = (byte)settings.GetValue("sensorGadget.Opacity", 255);      
      for (int i = 0; i < 5; i++) {
        MenuItem item = new MenuItem((20 * (i + 1)).ToString() + " %");
        byte o = (byte)(51 * (i + 1));
        item.Tag = o;
        item.Checked = Opacity == o;
        item.Click += delegate(object sender, EventArgs e) {
          Opacity = (byte)item.Tag;
          settings.SetValue("sensorGadget.Opacity", Opacity);
          foreach (MenuItem mi in opacityMenu.MenuItems)
            mi.Checked = (byte)mi.Tag == Opacity;          
        };
        opacityMenu.MenuItems.Add(item);
      }
      this.ContextMenu = contextMenu;

      alwaysOnTop = new UserOption("sensorGadget.AlwaysOnTop", false, 
        alwaysOnTopItem, settings);
      alwaysOnTop.Changed += delegate(object sender, EventArgs e) {
        this.AlwaysOnTop = alwaysOnTop.Value;
      };
      lockPosition = new UserOption("sensorGadget.LockPosition", false,
        lockItem, settings);
      lockPosition.Changed += delegate(object sender, EventArgs e) {
        this.LockPosition = lockPosition.Value;
      };

      Resize();
    }

    private void HardwareRemoved(IHardware hardware) {
      hardware.SensorAdded -= new SensorEventHandler(SensorAdded);
      hardware.SensorRemoved -= new SensorEventHandler(SensorRemoved);
      foreach (ISensor sensor in hardware.Sensors)
        SensorRemoved(sensor);
      foreach (IHardware subHardware in hardware.SubHardware)
        HardwareRemoved(subHardware);
    }

    private void HardwareAdded(IHardware hardware) {
      foreach (ISensor sensor in hardware.Sensors)
        SensorAdded(sensor);
      hardware.SensorAdded += new SensorEventHandler(SensorAdded);
      hardware.SensorRemoved += new SensorEventHandler(SensorRemoved);
      foreach (IHardware subHardware in hardware.SubHardware)
        HardwareAdded(subHardware);
    }

    private void SensorAdded(ISensor sensor) {
      if (settings.GetValue(new Identifier(sensor.Identifier,
        "gadget").ToString(), false)) 
        Add(sensor);
    }

    private void SensorRemoved(ISensor sensor) {
      if (Contains(sensor))
        Remove(sensor, false);
    }

    public bool Contains(ISensor sensor) {
      foreach (IList<ISensor> list in sensors.Values)
        if (list.Contains(sensor))
          return true;
      return false;
    }

    public void Add(ISensor sensor) {
      if (Contains(sensor)) {
        return;
      } else {
        // get the right hardware
        IHardware hardware = sensor.Hardware;
        while (hardware.Parent != null)
          hardware = hardware.Parent;

        // get the sensor list associated with the hardware
        IList<ISensor> list;
        if (!sensors.TryGetValue(hardware, out list)) {
          list = new List<ISensor>();
          sensors.Add(hardware, list);
        }

        // insert the sensor at the right position
        int i = 0;
        while (i < list.Count && (list[i].SensorType < sensor.SensorType || 
          (list[i].SensorType == sensor.SensorType && 
           list[i].Index < sensor.Index))) i++;
        list.Insert(i, sensor);

        settings.SetValue(
          new Identifier(sensor.Identifier, "gadget").ToString(), true);
        
        Resize();
        Redraw();
      }
    }

    public void Remove(ISensor sensor) {
      Remove(sensor, true);
    }

    private void Remove(ISensor sensor, bool deleteConfig) {
      if (deleteConfig) 
        settings.Remove(new Identifier(sensor.Identifier, "gadget").ToString());

      foreach (KeyValuePair<IHardware, IList<ISensor>> keyValue in sensors)
        if (keyValue.Value.Contains(sensor)) {
          keyValue.Value.Remove(sensor);          
          if (keyValue.Value.Count == 0) {
            sensors.Remove(keyValue.Key);
            break;
          }
        }
      Resize();
      Redraw();
    }

    private void Resize() {
      int y = topBorder + 1;
      foreach (KeyValuePair<IHardware, IList<ISensor>> pair in sensors) {
        y += hardwareLineHeight;
        y += pair.Value.Count * sensorLineHeight;
      }
      y += bottomBorder + 2;
      y = Math.Max(y, topBorder + bottomBorder + 10);
      this.Size = new Size(130, y);
    }

    private void DrawBackground(Graphics g) {
      int w = Size.Width;
      int h = Size.Height;
      int t = topBorder;
      int b = bottomBorder;
      int l = leftBorder;
      int r = rightBorder;
      GraphicsUnit u = GraphicsUnit.Pixel;

      g.DrawImage(back, new Rectangle(0, 0, l, t),
        new Rectangle(0, 0, l, t), u);
      g.DrawImage(back, new Rectangle(l, 0, w - l - r, t),
        new Rectangle(l, 0, back.Width - l - r, t), u);
      g.DrawImage(back, new Rectangle(w - r, 0, r, t),
        new Rectangle(back.Width - r, 0, r, t), u);

      g.DrawImage(back, new Rectangle(0, t, l, h - t - b),
        new Rectangle(0, t, l, back.Height - t - b), u);
      g.DrawImage(back, new Rectangle(l, t, w - l - r, h - t - b),
        new Rectangle(l, t, back.Width - l - r, back.Height - t - b), u);
      g.DrawImage(back, new Rectangle(w - r, t, r, h - t - b),
        new Rectangle(back.Width - r, t, r, back.Height - t - b), u);

      g.DrawImage(back, new Rectangle(0, h - b, l, b),
        new Rectangle(0, back.Height - b, l, b), u);
      g.DrawImage(back, new Rectangle(l, h - b, w - l - r, b),
        new Rectangle(l, back.Height - b, back.Width - l - r, b), u);
      g.DrawImage(back, new Rectangle(w - r, h - b, r, b),
        new Rectangle(back.Width - r, back.Height - b, r, b), u);
    }

    private void DrawProgress(Graphics g, int x, int y, int width, int height,
      float progress) 
    {
      g.DrawImage(barBack, 
        new RectangleF(x + width * progress, y, width * (1 - progress), height), 
        new RectangleF(barBack.Width * progress, 0, 
          (1 - progress) * barBack.Width, barBack.Height), 
        GraphicsUnit.Pixel);
      g.DrawImage(barblue,
        new RectangleF(x, y, width * progress, height),
        new RectangleF(0, 0, progress * barblue.Width, barblue.Height),
        GraphicsUnit.Pixel);
    }

    protected override void OnPaint(PaintEventArgs e) {
      Graphics g = e.Graphics;
      int w = Size.Width;
      int h = Size.Height;

      g.Clear(Color.Transparent);

      DrawBackground(g);

      StringFormat stringFormat = new StringFormat();
      stringFormat.Alignment = StringAlignment.Far;

      int x;
      int y = topBorder + 1;
      foreach (KeyValuePair<IHardware, IList<ISensor>> pair in sensors) {
        x = leftBorder + 1;
        g.DrawImage(HardwareTypeImage.Instance.GetImage(pair.Key.HardwareType),
          new Rectangle(x, y + 2, iconSize, iconSize));
        x += iconSize + 1;
        g.DrawString(pair.Key.Name, largeFont, Brushes.White,
          new Rectangle(x, y, w - rightBorder - x, 15));
        y += hardwareLineHeight;

        foreach (ISensor sensor in pair.Value) {

          g.DrawString(sensor.Name + ":", smallFont, darkWhite,
            new Rectangle(9, y, 64, 15));
          
          if (sensor.SensorType != SensorType.Load && 
            sensor.SensorType != SensorType.Control) 
          {
            string format = "";
            switch (sensor.SensorType) {
              case SensorType.Voltage:
                format = "{0:F2} V";
                break;
              case SensorType.Clock:
                format = "{0:F0} MHz";
                break;
              case SensorType.Temperature:
                format = "{0:F1} °C";
                break;
              case SensorType.Fan:
                format = "{0:F0} RPM";
                break;
              case SensorType.Flow:
                format = "{0:F0} L/h";
                break;
            }

            string formattedValue;
            if (sensor.SensorType == SensorType.Temperature &&
              unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit) {
              formattedValue = string.Format("{0:F1} °F",
                sensor.Value * 1.8 + 32);
            } else {
              formattedValue = string.Format(format, sensor.Value);
            }

            x = 75;
            g.DrawString(formattedValue, smallFont, darkWhite,
              new RectangleF(x, y, w - x - 9, 15), stringFormat);            
          } else {
            x = 80;
            DrawProgress(g, x, y + 4, w - x - 9, 6, 0.01f * sensor.Value.Value);
          }

          y += sensorLineHeight;
        }
      }
    }

    private class HardwareComparer : IComparer<IHardware> {
      public int Compare(IHardware x, IHardware y) {
        if (x == null && y == null)
          return 0;
        if (x == null)
          return -1;
        if (y == null)
          return 1;

        if (x.HardwareType != y.HardwareType)
          return x.HardwareType.CompareTo(y.HardwareType);

        return x.Name.CompareTo(y.Name);
      }
    }
  }
}

