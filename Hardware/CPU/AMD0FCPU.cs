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
using System.Diagnostics;
using System.Text;


namespace OpenHardwareMonitor.Hardware.CPU {
  public class AMD0FCPU : Hardware, IHardware {

    private string name;
    private Image icon;

    private int processorIndex;
    private uint pciAddress;

    private Sensor[] coreTemperatures;    

    private Sensor totalLoad;
    private Sensor[] coreLoads;

    private CPULoad cpuLoad;

    private const ushort PCI_AMD_VENDOR_ID = 0x1022;
    private const ushort PCI_AMD_0FH_MISCELLANEOUS_DEVICE_ID = 0x1103;
    private const uint THERMTRIP_STATUS_REGISTER = 0xE4;
    private const byte THERM_SENSE_CORE_SEL_CPU0 = 0x4;
    private const byte THERM_SENSE_CORE_SEL_CPU1 = 0x0;

    public AMD0FCPU(int processorIndex, CPUID[][] cpuid) {

      this.processorIndex = processorIndex;
      this.name = cpuid[0][0].Name;
      this.icon = Utilities.EmbeddedResources.GetImage("cpu.png");

      int coreCount = cpuid.Length;      

      totalLoad = new Sensor("CPU Total", 0, SensorType.Load, this);
        
      float offset = -49.0f;

      // AM2+ 65nm +21 offset
      uint model = cpuid[0][0].Model;
      if (model >= 0x69 && model != 0xc1 && model != 0x6c && model != 0x7c) 
        offset += 21;

      // check if processor supports a digital thermal sensor 
      if (cpuid[0][0].ExtData.GetLength(0) > 7 && 
        (cpuid[0][0].ExtData[7, 3] & 1) != 0) 
      {
        coreTemperatures = new Sensor[coreCount];
        for (int i = 0; i < coreCount; i++) {
          coreTemperatures[i] =
            new Sensor("Core #" + (i + 1), i, null, SensorType.Temperature,
              this, new ParameterDescription[] { 
                new ParameterDescription("Offset", 
                  "Temperature offset of the thermal sensor.\n" + 
                  "Temperature = Value + Offset.", offset)
          });
        }
      } else {
        coreTemperatures = new Sensor[0];
      }

      coreLoads = new Sensor[coreCount];
      for (int i = 0; i < coreCount; i++) 
        coreLoads[i] = new Sensor("Core #" + (i + 1), i + 1,
          SensorType.Load, this);     

      cpuLoad = new CPULoad(cpuid);
      if (cpuLoad.IsAvailable) {
        foreach (Sensor sensor in coreLoads)
          ActivateSensor(sensor);
        ActivateSensor(totalLoad);
      }

      pciAddress = WinRing0.FindPciDeviceById(PCI_AMD_VENDOR_ID,
        PCI_AMD_0FH_MISCELLANEOUS_DEVICE_ID, (byte)processorIndex);

      Update();                   
    }

    public string Name {
      get { return name; }
    }

    public Identifier Identifier {
      get { return new Identifier("amdcpu", processorIndex.ToString()); }
    }

    public Image Icon {
      get { return icon; }
    }

    public string GetReport() {
      return null;
    }

    public void Update() {
      if (pciAddress != 0xFFFFFFFF) {
        for (uint i = 0; i < coreTemperatures.Length; i++) {
          if (WinRing0.WritePciConfigDwordEx(
            pciAddress, THERMTRIP_STATUS_REGISTER,
            i > 0 ? THERM_SENSE_CORE_SEL_CPU1 : THERM_SENSE_CORE_SEL_CPU0)) {
            uint value;
            if (WinRing0.ReadPciConfigDwordEx(
              pciAddress, THERMTRIP_STATUS_REGISTER, out value)) {
              coreTemperatures[i].Value = ((value >> 16) & 0xFF) + 
                coreTemperatures[i].Parameters[0].Value;
              ActivateSensor(coreTemperatures[i]);
            } else {
              DeactivateSensor(coreTemperatures[i]);
            }
          }
        }
      }

      if (cpuLoad.IsAvailable) {
        cpuLoad.Update();
        for (int i = 0; i < coreLoads.Length; i++)
          coreLoads[i].Value = cpuLoad.GetCoreLoad(i);
        totalLoad.Value = cpuLoad.GetTotalLoad();
      }
    }   
  }
}
