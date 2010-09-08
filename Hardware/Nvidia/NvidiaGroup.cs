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

using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.Nvidia {

  internal class NvidiaGroup : IGroup {
   
    private List<IHardware> hardware = new List<IHardware>();
    private StringBuilder report = new StringBuilder();

    public NvidiaGroup(ISettings settings) {
      if (!NVAPI.IsAvailable)
        return;

      report.AppendLine("NVAPI");
      report.AppendLine();

      string version;
      if (NVAPI.NvAPI_GetInterfaceVersionString(out version) == NvStatus.OK) {
        report.Append("Version: ");
        report.AppendLine(version);
      }

      NvPhysicalGpuHandle[] handles = 
        new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
      int count;
      if (NVAPI.NvAPI_EnumPhysicalGPUs == null) {
        report.AppendLine("Error: NvAPI_EnumPhysicalGPUs not available");
        report.AppendLine();
        return;
      } else {        
        NvStatus status = NVAPI.NvAPI_EnumPhysicalGPUs(handles, out count);
        if (status != NvStatus.OK) {
          report.AppendLine("Status: " + status.ToString());
          report.AppendLine();
          return;
        }
      }

      IDictionary<NvPhysicalGpuHandle, NvDisplayHandle> displayHandles =
        new Dictionary<NvPhysicalGpuHandle, NvDisplayHandle>();

      if (NVAPI.NvAPI_EnumNvidiaDisplayHandle != null &&
        NVAPI.NvAPI_GetPhysicalGPUsFromDisplay != null) 
      {
        NvStatus status = NvStatus.OK;
        int i = 0;
        while (status == NvStatus.OK) {
          NvDisplayHandle displayHandle = new NvDisplayHandle();
          status = NVAPI.NvAPI_EnumNvidiaDisplayHandle(i, ref displayHandle);
          i++;

          if (status == NvStatus.OK) {
            NvPhysicalGpuHandle[] handlesFromDisplay =
              new NvPhysicalGpuHandle[NVAPI.MAX_PHYSICAL_GPUS];
            uint countFromDisplay;
            if (NVAPI.NvAPI_GetPhysicalGPUsFromDisplay(displayHandle,
              handlesFromDisplay, out countFromDisplay) == NvStatus.OK) {
              for (int j = 0; j < countFromDisplay; j++) {
                if (!displayHandles.ContainsKey(handlesFromDisplay[j]))
                  displayHandles.Add(handlesFromDisplay[j], displayHandle);
              }
            }
          }
        }
      }

      report.Append("Number of GPUs: ");
      report.AppendLine(count.ToString(CultureInfo.InvariantCulture));      
      
      for (int i = 0; i < count; i++) {    
        NvDisplayHandle displayHandle;
        if (displayHandles.TryGetValue(handles[i], out displayHandle))
          hardware.Add(new NvidiaGPU(i, handles[i], displayHandle, settings));                            
        else
          hardware.Add(new NvidiaGPU(i, handles[i], null, settings));   
      }

      report.AppendLine();
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      return report.ToString();
    }

    public void Close() { }
  }
}
