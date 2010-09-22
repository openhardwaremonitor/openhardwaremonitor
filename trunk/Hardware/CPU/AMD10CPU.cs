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

  internal sealed class AMD10CPU : AMDCPU {

    private readonly Sensor coreTemperature;

    private const byte MISCELLANEOUS_CONTROL_FUNCTION = 3;
    private const ushort MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1203;
    private const byte REPORTED_TEMPERATURE_CONTROL_REGISTER = 0xA4;
    
    private readonly uint miscellaneousControlAddress;

    public AMD10CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) 
    {            
      // AMD family 10h processors support only one temperature sensor
      coreTemperature = new Sensor(
        "Core" + (coreCount > 1 ? " #1 - #" + coreCount : ""), 0,
        SensorType.Temperature, this, new [] {
            new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
          }, settings);

      // get the pci address for the Miscellaneous Control registers 
      miscellaneousControlAddress = GetPciAddress(
        MISCELLANEOUS_CONTROL_FUNCTION, MISCELLANEOUS_CONTROL_DEVICE_ID);

      Update();                   
    }

    protected override uint[] GetMSRs() {
      return new uint[] { };
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();
      r.Append(base.GetReport());

      r.Append("Miscellaneous Control Address: ");
      r.AppendLine((miscellaneousControlAddress).ToString("X",
        CultureInfo.InvariantCulture));
      r.AppendLine();

      return r.ToString();
    }

    public override void Update() {
      base.Update();

      if (miscellaneousControlAddress != WinRing0.InvalidPciAddress) {
        uint value;
        if (WinRing0.ReadPciConfigDwordEx(miscellaneousControlAddress,
          REPORTED_TEMPERATURE_CONTROL_REGISTER, out value)) {
          coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
            coreTemperature.Parameters[0].Value;
          ActivateSensor(coreTemperature);
        } else {
          DeactivateSensor(coreTemperature);
        }
      }
    }
  }
}
