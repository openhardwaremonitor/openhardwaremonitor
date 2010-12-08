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
  Portions created by the Initial Developer are Copyright (C) 2010
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
  internal class NCT677X : ISuperIO {

    private readonly ushort port;
    private readonly byte revision;

    private readonly Chip chip;

    private readonly float?[] voltages = new float?[9];
    private readonly float?[] temperatures = new float?[3];
    private readonly float?[] fans = new float?[4];

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

    // Consts 
    private const ushort NUVOTON_VENDOR_ID = 0x5CA3;

    // Hardware Monitor Registers    
    private const ushort VENDOR_ID_HIGH_REGISTER = 0x804F;
    private const ushort VENDOR_ID_LOW_REGISTER = 0x004F;
    private const ushort VOLTAGE_VBAT_REG = 0x0551;

    private readonly ushort[] TEMPERATURE_REG = 
      { 0x150, 0x250, 0x27, 0x62B, 0x62C, 0x62D };
    private readonly ushort[] TEMPERATURE_HALF_REG = 
      { 0x151, 0x251, 0, 0x62E, 0x62E, 0x62E };    
    private readonly ushort[] TEMPERATURE_SRC_REG = 
      { 0x621, 0x622, 0x623, 0x624, 0x625, 0x626 };
    private readonly int[] TEMPERATURE_HALF_BIT = { 7, 7, -1, 0, 1, 2 };
    private readonly ushort[] VOLTAGE_REG = 
      { 0x20, 0x21, 0x22, 0x23, 0x24, 0x25, 0x26, 0x550, 0x551 };
    private readonly ushort[] FAN_RPM_REG = { 0x656, 0x658, 0x65A, 0x65C};

    private enum TemperatureSource : byte {
      SYSTIN = 1,
      CPUTIN = 2,
      AUXTIN = 3,
      SMBUSMASTER = 4,
      PECI0 = 5, 
      PECI1 = 6, 
      PECI2 = 7,
      PECI3 = 8,
      PECI4 = 9,
      PECI5 = 10,
      PECI6 = 11,
      PECI7 = 12,
      PCH_CHIP_CPU_MAX_TEMP = 13,
      PCH_CHIP_TEMP = 14,
      PCH_CPU_TEMP = 15,
      PCH_MCH_TEMP = 16, 
      PCH_DIM0_TEMP = 17,
      PCH_DIM1_TEMP = 18,
      PCH_DIM2_TEMP = 19,
      PCH_DIM3_TEMP = 20
    }

    public NCT677X(Chip chip, byte revision, ushort port) {
      this.chip = chip;
      this.revision = revision;
      this.port = port;

      if (!IsNuvotonVendor())
        return;      
    }

    private bool IsNuvotonVendor() {
      return ((ReadByte(VENDOR_ID_HIGH_REGISTER) << 8) |
        ReadByte(VENDOR_ID_LOW_REGISTER)) == NUVOTON_VENDOR_ID;
    }

    public byte? ReadGPIO(int index) {
      return null;
    }

    public void WriteGPIO(int index, byte value) { }

    public Chip Chip { get { return chip; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }

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

      for (int i = 0; i < TEMPERATURE_REG.Length; i++) {
        int value = ((sbyte)ReadByte(TEMPERATURE_REG[i])) << 1;
        if (TEMPERATURE_HALF_BIT[i] > 0) {
          value |= ((ReadByte(TEMPERATURE_HALF_REG[i]) >>
            TEMPERATURE_HALF_BIT[i]) & 0x1);
        }

        TemperatureSource source = (TemperatureSource)
          ReadByte(TEMPERATURE_SRC_REG[i]);

        float? temperature = 0.5f * value;
        if (temperature > 125 || temperature < -55)
          temperature = null;

        switch (source) {
          case TemperatureSource.CPUTIN: temperatures[0] = temperature; break;
          case TemperatureSource.AUXTIN: temperatures[1] = temperature; break;
          case TemperatureSource.SYSTIN: temperatures[2] = temperature; break;
        }
      }

      for (int i = 0; i < fans.Length; i++) {
        byte high = ReadByte(FAN_RPM_REG[i]);
        byte low = ReadByte((ushort)(FAN_RPM_REG[i] + 1));
        fans[i] = (high << 8) | low;
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
