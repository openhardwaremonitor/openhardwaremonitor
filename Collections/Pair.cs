/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

namespace OpenHardwareMonitor.Collections
{
    public struct Pair<F, S>
    {
        public Pair(F first, S second)
        {
            First = first;
            Second = second;
        }

        public F First { get; set; }

        public S Second { get; set; }

        public override int GetHashCode()
        {
            return (First != null ? First.GetHashCode() : 0) ^
                   (Second != null ? Second.GetHashCode() : 0);
        }
    }
}