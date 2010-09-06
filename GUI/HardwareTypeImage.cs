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
  Portions created by the Initial Developer are Copyright (C) 2010
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
using System.Drawing;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI {
  public class HardwareTypeImage {
    private static HardwareTypeImage instance = new HardwareTypeImage();

    private IDictionary<HardwareType, Image> images = 
      new Dictionary<HardwareType, Image>();

    private HardwareTypeImage() { }

    public static HardwareTypeImage Instance {
      get { return instance; }
    }

    public Image GetImage(HardwareType hardwareType) {
      Image image;
      if (images.TryGetValue(hardwareType, out image)) {
        return image;
      } else {
        switch (hardwareType) {
          case HardwareType.CPU:
            image = Utilities.EmbeddedResources.GetImage("cpu.png");
            break;
          case HardwareType.GpuNvidia:
            image = Utilities.EmbeddedResources.GetImage("nvidia.png");
            break;
          case HardwareType.GpuAti:
            image = Utilities.EmbeddedResources.GetImage("ati.png");
            break;
          case HardwareType.HDD:
            image = Utilities.EmbeddedResources.GetImage("hdd.png");
            break;
          case HardwareType.Heatmaster:
            image = Utilities.EmbeddedResources.GetImage("bigng.png");
            break;
          case HardwareType.Mainboard:
            image = Utilities.EmbeddedResources.GetImage("mainboard.png");
            break;
          case HardwareType.SuperIO:
            image = Utilities.EmbeddedResources.GetImage("chip.png");
            break;
          case HardwareType.TBalancer:
            image = Utilities.EmbeddedResources.GetImage("bigng.png");
            break;
          default:
            image = new Bitmap(1, 1);
            break;
        }
        images.Add(hardwareType, image);
        return image;
      }
    }
  }
}
