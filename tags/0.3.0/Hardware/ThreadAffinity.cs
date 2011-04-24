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
  
  internal static class ThreadAffinity {
  
    public static ulong Set(ulong mask) { 
      if (mask == 0)
        return 0;
        
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) { // Unix
        ulong result = 0;
        if (NativeMethods.sched_getaffinity(0, (IntPtr)Marshal.SizeOf(result), 
          ref result) != 0)          
          return 0;
        if (NativeMethods.sched_setaffinity(0, (IntPtr)Marshal.SizeOf(mask), 
          ref mask) != 0)
          return 0;
        return result;
      } else { // Windows      
        return (ulong)NativeMethods.SetThreadAffinityMask(
          NativeMethods.GetCurrentThread(), (UIntPtr)mask);
      }
    }
  
    private static class NativeMethods {      
      private const string KERNEL = "kernel32.dll";

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern UIntPtr
        SetThreadAffinityMask(IntPtr handle, UIntPtr mask);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetCurrentThread();       
      
      private const string LIBC = "libc";
      
      [DllImport(LIBC)]
      public static extern int sched_getaffinity(int pid, IntPtr maskSize,
        ref ulong mask);
      
      [DllImport(LIBC)]
      public static extern int sched_setaffinity(int pid, IntPtr maskSize,
        ref ulong mask);  
    }  
  }
}

