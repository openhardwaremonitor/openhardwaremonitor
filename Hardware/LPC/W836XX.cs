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
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  public class W836XX : LPCHardware, IHardware {

    private ushort address;
    private byte revision;

    private bool available;

    private Sensor[] temperatures;
    private Sensor[] fans;
    private Sensor[] voltages;

    private float[] voltageGains;
    private string[] fanNames;

    // Consts 
    private const ushort WINBOND_VENDOR_ID = 0x5CA3;
    private const byte HIGH_BYTE = 0x80;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte VOLTAGE_BASE_REG = 0x20;
    private const byte BANK_SELECT_REGISTER = 0x4E;
    private const byte VENDOR_ID_REGISTER = 0x4F;
    private const byte TEMPERATURE_SOURCE_SELECT_REG = 0x49;

    private string[] TEMPERATURE_NAME = 
      new string[] {"CPU", "Auxiliary", "System"};
    private byte[] TEMPERATURE_REG = new byte[] { 0x50, 0x50, 0x27 };
    private byte[] TEMPERATURE_BANK = new byte[] { 1, 2, 0 };

    private byte[] FAN_TACHO_REG = new byte[] { 0x28, 0x29, 0x2A, 0x3F, 0x53 };
    private byte[] FAN_TACHO_BANK = new byte[] { 0, 0, 0, 0, 5 };       
    private byte[] FAN_BIT_REG = new byte[] { 0x47, 0x4B, 0x4C, 0x59, 0x5D };
    private byte[] FAN_DIV_BIT0 = new byte[] { 36, 38, 30, 8, 10 };
    private byte[] FAN_DIV_BIT1 = new byte[] { 37, 39, 31, 9, 11 };
    private byte[] FAN_DIV_BIT2 = new byte[] { 5, 6, 7, 23, 15 };

    private byte ReadByte(byte bank, byte register) {
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte(
        (ushort)(address + DATA_REGISTER_OFFSET));
    } 

    private void WriteByte(byte bank, byte register, byte value) {
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      WinRing0.WriteIoPortByte(
         (ushort)(address + DATA_REGISTER_OFFSET), value); 
    }
   
    public W836XX(Chip chip, byte revision, ushort address) 
      : base(chip)
    {
      this.address = address;
      this.revision = revision;

      available = IsWinbondVendor();

      ParameterDescription[] parameter = new ParameterDescription[] {
        new ParameterDescription("Offset", "Temperature offset.", 0)
      };
      List<Sensor> list = new List<Sensor>();
      switch (chip) {
        case Chip.W83667HG:
        case Chip.W83667HGB:
          // do not add temperature sensor registers that read PECI
          byte flag = ReadByte(0, TEMPERATURE_SOURCE_SELECT_REG);
          if ((flag & 0x04) == 0)
            list.Add(new Sensor(TEMPERATURE_NAME[0], 0, null,
              SensorType.Temperature, this, parameter));
          if ((flag & 0x40) == 0)
            list.Add(new Sensor(TEMPERATURE_NAME[1], 1, null,
              SensorType.Temperature, this, parameter));
          list.Add(new Sensor(TEMPERATURE_NAME[2], 2, null,
            SensorType.Temperature, this, parameter));
          break;
        case Chip.W83627DHG:        
        case Chip.W83627DHGP:
          // do not add temperature sensor registers that read PECI
          byte sel = ReadByte(0, TEMPERATURE_SOURCE_SELECT_REG);
          if ((sel & 0x07) == 0)
            list.Add(new Sensor(TEMPERATURE_NAME[0], 0, null,
              SensorType.Temperature, this, parameter));
          if ((sel & 0x70) == 0)
            list.Add(new Sensor(TEMPERATURE_NAME[1], 1, null,
              SensorType.Temperature, this, parameter));
          list.Add(new Sensor(TEMPERATURE_NAME[2], 2, null,
            SensorType.Temperature, this, parameter));
          break;
        default:
          // no PECI support, add all sensors
          for (int i = 0; i < TEMPERATURE_NAME.Length; i++)
            list.Add(new Sensor(TEMPERATURE_NAME[i], i, null,
              SensorType.Temperature, this, parameter));
          break;
      }
      temperatures = list.ToArray();

      switch (chip) {
        case Chip.W83627DHG:
        case Chip.W83627DHGP:
        case Chip.W83627EHF:
        case Chip.W83667HG:
        case Chip.W83667HGB: 
          fanNames = new string[] { "System", "CPU", "Auxiliary", 
            "CPU #2", "Auxiliary #2" };
          voltageGains = new float[] { 1, 1, 1, 2, 1, 1, 1, 2 };
          voltages = new Sensor[3];
          voltages[0] = new Sensor("CPU VCore", 0, SensorType.Voltage, this);
          voltages[1] = new Sensor("+3.3V", 3, SensorType.Voltage, this);
          voltages[2] = new Sensor("Battery", 7, SensorType.Voltage, this);
          break;
        case Chip.W83627HF:
        case Chip.W83627THF:
        case Chip.W83687THF:
          fanNames = new string[] { "System", "CPU", "Auxiliary" };
          voltageGains = new float[] { 2, 1, 2, 1, 1, 1, 1, 2 };
          voltages = new Sensor[3];
          voltages[0] = new Sensor("CPU VCore", 0, SensorType.Voltage, this);
          voltages[1] = new Sensor("+3.3V", 2, SensorType.Voltage, this);
          voltages[2] = new Sensor("Battery", 7, SensorType.Voltage, this);
          break;
        default: fanNames = new string[0];
          break;
      }
      
      fans = new Sensor[fanNames.Length];
      for (int i = 0; i < fanNames.Length; i++)
        fans[i] = new Sensor(fanNames[i], i, SensorType.Fan, this);
    }

    public bool IsAvailable {
      get { return available; }
    }        

    private bool IsWinbondVendor() {
      ushort vendorId =
        (ushort)((ReadByte(HIGH_BYTE, VENDOR_ID_REGISTER) << 8) |
           ReadByte(0, VENDOR_ID_REGISTER));
      return vendorId == WINBOND_VENDOR_ID;
    }

    public void Update() {

      foreach (Sensor sensor in voltages) {
        if (sensor.Index < 7) {
          // two special VCore measurement modes for W83627THF
          if ((chip == Chip.W83627HF || chip == Chip.W83627THF || 
            chip == Chip.W83687THF) && sensor.Index == 0) 
          {
            byte vrmConfiguration = ReadByte(0, 0x18);
            int value = ReadByte(0, VOLTAGE_BASE_REG);
            if ((vrmConfiguration & 0x01) == 0)
              sensor.Value = 0.016f * value; // VRM8 formula
            else
              sensor.Value = 0.00488f * value + 0.69f; // VRM9 formula
          } else {
            int value = ReadByte(0, (byte)(VOLTAGE_BASE_REG + sensor.Index));
            sensor.Value = 0.008f * voltageGains[sensor.Index] * value;
          }
          if (sensor.Value > 0)
            ActivateSensor(sensor);
        } else {
          // Battery voltage
          bool valid = (ReadByte(0, 0x5D) & 0x01) > 0;
          if (valid) {
            sensor.Value =
              0.008f * voltageGains[sensor.Index] * ReadByte(5, 0x51);
            ActivateSensor(sensor);
          } else {
            sensor.Value = null;
          }
        }
      }

      foreach (Sensor sensor in temperatures) {
        int value = ((sbyte)ReadByte(TEMPERATURE_BANK[sensor.Index],
          TEMPERATURE_REG[sensor.Index])) << 1;
        if (TEMPERATURE_BANK[sensor.Index] > 0) 
          value |= ReadByte(TEMPERATURE_BANK[sensor.Index],
            (byte)(TEMPERATURE_REG[sensor.Index] + 1)) >> 7;

        float temperature = value / 2.0f;
        if (temperature <= 125 && temperature >= -55) {
          sensor.Value = temperature + sensor.Parameters[0].Value;
          ActivateSensor(sensor);
        } else {
          sensor.Value = null;
        }
      }

      long bits = 0;
      for (int i = 0; i < FAN_BIT_REG.Length; i++)
        bits = (bits << 8) | ReadByte(0, FAN_BIT_REG[i]);
      foreach (Sensor sensor in fans) {
        int count = ReadByte(FAN_TACHO_BANK[sensor.Index], 
          FAN_TACHO_REG[sensor.Index]);
        int divisorBits = (int)(
          (((bits >> FAN_DIV_BIT2[sensor.Index]) & 1) << 2) |
          (((bits >> FAN_DIV_BIT1[sensor.Index]) & 1) << 1) |
           ((bits >> FAN_DIV_BIT0[sensor.Index]) & 1));
        int divisor = 1 << divisorBits;
        float value = (count < 0xff) ? 1.35e6f / (count * divisor) : 0;
        sensor.Value = value;
        if (value > 0)
          ActivateSensor(sensor);        
      }     
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Chip ID: 0x"); r.AppendLine(chip.ToString("X"));
      r.Append("Chip revision: 0x"); r.AppendLine(revision.ToString("X"));
      r.Append("Base Adress: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();
      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0x7; i++) {
        r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte(0, (byte)((i << 4) | j)).ToString("X2"));
        }
        r.AppendLine();
      }
      for (int k = 1; k <= 15; k++) {
        r.AppendLine("Bank " + k);
        for (int i = 0x5; i < 0x6; i++) {
          r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
          for (int j = 0; j <= 0xF; j++) {
            r.Append(" ");
            r.Append(ReadByte((byte)(k),
              (byte)((i << 4) | j)).ToString("X2"));
          }
          r.AppendLine();
        }
      }
      r.AppendLine();

      return r.ToString();
    }
  }
}
