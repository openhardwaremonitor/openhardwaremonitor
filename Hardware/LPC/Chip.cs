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
  Portions created by the Initial Developer are Copyright (C) 2009-2011
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

namespace OpenHardwareMonitor.Hardware.LPC {

  internal enum Chip : ushort {
    Unknown = 0,

    ATK0110 = 0x0110,

    F71858 = 0x0507,
    F71862 = 0x0601, 
    F71869 = 0x0814,
    F71882 = 0x0541,
    F71889AD = 0x1005,
    F71889ED = 0x0909,
    F71889F = 0x0723,

    IT8712F = 0x8712,
    IT8716F = 0x8716,
    IT8718F = 0x8718,
    IT8720F = 0x8720,
    IT8721F = 0x8721,
    IT8726F = 0x8726,
    IT8728F = 0x8728,
    IT8772E = 0x8772,

    NCT6771F = 0xB470,
    NCT6776F = 0xC330,

    W83627DHG = 0xA020,
    W83627DHGP = 0xB070,
    W83627EHF = 0x8800,    
    W83627HF = 0x5200,
    W83627THF = 0x8280,
    W83667HG = 0xA510,
    W83667HGB = 0xB350,
    W83687THF = 0x8541
  }

  internal class ChipName {

    private ChipName() { }

    public static string GetName(Chip chip) {
      switch (chip) {
        case Chip.ATK0110: return "Asus ATK0110";

        case Chip.F71858: return "Fintek F71858";
        case Chip.F71862: return "Fintek F71862";
        case Chip.F71869: return "Fintek F71869";
        case Chip.F71882: return "Fintek F71882";
        case Chip.F71889AD: return "Fintek F71889AD";
        case Chip.F71889ED: return "Fintek F71889ED";
        case Chip.F71889F: return "Fintek F71889F";

        case Chip.IT8712F: return "ITE IT8712F";
        case Chip.IT8716F: return "ITE IT8716F";
        case Chip.IT8718F: return "ITE IT8718F";        
        case Chip.IT8720F: return "ITE IT8720F";
        case Chip.IT8721F: return "ITE IT8721F";
        case Chip.IT8726F: return "ITE IT8726F";
        case Chip.IT8728F: return "ITE IT8728F";
        case Chip.IT8772E: return "ITE IT8772E";

        case Chip.NCT6771F: return "Nuvoton NCT6771F";
        case Chip.NCT6776F: return "Nuvoton NCT6776F";

        case Chip.W83627DHG: return "Winbond W83627DHG";
        case Chip.W83627DHGP: return "Winbond W83627DHG-P";
        case Chip.W83627EHF: return "Winbond W83627EHF";
        case Chip.W83627HF: return "Winbond W83627HF";
        case Chip.W83627THF: return "Winbond W83627THF";
        case Chip.W83667HG: return "Winbond W83667HG";
        case Chip.W83667HGB: return "Winbond W83667HG-B";
        case Chip.W83687THF: return "Winbond W83687THF";

        case Chip.Unknown: return "Unkown";
        default: return "Unknown";
      }
    }
  }

}
