/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware {

  internal delegate void ControlEventHandler(Control control);

  internal class Control : IControl {
      private readonly ISettings settings;
    private ControlMode mode;
    private float softwareValue;

      public Control(ISensor sensor, ISettings settings, float minSoftwareValue,
      float maxSoftwareValue) 
    {
      Identifier = new Identifier(sensor.Identifier, "control");
      this.settings = settings;
      this.MinSoftwareValue = minSoftwareValue;
      this.MaxSoftwareValue = maxSoftwareValue;

      if (!float.TryParse(settings.GetValue(
          new Identifier(Identifier, "value").ToString(), "0"),
        NumberStyles.Float, CultureInfo.InvariantCulture,
        out softwareValue)) 
      {
        softwareValue = 0;
      }
            if (!int.TryParse(settings.GetValue(
                new Identifier(Identifier, "mode").ToString(),
                ((int)ControlMode.Undefined).ToString(CultureInfo.InvariantCulture)),
              NumberStyles.Integer, CultureInfo.InvariantCulture,
              out int mode))
            {
                this.mode = ControlMode.Undefined;
            }
            else
            {
                this.mode = (ControlMode)mode;
            }
        }

    public Identifier Identifier { get; }

      public ControlMode ControlMode {
      get => mode;
          private set {
        if (mode != value) {
          mode = value;
                    ControlModeChanged?.Invoke(this);
                    settings.SetValue(new Identifier(Identifier, "mode").ToString(),
            ((int)mode).ToString(CultureInfo.InvariantCulture));
        }
      }
    }

    public float SoftwareValue {
      get => softwareValue;
        private set {
        if (softwareValue != value) {
          softwareValue = value;
                    SoftwareControlValueChanged?.Invoke(this);
                    settings.SetValue(new Identifier(Identifier,
            "value").ToString(),
            value.ToString(CultureInfo.InvariantCulture));
        }
      }
    }

    public void SetDefault() {
      ControlMode = ControlMode.Default;
    }

    public float MinSoftwareValue { get; }

      public float MaxSoftwareValue { get; }

      public void SetSoftware(float value) {
      ControlMode = ControlMode.Software;
      SoftwareValue = value;
    }

    internal event ControlEventHandler ControlModeChanged;
    internal event ControlEventHandler SoftwareControlValueChanged;
  }
}
