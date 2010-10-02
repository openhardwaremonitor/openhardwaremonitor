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
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {

  internal enum Vendor {
    Unknown,
    Intel,
    AMD,
  }

  internal class CPUID {

    private readonly int thread;

    private readonly Vendor vendor = Vendor.Unknown;

    private readonly string cpuBrandString = "";
    private readonly string name = "";

    private readonly uint[,] cpuidData = new uint[0, 0];
    private readonly uint[,] cpuidExtData = new uint[0, 0];

    private readonly uint family;
    private readonly uint model;
    private readonly uint stepping;

    private readonly uint apicId;

    private readonly uint threadMaskWith;
    private readonly uint coreMaskWith;

    private readonly uint processorId;
    private readonly uint coreId;
    private readonly uint threadId;

    public const uint CPUID_0 = 0;
    public const uint CPUID_EXT = 0x80000000;

    private static void AppendRegister(StringBuilder b, uint value) {
      b.Append((char)((value) & 0xff));
      b.Append((char)((value >> 8) & 0xff));
      b.Append((char)((value >> 16) & 0xff));
      b.Append((char)((value >> 24) & 0xff));
    }

    private static uint NextLog2(long x) {
      if (x <= 0)
        return 0;

      x--;
      uint count = 0;
      while (x > 0) {
        x >>= 1;
        count++;
      }

      return count;
    }

    public CPUID(int thread) {
      this.thread = thread;

      uint maxCpuid = 0;
      uint maxCpuidExt = 0;

      uint eax, ebx, ecx, edx;

      if (thread >= 32)
        throw new ArgumentOutOfRangeException("thread");
      UIntPtr mask = (UIntPtr)(1L << thread);

      if (WinRing0.CpuidTx(CPUID_0, 0,
          out eax, out ebx, out ecx, out edx, mask)) {
        if (eax > 0)
          maxCpuid = eax;
        else
          return;

        StringBuilder vendorBuilder = new StringBuilder();
        AppendRegister(vendorBuilder, ebx);
        AppendRegister(vendorBuilder, edx);
        AppendRegister(vendorBuilder, ecx);
        string cpuVendor = vendorBuilder.ToString();
        switch (cpuVendor) {
          case "GenuineIntel":
            vendor = Vendor.Intel;
            break;
          case "AuthenticAMD":
            vendor = Vendor.AMD;
            break;
          default:
            vendor = Vendor.Unknown;
            break;
        }
        eax = ebx = ecx = edx = 0;
        if (WinRing0.CpuidTx(CPUID_EXT, 0,
          out eax, out ebx, out ecx, out edx, mask)) {
          if (eax > CPUID_EXT)
            maxCpuidExt = eax - CPUID_EXT;
          else
            return;
        } else {
          throw new ArgumentOutOfRangeException("thread");
        }
      } else {
        throw new ArgumentOutOfRangeException("thread");
      }

      maxCpuid = Math.Min(maxCpuid, 1024);
      maxCpuidExt = Math.Min(maxCpuidExt, 1024);   

      cpuidData = new uint[maxCpuid + 1, 4];
      for (uint i = 0; i < (maxCpuid + 1); i++)
        WinRing0.CpuidTx(CPUID_0 + i, 0, 
          out cpuidData[i, 0], out cpuidData[i, 1],
          out cpuidData[i, 2], out cpuidData[i, 3], mask);

      cpuidExtData = new uint[maxCpuidExt + 1, 4];
      for (uint i = 0; i < (maxCpuidExt + 1); i++)
        WinRing0.CpuidTx(CPUID_EXT + i, 0, 
          out cpuidExtData[i, 0], out cpuidExtData[i, 1], 
          out cpuidExtData[i, 2], out cpuidExtData[i, 3], mask);

      StringBuilder nameBuilder = new StringBuilder();
      for (uint i = 2; i <= 4; i++) {
        if (WinRing0.CpuidTx(CPUID_EXT + i, 0, 
          out eax, out ebx, out ecx, out edx, mask)) 
        {
          AppendRegister(nameBuilder, eax);
          AppendRegister(nameBuilder, ebx);
          AppendRegister(nameBuilder, ecx);
          AppendRegister(nameBuilder, edx);
        }
      }
      nameBuilder.Replace('\0', ' ');
      cpuBrandString = nameBuilder.ToString().Trim();
      nameBuilder.Replace("(R)", " ");
      nameBuilder.Replace("(TM)", " ");
      nameBuilder.Replace("(tm)", " ");
      nameBuilder.Replace("CPU", "");
      for (int i = 0; i < 10; i++) nameBuilder.Replace("  ", " ");
      name = nameBuilder.ToString();
      if (name.Contains("@"))
        name = name.Remove(name.LastIndexOf('@'));
      name = name.Trim();      

      this.family = ((cpuidData[1, 0] & 0x0FF00000) >> 20) +
        ((cpuidData[1, 0] & 0x0F00) >> 8);
      this.model = ((cpuidData[1, 0] & 0x0F0000) >> 12) +
        ((cpuidData[1, 0] & 0xF0) >> 4);
      this.stepping = (cpuidData[1, 0] & 0x0F);

      this.apicId = (cpuidData[1, 1] >> 24) & 0xFF;

      switch (vendor) {
        case Vendor.Intel:
          uint maxCoreAndThreadIdPerPackage = (cpuidData[1, 1] >> 16) & 0xFF;
          uint maxCoreIdPerPackage;
          if (maxCpuid >= 4)
            maxCoreIdPerPackage = ((cpuidData[4, 0] >> 26) & 0x3F) + 1;
          else
            maxCoreIdPerPackage = 1;
          threadMaskWith = 
            NextLog2(maxCoreAndThreadIdPerPackage / maxCoreIdPerPackage);
          coreMaskWith = NextLog2(maxCoreIdPerPackage);
          break;
        case Vendor.AMD:
          uint corePerPackage;
          if (maxCpuidExt >= 8)
            corePerPackage = (cpuidExtData[8, 2] & 0xFF) + 1;
          else
            corePerPackage = 1;
          threadMaskWith = 0;
          coreMaskWith = NextLog2(corePerPackage);
          break;
        default:
          threadMaskWith = 0;
          coreMaskWith = 0;
          break;
      }

      processorId = (apicId >> (int)(coreMaskWith + threadMaskWith));
      coreId = ((apicId >> (int)(threadMaskWith)) 
        - (processorId << (int)(coreMaskWith)));
      threadId = apicId
        - (processorId << (int)(coreMaskWith + threadMaskWith))
        - (coreId << (int)(threadMaskWith)); 
    }

    public string Name {
      get { return name; }
    }

    public string BrandString {
      get { return cpuBrandString; }
    }

    public int Thread {
      get { return thread; }
    }

    public Vendor Vendor {
      get { return vendor; }
    }

    public uint Family {
      get { return family; }
    }

    public uint Model {
      get { return model; }
    }

    public uint Stepping {
      get { return stepping; }
    }

    public uint ApicId {
      get { return apicId; }
    }

    public uint ProcessorId {
      get { return processorId; }
    }

    public uint CoreId {
      get { return coreId; }
    }

    public uint ThreadId {
      get { return threadId; }
    }

    public uint[,] Data {
      get { return cpuidData; }
    }

    public uint[,] ExtData {
      get { return cpuidExtData; }
    }
  }
}
