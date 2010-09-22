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
using System.Threading;

namespace OpenHardwareMonitor.Hardware.CPU {
  internal sealed class AMD0FCPU : AMDCPU {
    
    private readonly Sensor[] coreTemperatures;
    private readonly Sensor[] coreClocks;
    private readonly Sensor busClock;

    private const uint FIDVID_STATUS = 0xC0010042;

    private const byte MISCELLANEOUS_CONTROL_FUNCTION = 3;
    private const ushort MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1103;
    private const uint THERMTRIP_STATUS_REGISTER = 0xE4;
    private const byte THERM_SENSE_CORE_SEL_CPU0 = 0x4;
    private const byte THERM_SENSE_CORE_SEL_CPU1 = 0x0;

    private readonly uint miscellaneousControlAddress;

    public AMD0FCPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) 
    {
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
            new Sensor("Core #" + (i + 1), i, SensorType.Temperature,
              this, new [] { new ParameterDescription("Offset [°C]", 
                  "Temperature offset of the thermal sensor.\n" + 
                  "Temperature = Value + Offset.", offset)
          }, settings);
        }
      } else {
        coreTemperatures = new Sensor[0];
      }

      miscellaneousControlAddress = GetPciAddress(
        MISCELLANEOUS_CONTROL_FUNCTION, MISCELLANEOUS_CONTROL_DEVICE_ID);

      busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this, settings);
      coreClocks = new Sensor[coreCount];
      for (int i = 0; i < coreClocks.Length; i++) {
        coreClocks[i] = new Sensor(CoreString(i), i + 1, SensorType.Clock,
          this, settings);
        if (hasTSC)
          ActivateSensor(coreClocks[i]);
      }

      Update();                   
    }

    protected override uint[] GetMSRs() {
      return new [] { FIDVID_STATUS };
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();
      r.Append(base.GetReport());

      r.Append("Miscellaneous Control Address: 0x");
      r.AppendLine((miscellaneousControlAddress).ToString("X", 
        CultureInfo.InvariantCulture));
      r.AppendLine();

      return r.ToString();
    }

    public override void Update() {
      base.Update();

      if (miscellaneousControlAddress != WinRing0.InvalidPciAddress) {
        for (uint i = 0; i < coreTemperatures.Length; i++) {
          if (WinRing0.WritePciConfigDwordEx(
            miscellaneousControlAddress, THERMTRIP_STATUS_REGISTER,
            i > 0 ? THERM_SENSE_CORE_SEL_CPU1 : THERM_SENSE_CORE_SEL_CPU0)) {
            uint value;
            if (WinRing0.ReadPciConfigDwordEx(
              miscellaneousControlAddress, THERMTRIP_STATUS_REGISTER, 
              out value)) 
            {
              coreTemperatures[i].Value = ((value >> 16) & 0xFF) + 
                coreTemperatures[i].Parameters[0].Value;
              ActivateSensor(coreTemperatures[i]);
            } else {
              DeactivateSensor(coreTemperatures[i]);
            }
          }
        }
      }

      if (hasTSC) {
        double newBusClock = 0;

        for (int i = 0; i < coreClocks.Length; i++) {
          Thread.Sleep(1);

          uint eax, edx;
          if (WinRing0.RdmsrTx(FIDVID_STATUS, out eax, out edx,
            (UIntPtr)(1L << cpuid[i][0].Thread))) {
            // CurrFID can be found in eax bits 0-5, MaxFID in 16-21
            // 8-13 hold StartFID, we don't use that here.
            double curMP = 0.5 * ((eax & 0x3F) + 8);
            double maxMP = 0.5 * ((eax >> 16 & 0x3F) + 8);
            coreClocks[i].Value = (float)(curMP * MaxClock / maxMP);
            newBusClock = (float)(MaxClock / maxMP);
          } else {
            // Fail-safe value - if the code above fails, we'll use this instead
            coreClocks[i].Value = (float)MaxClock;
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
