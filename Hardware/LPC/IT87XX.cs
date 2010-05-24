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
  public class IT87XX : LPCHardware, IHardware {
        
    private bool available = false;
    private ushort address;

    private readonly ushort addressReg;
    private readonly ushort dataReg;

    private List<Sensor> voltages = new List<Sensor>(9);    
    private List<Sensor> temperatures = new List<Sensor>(3);    
    private Sensor[] fans;
   
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

    public IT87XX(Chip chip, ushort address, Mainboard.Manufacturer 
      mainboardManufacturer, Mainboard.Model mainboardModel) : base (chip) {
      
      this.address = address;
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

      string[] temperatureLabels;
      List<Voltage> voltageConfigs = new List<Voltage>();
      switch (mainboardManufacturer) {
        case Mainboard.Manufacturer.DFI:
          switch (mainboardModel) {
            case Mainboard.Model.LP_BI_P45_T2RS_Elite:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("FSB VTT", 1));
              voltageConfigs.Add(new Voltage("+3.3V", 2));
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("+12V", 4, 30, 10, 0));
              voltageConfigs.Add(new Voltage("NB Core", 5));
              voltageConfigs.Add(new Voltage("VDIMM", 6));
              voltageConfigs.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("VBat", 8));
              temperatureLabels = new string[] {
                "CPU", "System", "Chipset" };
              break;
            case Mainboard.Model.LP_DK_P55_T3eH9:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("VTT", 1));
              voltageConfigs.Add(new Voltage("+3.3V", 2));
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("+12V", 4, 30, 10, 0));
              voltageConfigs.Add(new Voltage("CPU PLL", 5));
              voltageConfigs.Add(new Voltage("DRAM", 6));
              voltageConfigs.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("VBat", 8));
              temperatureLabels = new string[] {
                "Chipset", "CPU PWM", "CPU" };
              break;
            default:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("VTT", 1, true));
              voltageConfigs.Add(new Voltage("+3.3V", 2, true));
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0, true));
              voltageConfigs.Add(new Voltage("+12V", 4, 30, 10, 0, true));
              voltageConfigs.Add(new Voltage("Voltage #6", 5, true));
              voltageConfigs.Add(new Voltage("DRAM", 6, true));
              voltageConfigs.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0, true));
              voltageConfigs.Add(new Voltage("VBat", 8));
              temperatureLabels = new string[] {
                "Temperature #1", "Temperature #2", "Temperature #3" };
              break;
          }
          break;

        case Mainboard.Manufacturer.Gigabyte:
          switch (mainboardModel) {            
            case Mainboard.Model.EP45_DS3R:
            case Mainboard.Model.P35_DS3:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("DRAM", 1));
              voltageConfigs.Add(new Voltage("+3.3V", 2));
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("+12V", 7, 27, 9.1f, 0));
              voltageConfigs.Add(new Voltage("VBat", 8));    
              break;
            case Mainboard.Model.GA_MA785GMT_UD2H:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("DRAM", 1));
              voltageConfigs.Add(new Voltage("+3.3V", 2));
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
              voltageConfigs.Add(new Voltage("+12V", 4, 27, 9.1f, 0));
              voltageConfigs.Add(new Voltage("VBat", 8));   
              break;
            default:
              voltageConfigs.Add(new Voltage("CPU VCore", 0));
              voltageConfigs.Add(new Voltage("DRAM", 1, true));
              voltageConfigs.Add(new Voltage("+3.3V", 2, true));              
              voltageConfigs.Add(new Voltage("+5V", 3, 6.8f, 10, 0, true));
              voltageConfigs.Add(new Voltage("Voltage #5", 4, true));
              voltageConfigs.Add(new Voltage("Voltage #6", 5, true));
              voltageConfigs.Add(new Voltage("Voltage #7", 6, true));
              voltageConfigs.Add(new Voltage("+12V", 7, 27, 9.1f, 0, true));
              voltageConfigs.Add(new Voltage("VBat", 8));              
              break;
          }
          temperatureLabels = new string[] { "System", "CPU" };
          break; 

        default:
          voltageConfigs.Add(new Voltage("CPU VCore", 0));
          voltageConfigs.Add(new Voltage("Voltage #2", 1, true));
          voltageConfigs.Add(new Voltage("Voltage #3", 2, true));
          voltageConfigs.Add(new Voltage("Voltage #4", 3, true));
          voltageConfigs.Add(new Voltage("Voltage #5", 4, true));
          voltageConfigs.Add(new Voltage("Voltage #6", 5, true));
          voltageConfigs.Add(new Voltage("Voltage #7", 6, true));
          voltageConfigs.Add(new Voltage("Voltage #8", 7, true));
          voltageConfigs.Add(new Voltage("VBat", 8));
          temperatureLabels = new string[] {
            "Temperature #1", "Temperature #2", "Temperature #3" };
          break;
      }

      string formula = "Voltage = value + (value - Vf) * Ri / Rf.";
      foreach (Voltage voltage in voltageConfigs)
        voltages.Add(new Sensor(voltage.Name, voltage.Index, voltage.Hidden, 
          null, SensorType.Voltage, this, new ParameterDescription[] {
          new ParameterDescription("Ri [kΩ]", "Input resistance.\n" + 
            formula, voltage.Ri),
          new ParameterDescription("Rf [kΩ]", "Reference resistance.\n" + 
            formula, voltage.Rf),
          new ParameterDescription("Vf [V]", "Reference voltage.\n" + 
            formula, voltage.Vf)
          }));  

      for (int i = 0; i < temperatureLabels.Length; i++)
        if (temperatureLabels[i] != null) {
          temperatures.Add(new Sensor(temperatureLabels[i], i, null,
            SensorType.Temperature, this, new ParameterDescription[] {
            new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
          }));
        }

      fans = new Sensor[5];
      for (int i = 0; i < fans.Length; i++)
        fans[i] = new Sensor("Fan #" + (i + 1), i, SensorType.Fan, this);           

      available = true;
    }

    public bool IsAvailable {
      get { return available; } 
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
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
          bool valid;
          byte value = ReadByte((byte)((i << 4) | j), out valid);
          if (valid)
            r.Append(value.ToString("X2"));
          else
            r.Append("??");
        }
        r.AppendLine();
      }
      r.AppendLine();

      return r.ToString();
    }

    public override void Update() {

      foreach (Sensor sensor in voltages) {
        bool valid;
        float value = 0.001f * ((int)ReadByte(
          (byte)(VOLTAGE_BASE_REG + sensor.Index), out valid) << 4);
        if (!valid)
          continue;

        sensor.Value = value + (value - sensor.Parameters[2].Value) *
          sensor.Parameters[0].Value / sensor.Parameters[1].Value;
        if (value > 0)
          ActivateSensor(sensor);        
      }

      foreach (Sensor sensor in temperatures) {
        bool valid;
        sbyte value = (sbyte)ReadByte(
          (byte)(TEMPERATURE_BASE_REG + sensor.Index), out valid);
        if (!valid)
          continue;

        sensor.Value = value + sensor.Parameters[0].Value;
        if (value < sbyte.MaxValue && value > 0)
          ActivateSensor(sensor);        
      }

      foreach (Sensor sensor in fans) {
        bool valid;
        int value = ReadByte(FAN_TACHOMETER_REG[sensor.Index], out valid);
        if (!valid) 
          continue;
        value |= ReadByte(FAN_TACHOMETER_EXT_REG[sensor.Index], out valid) << 8;
        if (!valid)
          continue;

        if (value > 0x3f) {
          sensor.Value = (value < 0xffff) ? 1.35e6f / ((value) * 2) : 0;
          if (sensor.Value > 0)
            ActivateSensor(sensor);
        } else {
          sensor.Value = null;
        }
      }      
    }

    private class Voltage {
      public readonly string Name;
      public readonly int Index;
      public readonly float Ri;
      public readonly float Rf;
      public readonly float Vf;
      public readonly bool Hidden;

      public Voltage(string name, int index) :
        this(name, index, 0, 1, 0, false) { }

      public Voltage(string name, int index, bool hidden) :
        this(name, index, 0, 1, 0, hidden) { }

      public Voltage(string name, int index, float ri, float rf, float vf) :
        this(name, index, ri, rf, vf, false) { }

      public Voltage(string name, int index, float ri, float rf, float vf, 
        bool hidden) 
      {
        this.Name = name;
        this.Index = index;
        this.Ri = ri;
        this.Rf = rf;
        this.Vf = vf;
        this.Hidden = hidden;
      }
    }
  }
}
