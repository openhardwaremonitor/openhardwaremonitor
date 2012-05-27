/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2011-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Hardware.HDD {
  using System.Collections.Generic;

  [NamePrefix("PLEXTOR")]
  internal class SSDPlextor : AbstractHarddrive {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToInt),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToInt),
    };

    public SSDPlextor(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, index, smartAttributes, settings) {}
  }
}
