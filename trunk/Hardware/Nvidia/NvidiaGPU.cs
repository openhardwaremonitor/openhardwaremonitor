/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;
using System.Drawing;

namespace OpenHardwareMonitor.Hardware.Nvidia {
  public class NvidiaGPU : Hardware, IHardware {

    private string name;
    private Image icon;
    private int adapterIndex;
    private NvPhysicalGpuHandle handle;

    private Sensor[] temperatures;
    private Sensor fan = null;

    public NvidiaGPU(int adapterIndex, NvPhysicalGpuHandle handle) {
      string gpuName;
      if (NVAPI.NvAPI_GPU_GetFullName(handle, out gpuName) == NvStatus.OK) {
        this.name = "NVIDIA " + gpuName.Trim();
      } else {
        this.name = "NVIDIA";
      }
      this.icon = Utilities.EmbeddedResources.GetImage("nvidia.png");
      this.adapterIndex = adapterIndex;
      this.handle = handle;

      NvGPUThermalSettings settings = GetThermalSettings();
      temperatures = new Sensor[settings.Count];
      for (int i = 0; i < temperatures.Length; i++) {
        NvSensor sensor = settings.Sensor[i];
        string name;
        switch (sensor.Target) {
          case NvThermalTarget.BOARD: name = "GPU Board"; break;
          case NvThermalTarget.GPU: name = "GPU Core"; break;
          case NvThermalTarget.MEMORY: name = "GPU Memory"; break;
          case NvThermalTarget.POWER_SUPPLY: name = "GPU Power Supply"; break;
          case NvThermalTarget.UNKNOWN: name = "GPU Unknown"; break;
          default: name = "GPU"; break;
        }
        temperatures[i] = new Sensor(name, i, sensor.DefaultMaxTemp,
          SensorType.Temperature, this);
        ActivateSensor(temperatures[i]);
      }

      int value;
      if (NVAPI.NvAPI_GPU_GetTachReading != null &&
        NVAPI.NvAPI_GPU_GetTachReading(handle, out value) == NvStatus.OK) {
        if (value > 0) {
          fan = new Sensor("GPU", 0, SensorType.Fan, this);
          ActivateSensor(fan);
        }
      }
    }

    public string Name {
      get { return name; }
    }

    public string Identifier {
      get { return "/nvidiagpu/" + adapterIndex; }
    }

    public Image Icon {
      get { return icon; }
    }

    public string GetReport() {
      return null;
    }

    private NvGPUThermalSettings GetThermalSettings() {
      NvGPUThermalSettings settings = new NvGPUThermalSettings();
      settings.Version = NVAPI.GPU_THERMAL_SETTINGS_VER;
      settings.Count = NVAPI.MAX_THERMAL_SENSORS_PER_GPU;
      settings.Sensor = new NvSensor[NVAPI.MAX_THERMAL_SENSORS_PER_GPU];
      if (NVAPI.NvAPI_GPU_GetThermalSettings(handle, (int)NvThermalTarget.ALL,
        ref settings) != NvStatus.OK) {
        settings.Count = 0;        
      }
      return settings;
    }

    public void Update() {
      NvGPUThermalSettings settings = GetThermalSettings();
      foreach (Sensor sensor in temperatures) 
        sensor.Value = settings.Sensor[sensor.Index].CurrentTemp;

      if (fan != null) {
        int value = 0;
        NVAPI.NvAPI_GPU_GetTachReading(handle, out value);
        fan.Value = value;
      }
    }
  }
}
