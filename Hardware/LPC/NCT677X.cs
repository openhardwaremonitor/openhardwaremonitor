/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  internal class NCT677X : ISuperIO {

    private readonly ushort port;
    private readonly byte revision;

    private readonly Chip chip;

    private readonly float?[] voltages = new float?[9];
    private readonly float?[] temperatures = new float?[4];
    private readonly float?[] fans = new float?[0];
    private readonly float?[] controls = new float?[3];

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
    private const ushort VENDOR_ID_HIGH_REGISTER = 0x804F;
    private const ushort VENDOR_ID_LOW_REGISTER = 0x004F;
    private const ushort VOLTAGE_VBAT_REG = 0x0551;

    private readonly ushort[] TEMPERATURE_REG = 
      { 0x027, 0x73, 0x75, 0x77, 0x150, 0x250, 0x62B, 0x62C, 0x62D };
    private readonly ushort[] TEMPERATURE_HALF_REG = 
      { 0, 0x74, 0x76, 0x78, 0x151, 0x251, 0x62E, 0x62E, 0x62E };    
    private readonly ushort[] TEMPERATURE_SRC_REG = 
      { 0x621, 0x100, 0x200, 0x300, 0x622, 0x623, 0x624, 0x625, 0x626 };
    private readonly int[] TEMPERATURE_HALF_BIT =
      { -1, 7, 7, 7, 7, 7, 0, 1, 2 };
    private readonly ushort[] VOLTAGE_REG = 
      { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x550, 0x551 };
    private readonly ushort[] FAN_RPM_REG = 
      { 0x656, 0x658, 0x65A, 0x65C, 0x65E};
    private readonly ushort[] FAN_PWM_OUT_REG = 
      { 0x001, 0x003, 0x011 };
    private readonly ushort[] FAN_PWM_COMMAND_REG = 
      { 0x109, 0x209, 0x309 };
    private readonly ushort[] FAN_CONTROL_MODE_REG = 
      { 0x102, 0x202, 0x302 };

    private readonly int minFanRPM;

    private bool[] restoreDefaultFanControlRequired = { false, false, false };
    private byte[] initialFanControlMode = new byte[3];
    private byte[] initialFanPwmCommand = new byte[3];

    private enum SourceNCT6771F : byte {
      SYSTIN = 1,
      CPUTIN = 2,
      AUXTIN = 3,
      SMBUSMASTER = 4,
      PECI_0 = 5, 
      PECI_1 = 6, 
      PECI_2 = 7,
      PECI_3 = 8,
      PECI_4 = 9,
      PECI_5 = 10,
      PECI_6 = 11,
      PECI_7 = 12,
      PCH_CHIP_CPU_MAX_TEMP = 13,
      PCH_CHIP_TEMP = 14,
      PCH_CPU_TEMP = 15,
      PCH_MCH_TEMP = 16, 
      PCH_DIM0_TEMP = 17,
      PCH_DIM1_TEMP = 18,
      PCH_DIM2_TEMP = 19,
      PCH_DIM3_TEMP = 20
    }

    private enum SourceNCT6776F : byte {
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

    public NCT677X(Chip chip, byte revision, ushort port) {
      this.chip = chip;
      this.revision = revision;
      this.port = port;

      if (!IsNuvotonVendor())
        return;

      switch (chip) {
        case LPC.Chip.NCT6771F:
          fans = new float?[4];
          // min value RPM value with 16-bit fan counter
          minFanRPM = (int)(1.35e6 / 0xFFFF);
          break;
        case LPC.Chip.NCT6776F:
          fans = new float?[5];
          // min value RPM value with 13-bit fan counter
          minFanRPM = (int)(1.35e6 / 0x1FFF);
          break;        
      }
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
      if (!Ring0.WaitIsaBusMutex(10))
        return;

      for (int i = 0; i < voltages.Length; i++) {
        float value = 0.008f * ReadByte(VOLTAGE_REG[i]);
        bool valid = value > 0;

        // check if battery voltage monitor is enabled
        if (valid && VOLTAGE_REG[i] == VOLTAGE_VBAT_REG) 
          valid = (ReadByte(0x005D) & 0x01) > 0;

        voltages[i] = valid ? value : (float?)null;
      }

      for (int i = TEMPERATURE_REG.Length - 1; i >= 0 ; i--) {
        int value = ((sbyte)ReadByte(TEMPERATURE_REG[i])) << 1;
        if (TEMPERATURE_HALF_BIT[i] > 0) {
          value |= ((ReadByte(TEMPERATURE_HALF_REG[i]) >>
            TEMPERATURE_HALF_BIT[i]) & 0x1);
        }

        byte source = ReadByte(TEMPERATURE_SRC_REG[i]);

        float? temperature = 0.5f * value;
        if (temperature > 125 || temperature < -55)
          temperature = null;

        switch (chip) {
          case Chip.NCT6771F:
            switch ((SourceNCT6771F)source) {
              case SourceNCT6771F.PECI_0: temperatures[0] = temperature; break;
              case SourceNCT6771F.CPUTIN: temperatures[1] = temperature; break;
              case SourceNCT6771F.AUXTIN: temperatures[2] = temperature; break;
              case SourceNCT6771F.SYSTIN: temperatures[3] = temperature; break;
              
            } break;
          case Chip.NCT6776F:
            switch ((SourceNCT6776F)source) {
              case SourceNCT6776F.PECI_0: temperatures[0] = temperature; break;
              case SourceNCT6776F.CPUTIN: temperatures[1] = temperature; break;
              case SourceNCT6776F.AUXTIN: temperatures[2] = temperature; break;
              case SourceNCT6776F.SYSTIN: temperatures[3] = temperature; break;              
            } break;
        }  
      }

      for (int i = 0; i < fans.Length; i++) {
        byte high = ReadByte(FAN_RPM_REG[i]);
        byte low = ReadByte((ushort)(FAN_RPM_REG[i] + 1));
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

      ushort[] addresses = new ushort[] { 
        0x000, 0x010, 0x020, 0x030, 0x040, 0x050, 0x060, 0x070,
        0x100, 0x110, 0x120, 0x130, 0x140, 0x150, 
        0x200,        0x220, 0x230, 0x240, 0x250,
        0x300,        0x320, 0x330, 0x340, 
        0x400, 0x410, 0x420,        0x440, 0x450, 0x460, 
        0x500,                             0x550, 
        0x600, 0x610 ,0x620, 0x630, 0x640, 0x650, 0x660, 0x670, 
        0xA00, 0xA10, 0xA20, 0xA30,        0xA50, 0xA60, 0xA70, 
        0xB00, 0xB10, 0xB20, 0xB30,        0xB50, 0xB60, 0xB70, 
        0xC00, 0xC10, 0xC20, 0xC30,        0xC50, 0xC60, 0xC70,
        0xD00, 0xD10, 0xD20, 0xD30,        0xD50, 0xD60, 
        0xE00, 0xE10, 0xE20, 0xE30, 
        0xF00, 0xF10, 0xF20, 0xF30};

      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      foreach (ushort address in addresses) {
          r.Append(" ");
          r.Append(address.ToString("X3", CultureInfo.InvariantCulture));
          r.Append("  ");
          for (ushort j = 0; j <= 0xF; j++) {
            r.Append(" ");
            r.Append(ReadByte((ushort)(address | j)).ToString(
              "X2", CultureInfo.InvariantCulture));
          }
          r.AppendLine();
      }
      r.AppendLine();

      Ring0.ReleaseIsaBusMutex();

      return r.ToString();
    }
  }
}
