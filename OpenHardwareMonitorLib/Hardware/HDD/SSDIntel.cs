/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds
  Copyright (C) 2011 Roland Reinl <roland-reinl@gmx.de>
	
*/

using System.Collections.Generic;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware.HDD {
   
  [NamePrefix("INTEL SSD"), 
   RequireSmart(0xE1), RequireSmart(0xE8), RequireSmart(0xE9)]
  internal class SSDIntel : ATAStorage {

    private static readonly IEnumerable<SmartAttribute> smartAttributes =
      new List<SmartAttribute> {

      new SmartAttribute(0x01, SmartNames.ReadErrorRate),
      new SmartAttribute(0x03, SmartNames.SpinUpTime),
      new SmartAttribute(0x04, SmartNames.StartStopCount, RawToValue),
      new SmartAttribute(0x05, SmartNames.ReallocatedSectorsCount),
      new SmartAttribute(0x09, SmartNames.PowerOnHours, RawToValue),
      new SmartAttribute(0x0C, SmartNames.PowerCycleCount, RawToValue),
      new SmartAttribute(0xAA, SmartNames.AvailableReservedSpace),
      new SmartAttribute(0xAB, SmartNames.ProgramFailCount),
      new SmartAttribute(0xAC, SmartNames.EraseFailCount),
      new SmartAttribute(0xAE, SmartNames.UnexpectedPowerLossCount, RawToValue),
      new SmartAttribute(0xB7, SmartNames.SataDownshiftErrorCount, RawToValue),
      new SmartAttribute(0xBB, SmartNames.UncorrectableErrorCount, RawToValue),
      new SmartAttribute(0xB8, SmartNames.EndToEndError),
      new SmartAttribute(0xBE, SmartNames.Temperature,
        (byte[] r, byte v, IReadOnlyArray<IParameter> p)
          => { return SignedRawToValue(r, 1) + (p == null ? 0 : p[0].Value); }, // Only 1 byte seem to contain the temperature, the upper bytes contain something else
          SensorType.Temperature, 0, SmartNames.Temperature, false,
        new[] { new ParameterDescription("Offset [°C]",
                  "Temperature offset of the thermal sensor.\n" +
                  "Temperature = Value + Offset.", 0) }),
      new SmartAttribute(0xC0, SmartNames.UnsafeShutdownCount), 
      new SmartAttribute(0xC7, SmartNames.CRCErrorCount, RawToValue),
      new SmartAttribute(0xE1, SmartNames.HostWrites, 
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) 
          => { return RawToValue(r, v, p) / 0x20; }, 
        SensorType.Data, 0, SmartNames.HostWrites),
      new SmartAttribute(0xE8, SmartNames.RemainingLife, 
        null, SensorType.Level, 0, SmartNames.RemainingLife),
      new SmartAttribute(0xE9, SmartNames.MediaWearOutIndicator),
      new SmartAttribute(0xF1, SmartNames.HostWrites,
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) 
          => { return RawToValue(r, v, p) / 0x20; }, 
        SensorType.Data, 0, SmartNames.HostWrites),
      new SmartAttribute(0xF2, SmartNames.HostReads, 
        (byte[] r, byte v, IReadOnlyArray<IParameter> p) 
          => { return RawToValue(r, v, p) / 0x20; }, 
        SensorType.Data, 1, SmartNames.HostReads),      
    };

    public SSDIntel(ISmart smart, string name, string firmwareRevision, 
      int index, ISettings settings)
      : base(smart, name, firmwareRevision, "ssd", index, smartAttributes, settings) {}
  }
}
