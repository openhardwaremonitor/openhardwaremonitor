/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI
{
    public class SensorNotifyIcon : IDisposable
    {
        private readonly Bitmap bitmap;
        private Brush brush;
        private Color color;
        private Brush darkBrush;
        private Color darkColor;
        private readonly Font font;
        private readonly Graphics graphics;
        private readonly NotifyIconAdv notifyIcon;
        private readonly Pen pen;

        private readonly Font smallFont;

        private readonly UnitManager unitManager;

        public SensorNotifyIcon(SystemTray sensorSystemTray, ISensor sensor,
            bool balloonTip, PersistentSettings settings, UnitManager unitManager)
        {
            this.unitManager = unitManager;
            Sensor = sensor;
            notifyIcon = new NotifyIconAdv();

            var defaultColor = Color.Black;
            if (sensor.SensorType == SensorType.Load ||
                sensor.SensorType == SensorType.Control ||
                sensor.SensorType == SensorType.Level)
            {
                defaultColor = Color.FromArgb(0xff, 0x70, 0x8c, 0xf1);
            }
            Color = settings.GetValue(new Identifier(sensor.Identifier,
                "traycolor").ToString(), defaultColor);

            pen = new Pen(Color.FromArgb(96, Color.Black));

            var contextMenu = new ContextMenu();
            var hideShowItem = new MenuItem("Hide/Show");
            hideShowItem.Click += delegate { sensorSystemTray.SendHideShowCommand(); };
            contextMenu.MenuItems.Add(hideShowItem);
            contextMenu.MenuItems.Add(new MenuItem("-"));
            var removeItem = new MenuItem("Remove Sensor");
            removeItem.Click += delegate { sensorSystemTray.Remove(this.Sensor); };
            contextMenu.MenuItems.Add(removeItem);
            var colorItem = new MenuItem("Change Color...");
            colorItem.Click += delegate
            {
                var dialog = new ColorDialog();
                dialog.Color = Color;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Color = dialog.Color;
                    settings.SetValue(new Identifier(sensor.Identifier,
                        "traycolor").ToString(), Color);
                }
            };
            contextMenu.MenuItems.Add(colorItem);
            contextMenu.MenuItems.Add(new MenuItem("-"));
            var exitItem = new MenuItem("Exit");
            exitItem.Click += delegate { sensorSystemTray.SendExitCommand(); };
            contextMenu.MenuItems.Add(exitItem);
            notifyIcon.ContextMenu = contextMenu;
            notifyIcon.DoubleClick += delegate { sensorSystemTray.SendHideShowCommand(); };

            // get the default dpi to create an icon with the correct size
            float dpiX, dpiY;
            using (var b = new Bitmap(1, 1, PixelFormat.Format32bppArgb))
            {
                dpiX = b.HorizontalResolution;
                dpiY = b.VerticalResolution;
            }

            // adjust the size of the icon to current dpi (default is 16x16 at 96 dpi) 
            var width = (int) Math.Round(16*dpiX/96);
            var height = (int) Math.Round(16*dpiY/96);

            // make sure it does never get smaller than 16x16
            width = width < 16 ? 16 : width;
            height = height < 16 ? 16 : height;

            // adjust the font size to the icon size
            var family = SystemFonts.MessageBoxFont.FontFamily;
            float baseSize;
            switch (family.Name)
            {
                case "Segoe UI":
                    baseSize = 12;
                    break;
                case "Tahoma":
                    baseSize = 11;
                    break;
                default:
                    baseSize = 12;
                    break;
            }

            font = new Font(family,
                baseSize*width/16.0f, GraphicsUnit.Pixel);
            smallFont = new Font(family,
                0.75f*baseSize*width/16.0f, GraphicsUnit.Pixel);

            bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            graphics = Graphics.FromImage(bitmap);

            if (Environment.OSVersion.Version.Major > 5)
            {
                graphics.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
            }
        }

        public ISensor Sensor { get; }

        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                darkColor = Color.FromArgb(255,
                    color.R/3,
                    color.G/3,
                    color.B/3);
                var brush = this.brush;
                this.brush = new SolidBrush(color);
                if (brush != null)
                    brush.Dispose();
                var darkBrush = this.darkBrush;
                this.darkBrush = new SolidBrush(darkColor);
                if (darkBrush != null)
                    darkBrush.Dispose();
            }
        }

        public void Dispose()
        {
            var icon = notifyIcon.Icon;
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
            font.Dispose();
            smallFont.Dispose();
        }

        private string GetString()
        {
            if (!Sensor.Value.HasValue)
                return "-";

            switch (Sensor.SensorType)
            {
                case SensorType.Voltage:
                    return string.Format("{0:F1}", Sensor.Value);
                case SensorType.Clock:
                    return string.Format("{0:F1}", 1e-3f*Sensor.Value);
                case SensorType.Load:
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Temperature:
                    if (unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit)
                        return string.Format("{0:F0}",
                            UnitManager.CelsiusToFahrenheit(Sensor.Value));
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Fan:
                    return string.Format("{0:F1}", 1e-3f*Sensor.Value);
                case SensorType.Flow:
                    return string.Format("{0:F1}", 1e-3f*Sensor.Value);
                case SensorType.Control:
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Level:
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Power:
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Data:
                    return string.Format("{0:F0}", Sensor.Value);
                case SensorType.Factor:
                    return string.Format("{0:F1}", Sensor.Value);
            }
            return "-";
        }

        private Icon CreateTransparentIcon()
        {
            var text = GetString();
            var count = 0;
            for (var i = 0; i < text.Length; i++)
                if ((text[i] >= '0' && text[i] <= '9') || text[i] == '-')
                    count++;
            var small = count > 2;

            graphics.Clear(Color.Black);
            TextRenderer.DrawText(graphics, text, small ? smallFont : font,
                new Point(-2, small ? 1 : 0), Color.White, Color.Black);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

            var Scan0 = data.Scan0;

            var numBytes = bitmap.Width*bitmap.Height*4;
            var bytes = new byte[numBytes];
            Marshal.Copy(Scan0, bytes, 0, numBytes);
            bitmap.UnlockBits(data);

            byte red, green, blue;
            for (var i = 0; i < bytes.Length; i += 4)
            {
                blue = bytes[i];
                green = bytes[i + 1];
                red = bytes[i + 2];

                bytes[i] = color.B;
                bytes[i + 1] = color.G;
                bytes[i + 2] = color.R;
                bytes[i + 3] = (byte) (0.3*red + 0.59*green + 0.11*blue);
            }

            return IconFactory.Create(bytes, bitmap.Width, bitmap.Height,
                PixelFormat.Format32bppArgb);
        }

        private Icon CreatePercentageIcon()
        {
            try
            {
                graphics.Clear(Color.Transparent);
            }
            catch (ArgumentException)
            {
                graphics.Clear(Color.Black);
            }
            graphics.FillRectangle(darkBrush, 0.5f, -0.5f, bitmap.Width - 2, bitmap.Height);
            var value = Sensor.Value.GetValueOrDefault();
            var y = 0.16f*(100 - value);
            graphics.FillRectangle(brush, 0.5f, -0.5f + y, bitmap.Width - 2, bitmap.Height - y);
            graphics.DrawRectangle(pen, 1, 0, bitmap.Width - 3, bitmap.Height - 1);

            var data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var bytes = new byte[bitmap.Width*bitmap.Height*4];
            Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
            bitmap.UnlockBits(data);

            return IconFactory.Create(bytes, bitmap.Width, bitmap.Height,
                PixelFormat.Format32bppArgb);
        }

        public void Update()
        {
            var icon = notifyIcon.Icon;

            switch (Sensor.SensorType)
            {
                case SensorType.Load:
                case SensorType.Control:
                case SensorType.Level:
                    notifyIcon.Icon = CreatePercentageIcon();
                    break;
                default:
                    notifyIcon.Icon = CreateTransparentIcon();
                    break;
            }

            if (icon != null)
                icon.Dispose();

            var format = "";
            switch (Sensor.SensorType)
            {
                case SensorType.Voltage:
                    format = "\n{0}: {1:F2} V";
                    break;
                case SensorType.Clock:
                    format = "\n{0}: {1:F0} MHz";
                    break;
                case SensorType.Load:
                    format = "\n{0}: {1:F1} %";
                    break;
                case SensorType.Temperature:
                    format = "\n{0}: {1:F1} °C";
                    break;
                case SensorType.Fan:
                    format = "\n{0}: {1:F0} RPM";
                    break;
                case SensorType.Flow:
                    format = "\n{0}: {1:F0} L/h";
                    break;
                case SensorType.Control:
                    format = "\n{0}: {1:F1} %";
                    break;
                case SensorType.Level:
                    format = "\n{0}: {1:F1} %";
                    break;
                case SensorType.Power:
                    format = "\n{0}: {1:F0} W";
                    break;
                case SensorType.Data:
                    format = "\n{0}: {1:F0} GB";
                    break;
                case SensorType.Factor:
                    format = "\n{0}: {1:F3} GB";
                    break;
            }
            var formattedValue = string.Format(format, Sensor.Name, Sensor.Value);

            if (Sensor.SensorType == SensorType.Temperature &&
                unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit)
            {
                format = "\n{0}: {1:F1} °F";
                formattedValue = string.Format(format, Sensor.Name,
                    UnitManager.CelsiusToFahrenheit(Sensor.Value));
            }

            var hardwareName = Sensor.Hardware.Name;
            hardwareName = hardwareName.Substring(0,
                Math.Min(63 - formattedValue.Length, hardwareName.Length));
            var text = hardwareName + formattedValue;
            if (text.Length > 63)
                text = null;

            notifyIcon.Text = text;
            notifyIcon.Visible = true;
        }
    }
}