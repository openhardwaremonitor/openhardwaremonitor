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
  Portions created by the Initial Developer are Copyright (C) 2011
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
using System.IO.Compression;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenHardwareMonitor.Hardware.Mainboard {
  
  internal class GigabyteTAMG {
    private byte[] table;

    private Sensor[] sensors;

    private struct Sensor {
      public string Name;
      public SensorType Type;
      public int Channel;
      public float Value;
    }

    private enum SensorType {
      Voltage = 1,
      Temperature = 2,      
      Fan = 4,
      Case = 8,
    }

    public GigabyteTAMG(byte[] table) {
      if (table == null)
        throw new ArgumentNullException("table");

      this.table = table;
      
      int index = IndexOf(table, Encoding.ASCII.GetBytes("$HEALTH$"), 0);

      if (index >= 0) {
        index += 8;
        using (MemoryStream m =
          new MemoryStream(table, index, table.Length - index))
        using (BinaryReader r = new BinaryReader(m)) {
          try {
            r.ReadInt64();
            int count = r.ReadInt32();
            r.ReadInt64();
            r.ReadInt32();
            sensors = new Sensor[count];
            for (int i = 0; i < sensors.Length; i++) {
              sensors[i].Name = new string(r.ReadChars(32)).TrimEnd('\0');
              sensors[i].Type = (SensorType)r.ReadByte();
              sensors[i].Channel = r.ReadInt16();
              sensors[i].Channel |= r.ReadByte() << 24;
              r.ReadInt64();
              int value = r.ReadInt32();
              switch (sensors[i].Type) {
                case SensorType.Voltage:
                  sensors[i].Value = 1e-3f * value; break;
                default:
                  sensors[i].Value = value; break;
              }
              r.ReadInt64();
            }
          } catch (IOException) { sensors = new Sensor[0]; }
        }
      } else {
        sensors = new Sensor[0]; 
      }
    }

    public static int IndexOf(byte[] array, byte[] pattern, int startIndex) {
      if (array == null || pattern == null || pattern.Length > array.Length)
        return -1;

      for (int i = startIndex; i < array.Length - pattern.Length; i++) {
        bool found = true;
        for (int j = 0; j < pattern.Length; j++) {
          if (array[i + j] != pattern[j]) {
            found = false;
            break;
          }
        }
        if (found) 
          return i;
      }
      return -1;
    }

    private string GetCompressedAndEncodedTable() {
      string base64;
      using (MemoryStream m = new MemoryStream()) {
        using (GZipStream c = new GZipStream(m, CompressionMode.Compress)) {
          c.Write(table, 0, table.Length);          
        }
        base64 = Convert.ToBase64String(m.ToArray());
      }

      StringBuilder r = new StringBuilder();
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

      return r.ToString();
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      if (sensors.Length > 0) {
        r.AppendLine("Gigabyte TAMG Sensors");
        r.AppendLine();

        foreach (Sensor sensor in sensors) {
          r.AppendFormat(" {0,-10}: {1,8:G6} ({2})", sensor.Name, sensor.Value,
            sensor.Type);
          r.AppendLine();
        }
        r.AppendLine();
      }

      if (table.Length > 0) {
        r.AppendLine("Gigabyte TAMG Table");
        r.AppendLine();
        r.Append(GetCompressedAndEncodedTable());
        r.AppendLine();
      }

      return r.ToString();
    }
  }
}
