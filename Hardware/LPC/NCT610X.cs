/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2013 Michael Möller <mmoeller@openhardwaremonitor.org>
  Copyright (C) 2015 Dawid Gan <deveee@gmail.com>
	
*/

using System;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  internal class NCT610X : ISuperIO {

    private readonly ushort port;
    private readonly byte revision;

    private readonly Chip chip;

    private readonly bool isNuvotonVendor;

    private readonly float?[] voltages = new float?[0];
    private readonly float?[] temperatures = new float?[0];
    private readonly float?[] fans = new float?[0];
    private readonly float?[] controls = new float?[0];

    // Hardware Monitor
    private const uint ADDRESS_REGISTER_OFFSET = 0x05;
    private const uint DATA_REGISTER_OFFSET = 0x06;
    private const byte BANK_SELECT_REGISTER = 0x4E;

    private byte ReadByte(ushort address) {
      byte bank = (byte)(address >> 8);
      byte register = (byte)(address & 0xFF);
      Ring0.WriteIoPort(port + ADDRESS_REGISTER_OFFSET, BANK_SELECT_REGISTER);
      Ring0.WriteIoPort(port + DATA_REGISTER_OFFSET, bank);
      Ring0.WriteIoPort(port + ADDRESS_REGISTER_OFFSET, register);
      return Ring0.ReadIoPort(port + DATA_REGISTER_OFFSET);
    }

    private void WriteByte(ushort address, byte value) {
      byte bank = (byte)(address >> 8);
      byte register = (byte)(address & 0xFF);
      Ring0.WriteIoPort(port + ADDRESS_REGISTER_OFFSET, BANK_SELECT_REGISTER);
      Ring0.WriteIoPort(port + DATA_REGISTER_OFFSET, bank);
      Ring0.WriteIoPort(port + ADDRESS_REGISTER_OFFSET, register);
      Ring0.WriteIoPort(port + DATA_REGISTER_OFFSET, value);
    }

    // Consts 
    private const ushort NUVOTON_VENDOR_ID = 0x5CA3;

    // Hardware Monitor Registers    
    private const ushort VENDOR_ID_HIGH_REGISTER = 0x80FE;
    private const ushort VENDOR_ID_LOW_REGISTER = 0x00FE;  
    
    private readonly ushort[] FAN_PWM_OUT_REG = 
      { 0x04A, 0x04B, 0x04C };
    private readonly ushort[] FAN_PWM_COMMAND_REG = 
      { 0x119, 0x129, 0x139 };
    private readonly ushort[] FAN_CONTROL_MODE_REG = 
      { 0x113, 0x123, 0x133 };

    private readonly ushort fanRpmBaseRegister;
    private readonly int minFanRPM;

    private bool[] restoreDefaultFanControlRequired = new bool[6];       
    private byte[] initialFanControlMode = new byte[6];
    private byte[] initialFanPwmCommand = new byte[6];

    private readonly ushort?[] voltageRegisters;
    private readonly ushort voltageVBatRegister;

    private readonly byte[] temperaturesSource;

    private readonly ushort[] temperatureRegister;
    private readonly ushort[] temperatureHalfRegister;
    private readonly int[] temperatureHalfBit;  
    private readonly ushort[] temperatureSourceRegister;        

    private readonly ushort?[] alternateTemperatureRegister;


    private enum SourceNCT610X : byte
    {
        SYSTIN = 1,
        CPUTIN = 2,
        AUXTIN = 3,
        SMBUSMASTER_0 = 4,
        SMBUSMASTER_1 = 5,
        SMBUSMASTER_2 = 6,
        SMBUSMASTER_3 = 7,
        SMBUSMASTER_4 = 8,
        SMBUSMASTER_5 = 9,
        SMBUSMASTER_6 = 10,
        SMBUSMASTER_7 = 11,
        PECI_0 = 12,
        PECI_1 = 13,
        PCH_CHIP_CPU_MAX_TEMP = 14,
        PCH_CHIP_TEMP = 15,
        PCH_CPU_TEMP = 16,
        PCH_MCH_TEMP = 17,
        PCH_DIM0_TEMP = 18,
        PCH_DIM1_TEMP = 19,
        PCH_DIM2_TEMP = 20,
        PCH_DIM3_TEMP = 21,
        BYTE_TEMP = 22
    }

    public NCT610X(Chip chip, byte revision, ushort port)
    {
        this.chip = chip;
        this.revision = revision;
        this.port = port;

        this.isNuvotonVendor = IsNuvotonVendor();

        if (!isNuvotonVendor)
            return;

        fans = new float?[3];
        controls = new float?[3];

        fanRpmBaseRegister = 0x030;

        // min value RPM value with 13-bit fan counter
        minFanRPM = (int)(1.35e6 / 0x1FFF);

        voltages = new float?[15];
        voltageRegisters = new ushort?[] 
        { 0x300, 0x301, 0x302, 0x303, 0x304, 0x305, null, 0x307, 0x308, 
            0x309, null, null, null, null, null };
        voltageVBatRegister = 0x308;

        temperatures = new float?[4];
        temperaturesSource = new byte[] {
            (byte)SourceNCT610X.PECI_0,
            (byte)SourceNCT610X.SYSTIN,
            (byte)SourceNCT610X.CPUTIN,
            (byte)SourceNCT610X.AUXTIN
        };

        temperatureRegister = new ushort[]
        { 0x027, 0x018, 0x019, 0x01A };
        temperatureHalfRegister = new ushort[]
        { 0, 0x01B, 0x11B, 0x21B };              
        temperatureHalfBit = new int[]
        { -1, 7, 7, 7 };
        temperatureSourceRegister = new ushort[] 
        { 0x621, 0x100, 0x200, 0x300 };

        alternateTemperatureRegister = new ushort?[] 
        {null, 0x018, 0x019, 0x01A };
    }

    private bool IsNuvotonVendor() {
        return ((ReadByte(VENDOR_ID_HIGH_REGISTER) << 8) |
        ReadByte(VENDOR_ID_LOW_REGISTER)) == NUVOTON_VENDOR_ID;
    }

    public byte? ReadGPIO(int index) {
      return null;
    }

    public void WriteGPIO(int index, byte value) { }

    private void SaveDefaultFanControl(int index) {
      if (!restoreDefaultFanControlRequired[index]) {
        initialFanControlMode[index] = ReadByte(FAN_CONTROL_MODE_REG[index]);
        initialFanPwmCommand[index] = ReadByte(FAN_PWM_COMMAND_REG[index]);
        restoreDefaultFanControlRequired[index] = true;
      }
    }

    private void RestoreDefaultFanControl(int index) {
      if (restoreDefaultFanControlRequired[index]) {
        WriteByte(FAN_CONTROL_MODE_REG[index], initialFanControlMode[index]);
        WriteByte(FAN_PWM_COMMAND_REG[index], initialFanPwmCommand[index]);
        restoreDefaultFanControlRequired[index] = false;
      }
    }

    public void SetControl(int index, byte? value) {
      if (!isNuvotonVendor)
        return;

      if (index < 0 || index >= controls.Length)
        throw new ArgumentOutOfRangeException("index");

      if (!Ring0.WaitIsaBusMutex(10))
        return;

      if (value.HasValue) {
        SaveDefaultFanControl(index);

        // set manual mode
        WriteByte(FAN_CONTROL_MODE_REG[index], 0);

        // set output value
        WriteByte(FAN_PWM_COMMAND_REG[index], value.Value);  
      } else {
        RestoreDefaultFanControl(index);
      }

      Ring0.ReleaseIsaBusMutex();
    }   

    public Chip Chip { get { return chip; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }
    public float?[] Controls { get { return controls; } }

    public void Update() {
      if (!isNuvotonVendor)
        return;

      if (!Ring0.WaitIsaBusMutex(10))
        return;

      for (int i = 0; i < voltages.Length; i++) {
        if (!voltageRegisters[i].HasValue)
            continue;

        float value = 0.008f * ReadByte(voltageRegisters[i].Value);
        bool valid = value > 0;

        // check if battery voltage monitor is enabled
        if (valid && voltageRegisters[i] == voltageVBatRegister) 
          valid = (ReadByte(0x0318) & 0x01) > 0;

        voltages[i] = valid ? value : (float?)null;
      }

      int temperatureSourceMask = 0;
      for (int i = temperatureRegister.Length - 1; i >= 0 ; i--) {
        int value = ((sbyte)ReadByte(temperatureRegister[i])) << 1;
        if (temperatureHalfBit[i] > 0) {
          value |= ((ReadByte(temperatureHalfRegister[i]) >>
            temperatureHalfBit[i]) & 0x1);
        }

        byte source = ReadByte(temperatureSourceRegister[i]);
        temperatureSourceMask |= 1 << source;

        float? temperature = 0.5f * value;
        if (temperature > 125 || temperature < -55)
          temperature = null;

        for (int j = 0; j < temperatures.Length; j++) 
          if (temperaturesSource[j] == source)
            temperatures[j] = temperature; 
      }
      for (int i = 0; i < alternateTemperatureRegister.Length; i++) {
        if (!alternateTemperatureRegister[i].HasValue)
          continue;

        if ((temperatureSourceMask & (1 << temperaturesSource[i])) > 0)
          continue;

        float? temperature = (sbyte)
          ReadByte(alternateTemperatureRegister[i].Value);

        if (temperature > 125 || temperature < -55)
          temperature = null;

        temperatures[i] = temperature;
      }

      for (int i = 0; i < fans.Length; i++) {
        byte high = ReadByte((ushort)(fanRpmBaseRegister + (i << 1)));
        byte low = ReadByte((ushort)(fanRpmBaseRegister + (i << 1) + 1));
        int value = (high << 8) | low;

        fans[i] = value > minFanRPM ? value : 0;
      }

      for (int i = 0; i < controls.Length; i++) {
        int value = ReadByte(FAN_PWM_OUT_REG[i]);
        controls[i] = value / 2.55f;
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
      r.AppendLine(port.ToString("X4", CultureInfo.InvariantCulture));
      r.AppendLine();

      if (!Ring0.WaitIsaBusMutex(100))
        return r.ToString();

      Ring0.ReleaseIsaBusMutex();

      return r.ToString();
    }
  }
}
