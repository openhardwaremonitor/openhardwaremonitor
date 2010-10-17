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
  internal class F718XX : ISuperIO {

    private readonly ushort address;
    private readonly Chip chip;

    private readonly float?[] voltages;
    private readonly float?[] temperatures;
    private readonly float?[] fans;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte VOLTAGE_BASE_REG = 0x20;
    private const byte TEMPERATURE_CONFIG_REG = 0x69;
    private const byte TEMPERATURE_BASE_REG = 0x70;
    private readonly byte[] FAN_TACHOMETER_REG = 
      new byte[] { 0xA0, 0xB0, 0xC0, 0xD0 };
    
    private byte ReadByte(byte register) {
      WinRing0.WriteIoPortByte(
        (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte((ushort)(address + DATA_REGISTER_OFFSET));
    }

    public byte? ReadGPIO(int index) {
      return null;
    }

    public void WriteGPIO(int index, byte value) { }

    public F718XX(Chip chip, ushort address) {
      this.address = address;
      this.chip = chip;

      voltages = new float?[chip == Chip.F71858 ? 3 : 9];
      temperatures = new float?[3];
      fans = new float?[chip == Chip.F71882 || chip == Chip.F71858? 4 : 3];
    }

    public Chip Chip { get { return chip; } }
    public float?[] Voltages { get { return voltages; } }
    public float?[] Temperatures { get { return temperatures; } }
    public float?[] Fans { get { return fans; } }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Base Adress: 0x"); 
      r.AppendLine(address.ToString("X4", CultureInfo.InvariantCulture));
      r.AppendLine();

      if (!WinRing0.WaitIsaBusMutex(100))
        return r.ToString();

      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0xF; i++) {
        r.Append(" "); 
        r.Append((i << 4).ToString("X2", CultureInfo.InvariantCulture)); 
        r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte((byte)((i << 4) | j)).ToString("X2", 
            CultureInfo.InvariantCulture));
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
        int value = ReadByte((byte)(VOLTAGE_BASE_REG + i));
        voltages[i] = 0.008f * value;
      }
     
      for (int i = 0; i < temperatures.Length; i++) {
        switch (chip) {
          case Chip.F71858: {
              int tableMode = 0x3 & ReadByte(TEMPERATURE_CONFIG_REG);
              int high = 
                ReadByte((byte)(TEMPERATURE_BASE_REG + 2 * i));
              int low =
                ReadByte((byte)(TEMPERATURE_BASE_REG + 2 * i + 1));              
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
                temperatures[i] = value / 128.0f;
              } else {
                temperatures[i] = null;
              }
          } break;
          default: {
            sbyte value = (sbyte)ReadByte((byte)(
              TEMPERATURE_BASE_REG + 2 * (i + 1)));            
            if (value < sbyte.MaxValue && value > 0)
              temperatures[i] = value;
            else
              temperatures[i] = null;
          } break;
        }
      }

      for (int i = 0; i < fans.Length; i++) {
        int value = ReadByte(FAN_TACHOMETER_REG[i]) << 8;
        value |= ReadByte((byte)(FAN_TACHOMETER_REG[i] + 1));

        if (value > 0) 
          fans[i] = (value < 0x0fff) ? 1.5e6f / value : 0;
        else 
          fans[i] = null;        
      }

      WinRing0.ReleaseIsaBusMutex();
    }
  }
}
