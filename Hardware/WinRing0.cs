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
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenHardwareMonitor.Hardware {

  internal class WinRing0 {

    private WinRing0() { }
    
    public enum OlsDllStatus{
      OLS_DLL_NO_ERROR                        = 0,
      OLS_DLL_UNSUPPORTED_PLATFORM            = 1,
      OLS_DLL_DRIVER_NOT_LOADED               = 2,
      OLS_DLL_DRIVER_NOT_FOUND                = 3,
      OLS_DLL_DRIVER_UNLOADED                 = 4,
      OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK    = 5,
      OLS_DLL_UNKNOWN_ERROR                   = 9
    }

    private static bool available;
    private static Mutex isaBusMutex;

    private static string GetDllName() {   
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) {
        if (IntPtr.Size == 4) {
          return "libring0.so";
        } else {
          return "libring0x64.so";
        }
      } else {
        if (IntPtr.Size == 4) {
          return "WinRing0.dll";
        } else {
          return "WinRing0x64.dll";
        }
      }                       
    }
    
    private delegate bool InitializeOlsDelegate();
    private delegate void DeinitializeOlsDelegate();
    
    public delegate bool IsCpuidDelegate();
    public delegate bool CpuidTxDelegate(uint index, uint ecxValue,
      out uint eax, out uint ebx, out uint ecx, out uint edx,
      UIntPtr threadAffinityMask);
    public delegate bool RdmsrDelegate(uint index, out uint eax, out uint edx);
    public delegate bool RdmsrTxDelegate(uint index, out uint eax, out uint edx,
      UIntPtr threadAffinityMask);
    public delegate byte ReadIoPortByteDelegate(ushort port);
    public delegate void WriteIoPortByteDelegate(ushort port, byte value);
    public delegate bool ReadPciConfigDwordExDelegate(uint pciAddress, 
      uint regAddress, out uint value);
    public delegate bool WritePciConfigDwordExDelegate(uint pciAddress, 
      uint regAddress, uint value);
    public delegate bool RdtscTxDelegate(out uint eax, out uint edx,
      UIntPtr threadAffinityMask);
    public delegate bool RdtscDelegate(out uint eax, out uint edx);

    private static readonly InitializeOlsDelegate InitializeOls = 
      CreateDelegate<InitializeOlsDelegate>("InitializeOls");
    private static readonly DeinitializeOlsDelegate DeinitializeOls =
      CreateDelegate<DeinitializeOlsDelegate>("DeinitializeOls");

    public static readonly IsCpuidDelegate IsCpuid =
      CreateDelegate<IsCpuidDelegate>("IsCpuid");
    public static readonly CpuidTxDelegate CpuidTx =
      CreateDelegate<CpuidTxDelegate>("CpuidTx");
    public static readonly RdmsrDelegate Rdmsr =
      CreateDelegate<RdmsrDelegate>("Rdmsr");
    public static readonly RdmsrTxDelegate RdmsrTx =
      CreateDelegate<RdmsrTxDelegate>("RdmsrTx");
    public static readonly ReadIoPortByteDelegate ReadIoPortByte =
      CreateDelegate<ReadIoPortByteDelegate>("ReadIoPortByte");
    public static readonly WriteIoPortByteDelegate WriteIoPortByte =
      CreateDelegate<WriteIoPortByteDelegate>("WriteIoPortByte");
    public static readonly ReadPciConfigDwordExDelegate ReadPciConfigDwordEx =
      CreateDelegate<ReadPciConfigDwordExDelegate>("ReadPciConfigDwordEx");
    public static readonly WritePciConfigDwordExDelegate WritePciConfigDwordEx =
      CreateDelegate<WritePciConfigDwordExDelegate>("WritePciConfigDwordEx");
    public static readonly RdtscTxDelegate RdtscTx =
      CreateDelegate<RdtscTxDelegate>("RdtscTx");
    public static readonly RdtscDelegate Rdtsc =
      CreateDelegate<RdtscDelegate>("Rdtsc");
 
    private static T CreateDelegate<T>(string entryPoint) where T : class {
      DllImportAttribute attribute = new DllImportAttribute(GetDllName());
      attribute.CallingConvention = CallingConvention.Winapi;
      attribute.PreserveSig = true;
      attribute.EntryPoint = entryPoint;
      attribute.CharSet = CharSet.Auto;
      T result;
      PInvokeDelegateFactory.CreateDelegate(attribute, out result);
      return result;
    }

    public static void Open() {
      try {
        if (InitializeOls != null && InitializeOls())
          available = true;
      } catch (DllNotFoundException) { }   
      
      isaBusMutex = new Mutex(false, "Access_ISABUS.HTP.Method");
    }

    public static bool IsAvailable {
      get { return available; }
    }

    public static void Close() {
      if (available)
        DeinitializeOls();        
      isaBusMutex.Close();      
    }    

    public static bool WaitIsaBusMutex(int millisecondsTimeout) {
      try {
        return isaBusMutex.WaitOne(millisecondsTimeout, false);
      } catch (AbandonedMutexException) { return false; } 
        catch (InvalidOperationException) { return false; }     
    }

    public static void ReleaseIsaBusMutex() {
      isaBusMutex.ReleaseMutex();
    }

    public const uint InvalidPciAddress = 0xFFFFFFFF;

    public static uint GetPciAddress(byte bus, byte device, byte function)	{
      return 
        (uint)(((bus & 0xFF) << 8) | ((device & 0x1F) << 3) | (function & 7));
    }
  }
}
