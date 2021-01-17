/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Drawing;

namespace OpenHardwareMonitor.GUI {

  public static class DpiHelper {
    public const double LogicalDpi = 96.0;

    private static double deviceDpi;
    public static double DeviceDpi {
      get {
        if (deviceDpi == 0.0) {
          try {
            using (Graphics g = Graphics.FromHwnd(IntPtr.Zero)) {
              deviceDpi = g.DpiX;
            }
          } catch { }
          if (deviceDpi == 0.0)
            deviceDpi = LogicalDpi;
        }
        return deviceDpi;
      }
    }

    private static double logicalToDeviceUnitsScalingFactor;
    public static double LogicalToDeviceUnitsScalingFactor {
      get {
        if (logicalToDeviceUnitsScalingFactor == 0.0) {
          logicalToDeviceUnitsScalingFactor = DeviceDpi / LogicalDpi;
        }
        return logicalToDeviceUnitsScalingFactor;
      }
    }

    public static int LogicalToDeviceUnits(int value) {
      return (int)Math.Round(LogicalToDeviceUnitsScalingFactor * (double)value);
    }

    public static Size LogicalToDeviceUnits(Size logicalSize) {
      return new Size(LogicalToDeviceUnits(logicalSize.Width),
        LogicalToDeviceUnits(logicalSize.Height));
    }
  }
}
