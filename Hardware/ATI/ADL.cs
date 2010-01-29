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
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware.ATI {
  
  [StructLayout(LayoutKind.Sequential)]
  public struct ADLAdapterInfo {
    public int Size;
    public int AdapterIndex;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string UDID;
    public int BusNumber;
    public int DeviceNumber;
    public int FunctionNumber;
    public int VendorID;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string AdapterName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string DisplayName;
    public int Present;
    public int Exist;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string DriverPath;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string DriverPathExt;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = ADL.ADL_MAX_PATH)]
    public string PNPString;
    public int OSDisplayIndex;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ADLPMActivity {
    public int Size;
    public int EngineClock;
    public int MemoryClock;
    public int Vddc;
    public int ActivityPercent;
    public int CurrentPerformanceLevel;
    public int CurrentBusSpeed;
    public int CurrentBusLanes;
    public int MaximumBusLanes;
    public int Reserved;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ADLTemperature {
    public int Size;
    public int Temperature;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ADLFanSpeedValue {
    public int Size;
    public int SpeedType;
    public int FanSpeed;
    public int Flags;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct ADLFanSpeedInfo {
    public int Size;
    public int Flags;
    public int MinPercent;
    public int MaxPercent;
    public int MinRPM;
    public int MaxRPM;
  }

  public class ADL {
    public const int ADL_MAX_PATH = 256;
    public const int ADL_MAX_ADAPTERS = 40;
    public const int ADL_MAX_DISPLAYS = 40;
    public const int ADL_MAX_DEVICENAME = 32;
    public const int ADL_OK = 0;
    public const int ADL_FAIL = -1;
    public const int ADL_DRIVER_OK = 0;
    public const int ADL_MAX_GLSYNC_PORTS = 8;
    public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
    public const int ADL_MAX_NUM_DISPLAYMODES = 1024;

    public const int ADL_DL_FANCTRL_SPEED_TYPE_PERCENT = 1;
    public const int ADL_DL_FANCTRL_SPEED_TYPE_RPM = 2;

    public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_READ = 1;
    public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_WRITE = 2;
    public const int ADL_DL_FANCTRL_SUPPORTS_RPM_READ = 4;
    public const int ADL_DL_FANCTRL_SUPPORTS_RPM_WRITE = 8;

    public const int ATI_VENDOR_ID1 = 1002;
    public const int ATI_VENDOR_ID2 = 0x1002;

    private delegate int ADL_Main_Control_CreateDelegate(
      ADL_Main_Memory_AllocDelegate callback, int enumConnectedAdapters);
    private delegate int ADL_Adapter_AdapterInfo_GetDelegate(IntPtr info,
      int size);

    public delegate int ADL_Main_Control_DestroyDelegate();
    public delegate int ADL_Adapter_NumberOfAdapters_GetDelegate(
      ref int numAdapters);    
    public delegate int ADL_Adapter_ID_GetDelegate(int adapterIndex,
      out int adapterID);
    public delegate int ADL_Display_AdapterID_GetDelegate(int adapterIndex,
      out int adapterID);      	
    public delegate int ADL_Adapter_Active_GetDelegate(int adapterIndex,
      out int status);
    public delegate int ADL_Overdrive5_CurrentActivity_GetDelegate(
      int iAdapterIndex, ref ADLPMActivity activity);
    public delegate int ADL_Overdrive5_Temperature_GetDelegate(int adapterIndex,
        int thermalControllerIndex, ref ADLTemperature temperature);
    public delegate int ADL_Overdrive5_FanSpeed_GetDelegate(int adapterIndex,
        int thermalControllerIndex, ref	ADLFanSpeedValue fanSpeedValue);
    public delegate int ADL_Overdrive5_FanSpeedInfo_GetDelegate(
      int adapterIndex, int thermalControllerIndex,
      ref ADLFanSpeedInfo fanSpeedInfo);

    private static ADL_Main_Control_CreateDelegate
      _ADL_Main_Control_Create;
    private static ADL_Adapter_AdapterInfo_GetDelegate
      _ADL_Adapter_AdapterInfo_Get;

    public static ADL_Main_Control_DestroyDelegate 
      ADL_Main_Control_Destroy;
    public static ADL_Adapter_NumberOfAdapters_GetDelegate
      ADL_Adapter_NumberOfAdapters_Get;
    public static ADL_Adapter_ID_GetDelegate 
      _ADL_Adapter_ID_Get;
    public static ADL_Display_AdapterID_GetDelegate 
      _ADL_Display_AdapterID_Get;
    public static ADL_Adapter_Active_GetDelegate 
      ADL_Adapter_Active_Get;
    public static ADL_Overdrive5_CurrentActivity_GetDelegate
      ADL_Overdrive5_CurrentActivity_Get;
    public static ADL_Overdrive5_Temperature_GetDelegate
      ADL_Overdrive5_Temperature_Get;
    public static ADL_Overdrive5_FanSpeed_GetDelegate
      ADL_Overdrive5_FanSpeed_Get;
    public static ADL_Overdrive5_FanSpeedInfo_GetDelegate
      ADL_Overdrive5_FanSpeedInfo_Get;

    private static string dllName;

    private static void GetDelegate<T>(string entryPoint, out T newDelegate)
      where T : class 
    {
      DllImportAttribute attribute = new DllImportAttribute(dllName);
      attribute.CallingConvention = CallingConvention.StdCall;
      attribute.PreserveSig = true;
      attribute.EntryPoint = entryPoint;
      PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
    }

    private static void CreateDelegates(string name) {
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        dllName = name + ".so";
      else
        dllName = name + ".dll";

      GetDelegate("ADL_Main_Control_Create",
        out _ADL_Main_Control_Create);
      GetDelegate("ADL_Adapter_AdapterInfo_Get",
        out _ADL_Adapter_AdapterInfo_Get);
      GetDelegate("ADL_Main_Control_Destroy",
        out ADL_Main_Control_Destroy);
      GetDelegate("ADL_Adapter_NumberOfAdapters_Get",
        out ADL_Adapter_NumberOfAdapters_Get);
      GetDelegate("ADL_Adapter_ID_Get",
        out _ADL_Adapter_ID_Get);
      GetDelegate("ADL_Display_AdapterID_Get", 
        out _ADL_Display_AdapterID_Get);
      GetDelegate("ADL_Adapter_Active_Get",
        out ADL_Adapter_Active_Get);
      GetDelegate("ADL_Overdrive5_CurrentActivity_Get",
        out ADL_Overdrive5_CurrentActivity_Get);
      GetDelegate("ADL_Overdrive5_Temperature_Get",
        out ADL_Overdrive5_Temperature_Get);
      GetDelegate("ADL_Overdrive5_FanSpeed_Get",
        out ADL_Overdrive5_FanSpeed_Get);
      GetDelegate("ADL_Overdrive5_FanSpeedInfo_Get",
        out ADL_Overdrive5_FanSpeedInfo_Get);
    }

    static ADL() {
      CreateDelegates("atiadlxx");
    }

    private ADL() { }

    public static int ADL_Main_Control_Create(int enumConnectedAdapters) {
      try {
        return _ADL_Main_Control_Create(Main_Memory_Alloc,
          enumConnectedAdapters);
      } catch (DllNotFoundException) {
        CreateDelegates("atiadlxy");
        return _ADL_Main_Control_Create(Main_Memory_Alloc,
          enumConnectedAdapters);
      }
    }

    public static int ADL_Adapter_AdapterInfo_Get(ADLAdapterInfo[] info) {
      int elementSize = Marshal.SizeOf(typeof(ADLAdapterInfo));
      int size = info.Length * elementSize;
      IntPtr ptr = Marshal.AllocHGlobal(size);
      int result = _ADL_Adapter_AdapterInfo_Get(ptr, size);
      for (int i = 0; i < info.Length; i++)
        info[i] = (ADLAdapterInfo)
          Marshal.PtrToStructure((IntPtr)((long)ptr + i * elementSize),
          typeof(ADLAdapterInfo));
      Marshal.FreeHGlobal(ptr);
      return result;
    }

    public static int ADL_Adapter_ID_Get(int adapterIndex,
      out int adapterID) {
      try {
        return _ADL_Adapter_ID_Get(adapterIndex, out adapterID);
      } catch (EntryPointNotFoundException) {
        try {
          return _ADL_Display_AdapterID_Get(adapterIndex, out adapterID);
        } catch (EntryPointNotFoundException) {
          adapterID = 1;
          return ADL_OK;
        }
      }
    }

    private delegate IntPtr ADL_Main_Memory_AllocDelegate(int size);

    private static IntPtr Main_Memory_Alloc(int size) {
      return Marshal.AllocHGlobal(size);;
    }

    private static void Main_Memory_Free(IntPtr buffer) {
      if (IntPtr.Zero != buffer)
        Marshal.FreeHGlobal(buffer);
    }
  }
}
