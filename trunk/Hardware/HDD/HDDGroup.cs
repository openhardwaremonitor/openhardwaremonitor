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
        
        if (attributes.Length < 1) {
          SMART.CloseHandle(handle);
          continue;
        }

        SMART.AttributeID ssdLifeID = GetSSDLifeID(attributes);
        if (ssdLifeID == SMART.AttributeID.None) {
          SMART.AttributeID temperatureID = GetTemperatureIndex(attributes);

          if (temperatureID != SMART.AttributeID.None) {
            hardware.Add(new HDD(name, handle, drive, temperatureID, 
              SMART.AttributeID.None, settings));
            continue;
          }
        } else {
          hardware.Add(new HDD(name, handle, drive, SMART.AttributeID.None, 
            ssdLifeID, settings));
          continue;
        }
        
        SMART.CloseHandle(handle);
      }
    }

    private SMART.AttributeID GetSSDLifeID(SMART.DriveAttribute[] attributes) {
      // ID E9 is present on Intel, JM, SF and Samsung (different meanings)
      // ID D2 is present on Indilinx
      // Neither ID has been found on a mechanical hard drive (yet),
      // so this seems like a good way to check if it's an SSD.
      bool isKnownSSD = (
        Array.Exists(attributes, attr => attr.ID == new SMART.AttributeID(0xE9)) ||
        Array.Exists(attributes, attr => attr.ID == new SMART.AttributeID(0xD2))
      );

      if (!isKnownSSD) return SMART.AttributeID.None;

      // We start with a traditional loop, because there are 4 unique ID's
      // that potentially identify one of the vendors
      for (int i = 0; i < attributes.Length; i++) {
        if (attributes[i].ID == SMART.SamsungAttributes.RemainingLife)
          return SMART.SamsungAttributes.RemainingLife;
        
        if (attributes[i].ID == SMART.SandForceAttributes.ProgramFailCount)
          return  SMART.SandForceAttributes.RemainingLife;
        
        if (attributes[i].ID == SMART.IndilinxAttributes.UnknownUnique)   
          return SMART.IndilinxAttributes.RemainingLife;
      }

      // TODO: Find out JMicron's Life attribute ID; their unique ID = 0xE4

      // For Intel, we make sure we have their 3 most important ID's
      // We do a traditional loop again, because we all we need to know
      // is whether we can find all 3; pointless to use Exists()
      int intelRegisterCount = 0;
      foreach (SMART.DriveAttribute attribute in attributes) {
        if (attribute.ID == SMART.IntelAttributes.HostWrites ||
          attribute.ID == SMART.IntelAttributes.RemainingLife ||
          attribute.ID == SMART.IntelAttributes.MediaWearOutIndicator
        )
          intelRegisterCount++;
      }

      return (intelRegisterCount == 3)
        ? SMART.IntelAttributes.RemainingLife
        : SMART.AttributeID.None;
    }

    private SMART.AttributeID GetTemperatureIndex(
      SMART.DriveAttribute[] attributes)
    {
      SMART.AttributeID[] validIds = new[] {
        SMART.CommonAttributes.Temperature,
        SMART.CommonAttributes.DriveTemperature,
        SMART.CommonAttributes.AirflowTemperature
      };

      foreach (SMART.AttributeID validId in validIds) {
        SMART.AttributeID id = validId;
        if (Array.Exists(attributes, attr => attr.ID == id))
          return validId;
      }

      return SMART.AttributeID.None;
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

        if (attributes.Length > 0) {
          r.AppendLine("Drive name: " + name);
          r.AppendLine();
          r.AppendFormat(CultureInfo.InvariantCulture, " {0}{1}{2}{3}{4}",
            ("ID").PadRight(6),
            ("RawValue").PadRight(20),
            ("WorstValue").PadRight(12),
            ("AttrValue").PadRight(12),
            Environment.NewLine);

          foreach (SMART.DriveAttribute a in attributes) {
            if (a.ID == SMART.AttributeID.None) continue;
            string raw = BitConverter.ToString(a.RawValue);
            r.AppendFormat(CultureInfo.InvariantCulture, " {0}{1}{2}{3}{4}",
              a.ID.ToString("d").PadRight(6), 
              raw.Replace("-", " ").PadRight(20),
              a.WorstValue.ToString(CultureInfo.InvariantCulture).PadRight(12),
              a.AttrValue.ToString(CultureInfo.InvariantCulture).PadRight(12),
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
