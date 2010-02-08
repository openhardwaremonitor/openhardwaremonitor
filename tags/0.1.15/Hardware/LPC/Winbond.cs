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
  public abstract class Winbond : LPCHardware {

    private ushort address;
    private byte revision;

    private bool available;

    // Consts 
    private const ushort WINBOND_VENDOR_ID = 0x5CA3;
    private const byte HIGH_BYTE = 0x80;

    // Hardware Monitor
    private const byte ADDRESS_REGISTER_OFFSET = 0x05;
    private const byte DATA_REGISTER_OFFSET = 0x06;

    // Hardware Monitor Registers
    private const byte BANK_SELECT_REGISTER = 0x04E;
    private const byte VENDOR_ID_REGISTER = 0x4F;

    protected byte ReadByte(byte bank, byte register) {
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), BANK_SELECT_REGISTER);
      WinRing0.WriteIoPortByte(
         (ushort)(address + DATA_REGISTER_OFFSET), bank);
      WinRing0.WriteIoPortByte(
         (ushort)(address + ADDRESS_REGISTER_OFFSET), register);
      return WinRing0.ReadIoPortByte(
        (ushort)(address + DATA_REGISTER_OFFSET));
    } 

    private bool IsWinbondVendor() {
      ushort vendorId =
        (ushort)((ReadByte(HIGH_BYTE, VENDOR_ID_REGISTER) << 8) |
           ReadByte(0, VENDOR_ID_REGISTER));
      return vendorId == WINBOND_VENDOR_ID;
    }

    public Winbond(Chip chip, byte revision, ushort address)
      : base(chip) 
    {
      this.address = address;
      this.revision = revision;

      available = IsWinbondVendor();
    }

    public bool IsAvailable {
      get { return available; }
    }    
   
    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC " + this.GetType().Name);
      r.AppendLine();
      r.Append("Chip ID: 0x"); r.AppendLine(chip.ToString("X"));
      r.Append("Chip revision: 0x"); r.AppendLine(revision.ToString("X"));
      r.Append("Base Adress: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();
      r.AppendLine("Hardware Monitor Registers");
      r.AppendLine();
      r.AppendLine("      00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i < 0x7; i++) {
        r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          r.Append(" ");
          r.Append(ReadByte(0, (byte)((i << 4) | j)).ToString("X2"));
        }
        r.AppendLine();
      }
      for (int k = 1; k <= 5; k++) {
        r.AppendLine("Bank " + k);
        for (int i = 0x5; i < 0x6; i++) {
          r.Append(" "); r.Append((i << 4).ToString("X2")); r.Append("  ");
          for (int j = 0; j <= 0xF; j++) {
            r.Append(" ");
            r.Append(ReadByte((byte)(k),
              (byte)((i << 4) | j)).ToString("X2"));
          }
          r.AppendLine();
        }
      }
      r.AppendLine();

      return r.ToString();
    }
  }
}
