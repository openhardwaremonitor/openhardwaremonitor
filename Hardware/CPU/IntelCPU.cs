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
using System.Reflection;
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {
  public class IntelCPU : IHardware {

    private string name;
    private Image icon;

    private Sensor[] coreTemperatures;

    private float tjMax = 0;
    private uint logicalProcessors;
    private uint logicalProcessorsPerCore;
    private uint coreCount;

    private const uint IA32_THERM_STATUS_MSR = 0x019C;
    private const uint IA32_TEMPERATURE_TARGET = 0x01A2;

    public IntelCPU(string name, uint family, uint model, uint stepping, 
      uint[,] cpuidData, uint[,] cpuidExtData) {
      
      this.name = name;
      this.icon = Utilities.EmbeddedResources.GetImage("cpu.png");
            
      logicalProcessors = 0;
      if (cpuidData.GetLength(0) > 0x0B) {
        uint eax, ebx, ecx, edx;
        WinRing0.CpuidEx(0x0B, 0, out eax, out ebx, out ecx, out edx);
        logicalProcessorsPerCore = ebx & 0xFF;
        if (logicalProcessorsPerCore > 0) {
          WinRing0.CpuidEx(0x0B, 1, out eax, out ebx, out ecx, out edx);
          logicalProcessors = ebx & 0xFF;
        }   
      }
      if (logicalProcessors <= 0 && cpuidData.GetLength(0) > 0x04) {
        logicalProcessors = ((cpuidData[4, 0] >> 26) & 0x3F) + 1;
        logicalProcessorsPerCore = 1;
      }
      if (logicalProcessors <= 0) {
        logicalProcessors = 1;
        logicalProcessorsPerCore = 1;
      }

      coreCount = logicalProcessors / logicalProcessorsPerCore;

      switch (family) {
        case 0x06: {
          switch (model) {
            case 0x0F: // Intel Core 65nm
              switch (stepping) {
                case 0x06: // B2
                  switch (coreCount) {
                    case 2:
                      tjMax = 80; break;
                    case 4:
                      tjMax = 90; break;
                    default:
                      tjMax = 85; break;
                  }
                  tjMax = 80; break;
                case 0x0B: // G0
                  tjMax = 90; break;
                case 0x0D: // M0
                  tjMax = 85; break;
                default:
                  tjMax = 85; break;
              } break;            
            case 0x17: // Intel Core 45nm
              tjMax = 100; break;
            case 0x1C: // Intel Atom 
              tjMax = 90; break;
            case 0x1A:
              uint eax = 0, edx = 0;
              if (WinRing0.RdmsrPx(
                  IA32_TEMPERATURE_TARGET, ref eax, ref edx, (UIntPtr)1)) {
                tjMax = (eax >> 16) & 0xFF;
              } else
                tjMax = 100;
              break;
            default:
              tjMax = 100; break;
          }
        } break;
        default: tjMax = 100; break;
      }

      coreTemperatures = new Sensor[coreCount];
      for (int i = 0; i < coreTemperatures.Length; i++)
        coreTemperatures[i] =
          new Sensor("Core #" + (i + 1), i, tjMax, SensorType.Temperature, 
            this);

      Update();                   
    }

    public string Name {
      get { return name; }
    }

    public string Identifier {
      get { return "/intelcpu/0"; }
    }

    public Image Icon {
      get { return icon; }
    }

    public ISensor[] Sensors {
      get {
        return coreTemperatures;
      }
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("Intel CPU");
      r.AppendLine();
      r.AppendFormat("Name: {0}{1}", name, Environment.NewLine);
      r.AppendFormat("Number of cores: {0}{1}", coreCount, 
        Environment.NewLine);
      r.AppendFormat("Threads per core: {0}{1}", logicalProcessorsPerCore,
        Environment.NewLine);
      r.AppendFormat("TjMax: {0}{1}", tjMax, Environment.NewLine);
      r.AppendLine();

      return r.ToString();
    }

    public void Update() {

      uint eax = 0, edx = 0;      
      for (int i = 0; i < coreTemperatures.Length; i++) {
        if (WinRing0.RdmsrPx(
          IA32_THERM_STATUS_MSR, ref eax, ref edx, 
            (UIntPtr)(1 << (int)(logicalProcessorsPerCore * i)))) 
        {
          // if reading is valid
          if ((eax & 0x80000000) != 0) {
            // get the dist from tjMax from bits 22:16
            coreTemperatures[i].Value = tjMax - ((eax & 0x007F0000) >> 16);
          }
        }        
      }  
    }

    #pragma warning disable 67
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    #pragma warning restore 67
  }
}
