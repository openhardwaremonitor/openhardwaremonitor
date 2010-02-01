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
  public class IT87XX : IHardware {

    private string name;
    private Image icon;
        
    private bool available = false;
    private Chip chip;
    private ushort address;

    private Sensor[] temperatures;
    private Sensor[] fans;
    private Sensor[] voltages;
    private List<ISensor> active = new List<ISensor>();
    private float[] voltageGains;
   
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
    
    private byte ReadByte(byte register) {
      WinRing0.WriteIoPortByte(
        (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte((ushort)(address + DATA_REGISTER_OFFSET));
    }

    public IT87XX(Chip chip, ushort address) {
      
      this.chip = chip;
      this.address = address;

      switch (chip) {
        case Chip.IT8716: name = "ITE IT8716"; break;
        case Chip.IT8718: name = "ITE IT8718"; break;
        case Chip.IT8720: name = "ITE IT8720"; break;
        case Chip.IT8726: name = "ITE IT8726"; break;
        default: return;
      }
      
      // Check vendor id
      byte vendorId = ReadByte(VENDOR_ID_REGISTER);      
      if (vendorId != ITE_VENDOR_ID)
        return;

      // Bit 0x10 of the configuration register should always be 1
      if ((ReadByte(CONFIGURATION_REGISTER) & 0x10) == 0)
        return;

      temperatures = new Sensor[3];
      for (int i = 0; i < temperatures.Length; i++) 
        temperatures[i] = new Sensor("Temperature #" + (i + 1), i,
          SensorType.Temperature, this);

      fans = new Sensor[5];
      for (int i = 0; i < fans.Length; i++)
        fans[i] = new Sensor("Fan #" + (i + 1), i, SensorType.Fan, this);

      voltageGains = new float[] { 
        1, 1, 1, (6.8f / 10 + 1), 1, 1, 1, 1, 1 };

      voltages = new Sensor[2];
      voltages[0] = new Sensor("CPU VCore", 0, SensorType.Voltage, this);
      voltages[1] = new Sensor("Battery", 8, SensorType.Voltage, this);           

      available = true;
      icon = Utilities.EmbeddedResources.GetImage("chip.png");
    }

    public bool IsAvailable {
      get { return available; } 
    }

    public string Name {
      get { return name; }
    }

    public string Identifier {
      get { return "/lpc/it87"; }
    }

    public Image Icon {
      get { return icon; }
    }

    public ISensor[] Sensors {
      get { return active.ToArray(); }
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC IT87XX");
      r.AppendLine();
      r.Append("Chip ID: 0x"); r.AppendLine(chip.ToString("X"));
      r.Append("Chip Name: "); r.AppendLine(name);
      r.Append("Base Address: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();
      r.AppendLine("Environment Controller Registers");
      r.AppendLine();

      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0xA; i++) {
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

    public void Update() {

      foreach (Sensor sensor in voltages) {
        int value = ReadByte((byte)(VOLTAGE_BASE_REG + sensor.Index));
        sensor.Value = voltageGains[sensor.Index] * 0.001f * (value << 4);
        if (sensor.Value > 0)
          ActivateSensor(sensor);
        else
          DeactivateSensor(sensor);
      }

      foreach (Sensor sensor in temperatures) {
        sbyte value = (sbyte)ReadByte((byte)(TEMPERATURE_BASE_REG + sensor.Index));
        sensor.Value = value;
        if (value < sbyte.MaxValue && value > 0)
          ActivateSensor(sensor);
        else
          DeactivateSensor(sensor);
      }

      foreach (Sensor sensor in fans) {
        int value = ReadByte(FAN_TACHOMETER_REG[sensor.Index]);
        value |= ReadByte(FAN_TACHOMETER_EXT_REG[sensor.Index]) << 8;

        if (value > 0) {
          sensor.Value = (value < 0xffff) ? 1.35e6f / ((value) * 2) : 0;
          ActivateSensor(sensor);
        } else {
          DeactivateSensor(sensor);
        }
      }      
    }

    private void ActivateSensor(Sensor sensor) {
      if (!active.Contains(sensor)) {
        active.Add(sensor);
        if (SensorAdded != null)
          SensorAdded(sensor);
      }
    }

    private void DeactivateSensor(Sensor sensor) {
      if (active.Contains(sensor)) {
        active.Remove(sensor);
        if (SensorRemoved != null)
          SensorRemoved(sensor);
      }
    }

    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;

  }
}
