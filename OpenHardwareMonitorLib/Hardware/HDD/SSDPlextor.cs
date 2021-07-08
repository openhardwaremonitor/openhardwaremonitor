/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2011-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;
  using OpenHardwareMonitor.Collections;

  [NamePrefix("PLEXTOR")]
  internal class SSDPlextor : ATAStorage {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToValue),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToValue),
      new SmartAttribute(0xF1, SmartNames.HostWrites, RawToGb, SensorType.Data, 
        0, SmartNames.HostWrites),
      new SmartAttribute(0xF2, SmartNames.HostReads, RawToGb, SensorType.Data, 
        1, SmartNames.HostReads),
    };

    public SSDPlextor(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, "ssd", index, smartAttributes, settings) {}

    private static double RawToGb(byte[] rawvalue, byte value,
      IReadOnlyArray<IParameter> parameters) 
    {
      return RawToValue(rawvalue, value, parameters) / 32;
    }
  }
}
