/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using static OpenHardwareMonitor.Hardware.Control;

namespace OpenHardwareMonitor.Hardware {

  public enum ControlMode {
    Undefined,
    Software,
    Default,
    SoftwareCurve
  }

  public interface IControl {

    Identifier Identifier { get; }

    ControlMode ControlMode { get; }

    ControlMode ActualControlMode { get; }

    float SoftwareValue { get; }

    void SetDefault();

    float MinSoftwareValue { get; }
    float MaxSoftwareValue { get; }

    void SetSoftware(float value);

    void SetSoftwareCurve(List<ISoftwareCurvePoint> points, ISensor sensor);
    SoftwareCurve GetSoftwareCurve();
    void NotifyHardwareAdded(List<IGroup> allhardware);
    void NotifyHardwareRemoved(IHardware hardware);
    void NotifyClosing();

    }

    public interface ISoftwareCurvePoint
    {
        float SensorValue { get; set; }
        float ControlValue { get; set; }
    }
}
