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
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.ATI {
  internal class ATIGroup : IGroup {

    private readonly List<ATIGPU> hardware = new List<ATIGPU>();
    private readonly StringBuilder report = new StringBuilder();

    public ATIGroup(ISettings settings) {
      try {
        int status = ADL.ADL_Main_Control_Create(1);

        report.AppendLine("AMD Display Library");
        report.AppendLine();
        report.Append("Status: ");
        report.AppendLine(status == ADL.ADL_OK ? "OK" : 
          status.ToString(CultureInfo.InvariantCulture));
        report.AppendLine();

        if (status == ADL.ADL_OK) {
          int numberOfAdapters = 0;
          ADL.ADL_Adapter_NumberOfAdapters_Get(ref numberOfAdapters);
          
          report.Append("Number of adapters: "); 
          report.AppendLine(numberOfAdapters.ToString(CultureInfo.InvariantCulture));
          report.AppendLine();

          if (numberOfAdapters > 0) {
            ADLAdapterInfo[] adapterInfo = new ADLAdapterInfo[numberOfAdapters];
            if (ADL.ADL_Adapter_AdapterInfo_Get(adapterInfo) == ADL.ADL_OK)
              for (int i = 0; i < numberOfAdapters; i++) {
                int isActive;
                ADL.ADL_Adapter_Active_Get(adapterInfo[i].AdapterIndex,
                  out isActive);
                int adapterID;
                ADL.ADL_Adapter_ID_Get(adapterInfo[i].AdapterIndex,
                  out adapterID);

                report.Append("AdapterIndex: "); 
                report.AppendLine(i.ToString(CultureInfo.InvariantCulture));
                report.Append("isActive: "); 
                report.AppendLine(isActive.ToString(CultureInfo.InvariantCulture));
                report.Append("AdapterName: "); 
                report.AppendLine(adapterInfo[i].AdapterName);     
                report.Append("UDID: ");
                report.AppendLine(adapterInfo[i].UDID);
                report.Append("Present: ");
                report.AppendLine(adapterInfo[i].Present.ToString(
                  CultureInfo.InvariantCulture));
                report.Append("VendorID: 0x");
                report.AppendLine(adapterInfo[i].VendorID.ToString("X",
                  CultureInfo.InvariantCulture));
                report.Append("BusNumber: ");
                report.AppendLine(adapterInfo[i].BusNumber.ToString(
                  CultureInfo.InvariantCulture));
                report.Append("DeviceNumber: ");
                report.AppendLine(adapterInfo[i].DeviceNumber.ToString(
                 CultureInfo.InvariantCulture));
                report.Append("FunctionNumber: ");
                report.AppendLine(adapterInfo[i].FunctionNumber.ToString(
                  CultureInfo.InvariantCulture));
                report.Append("AdapterID: 0x");
                report.AppendLine(adapterID.ToString("X", 
                  CultureInfo.InvariantCulture));

                if (!string.IsNullOrEmpty(adapterInfo[i].UDID) &&
                  adapterInfo[i].VendorID == ADL.ATI_VENDOR_ID) 
                {
                  bool found = false;
                  foreach (ATIGPU gpu in hardware)
                    if (gpu.BusNumber == adapterInfo[i].BusNumber &&
                      gpu.DeviceNumber == adapterInfo[i].DeviceNumber) {
                      found = true;
                      break;
                    }
                  if (!found)
                    hardware.Add(new ATIGPU(
                      adapterInfo[i].AdapterName.Trim(),
                      adapterInfo[i].AdapterIndex,
                      adapterInfo[i].BusNumber,
                      adapterInfo[i].DeviceNumber, settings));
                }

                report.AppendLine();
              }
          }
        }
      } catch (DllNotFoundException) { } 
        catch (EntryPointNotFoundException e) {
          report.AppendLine();
          report.AppendLine(e.ToString());
          report.AppendLine();        
        }
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      return report.ToString();
    }

    public void Close() {
      try {
        foreach (ATIGPU gpu in hardware)
          gpu.Close();
        ADL.ADL_Main_Control_Destroy();
      } catch (Exception) { }
    }
  }
}
