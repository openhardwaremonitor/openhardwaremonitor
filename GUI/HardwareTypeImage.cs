/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using System.Drawing;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI
{
    public class HardwareTypeImage
    {
        private readonly IDictionary<HardwareType, Image> images =
            new Dictionary<HardwareType, Image>();

        private HardwareTypeImage()
        {
        }

        public static HardwareTypeImage Instance { get; } = new HardwareTypeImage();

        public Image GetImage(HardwareType hardwareType)
        {
            if (images.TryGetValue(hardwareType, out Image image)) return image;
            switch (hardwareType)
            {
                case HardwareType.CPU:
                    image = EmbeddedResources.GetImage("cpu.png");
                    break;
                case HardwareType.GpuNvidia:
                    image = EmbeddedResources.GetImage("nvidia.png");
                    break;
                case HardwareType.GpuAti:
                    image = EmbeddedResources.GetImage("ati.png");
                    break;
                case HardwareType.HDD:
                    image = EmbeddedResources.GetImage("hdd.png");
                    break;
                case HardwareType.Heatmaster:
                    image = EmbeddedResources.GetImage("bigng.png");
                    break;
                case HardwareType.Mainboard:
                    image = EmbeddedResources.GetImage("mainboard.png");
                    break;
                case HardwareType.SuperIO:
                    image = EmbeddedResources.GetImage("chip.png");
                    break;
                case HardwareType.TBalancer:
                    image = EmbeddedResources.GetImage("bigng.png");
                    break;
                case HardwareType.RAM:
                    image = EmbeddedResources.GetImage("ram.png");
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