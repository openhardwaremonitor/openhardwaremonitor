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
using System.Collections.Generic;
using System.Management;
using System.Text;

namespace OpenHardwareMonitor.Hardware.SMBIOS {

  public class SMBIOSGroup : IGroup {

    private Structure[] table;

    private BIOSInformation biosInformation = null;

    private BaseBoardInformation baseBoardInformation = null;

    public SMBIOSGroup() {
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;
      
      List<Structure> structureList = new List<Structure>();

      try {
        ManagementObjectCollection collection = new ManagementObjectSearcher(
          "root\\WMI", "SELECT SMBiosData FROM MSSMBios_RawSMBiosTables").Get();

        byte[] raw = null;
        foreach (ManagementObject mo in collection) {
          raw = (byte[])mo["SMBiosData"];
          break;
        }

        if (raw != null && raw.Length > 0) {
          int offset = 0;
          byte type = raw[offset];
          while (offset < raw.Length && type != 127) {

            type = raw[offset]; offset++;
            int length = raw[offset]; offset++;
            ushort handle = (ushort)((raw[offset] << 8) | raw[offset + 1]);
            offset += 2;

            byte[] data = new byte[length];
            Array.Copy(raw, offset - 4, data, 0, length); offset += length - 4;

            List<string> stringsList = new List<string>();
            if (raw[offset] == 0)
              offset++;
            while (raw[offset] != 0) {
              StringBuilder sb = new StringBuilder();
              while (raw[offset] != 0) {
                sb.Append((char)raw[offset]); offset++;
              }
              offset++;
              stringsList.Add(sb.ToString());
            }
            offset++;
            switch (type) {
              case 0x00:
                this.biosInformation = new BIOSInformation(
                  type, handle, data, stringsList.ToArray());
                structureList.Add(this.biosInformation); break;
              case 0x02: this.baseBoardInformation = new BaseBoardInformation(
                  type, handle, data, stringsList.ToArray());
                structureList.Add(this.baseBoardInformation); break;
              default: structureList.Add(new Structure(
                type, handle, data, stringsList.ToArray())); break;
            }
          }
        }
      } catch (NotImplementedException) { } catch (ManagementException) { }
      
      table = structureList.ToArray();
    }

    public IHardware[] Hardware { get { return new IHardware[0]; } }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("SMBIOS");
      r.AppendLine();

      if (biosInformation != null) {
        r.Append("BIOS Vendor: "); r.AppendLine(biosInformation.Vendor);
        r.Append("BIOS Version: "); r.AppendLine(biosInformation.Version);
        r.AppendLine();
      }

      if (baseBoardInformation != null) {
        r.Append("Mainboard Manufacturer: "); 
        r.AppendLine(baseBoardInformation.Manufacturer);
        r.Append("Mainboard Name: "); 
        r.AppendLine(baseBoardInformation.ProductName);
        r.AppendLine();
      }
     
      return r.ToString();
    }

    public void Close() { }

    public class Structure {
      private byte type;
      private ushort handle;

      private byte[] data;
      private string[] strings;

      protected string GetString(int offset) {
        if (offset < data.Length && data[offset] > 0 &&
         data[offset] <= strings.Length)
          return strings[data[offset] - 1];
        else
          return "";
      }

      public Structure(byte type, ushort handle, byte[] data, string[] strings) 
      {
        this.type = type;
        this.handle = handle;
        this.data = data;
        this.strings = strings;
      }

      public byte Type { get { return type; } }

      public ushort Handle { get { return handle; } }
    }

    public class BIOSInformation : Structure {

      private string vendor;
      private string version;

      public BIOSInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) {

        this.vendor = GetString(0x04);
        this.version = GetString(0x05);
      }

      public string Vendor { get { return vendor; } }

      public string Version { get { return version; } }
    }

    public class BaseBoardInformation : Structure {

      private string manufacturer;
      private string productName;

      public BaseBoardInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) {

        this.manufacturer = GetString(0x04);
        this.productName = GetString(0x05);
      }

      public string Manufacturer { get { return manufacturer; } }

      public string ProductName { get { return productName; } }
    }
  }
}
