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
using System.IO.Ports;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.TBalancer {
  public class TBalancerGroup : IGroup {

    private List<TBalancer> hardware = new List<TBalancer>();
    private StringBuilder report = new StringBuilder();

    public TBalancerGroup() {

      uint numDevices;
      try {
        FTD2XX.FT_CreateDeviceInfoList(out numDevices);
      } catch (DllNotFoundException) { return; } 
        catch (ArgumentNullException) { return; }
        catch (EntryPointNotFoundException) { return; }
     
      FT_DEVICE_INFO_NODE[] info = new FT_DEVICE_INFO_NODE[numDevices];
      FTD2XX.FT_GetDeviceInfoList(info, ref numDevices);

      for (int i = 0; i < numDevices; i++) {
        report.Append("Device Index: "); report.AppendLine(i.ToString());
        
        FT_HANDLE handle;
        FT_STATUS status;
        status = FTD2XX.FT_Open(i, out handle);
        if (status != FT_STATUS.FT_OK) {
          report.AppendLine("Open Status: " + status);
          continue;
        }

        FTD2XX.FT_SetBaudRate(handle, 19200);
        FTD2XX.FT_SetDataCharacteristics(handle, 8, 1, 0);
        FTD2XX.FT_SetFlowControl(handle, FT_FLOW_CONTROL.FT_FLOW_RTS_CTS, 0x11, 
          0x13);
        FTD2XX.FT_SetTimeouts(handle, 1000, 1000);
        FTD2XX.FT_Purge(handle, FT_PURGE.FT_PURGE_ALL);
        
        status = FTD2XX.Write(handle, new byte[] { 0x38 });
        if (status != FT_STATUS.FT_OK) {
          report.AppendLine("Write Status: " + status);
          FTD2XX.FT_Close(handle);
          continue;
        }

        bool isValid = false;
        byte protocolVersion = 0;

        int j = 0;
        while (FTD2XX.BytesToRead(handle) == 0 && j < 2) {
          Thread.Sleep(100);
          j++;
        }
        if (FTD2XX.BytesToRead(handle) > 0) {
          if (FTD2XX.ReadByte(handle) == TBalancer.STARTFLAG) {
            while (FTD2XX.BytesToRead(handle) < 284 && j < 5) {
              Thread.Sleep(100);
              j++;
            }
            int length = FTD2XX.BytesToRead(handle);
            if (length >= 284) {
              byte[] data = new byte[285];
              data[0] = TBalancer.STARTFLAG;
              for (int k = 1; k < data.Length; k++)
                data[k] = FTD2XX.ReadByte(handle);

              // check protocol version 2X (protocols seen: 2C, 2A, 28)
              isValid = (data[274] & 0xF0) == 0x20;
              protocolVersion = data[274];
              if (!isValid) {
                report.Append("Status: Wrong Protocol Version: 0x");
                report.AppendLine(protocolVersion.ToString("X"));
              }
            } else {
              report.AppendLine("Status: Wrong Message Length: " + length);
            }
          } else {
            report.AppendLine("Status: Wrong Startflag");
          }
        } else {
          report.AppendLine("Status: No Response");
        }

        FTD2XX.FT_Purge(handle, FT_PURGE.FT_PURGE_ALL);
        FTD2XX.FT_Close(handle);

        if (isValid) {
          report.AppendLine("Status: OK");
          hardware.Add(new TBalancer(i, protocolVersion));
          return;
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
        report.Insert(0, "FTD2XX" + Environment.NewLine +
          Environment.NewLine);
        report.AppendLine();
        return report.ToString();
      } else
        return null;
    }

    public void Close() {
      foreach (TBalancer tbalancer in hardware)
        tbalancer.Close();
    }
  }
}
