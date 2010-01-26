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
using System.Runtime.InteropServices;
using System.Text;

namespace OpenHardwareMonitor.Hardware {

  public class WinRing0 {
    
    public enum OlsDllStatus{
      OLS_DLL_NO_ERROR                        = 0,
      OLS_DLL_UNSUPPORTED_PLATFORM            = 1,
      OLS_DLL_DRIVER_NOT_LOADED               = 2,
      OLS_DLL_DRIVER_NOT_FOUND                = 3,
      OLS_DLL_DRIVER_UNLOADED                 = 4,
      OLS_DLL_DRIVER_NOT_LOADED_ON_NETWORK    = 5,
      OLS_DLL_UNKNOWN_ERROR                   = 9
    }

    private static bool available = false;

    private static string GetDllName() {   
      int p = (int)System.Environment.OSVersion.Platform;
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
    
    public delegate uint GetDllStatusDelegate();
    public delegate bool IsCpuidDelegate();
    public delegate bool CpuidDelegate(uint index, out uint eax, out uint ebx, 
      out uint ecx, out uint edx);
    public delegate bool RdmsrPxDelegate(uint index, ref uint eax, ref uint edx, 
      UIntPtr processAffinityMask);
    public delegate byte ReadIoPortByteDelegate(ushort port);
    public delegate void WriteIoPortByteDelegate(ushort port, byte value);
    public delegate void SetPciMaxBusIndexDelegate(byte max);
    public delegate uint FindPciDeviceByIdDelegate(ushort vendorId, 
      ushort deviceId, byte index);
    public delegate bool ReadPciConfigDwordExDelegate(uint pciAddress, 
      uint regAddress, out uint value);

    private static InitializeOlsDelegate InitializeOls;
    private static DeinitializeOlsDelegate DeinitializeOls;

    public static GetDllStatusDelegate GetDllStatus;
    public static IsCpuidDelegate IsCpuid;
    public static CpuidDelegate Cpuid;
    public static RdmsrPxDelegate RdmsrPx;
    public static ReadIoPortByteDelegate ReadIoPortByte;
    public static WriteIoPortByteDelegate WriteIoPortByte;
    public static SetPciMaxBusIndexDelegate SetPciMaxBusIndex;
    public static FindPciDeviceByIdDelegate FindPciDeviceById;
    public static ReadPciConfigDwordExDelegate ReadPciConfigDwordEx;

    private static void GetDelegate<T>(string entryPoint, out T newDelegate) 
      where T : class 
    {
      DllImportAttribute attribute = new DllImportAttribute(GetDllName());
      attribute.CallingConvention = CallingConvention.Winapi;
      attribute.PreserveSig = true;
      attribute.EntryPoint = entryPoint;
      attribute.CharSet = CharSet.Auto;
      PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
    }

    static WinRing0() {
      GetDelegate("InitializeOls", out InitializeOls);
      GetDelegate("DeinitializeOls", out DeinitializeOls);
      GetDelegate("GetDllStatus", out GetDllStatus);
      GetDelegate("IsCpuid", out IsCpuid);
      GetDelegate("Cpuid", out Cpuid);
      GetDelegate("RdmsrPx", out  RdmsrPx);
      GetDelegate("ReadIoPortByte", out ReadIoPortByte);
      GetDelegate("WriteIoPortByte", out WriteIoPortByte);
      GetDelegate("SetPciMaxBusIndex", out SetPciMaxBusIndex);
      GetDelegate("FindPciDeviceById", out FindPciDeviceById);
      GetDelegate("ReadPciConfigDwordEx", out ReadPciConfigDwordEx);

      try {
        if (InitializeOls != null && InitializeOls())
          available = true;
      } catch (DllNotFoundException) { }       
    }
    
    public static bool IsAvailable {
      get { return available; }
    }

    private static Deinitializer deinitializer = new Deinitializer();
    private class Deinitializer {
      ~Deinitializer() {
        if (available)
          DeinitializeOls();
      }
    }
  }
}
