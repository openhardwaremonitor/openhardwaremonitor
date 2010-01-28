
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
    F71882FG = 0x0541
  }

}
