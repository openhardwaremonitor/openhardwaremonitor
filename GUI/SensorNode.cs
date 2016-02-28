/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI
{
    public class SensorNode : Node
    {
        private readonly string format;
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
                case SensorType.Factor:
                    format = "{0:F3}";
                    break;
            }

            var hidden = settings.GetValue(new Identifier(sensor.Identifier,
                "hidden").ToString(), sensor.IsDefaultHidden);
            base.IsVisible = !hidden;

            Plot = settings.GetValue(new Identifier(sensor.Identifier,
                "plot").ToString(), false);
        }

        public override string Text
        {
            get { return Sensor.Name; }
            set { Sensor.Name = value; }
        }

        public override bool IsVisible
        {
            get { return base.IsVisible; }
            set
            {
                base.IsVisible = value;
                settings.SetValue(new Identifier(Sensor.Identifier,
                    "hidden").ToString(), !value);
            }
        }

        public bool Plot
        {
            get { return plot; }
            set
            {
                plot = value;
                settings.SetValue(new Identifier(Sensor.Identifier, "plot").ToString(),
                    value);
                if (PlotSelectionChanged != null)
                    PlotSelectionChanged(this, null);
            }
        }

        public ISensor Sensor { get; }

        public string Value
        {
            get { return ValueToString(Sensor.Value); }
        }

        public string Min
        {
            get { return ValueToString(Sensor.Min); }
        }

        public string Max
        {
            get { return ValueToString(Sensor.Max); }
        }

        public string ValueToString(float? value)
        {
            if (value.HasValue)
            {
                if (Sensor.SensorType == SensorType.Temperature &&
                    unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit)
                {
                    return string.Format("{0:F1} °F", value*1.8 + 32);
                }
                return string.Format(format, value);
            }
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

            return (Sensor == s.Sensor);
        }

        public override int GetHashCode()
        {
            return Sensor.GetHashCode();
        }
    }
}