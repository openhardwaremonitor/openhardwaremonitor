/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Paul Werelds <paul@werelds.net>
	
*/

namespace OpenHardwareMonitor.WMI {
  interface IWmiObject {
    // Both of these get exposed to WMI
    string Name { get; }
    string Identifier { get; }

    // Not exposed.
    void Update();
  }
}
