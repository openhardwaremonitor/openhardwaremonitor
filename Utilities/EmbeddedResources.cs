/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Drawing;
using System.Reflection;

namespace OpenHardwareMonitor.Utilities
{
    public class EmbeddedResources
    {
        public static Image GetImage(string name)
        {
            name = "OpenHardwareMonitor.Resources." + name;

            var names =
                Assembly.GetExecutingAssembly().GetManifestResourceNames();
            for (var i = 0; i < names.Length; i++)
                if (names[i].Replace('\\', '.') == name)
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]))
                    {
                        // "You must keep the stream open for the lifetime of the Image."
                        var image = Image.FromStream(stream);

                        // so we just create a copy of the image 
                        var bitmap = new Bitmap(image);

                        // and dispose it right here
                        image.Dispose();

                        return bitmap;
                    }

            return new Bitmap(1, 1);
        }

        public static Icon GetIcon(string name)
        {
            name = "OpenHardwareMonitor.Resources." + name;

            var names =
                Assembly.GetExecutingAssembly().GetManifestResourceNames();
            for (var i = 0; i < names.Length; i++)
                if (names[i].Replace('\\', '.') == name)
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(names[i]))
                    {
                        return new Icon(stream);
                    }

            return null;
        }
    }
}