/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;

  [NamePrefix(""), RequireSmart(0xB0), RequireSmart(0xB1), RequireSmart(0xB2), 
    RequireSmart(0xB3), RequireSmart(0xB4), RequireSmart(0xB5), 
    RequireSmart(0xB6), RequireSmart(0xB7)]
  internal class SSDSamsung : AbstractHarddrive {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToInt),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToInt),
      new SmartAttribute(0xAF, SmartNames.ProgramFailCountChip, RawToInt),
      new SmartAttribute(0xB0, SmartNames.EraseFailCountChip, RawToInt),
      new SmartAttribute(0xB1, SmartNames.WearLevelingCount, RawToInt),
      new SmartAttribute(0xB2, SmartNames.UsedReservedBlockCountChip, RawToInt),
      new SmartAttribute(0xB3, SmartNames.UsedReservedBlockCountTotal, RawToInt),

      // Unused Reserved Block Count (Total)
      new SmartAttribute(0xB4, SmartNames.RemainingLife,
        null, SensorType.Level, 0),
      
      new SmartAttribute(0xB5, SmartNames.ProgramFailCountTotal, RawToInt),
      new SmartAttribute(0xB6, SmartNames.EraseFailCountTotal, RawToInt),
      new SmartAttribute(0xB7, SmartNames.RuntimeBadBlockTotal, RawToInt),
      new SmartAttribute(0xBB, SmartNames.UncorrectableErrorCount, RawToInt),
      new SmartAttribute(0xBE, SmartNames.TemperatureExceedCount, RawToInt),
      new SmartAttribute(0xC2, SmartNames.AirflowTemperature),
      new SmartAttribute(0xC3, SmartNames.ECCRate),
      new SmartAttribute(0xC6, SmartNames.OffLineUncorrectableErrorCount, RawToInt),
      new SmartAttribute(0xC7, SmartNames.CRCErrorCount, RawToInt),
      new SmartAttribute(0xC9, SmartNames.SupercapStatus),
      new SmartAttribute(0xCA, SmartNames.ExceptionModeStatus)
    };

    public SSDSamsung(ISmart smart, string name, string firmwareRevision,
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, index, smartAttributes, settings) { }
  }
}
