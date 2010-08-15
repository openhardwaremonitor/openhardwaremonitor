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
using System.IO;
using System.Management;
using System.Text;

namespace OpenHardwareMonitor.Hardware.Mainboard {

  internal class SMBIOS {

    private byte[] raw;
    private Structure[] table;

    private BIOSInformation biosInformation = null;
    private BaseBoardInformation baseBoardInformation = null;

    private static string ReadSysFS(string path) {
      try {
        if (File.Exists(path)) {
          using (StreamReader reader = new StreamReader(path)) 
            return reader.ReadLine();
        } else {
          return null;
        }
      } catch {
        return null;
      }
    }
    
    public SMBIOS() {
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) {
        this.raw = null;
        this.table = null;
        
        string boardVendor = ReadSysFS("/sys/class/dmi/id/board_vendor");
        string boardName = ReadSysFS("/sys/class/dmi/id/board_name");        
        string boardVersion = ReadSysFS("/sys/class/dmi/id/board_version");        
        this.baseBoardInformation = new BaseBoardInformation(
          boardVendor, boardName, boardVersion, null);
        
        string biosVendor = ReadSysFS("/sys/class/dmi/id/bios_vendor");
        string biosVersion = ReadSysFS("/sys/class/dmi/id/bios_version");
        this.biosInformation = new BIOSInformation(biosVendor, biosVersion);
        
      } else {              
        List<Structure> structureList = new List<Structure>();

        raw = null;
        try {
          ManagementObjectCollection collection;
          using (ManagementObjectSearcher searcher = 
            new ManagementObjectSearcher("root\\WMI", 
              "SELECT SMBiosData FROM MSSMBios_RawSMBiosTables")) {
            collection = searcher.Get();
          }
         
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
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      if (BIOS != null) {
        r.Append("BIOS Vendor: "); r.AppendLine(BIOS.Vendor);
        r.Append("BIOS Version: "); r.AppendLine(BIOS.Version);
        r.AppendLine();
      }

      if (Board != null) {
        r.Append("Mainboard Manufacturer: ");
        r.AppendLine(Board.ManufacturerName);
        r.Append("Mainboard Name: ");
        r.AppendLine(Board.ProductName);
        r.Append("Mainboard Version: ");
        r.AppendLine(Board.Version);
        r.AppendLine();
      }

      if (raw != null) {
        string base64 = Convert.ToBase64String(raw);
        r.AppendLine("SMBIOS Table");
        r.AppendLine();

        for (int i = 0; i < Math.Ceiling(base64.Length / 64.0); i++) {
          r.Append(" ");
          for (int j = 0; j < 0x40; j++) {
            int index = (i << 6) | j;
            if (index < base64.Length) {              
              r.Append(base64[index]);
            }
          }
          r.AppendLine();
        }
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
      
      public BIOSInformation(string vendor, string version) 
        : base (0x00, 0, null, null) 
      {
        this.vendor = vendor;
        this.version = version;
      }
      
      public BIOSInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) 
      {
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

      private void SetManufacturerName(string name) {
        this.manufacturerName = name;
        
        switch (name) {
          case "ASRock":
            manufacturer = Manufacturer.ASRock; break;
          case "ASUSTeK Computer INC.":
            manufacturer = Manufacturer.ASUS; break;
          case "Dell Inc.":
            manufacturer = Manufacturer.Dell; break;
          case "DFI":
          case "DFI Inc.":            
            manufacturer = Manufacturer.DFI; break;
          case "EPoX COMPUTER CO., LTD":
            manufacturer = Manufacturer.EPoX; break;
          case "EVGA":
            manufacturer = Manufacturer.EVGA; break;
          case "First International Computer, Inc.":
            manufacturer = Manufacturer.FIC; break;
          case "Gigabyte Technology Co., Ltd.":
            manufacturer = Manufacturer.Gigabyte; break;
          case "Hewlett-Packard":
            manufacturer = Manufacturer.HP; break;
          case "IBM":
            manufacturer = Manufacturer.IBM; break;
          case "MICRO-STAR INTERNATIONAL CO., LTD":
          case "MICRO-STAR INTERNATIONAL CO.,LTD":
            manufacturer = Manufacturer.MSI; break;
          case "XFX":
            manufacturer = Manufacturer.XFX; break;
          case "To be filled by O.E.M.":
            manufacturer = Manufacturer.Unknown; break;
          default:
            manufacturer = Manufacturer.Unknown; break;
        }
      }
      
      private void SetProductName(string name) {
        this.productName = name;
        
        switch (name) {
          case "880GMH/USB3":
            model = Model._880GMH_USB3; break;
          case "Crosshair III Formula":
            model = Model.Crosshair_III_Formula; break;
          case "M2N-SLI DELUXE":
            model = Model.M2N_SLI_DELUXE; break;
          case "M4A79XTD EVO":
            model = Model.M4A79XTD_EVO; break;
          case "P5W DH Deluxe":
            model = Model.P5W_DH_Deluxe; break;
          case "P6X58D-E":
            model = Model.P6X58D_E; break;
          case "LP BI P45-T2RS Elite":
            model = Model.LP_BI_P45_T2RS_Elite; break;
          case "LP DK P55-T3eH9":
            model = Model.LP_DK_P55_T3eH9; break;
          case "X58 SLI Classified":
            model = Model.X58_SLI_Classified; break;
          case "965P-S3":
            model = Model._965P_S3; break;
          case "EP45-DS3R":
            model = Model.EP45_DS3R; break;
          case "EP45-UD3R":
            model = Model.EP45_UD3R; break;
          case "EX58-EXTREME":
            model = Model.EX58_EXTREME; break;
          case "GA-MA770T-UD3":
            model = Model.GA_MA770T_UD3; break;
          case "GA-MA785GMT-UD2H":
            model = Model.GA_MA785GMT_UD2H; break;
          case "P35-DS3":
            model = Model.P35_DS3; break;
          case "P35-DS3L":
            model = Model.P35_DS3L; break;
          case "P55-UD4":
            model = Model.P55_UD4; break;
          case "X38-DS5":
            model = Model.X38_DS5; break;
          case "X58A-UD3R":
            model = Model.X58A_UD3R; break;
          case "To be filled by O.E.M.":
            model = Model.Unknown; break;
          default:
            model = Model.Unknown; break;
        }
      }
      
      public BaseBoardInformation(string manufacturerName, string productName, 
        string version, string serialNumber) 
        : base(0x02, 0, null, null) 
      {        
        SetManufacturerName(manufacturerName);
        SetProductName(productName);
        this.version = version;
        this.serialNumber = serialNumber;
      }
      
      public BaseBoardInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) {

        SetManufacturerName(GetString(0x04).Trim());
        SetProductName(GetString(0x05).Trim());
        this.version = GetString(0x06).Trim();
        this.serialNumber = GetString(0x07).Trim();               
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
