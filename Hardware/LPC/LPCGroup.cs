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
    private ushort[] REGISTER_PORTS = new ushort[] { 0x2e, 0x4e };
    private ushort[] VALUE_PORTS = new ushort[] { 0x2f, 0x4f };

    private ushort registerPort;
    private ushort valuePort;

    // Registers
    private const byte CONFIGURATION_CONTROL_REGISTER = 0x02;
    private const byte DEVCIE_SELECT_REGISTER = 0x07;
    private const byte CHIP_ID_REGISTER = 0x20;
    private const byte CHIP_REVISION_REGISTER = 0x21;
    private const byte BASE_ADDRESS_REGISTER = 0x60;

    private byte ReadByte(byte register) {
      WinRing0.WriteIoPortByte(registerPort, register);
      return WinRing0.ReadIoPortByte(valuePort);
    } 

    private ushort ReadWord(byte register) {
      return (ushort)((ReadByte(register) << 8) | 
        ReadByte((byte)(register + 1)));
    }

    private void Select(byte logicalDeviceNumber) {
      WinRing0.WriteIoPortByte(registerPort, DEVCIE_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(valuePort, logicalDeviceNumber);
    }

    private const byte IT87_ENVIRONMENT_CONTROLLER_LDN = 0x04;    

    private void IT87Enter() {
      WinRing0.WriteIoPortByte(registerPort, 0x87);
      WinRing0.WriteIoPortByte(registerPort, 0x01);
      WinRing0.WriteIoPortByte(registerPort, 0x55);
      WinRing0.WriteIoPortByte(registerPort, 0x55);
    }

    internal void IT87Exit() {
      WinRing0.WriteIoPortByte(registerPort, CONFIGURATION_CONTROL_REGISTER);
      WinRing0.WriteIoPortByte(valuePort, 0x02);
    }

    // Winbond, Fintek
    private const byte FINTEK_VENDOR_ID_REGISTER = 0x23;
    private const ushort FINTEK_VENDOR_ID = 0x1934;

    private const byte W83627DHG_HARDWARE_MONITOR_LDN = 0x0B;

    private const byte F71858_HARDWARE_MONITOR_LDN = 0x02;
    private const byte FINTEK_HARDWARE_MONITOR_LDN = 0x04;

    private void WinbondFintekEnter() {
      WinRing0.WriteIoPortByte(registerPort, 0x87);
      WinRing0.WriteIoPortByte(registerPort, 0x87);
    }

    private void WinbondFintekExit() {
      WinRing0.WriteIoPortByte(registerPort, 0xAA);      
    }

    public LPCGroup() {
      if (!WinRing0.IsAvailable)
        return;

      for (int i = 0; i < REGISTER_PORTS.Length; i++) {
        registerPort = REGISTER_PORTS[i];
        valuePort = VALUE_PORTS[i];

        WinbondFintekEnter();

        byte logicalDeviceNumber;
        byte id = ReadByte(CHIP_ID_REGISTER);
        byte revision = ReadByte(CHIP_REVISION_REGISTER);
        switch (id) {
          case 0x05:
            switch (revision) {
              case 0x41:
                chip = Chip.F71882;
                logicalDeviceNumber = FINTEK_HARDWARE_MONITOR_LDN;
                break;
              default:
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;
          case 0x06:
            switch (revision) {             
              case 0x01:
                chip = Chip.F71862;
                logicalDeviceNumber = FINTEK_HARDWARE_MONITOR_LDN;
                break;
              default:
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;
          case 0x07:
            switch (revision) {
              case 0x23:
                chip = Chip.F71889;
                logicalDeviceNumber = FINTEK_HARDWARE_MONITOR_LDN;
                break;
              default:
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;
          case 0x08:
            switch (revision) {
              case 0x14:
                chip = Chip.F71869;
                logicalDeviceNumber = FINTEK_HARDWARE_MONITOR_LDN;
                break;
              default:
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;
          case 0xA0:
            switch (revision & 0xF0) {
              case 0x20: 
                chip = Chip.W83627DHG;
                logicalDeviceNumber = W83627DHG_HARDWARE_MONITOR_LDN;  
                break;
              default: 
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;
          case 0xB0:
            switch (revision & 0xF0) {
              case 0x70:
                chip = Chip.W83627DHGP;
                logicalDeviceNumber = W83627DHG_HARDWARE_MONITOR_LDN;
                break;
              default:
                chip = Chip.Unknown;
                logicalDeviceNumber = 0;
                break;
            } break;  
          default:
            chip = Chip.Unknown; 
            logicalDeviceNumber = 0;
            break;
        }
        if (chip != Chip.Unknown) {

          Select(logicalDeviceNumber);
          ushort address = ReadWord(BASE_ADDRESS_REGISTER);
          Thread.Sleep(1);
          ushort verify = ReadWord(BASE_ADDRESS_REGISTER);

          ushort vendorID = 0;
          if (chip == Chip.F71862 || chip == Chip.F71882 || chip == Chip.F71889)
            vendorID = ReadWord(FINTEK_VENDOR_ID_REGISTER);

          WinbondFintekExit();

          if (address != verify || address == 0 || (address & 0xF007) != 0)
            return;
          
          switch (chip) {
            case Chip.W83627DHG:
            case Chip.W83627DHGP:
              W83627DHG w83627dhg = new W83627DHG(chip, revision, address);
              if (w83627dhg.IsAvailable)
                hardware.Add(w83627dhg);
              break;
            case Chip.F71862:
            case Chip.F71882:
            case Chip.F71889: 
              if (vendorID == FINTEK_VENDOR_ID)
                hardware.Add(new F718XX(chip, address));
              break;
            case Chip.F71869:
              hardware.Add(new F718XX(chip, address));
              break;
            default: break;
          }
          
          return;
        }

        IT87Enter();

        switch (ReadWord(CHIP_ID_REGISTER)) {
          case 0x8716: chip = Chip.IT8716; break;
          case 0x8718: chip = Chip.IT8718; break;
          case 0x8720: chip = Chip.IT8720; break;
          case 0x8726: chip = Chip.IT8726; break;
          default: chip = Chip.Unknown; break;
        }

        if (chip != Chip.Unknown) {
          Select(IT87_ENVIRONMENT_CONTROLLER_LDN);
          ushort address = ReadWord(BASE_ADDRESS_REGISTER);
          Thread.Sleep(1);
          ushort verify = ReadWord(BASE_ADDRESS_REGISTER);

          IT87Exit();

          if (address != verify || address == 0 || (address & 0xF007) != 0)
            return;

          IT87XX it87 = new IT87XX(chip, address);
          if (it87.IsAvailable)
            hardware.Add(it87);

          return;
        }
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
