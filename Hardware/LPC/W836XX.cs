/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  internal class W836XX : ISuperIO {

    private readonly ushort address;
    private readonly byte revision;

    private readonly Chip chip;

    private readonly float?[] voltages = new float?[0];
    private readonly float?[] temperatures = new float?[0];    
    private readonly float?[] fans = new float?[0];
    private readonly float?[] controls = new float?[0];

    private readonly bool[] peciTemperature = new bool[0];
    private readonly byte[] voltageRegister = new byte[0];
    private readonly byte[] voltageBank = new byte[0];
    private readonly float voltageGain = 0.008f;

    // Consts 
    private const ushort WINBOND_VENDOR_ID = 0x5CA3;
    private const byte HIGH_BYTE = 0x80;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte VOLTAGE_VBAT_REG = 0x51;
    private const byte BANK_SELECT_REGISTER = 0x4E;
    private const byte VENDOR_ID_REGISTER = 0x4F;
    private const byte TEMPERATURE_SOURCE_SELECT_REG = 0x49;

    private readonly byte[] TEMPERATURE_REG = new byte[] { 0x50, 0x50, 0x27 };
    private readonly byte[] TEMPERATURE_BANK = new byte[] { 1, 2, 0 };

    private readonly byte[] FAN_TACHO_REG = 
      new byte[] { 0x28, 0x29, 0x2A, 0x3F, 0x53 };
    private readonly byte[] FAN_TACHO_BANK = 
      new byte[] { 0, 0, 0, 0, 5 };       
    private readonly byte[] FAN_BIT_REG =
      new byte[] { 0x47, 0x4B, 0x4C, 0x59, 0x5D };
    private readonly byte[] FAN_DIV_BIT0 = new byte[] { 36, 38, 30, 8, 10 };
    private readonly byte[] FAN_DIV_BIT1 = new byte[] { 37, 39, 31, 9, 11 };
    private readonly byte[] FAN_DIV_BIT2 = new byte[] { 5, 6, 7, 23, 15 };

    // Control vars
    private readonly byte[] fanPwmRegister = new byte[0];
    private readonly byte[] fanPrimaryControlModeRegister = new byte[0];
    private readonly byte[] fanPrimaryControlValue = new byte[0];
    private readonly byte[] fanSecondaryControlModeRegister = new byte[0];
    private readonly byte[] fanSecondaryControlValue = new byte[0];
    private readonly byte[] fanTertiaryControlModeRegister = new byte[0];
    private readonly byte[] fanTertiaryControlValue = new byte[0];

    private readonly byte[] initialFanPrimaryControlValue = new byte[0];
    private readonly byte[] initialFanSecondaryControlValue = new byte[0];
    private readonly byte[] initialFanTertiaryControlValue = new byte[0];

    private bool[] restoreDefaultFanPwmControlRequired = new bool[0];    

    private byte ReadByte(byte bank, byte register) {
      Ring0.WriteIoPort(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      Ring0.WriteIoPort(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      Ring0.WriteIoPort(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return Ring0.ReadIoPort(
        (ushort)(address + DATA_REGISTER_OFFSET));
    } 

    private void WriteByte(byte bank, byte register, byte value) {
      Ring0.WriteIoPort(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      Ring0.WriteIoPort(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      Ring0.WriteIoPort(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      Ring0.WriteIoPort(
         (ushort)(address + DATA_REGISTER_OFFSET), value); 
    }

    public byte? ReadGPIO(int index) {
      return null;
    }

    public void WriteGPIO(int index, byte value) { }

    public void SetControl(int index, byte? value) {
      if (index < 0 || index >= Controls.Length)
          throw new ArgumentOutOfRangeException("index");

      if (!Ring0.WaitIsaBusMutex(10))
          return;

      if (value.HasValue) {
        SaveDefaultFanPwmControl(index);
        if (fanPrimaryControlModeRegister.Length > 0) {
          WriteByte(0, fanPrimaryControlModeRegister[index], (byte)(fanPrimaryControlValue[index] & ReadByte(0, fanPrimaryControlModeRegister[index])));
          if(fanSecondaryControlModeRegister.Length>0) {
            if (fanSecondaryControlModeRegister[index] != fanPrimaryControlModeRegister[index]) {
              WriteByte(0, fanSecondaryControlModeRegister[index], (byte)(fanSecondaryControlValue[index] & ReadByte(0, fanSecondaryControlModeRegister[index])));
            }
            if(fanTertiaryControlModeRegister.Length>0) {
              if (fanTertiaryControlModeRegister[index] != fanSecondaryControlModeRegister[index]) {
                WriteByte(0, fanTertiaryControlModeRegister[index], (byte)(fanTertiaryControlValue[index] & ReadByte(0, fanTertiaryControlModeRegister[index])));
              }
            }
          }
        }
        // set output value
        WriteByte(0, fanPwmRegister[index], (byte)(value.Value));
      } else {
        RestoreDefaultFanPwmControl(index);
      }

      Ring0.ReleaseIsaBusMutex();
    }

    private void SaveDefaultFanPwmControl(int index) //added to save initial control values
    { 
      if (fanPrimaryControlModeRegister.Length > 0 && initialFanPrimaryControlValue.Length>0 && fanPrimaryControlValue.Length>0 && restoreDefaultFanPwmControlRequired.Length>0) {
        if (!restoreDefaultFanPwmControlRequired[index])  {
          initialFanPrimaryControlValue[index] = ReadByte(0, fanPrimaryControlModeRegister[index]);
          if(fanSecondaryControlModeRegister.Length>0 && initialFanSecondaryControlValue.Length>0 && fanSecondaryControlValue.Length>0) {
            if (fanSecondaryControlModeRegister[index] != fanPrimaryControlModeRegister[index]) {
              initialFanSecondaryControlValue[index] = ReadByte(0, fanSecondaryControlModeRegister[index]);   
            }
            if(fanTertiaryControlModeRegister.Length>0 && initialFanTertiaryControlValue.Length>0 && fanTertiaryControlValue.Length>0) {
              if (fanTertiaryControlModeRegister[index] != fanSecondaryControlModeRegister[index]) {
                initialFanTertiaryControlValue[index] = ReadByte(0, fanTertiaryControlModeRegister[index]);   
              }
            }
          }
          restoreDefaultFanPwmControlRequired[index] = true;
        }
      }
    }

    private void RestoreDefaultFanPwmControl(int index) //added to restore initial control values
    {
      if (fanPrimaryControlModeRegister.Length > 0 && initialFanPrimaryControlValue.Length>0 && fanPrimaryControlValue.Length>0 && restoreDefaultFanPwmControlRequired.Length>0) {
        if (restoreDefaultFanPwmControlRequired[index])  {
          WriteByte(0, fanPrimaryControlModeRegister[index], (byte)( (initialFanPrimaryControlValue[index] & ~fanPrimaryControlValue[index]) | ReadByte(0, fanPrimaryControlModeRegister[index]))); //bitwise operands to change only desired bits
          if(fanSecondaryControlModeRegister.Length>0 && initialFanSecondaryControlValue.Length>0 && fanSecondaryControlValue.Length>0) {
            if (fanSecondaryControlModeRegister[index] != fanPrimaryControlModeRegister[index]) {
              WriteByte(0, fanSecondaryControlModeRegister[index], (byte)( (initialFanSecondaryControlValue[index] & ~fanSecondaryControlValue[index]) | ReadByte(0, fanSecondaryControlModeRegister[index]))); //bitwise operands to change only desired bits
            }
            if(fanTertiaryControlModeRegister.Length>0 && initialFanTertiaryControlValue.Length>0 && fanTertiaryControlValue.Length>0) {
              if (fanTertiaryControlModeRegister[index] != fanSecondaryControlModeRegister[index]) {
                WriteByte(0, fanTertiaryControlModeRegister[index], (byte)( (initialFanTertiaryControlValue[index] & ~fanTertiaryControlValue[index]) | ReadByte(0, fanTertiaryControlModeRegister[index]))); //bitwise operands to change only desired bits
              }
            }
          }
          restoreDefaultFanPwmControlRequired[index] = false;
        }
      }
    }

    public W836XX(Chip chip, byte revision, ushort address) {
      this.address = address;
      this.revision = revision;
      this.chip = chip;

      if (!IsWinbondVendor())
        return;
      
      temperatures = new float?[3];
      peciTemperature = new bool[3];
      switch (chip) {
        case Chip.W83667HG:
        case Chip.W83667HGB:
          // note temperature sensor registers that read PECI
          byte flag = ReadByte(0, TEMPERATURE_SOURCE_SELECT_REG);
          peciTemperature[0] = (flag & 0x04) != 0;
          peciTemperature[1] = (flag & 0x40) != 0;
          peciTemperature[2] = false;
          break;
        case Chip.W83627DHG:        
        case Chip.W83627DHGP:
          // note temperature sensor registers that read PECI
          byte sel = ReadByte(0, TEMPERATURE_SOURCE_SELECT_REG);
          peciTemperature[0] = (sel & 0x07) != 0;
          peciTemperature[1] = (sel & 0x70) != 0;
          peciTemperature[2] = false;
          break;
        default:
          // no PECI support
          peciTemperature[0] = false;
          peciTemperature[1] = false;
          peciTemperature[2] = false;
          break;
      }

      switch (chip) {
        case Chip.W83627EHF:
            voltages = new float?[10];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x50, 0x51, 0x52 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 0, 0, 5, 5, 5 };
            voltageGain = 0.008f;
            fans = new float?[5];
            fanPwmRegister = new byte[] { 0x01, 0x03, 0x11 }; // Fan PWM values.
            fanPrimaryControlModeRegister = new byte[] { 0x04, 0x04, 0x12 }; // Primary control register.
            fanPrimaryControlValue = new byte[] { 0b11110011, 0b11001111, 0b11111001 }; // Values to gain control of fans.
            initialFanPrimaryControlValue = new byte[3]; // To store default value.
            initialFanSecondaryControlValue = new byte[3]; // To store default value.
            controls = new float?[3];
            break;
        case Chip.W83627DHG:
        case Chip.W83627DHGP:
            voltages = new float?[9];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x50, 0x51 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 0, 0, 5, 5 };
            voltageGain = 0.008f;
            fans = new float?[5];
            fanPwmRegister = new byte[] { 0x01, 0x03, 0x11 }; // Fan PWM values
            fanPrimaryControlModeRegister = new byte[] { 0x04, 0x04, 0x12 }; // Primary control register
            fanPrimaryControlValue = new byte[] { 0b11110011, 0b11001111, 0b11111001 }; // Values to gain control of fans
            initialFanPrimaryControlValue = new byte[3]; // To store default value
            initialFanSecondaryControlValue = new byte[3]; // To store default value.
            controls = new float?[3];
            break;
        case Chip.W83667HG:
        case Chip.W83667HGB:
            voltages = new float?[9];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x50, 0x51 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 0, 0, 5, 5 };
            voltageGain = 0.008f;
            fans = new float?[5];
            fanPwmRegister = new byte[] { 0x01, 0x03, 0x11 }; // Fan PWM values.
            fanPrimaryControlModeRegister = new byte[] { 0x04, 0x04, 0x12 }; // Primary control register.
            fanPrimaryControlValue = new byte[] { 0b11110011, 0b11001111, 0b11111001 }; // Values to gain control of fans.
            fanSecondaryControlModeRegister = new byte[] { 0x7c, 0x7c, 0x7c }; // Secondary control register for SmartFan4.
            fanSecondaryControlValue = new byte[] { 0b11101111, 0b11011111, 0b10111111 }; // Values for secondary register to gain control of fans.
            fanTertiaryControlModeRegister = new byte[] { 0x62, 0x7c, 0x62 }; // Tertiary control register. 2nd fan doesn't have Tertiary control, same as secondary to avoid change.
            fanTertiaryControlValue = new byte[] { 0b11101111, 0b11011111, 0b11011111 }; // Values for tertiary register to gain control of fans. 2nd fan doesn't have Tertiary control, same as secondary to avoid change.
            initialFanPrimaryControlValue = new byte[3]; // To store default value.
            initialFanSecondaryControlValue = new byte[3]; // To store default value.
            initialFanTertiaryControlValue = new byte[3]; // To store default value.
            controls = new float?[3];
            break;
        case Chip.W83627HF:
            voltages = new float?[7];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x50, 0x51 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 5, 5 };
            voltageGain = 0.016f;
            fans = new float?[3];
            fanPwmRegister = new byte[] { 0x5A, 0x5B }; // Fan PWM values.
            controls = new float?[2];
            break;
        case Chip.W83627THF:
            voltages = new float?[7];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x50, 0x51 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 5, 5 };
            voltageGain = 0.016f;
            fans = new float?[3];
            fanPwmRegister = new byte[] { 0x01, 0x03, 0x11 }; // Fan PWM values.
            fanPrimaryControlModeRegister = new byte[] { 0x04, 0x04, 0x12 }; // Primary control register.
            fanPrimaryControlValue = new byte[] { 0b11110011, 0b11001111, 0b11111001 }; // Values to gain control of fans.
            initialFanPrimaryControlValue = new byte[3]; // To store default value.
            controls = new float?[3];
            break;
        case Chip.W83687THF:
            voltages = new float?[7];
            voltageRegister = new byte[] { 0x20, 0x21, 0x22, 0x23, 0x24, 0x50, 0x51 };
            voltageBank = new byte[] { 0, 0, 0, 0, 0, 5, 5 };
            voltageGain = 0.016f;
            fans = new float?[3];
            break;
      }
    }    

    private bool IsWinbondVendor() {
      ushort vendorId =
        (ushort)((ReadByte(HIGH_BYTE, VENDOR_ID_REGISTER) << 8) |
           ReadByte(0, VENDOR_ID_REGISTER));
      return vendorId == WINBOND_VENDOR_ID;
    }

    private static ulong SetBit(ulong target, int bit, int value) {
      if ((value & 1) != value)
        throw new ArgumentException("Value must be one bit only.");

      if (bit < 0 || bit > 63)
        throw new ArgumentException("Bit out of range.");

      ulong mask = (((ulong)1) << bit);
      return value > 0 ? target | mask : target & ~mask;
    }

    public Chip Chip { get { return chip; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }
    public float?[] Controls { get { return controls; } }

    public void Update() {
      if (!Ring0.WaitIsaBusMutex(10))
        return;

      for (int i = 0; i < voltages.Length; i++) {
        if (voltageRegister[i] != VOLTAGE_VBAT_REG) {
          // two special VCore measurement modes for W83627THF
          float fvalue;
          if ((chip == Chip.W83627HF || chip == Chip.W83627THF || 
            chip == Chip.W83687THF) && i == 0) 
          {
            byte vrmConfiguration = ReadByte(0, 0x18);
            int value = ReadByte(voltageBank[i], voltageRegister[i]);
            if ((vrmConfiguration & 0x01) == 0)
              fvalue = 0.016f * value; // VRM8 formula
            else
              fvalue = 0.00488f * value + 0.69f; // VRM9 formula
          } else {
            int value = ReadByte(voltageBank[i], voltageRegister[i]);
            fvalue = voltageGain * value;
          }
          if (fvalue > 0)
            voltages[i] = fvalue;
          else
            voltages[i] = null;
        } else {
          // Battery voltage
          bool valid = (ReadByte(0, 0x5D) & 0x01) > 0;
          if (valid) {
            voltages[i] = voltageGain * ReadByte(5, VOLTAGE_VBAT_REG);
          } else {
            voltages[i] = null;
          }
        }
      }

      for (int i = 0; i < temperatures.Length; i++) {
        int value = ((sbyte)ReadByte(TEMPERATURE_BANK[i], 
          TEMPERATURE_REG[i])) << 1;
        if (TEMPERATURE_BANK[i] > 0) 
          value |= ReadByte(TEMPERATURE_BANK[i],
            (byte)(TEMPERATURE_REG[i] + 1)) >> 7;

        float temperature = value / 2.0f;
        if (temperature <= 125 && temperature >= -55 && !peciTemperature[i]) {
          temperatures[i] = temperature;
        } else {
          temperatures[i] = null;
        }
      }

      ulong bits = 0;
      for (int i = 0; i < FAN_BIT_REG.Length; i++)
        bits = (bits << 8) | ReadByte(0, FAN_BIT_REG[i]);
      ulong newBits = bits;
      for (int i = 0; i < fans.Length; i++) {
        int count = ReadByte(FAN_TACHO_BANK[i], FAN_TACHO_REG[i]);
        
        // assemble fan divisor
        int divisorBits = (int)(
          (((bits >> FAN_DIV_BIT2[i]) & 1) << 2) |
          (((bits >> FAN_DIV_BIT1[i]) & 1) << 1) |
           ((bits >> FAN_DIV_BIT0[i]) & 1));
        int divisor = 1 << divisorBits;
       
        float value = (count < 0xff) ? 1.35e6f / (count * divisor) : 0;
        fans[i] = value;

        // update fan divisor
        if (count > 192 && divisorBits < 7) 
          divisorBits++;
        if (count < 96 && divisorBits > 0)
          divisorBits--;

        newBits = SetBit(newBits, FAN_DIV_BIT2[i], (divisorBits >> 2) & 1);
        newBits = SetBit(newBits, FAN_DIV_BIT1[i], (divisorBits >> 1) & 1);
        newBits = SetBit(newBits, FAN_DIV_BIT0[i], divisorBits & 1);
      }
     
      // write new fan divisors 
      for (int i = FAN_BIT_REG.Length - 1; i >= 0; i--) {
        byte oldByte = (byte)(bits & 0xFF);
        byte newByte = (byte)(newBits & 0xFF);
        bits = bits >> 8;
        newBits = newBits >> 8;
        if (oldByte != newByte) 
          WriteByte(0, FAN_BIT_REG[i], newByte);        
      }

      // read fan speeds
      for (int i = 0; i < Controls.Length; i++) {
        byte value = ReadByte(0, fanPwmRegister[i]);
        Controls[i] = (float)Math.Round((value) * 100.0f / 0xFF);
      }

      Ring0.ReleaseIsaBusMutex();
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Chip ID: 0x"); r.AppendLine(chip.ToString("X"));
      r.Append("Chip revision: 0x");
      r.AppendLine(revision.ToString("X", CultureInfo.InvariantCulture));
      r.Append("Base Adress: 0x");
      r.AppendLine(address.ToString("X4", CultureInfo.InvariantCulture));
      r.AppendLine();

      if (!Ring0.WaitIsaBusMutex(100))
        return r.ToString();

      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0x7; i++) {
        r.Append(" ");
        r.Append((i << 4).ToString("X2", CultureInfo.InvariantCulture)); 
        r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte(0, (byte)((i << 4) | j)).ToString(
            "X2", CultureInfo.InvariantCulture));
        }
        r.AppendLine();
      }
      for (int k = 1; k <= 15; k++) {
        r.AppendLine("Bank " + k);
        for (int i = 0x5; i < 0x6; i++) {
          r.Append(" "); 
          r.Append((i << 4).ToString("X2", CultureInfo.InvariantCulture)); 
          r.Append("  ");
          for (int j = 0; j <= 0xF; j++) {
            r.Append(" ");
            r.Append(ReadByte((byte)(k), (byte)((i << 4) | j)).ToString(
              "X2", CultureInfo.InvariantCulture));
          }
          r.AppendLine();
        }
      }
      r.AppendLine();

      Ring0.ReleaseIsaBusMutex();

      return r.ToString();
    }
  } 
}
