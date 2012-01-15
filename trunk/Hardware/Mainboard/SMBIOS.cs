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
  Portions created by the Initial Developer are Copyright (C) 2009-2012
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

    private readonly byte[] raw;
    private readonly Structure[] table;

    private readonly Version version;
    private readonly BIOSInformation biosInformation;
    private readonly SystemInformation systemInformation;
    private readonly BaseBoardInformation baseBoardInformation;

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
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) {
        this.raw = null;
        this.table = null;
        
        string boardVendor = ReadSysFS("/sys/class/dmi/id/board_vendor");
        string boardName = ReadSysFS("/sys/class/dmi/id/board_name");        
        string boardVersion = ReadSysFS("/sys/class/dmi/id/board_version");        
        this.baseBoardInformation = new BaseBoardInformation(
          boardVendor, boardName, boardVersion, null);

        string systemVendor = ReadSysFS("/sys/class/dmi/id/sys_vendor");
        string productName = ReadSysFS("/sys/class/dmi/id/product_name");
        string productVersion = ReadSysFS("/sys/class/dmi/id/product_version");    
        this.systemInformation = new SystemInformation(systemVendor, 
          productName, productVersion, null, null);

        string biosVendor = ReadSysFS("/sys/class/dmi/id/bios_vendor");
        string biosVersion = ReadSysFS("/sys/class/dmi/id/bios_version");
        this.biosInformation = new BIOSInformation(biosVendor, biosVersion);
        
      } else {              
        List<Structure> structureList = new List<Structure>();

        raw = null;
        byte majorVersion = 0;
        byte minorVersion = 0;
        try {
          ManagementObjectCollection collection;
          using (ManagementObjectSearcher searcher = 
            new ManagementObjectSearcher("root\\WMI", 
              "SELECT * FROM MSSMBios_RawSMBiosTables")) {
            collection = searcher.Get();
          }
         
          foreach (ManagementObject mo in collection) {
            raw = (byte[])mo["SMBiosData"];
            majorVersion = (byte)mo["SmbiosMajorVersion"];
            minorVersion = (byte)mo["SmbiosMinorVersion"];            
            break;
          }
        } catch { }

        if (majorVersion > 0 || minorVersion > 0)
          version = new Version(majorVersion, minorVersion);
  
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
              case 0x01:
                this.systemInformation = new SystemInformation(
                  type, handle, data, stringsList.ToArray());
                structureList.Add(this.systemInformation); break;
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

      if (version != null) {
        r.Append("SMBIOS Version: "); r.AppendLine(version.ToString(2));
        r.AppendLine();
      }

      if (BIOS != null) {
        r.Append("BIOS Vendor: "); r.AppendLine(BIOS.Vendor);
        r.Append("BIOS Version: "); r.AppendLine(BIOS.Version);
        r.AppendLine();
      }

      if (System != null) {
        r.Append("System Manufacturer: ");
        r.AppendLine(System.ManufacturerName);
        r.Append("System Name: ");
        r.AppendLine(System.ProductName);
        r.Append("System Version: ");
        r.AppendLine(System.Version);
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

    public SystemInformation System {
      get { return systemInformation; }
    }

    public BaseBoardInformation Board {
      get { return baseBoardInformation; }
    }

    public class Structure {
      private readonly byte type;
      private readonly ushort handle;

      private readonly byte[] data;
      private readonly string[] strings;

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

      private readonly string vendor;
      private readonly string version;
      
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

    public class SystemInformation : Structure {

      private readonly string manufacturerName;
      private readonly string productName;
      private readonly string version;
      private readonly string serialNumber;
      private readonly string family;

      public SystemInformation(string manufacturerName, string productName, 
        string version, string serialNumber, string family) 
        : base (0x01, 0, null, null) 
      {
        this.manufacturerName = manufacturerName;
        this.productName = productName;
        this.version = version;
        this.serialNumber = serialNumber;
        this.family = family;
      }

      public SystemInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) 
      {
        this.manufacturerName = GetString(0x04);
        this.productName = GetString(0x05);
        this.version = GetString(0x06);
        this.serialNumber = GetString(0x07);
        this.family = GetString(0x1A);
      }

      public string ManufacturerName { get { return manufacturerName; } }

      public string ProductName { get { return productName; } }

      public string Version { get { return version; } }

      public string SerialNumber { get { return serialNumber; } }

      public string Family { get { return family; } }

    }

    public class BaseBoardInformation : Structure {

      private readonly string manufacturerName;
      private readonly string productName;
      private readonly string version;
      private readonly string serialNumber;
      private readonly Manufacturer manufacturer;
      private readonly Model model;

      private static Manufacturer GetManufacturer(string name) {               
        switch (name) {
          case "Alienware":
            return Manufacturer.Alienware;
          case "Apple Inc.":
            return Manufacturer.Apple;
          case "ASRock":
            return Manufacturer.ASRock;
          case "ASUSTeK Computer INC.":
          case "ASUSTeK COMPUTER INC.":
            return Manufacturer.ASUS;
          case "Dell Inc.":
            return Manufacturer.Dell;
          case "DFI":
          case "DFI Inc.":            
            return Manufacturer.DFI;
          case "ECS":
            return Manufacturer.ECS;
          case "EPoX COMPUTER CO., LTD":
            return Manufacturer.EPoX;
          case "EVGA":
            return  Manufacturer.EVGA;
          case "First International Computer, Inc.":
            return Manufacturer.FIC;
          case "FUJITSU":
          case "FUJITSU SIEMENS":
            return Manufacturer.Fujitsu;
          case "Gigabyte Technology Co., Ltd.":
            return  Manufacturer.Gigabyte;
          case "Hewlett-Packard":
            return  Manufacturer.HP;
          case "IBM":
            return  Manufacturer.IBM;
          case "Intel":
          case "Intel Corp.":
          case "Intel Corporation":
          case "INTEL Corporation":
            return Manufacturer.Intel;   
          case "Lenovo":
          case "LENOVO":
            return Manufacturer.Lenovo;
          case "Micro-Star International":
          case "MICRO-STAR INTERNATIONAL CO., LTD":
          case "MICRO-STAR INTERNATIONAL CO.,LTD":
          case "MSI":
            return Manufacturer.MSI;
          case "Shuttle":
            return Manufacturer.Shuttle;
          case "Supermicro":
            return Manufacturer.Supermicro;
          case "TOSHIBA":
            return Manufacturer.Toshiba;
          case "XFX":
            return  Manufacturer.XFX;
          case "To be filled by O.E.M.":
            return  Manufacturer.Unknown;
          default:
            return  Manufacturer.Unknown;
        }
      }

      private static Model GetModel(string name) {
        switch (name) {
          case "880GMH/USB3":
            return Model._880GMH_USB3;
          case "ASRock AOD790GX/128M":
            return Model.AOD790GX_128M;
          case "P55 Deluxe":
            return Model.P55_Deluxe;
          case "Crosshair III Formula":
            return Model.Crosshair_III_Formula;
          case "M2N-SLI DELUXE":
            return Model.M2N_SLI_DELUXE;
          case "M4A79XTD EVO":
            return Model.M4A79XTD_EVO;
          case "P5W DH Deluxe":
            return Model.P5W_DH_Deluxe;
          case "P6X58D-E":
            return Model.P6X58D_E;
          case "P8P67":
            return Model.P8P67;
          case "P8P67 EVO":
            return Model.P8P67_EVO;
          case "P8P67 PRO":
            return Model.P8P67_PRO;
          case "P8P67-M PRO":
            return Model.P8P67_M_PRO;
          case "P9X79":
            return Model.P9X79;
          case "Rampage Extreme":
            return Model.Rampage_Extreme;
          case "Rampage II GENE":
            return Model.Rampage_II_GENE;
          case "LP BI P45-T2RS Elite":
            return Model.LP_BI_P45_T2RS_Elite;
          case "LP DK P55-T3eH9":
            return Model.LP_DK_P55_T3eH9;
          case "A890GXM-A":
            return Model.A890GXM_A;
          case "X58 SLI Classified":
            return Model.X58_SLI_Classified;
          case "965P-S3":
            return Model._965P_S3;
          case "EP45-DS3R":
            return Model.EP45_DS3R;
          case "EP45-UD3R":
            return Model.EP45_UD3R;
          case "EX58-EXTREME":
            return Model.EX58_EXTREME;
          case "GA-MA770T-UD3":
            return Model.GA_MA770T_UD3;
          case "GA-MA785GMT-UD2H":
            return Model.GA_MA785GMT_UD2H;
          case "H67A-UD3H-B3":
            return Model.H67A_UD3H_B3;
          case "P35-DS3":
            return Model.P35_DS3;
          case "P35-DS3L":
            return Model.P35_DS3L;
          case "P55-UD4":
            return Model.P55_UD4;
          case "P55M-UD4":
            return Model.P55M_UD4;
          case "P67A-UD4-B3":
            return Model.P67A_UD4_B3;
          case "X38-DS5":
            return Model.X38_DS5;
          case "X58A-UD3R":
            return Model.X58A_UD3R;
          case "Z68X-UD7-B3":
            return Model.Z68X_UD7_B3;
          case "FH67":
            return Model.FH67;
          case "Base Board Product Name":
          case "To be filled by O.E.M.":
            return Model.Unknown;
          default:
            return Model.Unknown;
        }
      }
      
      public BaseBoardInformation(string manufacturerName, string productName, 
        string version, string serialNumber) 
        : base(0x02, 0, null, null) 
      {
        this.manufacturerName = manufacturerName;
        this.manufacturer = GetManufacturer(manufacturerName);
        this.productName = productName;
        this.model = GetModel(productName);
        this.version = version;
        this.serialNumber = serialNumber;
      }
      
      public BaseBoardInformation(byte type, ushort handle, byte[] data,
        string[] strings)
        : base(type, handle, data, strings) {

        this.manufacturerName = GetString(0x04).Trim();
        this.manufacturer = GetManufacturer(this.manufacturerName);
        this.productName = GetString(0x05).Trim();
        this.model = GetModel(this.productName);
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
