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
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware {
  internal static class Opcode {
    private static IntPtr codeBuffer;

    public static void Open() {
      // No implementation for Unix systems
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;  

      byte[] rdtscCode;
      byte[] cpuidCode;
      if (IntPtr.Size == 4) {
        rdtscCode = RDTSC_32;
        cpuidCode = CPUID_32;
      } else {
        rdtscCode = RDTSC_64;
        cpuidCode = CPUID_64;
      }

      codeBuffer = NativeMethods.VirtualAlloc(IntPtr.Zero,
        (UIntPtr)(rdtscCode.Length + cpuidCode.Length),
      AllocationType.COMMIT | AllocationType.RESERVE, 
      MemoryProtection.EXECUTE_READWRITE);

      Marshal.Copy(rdtscCode, 0, codeBuffer, rdtscCode.Length);

      Rdtsc = Marshal.GetDelegateForFunctionPointer(
        codeBuffer, typeof(RdtscDelegate)) as RdtscDelegate;

      IntPtr cpuidAddress = (IntPtr)((long)codeBuffer + rdtscCode.Length);
      Marshal.Copy(cpuidCode, 0, cpuidAddress, cpuidCode.Length);

      Cpuid = Marshal.GetDelegateForFunctionPointer(
        cpuidAddress, typeof(CpuidDelegate)) as CpuidDelegate;
    }

    public static void Close() {
      Rdtsc = null;
      Cpuid = null;

      NativeMethods.VirtualFree(codeBuffer, UIntPtr.Zero, 
        FreeType.RELEASE);
    }

    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate ulong RdtscDelegate();

    public static RdtscDelegate Rdtsc;

    // unsigned __int64 __stdcall rdtsc() {
    //   return __rdtsc();
    // }

    private static readonly byte[] RDTSC_32 = new byte[] {
      0x0F, 0x31,                     // rdtsc   
      0xC3                            // ret  
    };

    private static readonly byte[] RDTSC_64 = new byte[] {
      0x0F, 0x31,                     // rdtsc  
      0x48, 0xC1, 0xE2, 0x20,         // shl rdx,20h  
      0x48, 0x0B, 0xC2,               // or rax,rdx  
      0xC3                            // ret  
    };
    
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate bool CpuidDelegate(uint index, uint ecxValue,
      out uint eax, out uint ebx, out uint ecx, out uint edx);

    public static CpuidDelegate Cpuid;


    // void __stdcall cpuidex(unsigned int index, unsigned int ecxValue, 
    //   unsigned int* eax, unsigned int* ebx, unsigned int* ecx, 
    //   unsigned int* edx)
    // {
    //   int info[4];	
    //   __cpuidex(info, index, ecxValue);
    //   *eax = info[0];
    //   *ebx = info[1];
    //   *ecx = info[2];
    //   *edx = info[3];
    // }

    private static readonly byte[] CPUID_32 = new byte[] {
      0x55,                           // push ebp  
      0x8B, 0xEC,                      // mov ebp,esp  
      0x83, 0xEC, 0x10,               // sub esp,10h  
      0x8B, 0x45, 0x08,               // mov eax,dword ptr [ebp+8]  
      0x8B, 0x4D, 0x0C,               // mov ecx,dword ptr [ebp+0Ch]  
      0x53,                           // push ebx  
      0x0F, 0xA2,                     // cpuid  
      0x56,                           // push esi  
      0x8D, 0x75, 0xF0,               // lea esi,[info]  
      0x89, 0x06,                      // mov dword ptr [esi],eax  
      0x8B, 0x45, 0x10,               // mov eax,dword ptr [eax]  
      0x89, 0x5E, 0x04,               // mov dword ptr [esi+4],ebx  
      0x89, 0x4E, 0x08,               // mov dword ptr [esi+8],ecx  
      0x89, 0x56, 0x0C,               // mov dword ptr [esi+0Ch],edx  
      0x8B, 0x4D, 0xF0,               // mov ecx,dword ptr [info]  
      0x89, 0x08,                      // mov dword ptr [eax],ecx  
      0x8B, 0x45, 0x14,               // mov eax,dword ptr [ebx]  
      0x8B, 0x4D, 0xF4,               // mov ecx,dword ptr [ebp-0Ch]  
      0x89, 0x08,                      // mov dword ptr [eax],ecx  
      0x8B, 0x45, 0x18,               // mov eax,dword ptr [ecx]  
      0x8B, 0x4D, 0xF8,               // mov ecx,dword ptr [ebp-8]  
      0x89, 0x08,                      // mov dword ptr [eax],ecx  
      0x8B, 0x45, 0x1C,               // mov eax,dword ptr [edx]  
      0x8B, 0x4D, 0xFC,               // mov ecx,dword ptr [ebp-4]  
      0x5E,                           // pop esi  
      0x89, 0x08,                     // mov dword ptr [eax],ecx  
      0x5B,                           // pop ebx  
      0xC9,                           // leave  
      0xC2, 0x18, 0x00                // ret 18h  
    };
             
    private static readonly byte[] CPUID_64 = new byte[] {
      0x48, 0x89, 0x5C, 0x24, 0x08,   // mov qword ptr [rsp+8],rbx  
      0x8B, 0xC1,                     // mov eax,ecx  
      0x8B, 0xCA,                     // mov ecx,edx  
      0x0F, 0xA2,                     // cpuid  
      0x41, 0x89, 0x00,               // mov dword ptr [r8],eax  
      0x48, 0x8B, 0x44, 0x24, 0x28,   // mov rax,qword ptr [ecx]  
      0x41, 0x89, 0x19,               // mov dword ptr [r9],ebx  
      0x48, 0x8B, 0x5C, 0x24, 0x08,   // mov rbx,qword ptr [rsp+8]  
      0x89, 0x08,                     // mov dword ptr [rax],ecx  
      0x48, 0x8B, 0x44, 0x24, 0x30,   // mov rax,qword ptr [rsp+30h]  
      0x89, 0x10,                     // mov dword ptr [rax],edx  
      0xC3                            // ret  
    };

    public static bool CpuidTx(uint index, uint ecxValue, 
      out uint eax, out uint ebx, out uint ecx, out uint edx, 
      UIntPtr threadAffinityMask) {

      IntPtr thread = NativeMethods.GetCurrentThread();
      UIntPtr mask = NativeMethods.SetThreadAffinityMask(thread, 
        threadAffinityMask);

      if (mask == UIntPtr.Zero) {
        eax = ebx = ecx = edx = 0;
        return false;
      }

      Cpuid(index, ecxValue, out eax, out ebx, out ecx, out edx);

      NativeMethods.SetThreadAffinityMask(thread, mask);
      
      return true;
    }

    [Flags()]
    public enum AllocationType : uint {
      COMMIT = 0x1000,
      RESERVE = 0x2000,
      RESET = 0x80000,
      LARGE_PAGES = 0x20000000,
      PHYSICAL = 0x400000,
      TOP_DOWN = 0x100000,
      WRITE_WATCH = 0x200000
    }

    [Flags()]
    public enum MemoryProtection : uint {
      EXECUTE = 0x10,
      EXECUTE_READ = 0x20,
      EXECUTE_READWRITE = 0x40,
      EXECUTE_WRITECOPY = 0x80,
      NOACCESS = 0x01,
      READONLY = 0x02,
      READWRITE = 0x04,
      WRITECOPY = 0x08,
      GUARD = 0x100,
      NOCACHE = 0x200,
      WRITECOMBINE = 0x400
    }

    [Flags]
    enum FreeType {
      DECOMMIT = 0x4000,
      RELEASE = 0x8000
    }

    private static class NativeMethods {
      private const string KERNEL = "kernel32.dll";

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr VirtualAlloc(IntPtr lpAddress, UIntPtr dwSize,
        AllocationType flAllocationType, MemoryProtection flProtect);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern bool VirtualFree(IntPtr lpAddress, UIntPtr dwSize,
        FreeType dwFreeType);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern UIntPtr
        SetThreadAffinityMask(IntPtr handle, UIntPtr mask);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetCurrentThread();
    }
  }
}
