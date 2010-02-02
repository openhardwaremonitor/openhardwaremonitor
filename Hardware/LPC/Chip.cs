
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {

  public enum Chip : ushort {
    Unknown = 0,
    IT8716F = 0x8716,
    IT8718F = 0x8718,
    IT8720F = 0x8720,
    IT8726F = 0x8726,
    W83627DHG = 0xA020,
    W83627DHGP = 0xB070,
    F71862 = 0x0601, 
    F71869 = 0x0814,
    F71882 = 0x0541,
    F71889 = 0x0723 
  }

}
