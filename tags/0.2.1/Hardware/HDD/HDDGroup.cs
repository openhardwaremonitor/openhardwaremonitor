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
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s): Paul Werelds

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
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal class HDDGroup : IGroup {

    private const int MAX_DRIVES = 32;

    private readonly List<HDD> hardware = new List<HDD>();

    public HDDGroup(ISettings settings) {
      int p = (int)Environment.OSVersion.Platform;
      if (p == 4 || p == 128) return;

      for (int drive = 0; drive < MAX_DRIVES; drive++) {
        IntPtr handle = SMART.OpenPhysicalDrive(drive);

        if (handle == SMART.INVALID_HANDLE_VALUE)
          continue;

        if (!SMART.EnableSmart(handle, drive)) {
          SMART.CloseHandle(handle);
          continue;
        }

        string name = SMART.ReadName(handle, drive);
        if (name == null) {
          SMART.CloseHandle(handle);
          continue;
        }

        SMART.DriveAttribute[] attributes = SMART.ReadSmart(handle, drive);
        if (attributes == null) {
          SMART.CloseHandle(handle);
          continue;
        }

        int attribute = -1;

        // search for the Temperature attribute
        for (int i = 0; i < attributes.Length; i++)
          if (attributes[i].ID == SMART.AttributeID.Temperature) {
            attribute = i;
            break;
          }

        // if no temperature attribute is found, search for DriveTemperature
        if (attribute == -1)
          for (int i = 0; i < attributes.Length; i++)
            if (attributes[i].ID == SMART.AttributeID.DriveTemperature) {
              attribute = i;
              break;
            }

        // if no temperature attribute is found, search for AirflowTemperature
        if (attribute == -1)
          for (int i = 0; i < attributes.Length; i++)
            if (attributes[i].ID == SMART.AttributeID.AirflowTemperature) {
              attribute = i;
              break;
            }

        if (attribute >= 0) {
          hardware.Add(new HDD(name, handle, drive, attribute, settings));
          continue;
        }

        SMART.CloseHandle(handle);
      }
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      int p = (int)Environment.OSVersion.Platform;
      if (p == 4 || p == 128) return null;

      StringBuilder r = new StringBuilder();

      r.AppendLine("S.M.A.R.T Data");
      r.AppendLine();

      for (int drive = 0; drive < MAX_DRIVES; drive++) {
        IntPtr handle = SMART.OpenPhysicalDrive(drive);

        if (handle == SMART.INVALID_HANDLE_VALUE)
          continue;

        if (!SMART.EnableSmart(handle, drive)) {
          SMART.CloseHandle(handle);
          continue;
        }

        string name = SMART.ReadName(handle, drive);
        if (name == null) {
          SMART.CloseHandle(handle);
          continue;
        }

        SMART.DriveAttribute[] attributes = SMART.ReadSmart(handle, drive);

        if (attributes != null) {
          r.AppendLine("Drive name: " + name);
          r.AppendLine();
          r.AppendFormat(CultureInfo.InvariantCulture, " {0}{1}{2}{3}{4}{5}",
            ("ID").PadRight(6),
            ("RawValue").PadRight(20),
            ("WorstValue").PadRight(12),
            ("AttrValue").PadRight(12),
            ("Name"),
            Environment.NewLine);

          foreach (SMART.DriveAttribute a in attributes) {
            if (a.ID == 0) continue;
            string raw = BitConverter.ToString(a.RawValue);
            r.AppendFormat(CultureInfo.InvariantCulture, " {0}{1}{2}{3}{4}{5}",
              a.ID.ToString("d").PadRight(6), 
              raw.Replace("-", " ").PadRight(20),
              a.WorstValue.ToString(CultureInfo.InvariantCulture).PadRight(12),
              a.AttrValue.ToString(CultureInfo.InvariantCulture).PadRight(12),
              a.ID,
              Environment.NewLine);
          }
          r.AppendLine();
        }

        SMART.CloseHandle(handle);
      }

      return r.ToString();
    }

    public void Close() {
      foreach (HDD hdd in hardware) 
        hdd.Close();
    }
  }
}
