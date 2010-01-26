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
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.LPC {
  public class LPCGroup : IGroup {
    private List<IHardware> hardware = new List<IHardware>();

    private Chip chip = Chip.Unknown;

    // I/O Ports
    private const ushort REGISTER_PORT = 0x2e;
    private const ushort VALUE_PORT = 0x2f;

    // Registers
    private const byte CONFIGURATION_CONTROL_REGISTER = 0x02;
    private const byte DEVCIE_SELECT_REGISTER = 0x07;
    private const byte CHIP_ID_REGISTER = 0x20;
    private const byte CHIP_REVISION_REGISTER = 0x21;

    private static byte ReadByte(byte register) {
      WinRing0.WriteIoPortByte(REGISTER_PORT, register);
      return WinRing0.ReadIoPortByte(VALUE_PORT);
    }

    private static ushort ReadWord(byte register) {
      ushort value;
      WinRing0.WriteIoPortByte(REGISTER_PORT, register);
      value = (ushort)(((ushort)WinRing0.ReadIoPortByte(VALUE_PORT)) << 8);
      WinRing0.WriteIoPortByte(REGISTER_PORT, (byte)(register + 1));
      value |= (ushort)WinRing0.ReadIoPortByte(VALUE_PORT);
      return value;
    }

    private static void Select(byte logicalDeviceNumber) {
      WinRing0.WriteIoPortByte(REGISTER_PORT, DEVCIE_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(VALUE_PORT, logicalDeviceNumber);
    }

    // IT87
    private const ushort IT8716F_CHIP_ID = 0x8716;
    private const ushort IT8718F_CHIP_ID = 0x8718;
    private const ushort IT8720F_CHIP_ID = 0x8720;
    private const ushort IT8726F_CHIP_ID = 0x8726;

    private const byte IT87_ENVIRONMENT_CONTROLLER_LDN = 0x04;
    private const byte IT87_ENVIRONMENT_CONTROLLER_BASE_ADDR_REG = 0x60;

    private static void IT87Enter() {
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x87);
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x01);
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x55);
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x55);
    }

    internal static void IT87Exit() {
      WinRing0.WriteIoPortByte(REGISTER_PORT, CONFIGURATION_CONTROL_REGISTER);
      WinRing0.WriteIoPortByte(VALUE_PORT, 0x02);
    }

    // Winbond
    private static void WinbondEnter() {
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x87);
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0x87);
    }

    private static void WinbondExit() {
      WinRing0.WriteIoPortByte(REGISTER_PORT, 0xAA);      
    }

    public LPCGroup() {
      if (!WinRing0.IsAvailable)
        return;

      WinbondEnter();

      byte id = ReadByte(CHIP_ID_REGISTER);
      byte revision = ReadByte(CHIP_REVISION_REGISTER);
      switch (id) {
        case 0xA0:
          switch (revision & 0xF0) {
            case 0x20: chip = Chip.W83627DHG; break;
            default: chip = Chip.Unknown; break;
          } break;
        default: chip = Chip.Unknown; break;
      }
      if (chip != Chip.Unknown) {

        WinbondExit();

        W83627DHG w83627dhg = new W83627DHG(revision);
        if (w83627dhg.IsAvailable)
          hardware.Add(w83627dhg);
        return;
      }

      IT87Enter();

      switch (ReadWord(CHIP_ID_REGISTER)) {
        case 0x8716: chip = Chip.IT8716F; break;
        case 0x8718: chip = Chip.IT8718F; break;
        case 0x8720: chip = Chip.IT8720F; break;
        case 0x8726: chip = Chip.IT8726F; break;
        default: chip = Chip.Unknown; break;
      }

      if (chip != Chip.Unknown) {        
        Select(IT87_ENVIRONMENT_CONTROLLER_LDN);
        ushort address = ReadWord(IT87_ENVIRONMENT_CONTROLLER_BASE_ADDR_REG);
        Thread.Sleep(1);
        ushort verify = ReadWord(IT87_ENVIRONMENT_CONTROLLER_BASE_ADDR_REG);

        IT87Exit();

        if (address != verify || address == 0 || (address & 0xF007) != 0)
          return;

        IT87 it87 = new IT87(chip, address);
        if (it87.IsAvailable)
          hardware.Add(it87);
        
        return;
      }                
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      return null;
    }

    public void Close() { }
  }
}
