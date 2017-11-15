/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2016 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Drawing;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI
{
    public class SensorNode : Node
    {
        private readonly string format;
        private Color? penColor;
        private bool plot;

        private readonly PersistentSettings settings;
        private readonly UnitManager unitManager;

        public SensorNode(ISensor sensor, PersistentSettings settings,
            UnitManager unitManager)
        {
            Sensor = sensor;
            this.settings = settings;
            this.unitManager = unitManager;
            switch (sensor.SensorType)
            {
                case SensorType.Voltage:
                    format = "{0:F3} V";
                    break;
                case SensorType.Clock:
                    format = "{0:F0} MHz";
                    break;
                case SensorType.Load:
                    format = "{0:F1} %";
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
                case SensorType.Control:
                    format = "{0:F1} %";
                    break;
                case SensorType.Level:
                    format = "{0:F1} %";
                    break;
                case SensorType.Power:
                    format = "{0:F1} W";
                    break;
                case SensorType.Data:
                    format = "{0:F1} GB";
                    break;
                case SensorType.SmallData:
                    format = "{0:F1} MB";
                    break;
                case SensorType.Factor:
                    format = "{0:F3}";
                    break;
            }

            var hidden = settings.GetValue(new Identifier(sensor.Identifier,
                "hidden").ToString(), sensor.IsDefaultHidden);
            base.IsVisible = !hidden;

            Plot = settings.GetValue(new Identifier(sensor.Identifier,
                "plot").ToString(), false);

            var id = new Identifier(sensor.Identifier, "penColor").ToString();
            if (settings.Contains(id))
                PenColor = settings.GetValue(id, Color.Black);
        }

        public override string Text
        {
            get => Sensor.Name;
            set => Sensor.Name = value;
        }

        public override bool IsVisible
        {
            get => base.IsVisible;
            set
            {
                base.IsVisible = value;
                settings.SetValue(new Identifier(Sensor.Identifier,
                    "hidden").ToString(), !value);
            }
        }

        public Color? PenColor
        {
            get => penColor;
            set
            {
                penColor = value;

                var id = new Identifier(Sensor.Identifier, "penColor").ToString();
                if (value.HasValue)
                    settings.SetValue(id, value.Value);
                else
                    settings.Remove(id);

                PlotSelectionChanged?.Invoke(this, null);
            }
        }

        public bool Plot
        {
            get => plot;
            set
            {
                plot = value;
                settings.SetValue(new Identifier(Sensor.Identifier, "plot").ToString(),
                    value);
                PlotSelectionChanged?.Invoke(this, null);
            }
        }

        public ISensor Sensor { get; }

        public string Value => ValueToString(Sensor.Value);

        public string Min => ValueToString(Sensor.Min);

        public string Max => ValueToString(Sensor.Max);

        public string ValueToString(float? value)
        {
            if (value.HasValue)
                if (Sensor.SensorType == SensorType.Temperature &&
                    unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit)
                    return string.Format("{0:F1} °F", value * 1.8 + 32);
                else return string.Format(format, value);
            return "-";
        }

        public event EventHandler PlotSelectionChanged;

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            var s = obj as SensorNode;
            if (s == null)
                return false;

            return Sensor == s.Sensor;
        }

        public override int GetHashCode()
        {
            return Sensor.GetHashCode();
        }
    }
}