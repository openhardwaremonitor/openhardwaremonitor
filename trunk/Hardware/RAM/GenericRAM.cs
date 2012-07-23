/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using Microsoft.VisualBasic.Devices;

namespace OpenHardwareMonitor.Hardware.RAM {
  internal class GenericRAM : Hardware {

    private Sensor loadSensor;
    private Sensor availableMemory;

    private ComputerInfo computerInfo;

    public GenericRAM(string name, ISettings settings)
      : base(name, new Identifier("ram"), settings)
    {   
      computerInfo = new ComputerInfo();
      loadSensor = new Sensor("Memory", 0, SensorType.Load, this, settings);
      ActivateSensor(loadSensor);

      availableMemory = new Sensor("Available Memory", 0, SensorType.Data, this, settings);
      ActivateSensor(availableMemory);
    }

    public override HardwareType HardwareType {
      get {
        return HardwareType.RAM;
      }
    }

    public override void Update() {
      loadSensor.Value = 100.0f - 
        (100.0f * computerInfo.AvailablePhysicalMemory) / 
        computerInfo.TotalPhysicalMemory;

      availableMemory.Value = (float)computerInfo.AvailablePhysicalMemory /
        (1024 * 1024 * 1024);
    }
  }
}
