
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {

  public enum Chip : ushort {
    Unknown = 0,
    IT8712F = 0x8712,
    IT8716F = 0x8716,
    IT8718F = 0x8718,
    IT8720F = 0x8720,
    IT8726F = 0x8726,
    W83627DHG = 0xA020,
    W83627DHGP = 0xB070,
    W83627EHF = 0x8860,    
    W83627HF = 0x5200,
    W83627THF = 0x8283,
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

}
