/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012-2015 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;
  using OpenHardwareMonitor.Collections;

  [NamePrefix("SAMSUNG"), RequireSmart(0xB7), RequireSmart(0xBB), 
    RequireSmart(0xC3), RequireSmart(0xC7)]
  internal class HDDSamsung : ATAStorage {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x05, SmartNames.ReallocatedSectorsCount),
      new SmartAttribute(0x09, SmartNames.PowerOnHours, (raw, v, p) => {
        int result = (raw[3] << 24) | (raw[2] << 16) | (raw[1] << 8) | raw[0];
        return result;
      }, SensorType.RawValue, 0, SmartNames.PowerOnHours, false),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToValue),
      new SmartAttribute(0xAF, SmartNames.ProgramFailCountChip, RawToValue),
      new SmartAttribute(0xB0, SmartNames.EraseFailCountChip, RawToValue),
      new SmartAttribute(0xB1, SmartNames.WearLevelingCount, RawToValue, SensorType.RawValue, 1, SmartNames.WearLevelingCount, true),
      new SmartAttribute(0xB2, SmartNames.UsedReservedBlockCountChip, RawToValue),
      new SmartAttribute(0xB3, SmartNames.UsedReservedBlockCountTotal, RawToValue),

      // Unused Reserved Block Count (Total)
      new SmartAttribute(0xB4, SmartNames.RemainingLife,
        null, SensorType.Level, 0, SmartNames.RemainingLife),
      
      new SmartAttribute(0xB5, SmartNames.ProgramFailCountTotal, RawToValue),
      new SmartAttribute(0xB6, SmartNames.EraseFailCountTotal, RawToValue),
      new SmartAttribute(0xB7, SmartNames.RuntimeBadBlockTotal, RawToValue),
      new SmartAttribute(0xBB, SmartNames.UncorrectableErrorCount, RawToValue, SensorType.RawValue, 2, SmartNames.UncorrectableErrorCount, true),
      new SmartAttribute(0xBE, SmartNames.Temperature, 
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) 
          => { return SignedRawToValue(r, 1) + (p == null ? 0 : p[0].Value); }, 
          SensorType.Temperature, 0, SmartNames.Temperature, false, 
        new[] { new ParameterDescription("Offset [°C]", 
                  "Temperature offset of the thermal sensor.\n" + 
                  "Temperature = Value + Offset.", 0) }),
      new SmartAttribute(0xC2, SmartNames.AirflowTemperature),
      new SmartAttribute(0xC3, SmartNames.ECCRate, RawToValue, SensorType.RawValue, 3, SmartNames.ECCRate, true),
      new SmartAttribute(0xC6, SmartNames.OffLineUncorrectableErrorCount, RawToValue),
      new SmartAttribute(0xC7, SmartNames.CRCErrorCount, RawToValue, SensorType.RawValue, 4, SmartNames.CRCErrorCount, true),
      new SmartAttribute(0xC9, SmartNames.SupercapStatus),
      new SmartAttribute(0xCA, SmartNames.ExceptionModeStatus),
      new SmartAttribute(0xEB, SmartNames.PowerRecoveryCount),
      new SmartAttribute(0xF1, SmartNames.TotalLbasWritten, 
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) => { 
          return (((long)r[5] << 40) | ((long)r[4] << 32) | ((long)r[3] << 24) | 
            ((long)r[2] << 16) | ((long)r[1] << 8) | r[0]) * 
            (512.0f / 1024 / 1024 / 1024);
        }, SensorType.Data, 0, "Total Bytes Written")
    };

    public HDDSamsung(ISmart smart, string name, string firmwareRevision,
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, "hdd", index, smartAttributes, settings) { }
  }
}
