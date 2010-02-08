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
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {

  public class CPUGroup : IGroup { 
    private List<IHardware> hardware = new List<IHardware>();

    private string cpuBrandString;
    private string cpuVendor;
    private uint[,] cpuidData;
    private uint[,] cpuidExtData;

    private uint family;
    private uint model;
    private uint stepping;

    private static uint CPUID = 0;
    private static uint CPUID_EXT = 0x80000000;

    public static void AppendRegister(StringBuilder b, uint value) {
      b.Append((char)((value) & 0xff));
      b.Append((char)((value >> 8) & 0xff));
      b.Append((char)((value >> 16) & 0xff));
      b.Append((char)((value >> 24) & 0xff));
    }

    public CPUGroup() {

      if (!WinRing0.IsAvailable) 
        return;

      if (WinRing0.IsCpuid()) {
        uint maxCPUID = 0;
        uint maxCPUID_EXT = 0;
        uint eax, ebx, ecx, edx;

        
        if (WinRing0.Cpuid(CPUID, out eax, out ebx, out ecx, out edx)) {
          maxCPUID = eax;
          StringBuilder vendorBuilder = new StringBuilder();
          AppendRegister(vendorBuilder, ebx);
          AppendRegister(vendorBuilder, edx);
          AppendRegister(vendorBuilder, ecx);
          cpuVendor = vendorBuilder.ToString();

          eax = ebx = ecx = edx = 0;
          if (WinRing0.Cpuid(CPUID_EXT, out eax, out ebx, out ecx, out edx)) {
            maxCPUID_EXT = eax - CPUID_EXT;
          }
        }
        if (maxCPUID == 0 || maxCPUID_EXT == 0)
          return;        

        cpuidData = new uint[maxCPUID + 1, 4];
        for (uint i = 0; i < (maxCPUID + 1); i++)
          WinRing0.Cpuid(CPUID + i, out cpuidData[i, 0], out cpuidData[i, 1],
            out cpuidData[i, 2], out cpuidData[i, 3]);

        cpuidExtData = new uint[maxCPUID_EXT + 1, 4];
        for (uint i = 0; i < (maxCPUID_EXT + 1); i++)
          WinRing0.Cpuid(CPUID_EXT + i, out cpuidExtData[i, 0],
            out cpuidExtData[i, 1], out cpuidExtData[i, 2],
            out cpuidExtData[i, 3]);

        StringBuilder nameBuilder = new StringBuilder();
        for (uint i = 2; i <= 4; i++) {
          if (WinRing0.Cpuid(CPUID_EXT + i, out eax, out ebx, out ecx, out edx)) 
          {
            AppendRegister(nameBuilder, eax);
            AppendRegister(nameBuilder, ebx);
            AppendRegister(nameBuilder, ecx);
            AppendRegister(nameBuilder, edx);
          }
        }
        cpuBrandString = nameBuilder.ToString().Trim('\0').Trim();
        nameBuilder.Replace("(R)", " ");
        nameBuilder.Replace("(TM)", " ");
        nameBuilder.Replace("(tm)", " ");
        nameBuilder.Replace("CPU", "");
        for (int i = 0; i < 10; i++) nameBuilder.Replace("  ", " ");
        string name = nameBuilder.ToString();
        if (name.Contains("@"))
          name = name.Remove(name.LastIndexOf('@')); 
        name = name.Trim();

        this.family = ((cpuidData[1, 0] & 0x0FF00000) >> 20) +
          ((cpuidData[1, 0] & 0x0F00) >> 8);
        this.model = ((cpuidData[1, 0] & 0x0F0000) >> 12) +
          ((cpuidData[1, 0] & 0xF0) >> 4);
        this.stepping = (cpuidData[1, 0] & 0x0F);

        switch (cpuVendor) {
          case "GenuineIntel":
            // check if processor supports a digital thermal sensor
            if (maxCPUID >= 6 && (cpuidData[6, 0] & 1) != 0) 
              hardware.Add(new IntelCPU(name, family, model, stepping, 
                cpuidData, cpuidExtData));
            break;
          case "AuthenticAMD":                       
            // check if processor supports a digital thermal sensor            
            if (maxCPUID_EXT >= 7 && (cpuidExtData[7, 3] & 1) != 0) {
              switch (family) {
                case 0x0F:
                  hardware.Add(new AMD0FCPU(name, family, model, stepping,
                    cpuidData, cpuidExtData));
                  break;
                case 0x10:
                  hardware.Add(new AMD10CPU(name, family, model, stepping,
                    cpuidData, cpuidExtData));
                  break;
                default:
                  break;
              }
            }
            break;
          default:
            break;
        }
      }
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    private void AppendCpuidData(StringBuilder r, uint[,] data, uint offset) {
      for (int i = 0; i < data.GetLength(0); i++) {
        r.Append(" ");
        r.Append((i + offset).ToString("X8"));
        for (int j = 0; j < 4; j++) {
          r.Append("  ");
          r.Append(data[i, j].ToString("X8"));
        }
        r.AppendLine();
      }
    }

    public string GetReport() {

      StringBuilder r = new StringBuilder();

      r.AppendLine("CPUID");
      r.AppendLine();
      r.AppendFormat("Processor Vendor: {0}{1}", cpuVendor, 
        Environment.NewLine);
      r.AppendFormat("Processor Brand: {0}{1}", cpuBrandString, 
        Environment.NewLine);
      r.AppendFormat("Family: 0x{0}{1}", family.ToString("X"),
        Environment.NewLine);
      r.AppendFormat("Model: 0x{0}{1}", model.ToString("X"),
        Environment.NewLine);
      r.AppendFormat("Stepping: 0x{0}{1}", stepping.ToString("X"),
        Environment.NewLine);
      r.AppendLine();

      r.AppendLine("CPUID Return Values");
      r.AppendLine();

      if (cpuidData != null) {
        r.AppendLine(" Function  EAX       EBX       ECX       EDX");
        AppendCpuidData(r, cpuidData, CPUID);
        AppendCpuidData(r, cpuidExtData, CPUID_EXT);
        r.AppendLine();
      }

      return r.ToString();
    }

    public void Close() { }
  }
}
