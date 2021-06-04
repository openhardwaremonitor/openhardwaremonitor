/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Text;

namespace OpenHardwareMonitor.Utilities {

  public class HttpUtility {
    public static string UrlEncode(string s) {
 
      int maxLength = 32765;
      var sb = new StringBuilder();
      int imax = s.Length / maxLength;

      for (int i = 0; i <= imax; i++) {
        sb.Append(
          Uri.EscapeDataString(i < imax
          ? s.Substring(maxLength * i, maxLength)
          : s.Substring(maxLength * i)));
      }

      return sb.ToString();
    }

  }
}
