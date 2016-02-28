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

namespace OpenHardwareMonitor.Hardware.HDD
{
    internal class HarddriveGroup : IGroup
    {
        private const int MAX_DRIVES = 32;

        private readonly List<AbstractHarddrive> hardware =
            new List<AbstractHarddrive>();

        public HarddriveGroup(ISettings settings)
        {
            var p = (int) Environment.OSVersion.Platform;
            if (p == 4 || p == 128) return;

            ISmart smart = new WindowsSmart();

            for (var drive = 0; drive < MAX_DRIVES; drive++)
            {
                var instance =
                    AbstractHarddrive.CreateInstance(smart, drive, settings);
                if (instance != null)
                {
                    hardware.Add(instance);
                }
            }
        }

        public IHardware[] Hardware
        {
            get { return hardware.ToArray(); }
        }

        public string GetReport()
        {
            return null;
        }

        public void Close()
        {
            foreach (var hdd in hardware)
                hdd.Close();
        }
    }
}