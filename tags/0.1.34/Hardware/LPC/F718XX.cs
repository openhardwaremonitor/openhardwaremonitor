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
  public class F718XX : LPCHardware, IHardware {

    private ushort address;

    private Sensor[] temperatures;
    private Sensor[] fans;
    private Sensor[] voltages;
    private float[] voltageGains;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte VOLTAGE_BASE_REG = 0x20;
    private const byte TEMPERATURE_CONFIG_REG = 0x69;
    private const byte TEMPERATURE_BASE_REG = 0x70;
    private byte[] FAN_TACHOMETER_REG = new byte[] { 0xA0, 0xB0, 0xC0, 0xD0 };
    
    private byte ReadByte(byte register) {
      WinRing0.WriteIoPortByte(
        (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte((ushort)(address + DATA_REGISTER_OFFSET));
    } 

    public F718XX(Chip chip, ushort address) : base(chip) {
      this.address = address;

      temperatures = new Sensor[3];
      for (int i = 0; i < temperatures.Length; i++)
        temperatures[i] = new Sensor("Temperature #" + (i + 1), i, null,
          SensorType.Temperature, this, new ParameterDescription[] {
            new ParameterDescription("Offset", "Temperature offset.", 0)
          });

      fans = new Sensor[chip == Chip.F71882 ? 4 : 3];
      for (int i = 0; i < fans.Length; i++)
        fans[i] = new Sensor("Fan #" + (i + 1), i, SensorType.Fan, this);

      switch (chip) {
        case Chip.F71858:
          voltageGains = new float[] { 1, 1, 1 };
          voltages = new Sensor[3];
          voltages[0] = new Sensor("VCC3V", 0, SensorType.Voltage, this);
          voltages[1] = new Sensor("VSB3V", 1, SensorType.Voltage, this);
          voltages[2] = new Sensor("Battery", 2, SensorType.Voltage, this);
          break;
        default:
          voltageGains = new float[] { 1, 0.5f, 1, 1, 1, 1, 1, 1, 1 };
          voltages = new Sensor[4];
          voltages[0] = new Sensor("VCC3V", 0, SensorType.Voltage, this);
          voltages[1] = new Sensor("CPU VCore", 1, SensorType.Voltage, this);
          voltages[2] = new Sensor("VSB3V", 7, SensorType.Voltage, this);
          voltages[3] = new Sensor("Battery", 8, SensorType.Voltage, this);
          break;
      }
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Base Adress: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();
      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();

      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0xF; i++) {
        r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte((byte)((i << 4) | j)).ToString("X2"));
        }
        r.AppendLine();
      }
      r.AppendLine();
      return r.ToString();
    }

    public override void Update() {

      foreach (Sensor sensor in voltages) {
        int value = ReadByte((byte)(VOLTAGE_BASE_REG + sensor.Index));
        sensor.Value = voltageGains[sensor.Index] * 0.001f * (value << 4);
        if (sensor.Value > 0)
          ActivateSensor(sensor);
      }
     
      foreach (Sensor sensor in temperatures) {
        switch (chip) {
          case Chip.F71858: {
              int tableMode = 0x3 & ReadByte((byte)(TEMPERATURE_CONFIG_REG));
              int high = 
                ReadByte((byte)(TEMPERATURE_BASE_REG + 2 * sensor.Index));
              int low =
                ReadByte((byte)(TEMPERATURE_BASE_REG + 2 * sensor.Index + 1));              
              if (high != 0xbb && high != 0xcc) {
                int bits = 0;
                switch (tableMode) {
                  case 0: bits = 0; break;
                  case 1: bits = 0; break;
                  case 2: bits = (high & 0x80) << 8; break;
                  case 3: bits = (low & 0x01) << 15; break;
                }
                bits |= high << 7;
                bits |= (low & 0xe0) >> 1;
                short value = (short)(bits & 0xfff0);
                sensor.Value = value / 128.0f;
                ActivateSensor(sensor);
              } else {
                sensor.Value = null;
              }
          } break;
          default: {
            sbyte value = (sbyte)ReadByte((byte)(
              TEMPERATURE_BASE_REG + 2 * (sensor.Index + 1)));
            sensor.Value = value + sensor.Parameters[0].Value;
            if (value < sbyte.MaxValue && value > 0)
              ActivateSensor(sensor);
          } break;
        }
      }

      foreach (Sensor sensor in fans) {
        int value = ReadByte(FAN_TACHOMETER_REG[sensor.Index]) << 8;
        value |= ReadByte((byte)(FAN_TACHOMETER_REG[sensor.Index] + 1));

        if (value > 0) {
          sensor.Value = (value < 0x0fff) ? 1.5e6f / value : 0;
          if (sensor.Value > 0)
            ActivateSensor(sensor);
        } else {
          sensor.Value = null;
        }
      }      
    }
  }
}
