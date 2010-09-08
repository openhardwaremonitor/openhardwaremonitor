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

using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  internal class IT87XX : ISuperIO {
       
    private ushort address;
    private Chip chip;
    private byte version;

    private readonly ushort addressReg;
    private readonly ushort dataReg;

    private float?[] voltages = new float?[0];
    private float?[] temperatures = new float?[0];
    private float?[] fans = new float?[0];

    private readonly float voltageGain;
   
    // Consts
    private const byte ITE_VENDOR_ID = 0x90;
       
    // Environment Controller
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Environment Controller Registers    
    private const byte CONFIGURATION_REGISTER = 0x00;
    private const byte TEMPERATURE_BASE_REG = 0x29;
    private const byte VENDOR_ID_REGISTER = 0x58;
    private const byte FAN_TACHOMETER_16_BIT_ENABLE_REGISTER = 0x0c;
    private byte[] FAN_TACHOMETER_REG = 
      new byte[] { 0x0d, 0x0e, 0x0f, 0x80, 0x82 };
    private byte[] FAN_TACHOMETER_EXT_REG =
      new byte[] { 0x18, 0x19, 0x1a, 0x81, 0x83 };
    private const byte VOLTAGE_BASE_REG = 0x20;

    private byte ReadByte(byte register, out bool valid) {
      WinRing0.WriteIoPortByte(addressReg, register);
      byte value = WinRing0.ReadIoPortByte(dataReg);
      valid = register == WinRing0.ReadIoPortByte(addressReg);
      return value;
    } 

    public IT87XX(Chip chip, ushort address, byte version) {
      
      this.address = address;
      this.chip = chip;
      this.version = version;
      this.addressReg = (ushort)(address + ADDRESS_REGISTER_OFFSET);
      this.dataReg = (ushort)(address + DATA_REGISTER_OFFSET);
      
      // Check vendor id
      bool valid;
      byte vendorId = ReadByte(VENDOR_ID_REGISTER, out valid);       
      if (!valid || vendorId != ITE_VENDOR_ID)
        return;

      // Bit 0x10 of the configuration register should always be 1
      if ((ReadByte(CONFIGURATION_REGISTER, out valid) & 0x10) == 0)
        return;
      if (!valid)
        return;

      voltages = new float?[9];
      temperatures = new float?[3];
      fans = new float?[5];

      // The IT8721F uses a 12mV resultion ADC, all others 16mV
      if (chip == Chip.IT8721F) {
        voltageGain = 0.012f;
      } else {
        voltageGain = 0.016f;
      }
    }

    public Chip Chip { get { return chip; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Chip ID: 0x"); r.AppendLine(chip.ToString("X"));
      r.Append("Chip Version: 0x"); r.AppendLine(
        version.ToString("X", CultureInfo.InvariantCulture));
      r.Append("Base Address: 0x"); r.AppendLine(
        address.ToString("X4", CultureInfo.InvariantCulture));
      r.AppendLine();

      if (!WinRing0.WaitIsaBusMutex(100))
        return r.ToString();

      r.AppendLine("Environment Controller Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0xA; i++) {
        r.Append(" "); 
        r.Append((i << 4).ToString("X2", CultureInfo.InvariantCulture)); 
        r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          bool valid;
          byte value = ReadByte((byte)((i << 4) | j), out valid);
          if (valid)
            r.Append(value.ToString("X2", CultureInfo.InvariantCulture));
          else
            r.Append("??");
        }
        r.AppendLine();
      }
      r.AppendLine();

      WinRing0.ReleaseIsaBusMutex();

      return r.ToString();
    }

    public void Update() {
      if (!WinRing0.WaitIsaBusMutex(10))
        return;

      for (int i = 0; i < voltages.Length; i++) {
        bool valid;
        
        float value = 
          voltageGain * ReadByte((byte)(VOLTAGE_BASE_REG + i), out valid);   

        if (!valid)
          continue;
        if (value > 0)
          voltages[i] = value;  
        else
          voltages[i] = null;
      }

      for (int i = 0; i < temperatures.Length; i++) {
        bool valid;
        sbyte value = (sbyte)ReadByte(
          (byte)(TEMPERATURE_BASE_REG + i), out valid);
        if (!valid)
          continue;

        if (value < sbyte.MaxValue && value > 0)
          temperatures[i] = value;
        else
          temperatures[i] = null;       
      }

      for (int i = 0; i < fans.Length; i++) {
        bool valid;
        int value = ReadByte(FAN_TACHOMETER_REG[i], out valid);
        if (!valid) 
          continue;
        value |= ReadByte(FAN_TACHOMETER_EXT_REG[i], out valid) << 8;
        if (!valid)
          continue;

        if (value > 0x3f) {
          fans[i] = (value < 0xffff) ? 1.35e6f / ((value) * 2) : 0;
        } else {
          fans[i] = null;
        }
      }

      WinRing0.ReleaseIsaBusMutex();
    }
  } 
}
