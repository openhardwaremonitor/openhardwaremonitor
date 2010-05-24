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

namespace OpenHardwareMonitor.Hardware.Mainboard {

  public class SMBIOS {

    private Structure[] table;

    private BIOSInformation biosInformation = null;
    private BaseBoardInformation baseBoardInformation = null;

    public SMBIOS() {
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;
      
      List<Structure> structureList = new List<Structure>();

      byte[] raw = null;
      try {
        ManagementObjectCollection collection = new ManagementObjectSearcher(
          "root\\WMI", "SELECT SMBiosData FROM MSSMBios_RawSMBiosTables").Get();
       
        foreach (ManagementObject mo in collection) {
          raw = (byte[])mo["SMBiosData"];
          break;
        }
      } catch { }      

      if (raw != null && raw.Length > 0) {
        int offset = 0;
        byte type = raw[offset];
        while (offset + 4 < raw.Length && type != 127) {

          type = raw[offset];
          int length = raw[offset + 1];
          ushort handle = (ushort)((raw[offset + 2] << 8) | raw[offset + 3]);

          if (offset + length > raw.Length)
            break;
          byte[] data = new byte[length];
          Array.Copy(raw, offset, data, 0, length);
          offset += length;

          List<string> stringsList = new List<string>();
          if (offset < raw.Length && raw[offset] == 0)
            offset++;

          while (offset < raw.Length && raw[offset] != 0) {
            StringBuilder sb = new StringBuilder();
            while (offset < raw.Length && raw[offset] != 0) {
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
            
      table = structureList.ToArray();
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();      

      if (biosInformation != null) {
        r.Append("BIOS Vendor: "); r.AppendLine(biosInformation.Vendor);
        r.Append("BIOS Version: "); r.AppendLine(biosInformation.Version);
        r.AppendLine();
      }

      if (baseBoardInformation != null) {
        r.Append("Mainboard Manufacturer: "); 
        r.AppendLine(baseBoardInformation.ManufacturerName);
        r.Append("Mainboard Name: "); 
        r.AppendLine(baseBoardInformation.ProductName);
        r.AppendLine();
      }
     
      return r.ToString();
    }

    public BIOSInformation BIOS {
      get { return biosInformation; }
    }

    public BaseBoardInformation Board {
      get { return baseBoardInformation; }
    }

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

      private string manufacturerName;
      private string productName;
      private string version;
      private string serialNumber;
      private Manufacturer manufacturer;
      private Model model;

      public BaseBoardInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) {

        this.manufacturerName = GetString(0x04).Trim();
        this.productName = GetString(0x05).Trim();
        this.version = GetString(0x06).Trim();
        this.serialNumber = GetString(0x07).Trim();

        switch (manufacturerName) {
          case "ASUSTeK Computer INC.":
            manufacturer = Manufacturer.ASUS; break;
          case "DFI":
          case "DFI Inc.":            
            manufacturer = Manufacturer.DFI; break;
          case "EPoX COMPUTER CO., LTD":
            manufacturer = Manufacturer.EPoX; break;
          case "Gigabyte Technology Co., Ltd.":
            manufacturer = Manufacturer.Gigabyte; break;
          case "IBM":
            manufacturer = Manufacturer.IBM; break;
          case "MICRO-STAR INTERNATIONAL CO., LTD":
          case "MICRO-STAR INTERNATIONAL CO.,LTD":
            manufacturer = Manufacturer.MSI; break;
          default:
            manufacturer = Manufacturer.Unknown; break;
        }

        switch (productName) {
          case "LP BI P45-T2RS Elite":
            model = Model.LP_BI_P45_T2RS_Elite; break;
          case "LP DK P55-T3eH9":
            model = Model.LP_DK_P55_T3eH9; break;
          case "EP45-DS3R":
            model = Model.EP45_DS3R; break;
          case "GA-MA785GMT-UD2H":
            model = Model.GA_MA785GMT_UD2H; break;
          case "P35-DS3":
            model = Model.P35_DS3; break;
          default:
            model = Model.Unknown; break;
        }
      }

      public string ManufacturerName { get { return manufacturerName; } }

      public string ProductName { get { return productName; } }

      public string Version { get { return version; } }

      public string SerialNumber { get { return serialNumber; } }

      public Manufacturer Manufacturer { get { return manufacturer; } }

      public Model Model { get { return model; } }

    }
  }
}
