/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using OpenHardwareMonitor.Hardware;
using System;
using System.Drawing;

namespace OpenHardwareMonitor.GUI {
  public class SensorNode : Node {
    
    private ISensor sensor;
    private PersistentSettings settings;
    private UnitManager unitManager;
    private string fixedFormat;
    private bool plot = false;
    private Color? penColor = null;

    public string ValueToString(float? value) {
      if (value.HasValue) {
        switch (sensor.SensorType) {
          case SensorType.Temperature:
            if (unitManager.TemperatureUnit == TemperatureUnit.Fahrenheit)
              return string.Format("{0:F1} °F", value * 1.8 + 32);
            else
              return string.Format("{0:F1} °C", value);
          case SensorType.Throughput:
            if (value < 1)
              return string.Format("{0:F1} KB/s", value * 0x400);
            else
              return string.Format("{0:F1} MB/s", value);  
          default:
            return string.Format(fixedFormat, value);
        }              
      } else
        return "-";
    }

    public SensorNode(ISensor sensor, PersistentSettings settings, 
      UnitManager unitManager) : base() {      
      this.sensor = sensor;
      this.settings = settings;
      this.unitManager = unitManager;
      switch (sensor.SensorType) {
        case SensorType.Voltage: fixedFormat = "{0:F3} V"; break;
        case SensorType.Clock: fixedFormat = "{0:F1} MHz"; break;
        case SensorType.Load: fixedFormat = "{0:F1} %"; break;
        case SensorType.Fan: fixedFormat = "{0:F0} RPM"; break;
        case SensorType.Flow: fixedFormat = "{0:F0} L/h"; break;
        case SensorType.Control: fixedFormat = "{0:F1} %"; break;
        case SensorType.Level: fixedFormat = "{0:F1} %"; break;
        case SensorType.Power: fixedFormat = "{0:F1} W"; break;
        case SensorType.Data: fixedFormat = "{0:F1} GB"; break;
        case SensorType.SmallData: fixedFormat = "{0:F1} MB"; break;
        case SensorType.Factor: fixedFormat = "{0:F3}"; break;
        default: fixedFormat = ""; break;
      }

      bool hidden = settings.GetValue(new Identifier(sensor.Identifier, 
        "hidden").ToString(), sensor.IsDefaultHidden);
      base.IsVisible = !hidden;

      this.Plot = settings.GetValue(new Identifier(sensor.Identifier, 
        "plot").ToString(), false);

      string id = new Identifier(sensor.Identifier, "penColor").ToString();
      if (settings.Contains(id))
        this.PenColor = settings.GetValue(id, Color.Black);
    }

    public override string Text {
      get { return sensor.Name; }
      set { sensor.Name = value; }
    }

    public override bool IsVisible {
      get { return base.IsVisible; }
      set { 
        base.IsVisible = value;
        settings.SetValue(new Identifier(sensor.Identifier,
          "hidden").ToString(), !value);
      }
    }

    public Color? PenColor {
      get { return penColor; }
      set {
        penColor = value;

        string id = new Identifier(sensor.Identifier, "penColor").ToString();
        if (value.HasValue)
          settings.SetValue(id, value.Value);
        else
          settings.Remove(id);

        if (PlotSelectionChanged != null)
          PlotSelectionChanged(this, null);
      }
    }

    public bool Plot {
      get { return plot; }
      set { 
        plot = value;
        settings.SetValue(new Identifier(sensor.Identifier, "plot").ToString(), 
          value);
        if (PlotSelectionChanged != null)
          PlotSelectionChanged(this, null);
      }
    }

    public event EventHandler PlotSelectionChanged;

    public ISensor Sensor {
      get { return sensor; }
    }

    public string Value {
      get { return ValueToString(sensor.Value); }
    }

    public string Min {
      get { return ValueToString(sensor.Min); }
    }

    public string Max {
      get { return ValueToString(sensor.Max); }
    }

    public override bool Equals(System.Object obj) {
      if (obj == null) 
        return false;

      SensorNode s = obj as SensorNode;
      if (s == null) 
        return false;

      return (sensor == s.sensor);
    }

    public override int GetHashCode() {
      return sensor.GetHashCode();
    }

  }
}
