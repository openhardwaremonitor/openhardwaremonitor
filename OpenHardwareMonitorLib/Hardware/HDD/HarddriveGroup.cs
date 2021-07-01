/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	Copyright (C) 2010 Paul Werelds
  Copyright (C) 2011 Roland Reinl <roland-reinl@gmx.de>

*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal class HarddriveGroup : IGroup {

    private const int MAX_DRIVES = 32;

    private readonly List<AbstractStorage> hardware = 
      new List<AbstractStorage>();

    public HarddriveGroup(ISettings settings) {
      if (OperatingSystem.IsUnix) 
        return;

      // A bit of a hack to make sure we get a 1:1 relationship between physical drives and SCSI Disks (as which NVME drives are recognized, see
      // NVMeGeneric.GetDeviceInfo for further details
      NVMeGeneric previousNvmeDisk = null;
      for (int drive = 0; drive < MAX_DRIVES; drive++) {
        AbstractStorage instance =
          AbstractStorage.CreateInstance(drive, previousNvmeDisk, settings);
        if (instance != null) {
          this.hardware.Add(instance);
        }

        if (instance is NVMeGeneric nvme) {
          previousNvmeDisk = nvme;
        }
      }
    }

    public IReadOnlyList<IHardware> Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      return null;
    }

    public void Close() {
      foreach (AbstractStorage hdd in hardware) 
        hdd.Dispose();
    }
  }
}
