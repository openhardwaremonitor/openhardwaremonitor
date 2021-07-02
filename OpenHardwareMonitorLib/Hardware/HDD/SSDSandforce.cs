/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2015 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds
	
*/

using System.Collections.Generic;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware.HDD {

  [NamePrefix(""), RequireSmart(0xAB), RequireSmart(0xB1)]
  internal class SSDSandforce : ATAStorage {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x01, SmartNames.RawReadErrorRate),
      new SmartAttribute(0x05, SmartNames.RetiredBlockCount, RawToValue),
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToValue),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToValue),
      new SmartAttribute(0xAB, SmartNames.ProgramFailCount, RawToValue),
      new SmartAttribute(0xAC, SmartNames.EraseFailCount, RawToValue),
      new SmartAttribute(0xAE, SmartNames.UnexpectedPowerLossCount, RawToValue),
      new SmartAttribute(0xB1, SmartNames.WearRangeDelta, RawToValue),
      new SmartAttribute(0xB5, SmartNames.AlternativeProgramFailCount, RawToValue),
      new SmartAttribute(0xB6, SmartNames.AlternativeEraseFailCount, RawToValue),
      new SmartAttribute(0xBB, SmartNames.UncorrectableErrorCount, RawToValue),
      new SmartAttribute(0xC2, SmartNames.Temperature,
        (byte[] raw, byte value, IReadOnlyArray<IParameter> p)
          => {
          int v = value;
          // Manually sign-extend
          if (v > 0x7F) {
            v = (int)(0xFFFFFF00 | v);
          }
          return v + (p == null ? 0 : p[0].Value);
        }, 
        SensorType.Temperature, 0, SmartNames.Temperature, true, 
        new[] { new ParameterDescription("Offset [°C]", 
                  "Temperature offset of the thermal sensor.\n" + 
                  "Temperature = Value + Offset.", 0) }), 
      new SmartAttribute(0xC3, SmartNames.UnrecoverableEcc), 
      new SmartAttribute(0xC4, SmartNames.ReallocationEventCount, RawToValue),
      new SmartAttribute(0xE7, SmartNames.RemainingLife, null, 
        SensorType.Level, 0, SmartNames.RemainingLife),
      new SmartAttribute(0xE9, SmartNames.ControllerWritesToNAND, RawToValue,
        SensorType.Data, 0, SmartNames.ControllerWritesToNAND),
      new SmartAttribute(0xEA, SmartNames.HostWritesToController, RawToValue, 
        SensorType.Data, 1, SmartNames.HostWritesToController),
      new SmartAttribute(0xF1, SmartNames.HostWrites, RawToValue, 
        SensorType.Data, 1, SmartNames.HostWrites),
      new SmartAttribute(0xF2, SmartNames.HostReads, RawToValue, 
        SensorType.Data, 2, SmartNames.HostReads)
    };

    private Sensor writeAmplification;

    public SSDSandforce(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings) 
      : base(smart, name, firmwareRevision, "ssd", index, smartAttributes, settings) 
    {
      this.writeAmplification = new Sensor("Write Amplification", 1, 
        SensorType.Factor, this, settings);    
    }

    public override void UpdateAdditionalSensors(DriveAttributeValue[] values) {
      double? controllerWritesToNAND = null;
      double? hostWritesToController = null;
      foreach (DriveAttributeValue value in values) {
        if (value.Identifier == 0xE9)
          controllerWritesToNAND = RawToValue(value.RawValue, value.AttrValue, null);

        if (value.Identifier == 0xEA)
          hostWritesToController = RawToValue(value.RawValue, value.AttrValue, null);
      }
      if (controllerWritesToNAND.HasValue && hostWritesToController.HasValue) {
        if (hostWritesToController.Value > 0)
          writeAmplification.Value =
            controllerWritesToNAND.Value / hostWritesToController.Value;
        else
          writeAmplification.Value = 0;
        ActivateSensor(writeAmplification);
      }
    }
  }
}
