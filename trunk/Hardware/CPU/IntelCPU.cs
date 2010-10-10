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
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {
  internal sealed class IntelCPU : GenericCPU {

    private enum Microarchitecture {
      Unknown,
      Core,
      Atom,
      Nehalem
    }

    private readonly Sensor[] coreTemperatures;
    private readonly Sensor[] coreClocks;
    private readonly Sensor busClock;

    private readonly Microarchitecture microarchitecture;
    private readonly double timeStampCounterMultiplier;

    private const uint IA32_THERM_STATUS_MSR = 0x019C;
    private const uint IA32_TEMPERATURE_TARGET = 0x01A2;
    private const uint IA32_PERF_STATUS = 0x0198;
    private const uint MSR_PLATFORM_INFO = 0xCE;

    private float[] Floats(float f) {
      float[] result = new float[coreCount];
      for (int i = 0; i < coreCount; i++)
        result[i] = f;
      return result;
    }

    public IntelCPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) 
    {
      // set tjMax
      float[] tjMax;
      switch (family) {
        case 0x06: {
            switch (model) {
              case 0x0F: // Intel Core 2 (65nm)
                microarchitecture = Microarchitecture.Core;
                switch (stepping) {
                  case 0x06: // B2
                    switch (coreCount) {
                      case 2:
                        tjMax = Floats(80 + 10); break;
                      case 4:
                        tjMax = Floats(90 + 10); break;
                      default:
                        tjMax = Floats(85 + 10); break;
                    }
                    tjMax = Floats(80 + 10); break;
                  case 0x0B: // G0
                    tjMax = Floats(90 + 10); break;
                  case 0x0D: // M0
                    tjMax = Floats(85 + 10); break;
                  default:
                    tjMax = Floats(85 + 10); break;
                } break;
              case 0x17: // Intel Core 2 (45nm)
                microarchitecture = Microarchitecture.Core;
                tjMax = Floats(100); break;
              case 0x1C: // Intel Atom (45nm)
                microarchitecture = Microarchitecture.Atom;
                switch (stepping) {
                  case 0x02: // C0
                    tjMax = Floats(90); break;
                  case 0x0A: // A0, B0
                    tjMax = Floats(100); break;
                  default:
                    tjMax = Floats(90); break;
                } break;
              case 0x1A: // Intel Core i7 LGA1366 (45nm)
              case 0x1E: // Intel Core i5, i7 LGA1156 (45nm)
              case 0x25: // Intel Core i3, i5, i7 LGA1156 (32nm)
              case 0x2C: // Intel Core i7 LGA1366 (32nm) 6 Core
                microarchitecture = Microarchitecture.Nehalem;
                uint eax, edx;
                tjMax = new float[coreCount];
                for (int i = 0; i < coreCount; i++) {
                  if (WinRing0.RdmsrTx(IA32_TEMPERATURE_TARGET, out eax,
                    out edx, (UIntPtr)(1L << cpuid[i][0].Thread))) {
                    tjMax[i] = (eax >> 16) & 0xFF;
                  } else {
                    tjMax[i] = 100;
                  }
                }                
                break;
              default:
                microarchitecture = Microarchitecture.Unknown;
                tjMax = Floats(100); 
                break;
            }
          } break;
        default:
          microarchitecture = Microarchitecture.Unknown;
          tjMax = Floats(100); 
          break;
      }

      // set timeStampCounterMultiplier
      switch (microarchitecture) {
        case Microarchitecture.Atom:
        case Microarchitecture.Core: {
            uint eax, edx;
            if (WinRing0.Rdmsr(IA32_PERF_STATUS, out eax, out edx)) {
              timeStampCounterMultiplier = 
                ((edx >> 8) & 0x1f) + 0.5 * ((edx >> 14) & 1);
            }
          } break;
        case Microarchitecture.Nehalem: {
            uint eax, edx;
            if (WinRing0.Rdmsr(MSR_PLATFORM_INFO, out eax, out edx)) {
              timeStampCounterMultiplier = (eax >> 8) & 0xff;
            }
          } break;
        default:
          timeStampCounterMultiplier = 1;
          break;
      }

      // check if processor supports a digital thermal sensor
      if (cpuid[0][0].Data.GetLength(0) > 6 &&
        (cpuid[0][0].Data[6, 0] & 1) != 0) {
        coreTemperatures = new Sensor[coreCount];
        for (int i = 0; i < coreTemperatures.Length; i++) {
          coreTemperatures[i] = new Sensor(CoreString(i), i,
            SensorType.Temperature, this, new [] { 
              new ParameterDescription(
                "TjMax [°C]", "TjMax temperature of the core.\n" + 
                "Temperature = TjMax - TSlope * Value.", tjMax[i]), 
              new ParameterDescription("TSlope [°C]", 
                "Temperature slope of the digital thermal sensor.\n" + 
                "Temperature = TjMax - TSlope * Value.", 1)}, settings);
          ActivateSensor(coreTemperatures[i]);
        }
      } else {
        coreTemperatures = new Sensor[0];
      }

      busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this, settings);
      coreClocks = new Sensor[coreCount];
      for (int i = 0; i < coreClocks.Length; i++) {
        coreClocks[i] =
          new Sensor(CoreString(i), i + 1, SensorType.Clock, this, settings);
        if (HasTimeStampCounter)
          ActivateSensor(coreClocks[i]);
      }

      Update();
    }

    protected override uint[] GetMSRs() {
      return new [] {
        MSR_PLATFORM_INFO,
        IA32_PERF_STATUS ,
        IA32_THERM_STATUS_MSR,
        IA32_TEMPERATURE_TARGET
      };
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();
      r.Append(base.GetReport());

      r.Append("Time Stamp Counter Multiplier: ");
      r.AppendLine(timeStampCounterMultiplier.ToString(
        CultureInfo.InvariantCulture));
      r.AppendLine();

      return r.ToString();
    }

    public override void Update() {
      base.Update();

      for (int i = 0; i < coreTemperatures.Length; i++) {
        uint eax, edx;
        if (WinRing0.RdmsrTx(
          IA32_THERM_STATUS_MSR, out eax, out edx,
            (UIntPtr)(1L << cpuid[i][0].Thread))) {
          // if reading is valid
          if ((eax & 0x80000000) != 0) {
            // get the dist from tjMax from bits 22:16
            float deltaT = ((eax & 0x007F0000) >> 16);
            float tjMax = coreTemperatures[i].Parameters[0].Value;
            float tSlope = coreTemperatures[i].Parameters[1].Value;
            coreTemperatures[i].Value = tjMax - tSlope * deltaT;
          } else {
            coreTemperatures[i].Value = null;
          }
        }
      }

      if (HasTimeStampCounter) {
        double newBusClock = 0;
        uint eax, edx;
        for (int i = 0; i < coreClocks.Length; i++) {
          System.Threading.Thread.Sleep(1);
          if (WinRing0.RdmsrTx(IA32_PERF_STATUS, out eax, out edx,
            (UIntPtr)(1L << cpuid[i][0].Thread))) 
          {
            newBusClock = 
              TimeStampCounterFrequency / timeStampCounterMultiplier;
            if (microarchitecture == Microarchitecture.Nehalem) {
              uint multiplier = eax & 0xff;
              coreClocks[i].Value = (float)(multiplier * newBusClock);
            } else {
              double multiplier = ((eax >> 8) & 0x1f) + 0.5 * ((eax >> 14) & 1);
              coreClocks[i].Value = (float)(multiplier * newBusClock);
            }            
          } else { 
            // if IA32_PERF_STATUS is not available, assume TSC frequency
            coreClocks[i].Value = (float)TimeStampCounterFrequency;
          }
        }
        if (newBusClock > 0) {
          this.busClock.Value = (float)newBusClock;
          ActivateSensor(this.busClock);
        }
      }
    }
  }
}
