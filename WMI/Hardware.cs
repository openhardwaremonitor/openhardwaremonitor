/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Paul Werelds <paul@werelds.net>
	
*/


using System.Management.Instrumentation;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.WMI
{
    [InstrumentationClass(InstrumentationType.Instance)]
    public class Hardware : IWmiObject
    {
        public Hardware(IHardware hardware)
        {
            Name = hardware.Name;
            Identifier = hardware.Identifier.ToString();
            HardwareType = hardware.HardwareType.ToString();
            Parent = hardware.Parent != null
                ? hardware.Parent.Identifier.ToString()
                : "";
        }

        public void Update()
        {
        }

        #region WMI Exposed

        public string HardwareType { get; }
        public string Identifier { get; }
        public string Name { get; }
        public string Parent { get; }

        #endregion
    }
}