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
  public class W83627 : Winbond, IHardware {
   
    private Sensor[] temperatures;
    private Sensor[] fans;
    private Sensor[] voltages;

    private float[] voltageGains;
    private string[] fanNames;

    // Hardware Monitor Registers
    private const byte VOLTAGE_BASE_REG = 0x20;   
    private const byte TEMPERATURE_BASE_REG = 0x50;
    private const byte TEMPERATURE_SYS_REG = 0x27;

    private byte[] FAN_TACHO_REG = new byte[] { 0x28, 0x29, 0x2A, 0x3F, 0x53 };
    private byte[] FAN_TACHO_BANK = new byte[] { 0, 0, 0, 0, 5 };       
    private byte[] FAN_BIT_REG = new byte[] { 0x47, 0x4B, 0x4C, 0x59, 0x5D };
    private byte[] FAN_DIV_BIT0 = new byte[] { 36, 38, 30, 8, 10 };
    private byte[] FAN_DIV_BIT1 = new byte[] { 37, 39, 31, 9, 11 };
    private byte[] FAN_DIV_BIT2 = new byte[] { 5, 6, 7, 23, 15 };

    public W83627(Chip chip, byte revision, ushort address) 
      : base(chip, revision, address)
    {
     
      temperatures = new Sensor[3];
      temperatures[0] = new Sensor("CPU", 0, SensorType.Temperature, this);
      temperatures[1] = new Sensor("Auxiliary", 1, SensorType.Temperature, this);
      temperatures[2] = new Sensor("System", 2, SensorType.Temperature, this);

      switch (chip) {
        case Chip.W83627DHG:
        case Chip.W83627DHGP: 
          fanNames = new string[] { "System", "CPU #1", "Auxiliary #1", 
            "CPU #2", "Auxiliary #2" };
          voltageGains = new float[] { 0.008f, 1, 1, 0.016f, 1, 1, 1, 0.016f };
          voltages = new Sensor[3];
          voltages[0] = new Sensor("CPU VCore", 0, SensorType.Voltage, this);
          voltages[1] = new Sensor("+3.3V", 3, SensorType.Voltage, this);
          voltages[2] = new Sensor("Battery", 7, SensorType.Voltage, this);
          break;
        case Chip.W83627HF: 
          fanNames = new string[] { "Fan #1", "Fan #2", "Fan #3" };
          voltageGains = new float[] { 0.016f, 1, 0.016f, 1, 1, 1, 1, 0.016f };
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

    public void Update() {
      foreach (Sensor sensor in voltages) {
        if (sensor.Index < 7) {
          int value = ReadByte(0, (byte)(VOLTAGE_BASE_REG + sensor.Index));
          sensor.Value = voltageGains[sensor.Index] * value;
          if (sensor.Value > 0)
            ActivateSensor(sensor);
          else
            DeactivateSensor(sensor);
        } else {
          // Battery voltage
          bool valid = (ReadByte(0, 0x5D) & 0x01) > 0;
          if (valid) {
            sensor.Value = voltageGains[sensor.Index] * 
              ReadByte(5, 0x51);
            ActivateSensor(sensor);
          } else
            DeactivateSensor(sensor);
        }
      }

      foreach (Sensor sensor in temperatures) {
        int value;
        if (sensor.Index < 2) {
          value = (sbyte)ReadByte((byte)(sensor.Index + 1), TEMPERATURE_BASE_REG);
          value = (value << 1) | ReadByte((byte)(sensor.Index + 1),
            (byte)(TEMPERATURE_BASE_REG + 1)) >> 7;
        } else {
          value = (sbyte)ReadByte(0, TEMPERATURE_SYS_REG) << 1;
        }
        sensor.Value = value / 2.0f;
        if (value < 0xFE)
          ActivateSensor(sensor);
        else
          DeactivateSensor(sensor);
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
        sensor.Value = (count < 0xff) ? 1.35e6f / (count * divisor) : 0;
        ActivateSensor(sensor);        
      }     
    }
  }
}
