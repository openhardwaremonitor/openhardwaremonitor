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
  Portions created by the Initial Developer are Copyright (C) 2010
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
using System.IO.Ports;
using System.Security;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace OpenHardwareMonitor.Hardware.Heatmaster {
  internal class HeatmasterGroup : IGroup {

    private List<Heatmaster> hardware = new List<Heatmaster>();
    private StringBuilder report = new StringBuilder();

    private static string ReadLine(SerialPort port, int timeout) {
      int i = 0;
      StringBuilder builder = new StringBuilder();
      while (i < timeout) {
        while (port.BytesToRead > 0) {
          byte b = (byte)port.ReadByte();
          switch (b) {
            case 0xAA: return ((char)b).ToString();
            case 0x0D: return builder.ToString();
            default: builder.Append((char)b); break;
          }
        }
        i++;
        Thread.Sleep(1);
      }
      throw new TimeoutException();
    }

    private static string[] GetRegistryPortNames() {
      List<string> result = new List<string>();
      try {
        RegistryKey key = Registry.LocalMachine.OpenSubKey(
          @"SYSTEM\CurrentControlSet\Enum\USB\Vid_10c4&Pid_ea60&Mi_00");
        if (key != null) {
          foreach (string subKeyName in key.GetSubKeyNames()) {
            RegistryKey subKey =
              key.OpenSubKey(subKeyName + "\\" + "Device Parameters");
            if (subKey != null) {
              string name = subKey.GetValue("PortName") as string;
              if (name != null)
                result.Add((string)name);
            }
          }
        }
      } catch (SecurityException) { }
      return result.ToArray();
    }

    public HeatmasterGroup(ISettings settings) {
      
      // No implementation for Heatmaster on Unix systems
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;

      string[] portNames = GetRegistryPortNames();      
      for (int i = portNames.Length - 1; i >= 0; i--) {
        try {

          SerialPort serialPort =
            new SerialPort(portNames[i], 38400, Parity.None, 8, StopBits.One);
          serialPort.NewLine = ((char)0x0D).ToString();

          bool isValid = false;
          report.Append("Port Name: "); report.AppendLine(portNames[i]);

          try {
            serialPort.Open();
          } catch (UnauthorizedAccessException) {
            report.AppendLine("Exception: Access Denied");
          }

          if (serialPort.IsOpen) {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer(); 
            serialPort.Write(new byte[] { 0xAA }, 0, 1);

            int j = 0;
            while (serialPort.BytesToRead == 0 && j < 10) {
              Thread.Sleep(20);
              j++;
            }
            if (serialPort.BytesToRead > 0) {
              bool flag = false;
              while (serialPort.BytesToRead > 0 && !flag) {
                flag |= (serialPort.ReadByte() == 0xAA);
              }
              if (flag) {
                serialPort.WriteLine("[0:0]RH");
                try {
                  int k = 0;
                  int revision = 0;
                  while (k < 5) {
                    string line = ReadLine(serialPort, 100);
                    if (line.StartsWith("-[0:0]RH:")) {
                      int.TryParse(line.Substring(9), out revision);
                      break;
                    }
                    k++;
                  }
                  isValid = (revision == 770); 
                  if (!isValid) {
                    report.Append("Status: Wrong Hardware Revision " + 
                      revision.ToString());
                  }                 
                } catch (TimeoutException) {
                  report.AppendLine("Status: Timeout Reading Revision");
                }
              } else {
                report.AppendLine("Status: Wrong Startflag");
              }
            } else {
              report.AppendLine("Status: No Response");
            }
            serialPort.DiscardInBuffer();
            serialPort.Close();
            serialPort.Dispose();
          } else {
            report.AppendLine("Status: Port not Open");
          }                          
          if (isValid) {
            report.AppendLine("Status: OK");
            hardware.Add(new Heatmaster(portNames[i], settings));
            return;
          }
        } catch (Exception e) {
          report.AppendLine(e.ToString());
        } 
        report.AppendLine();
      }
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      if (report.Length > 0) {
        report.Insert(0, "Serial Port Heatmaster" + Environment.NewLine +
          Environment.NewLine);
        report.AppendLine();
        return report.ToString();
      } else
        return null;
    }

    public void Close() {
      foreach (Heatmaster heatmaster in hardware)
        heatmaster.Close();
    }
  }
}
