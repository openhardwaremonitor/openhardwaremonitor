
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {

  internal enum Chip : ushort {
    Unknown = 0,
    IT8712F = 0x8712,
    IT8716F = 0x8716,
    IT8718F = 0x8718,
    IT8720F = 0x8720,
    IT8726F = 0x8726,
    W83627DHG = 0xA020,
    W83627DHGP = 0xB070,
    W83627EHF = 0x8800,    
    W83627HF = 0x5200,
    W83627THF = 0x8280,
    W83667HG = 0xA510,
    W83667HGB = 0xB350,
    W83687THF = 0x8541,
    F71858 = 0x0507,
    F71862 = 0x0601, 
    F71869 = 0x0814,
    F71882 = 0x0541,
    F71889ED = 0x0909,
    F71889F = 0x0723      
  }

  internal class ChipName {

    private ChipName() { }

    public static string GetName(Chip chip) {
      switch (chip) {
        case Chip.F71858: return "Fintek F71858";
        case Chip.F71862: return "Fintek F71862";
        case Chip.F71869: return "Fintek F71869";
        case Chip.F71882: return "Fintek F71882";
        case Chip.F71889ED: return "Fintek F71889ED";
        case Chip.F71889F: return "Fintek F71889F";
        case Chip.IT8712F: return "ITE IT8712F";
        case Chip.IT8716F: return "ITE IT8716F";
        case Chip.IT8718F: return "ITE IT8718F";
        case Chip.IT8720F: return "ITE IT8720F";
        case Chip.IT8726F: return "ITE IT8726F";
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
