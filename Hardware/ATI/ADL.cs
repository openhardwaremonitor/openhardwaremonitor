/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace OpenHardwareMonitor.Hardware.ATI {
  
  [StructLayout(LayoutKind.Sequential)]
  internal struct ADLAdapterInfo {
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
  internal struct ADLPMActivity {
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
  internal struct ADLTemperature {
    public int Size;
    public int Temperature;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct ADLFanSpeedValue {
    public int Size;
    public int SpeedType;
    public int FanSpeed;
    public int Flags;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct ADLFanSpeedInfo {
    public int Size;
    public int Flags;
    public int MinPercent;
    public int MaxPercent;
    public int MinRPM;
    public int MaxRPM;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct ADLSingleSensorData {
    public bool Supported;
    public int Value;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct ADLPMLogDataOutput {
    public int Size;
    [MarshalAs(UnmanagedType.ByValArray, SizeConst = ADL.ADL_PMLOG_MAX_SENSORS)]
    public ADLSingleSensorData[] Sensors;
  }

  internal enum ADLODNCurrentPowerType {
    TOTAL_POWER = 0,
    PPT_POWER,
    SOCKET_POWER,
    CHIP_POWER,
  }

  internal enum ADLODNTemperatureType {
    CORE = 1,
    MEMORY = 2,
    VRM_CORE = 3,
    VRM_MEMORY = 4,
    LIQUID = 5,
    PLX = 6,
    HOTSPOT = 7,
  }

  internal enum ADLSensorType {
    CLK_GFXCLK = 1,
    CLK_MEMCLK = 2,
    CLK_SOCCLK = 3,
    CLK_UVDCLK1 = 4,
    CLK_UVDCLK2 = 5,
    CLK_VCECLK = 6,
    CLK_VCNCLK = 7,
    TEMPERATURE_EDGE = 8,
    TEMPERATURE_MEM = 9,
    TEMPERATURE_VRVDDC = 10,
    TEMPERATURE_VRMVDD = 11,
    TEMPERATURE_LIQUID = 12,
    TEMPERATURE_PLX = 13,
    FAN_RPM = 14,
    FAN_PERCENTAGE = 15,
    SOC_VOLTAGE = 16,
    SOC_POWER = 17,
    SOC_CURRENT = 18,
    INFO_ACTIVITY_GFX = 19,
    INFO_ACTIVITY_MEM = 20,
    GFX_VOLTAGE = 21,
    MEM_VOLTAGE = 22,
    ASIC_POWER = 23,
    TEMPERATURE_VRSOC = 24,
    TEMPERATURE_VRMVDD0 = 25,
    TEMPERATURE_VRMVDD1 = 26,
    TEMPERATURE_HOTSPOT = 27,
    TEMPERATURE_GFX = 28,
    TEMPERATURE_SOC = 29,
    GFX_POWER = 30,
    GFX_CURRENT = 31,
    TEMPERATURE_CPU = 32,
    CPU_POWER = 33,
    CLK_CPUCLK = 34,
    THROTTLER_STATUS = 35,
    CLK_VCN1CLK1 = 36,
    CLK_VCN1CLK2 = 37,
    SMART_POWERSHIFT_CPU = 38,
    SMART_POWERSHIFT_DGPU = 39
  }

  internal class ADL {
    public const int ADL_MAX_PATH = 256;
    public const int ADL_MAX_ADAPTERS = 40;
    public const int ADL_MAX_DISPLAYS = 40;
    public const int ADL_MAX_DEVICENAME = 32;
    public const int ADL_OK = 0;
    public const int ADL_ERR = -1;
    public const int ADL_DRIVER_OK = 0;
    public const int ADL_MAX_GLSYNC_PORTS = 8;
    public const int ADL_MAX_GLSYNC_PORT_LEDS = 8;
    public const int ADL_MAX_NUM_DISPLAYMODES = 1024;
    public const int ADL_PMLOG_MAX_SENSORS = 256;

    public const int ADL_DL_FANCTRL_SPEED_TYPE_PERCENT = 1;
    public const int ADL_DL_FANCTRL_SPEED_TYPE_RPM = 2;

    public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_READ = 1;
    public const int ADL_DL_FANCTRL_SUPPORTS_PERCENT_WRITE = 2;
    public const int ADL_DL_FANCTRL_SUPPORTS_RPM_READ = 4;
    public const int ADL_DL_FANCTRL_SUPPORTS_RPM_WRITE = 8;
    public const int ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED = 1;

    public const int ATI_VENDOR_ID = 0x1002;

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
    public delegate int ADL_Overdrive5_FanSpeedToDefault_SetDelegate(
      int adapterIndex, int thermalControllerIndex);
    public delegate int ADL_Overdrive5_FanSpeed_SetDelegate(int adapterIndex,
      int thermalControllerIndex, ref	ADLFanSpeedValue fanSpeedValue);
    public delegate int ADL_Overdrive_CapsDelegate(int adapterIndex,
      out int supported, out int enabled, out int version);
    private delegate int ADL2_Main_Control_CreateDelegate(
      ADL_Main_Memory_AllocDelegate callback, int enumConnectedAdapters, 
      out IntPtr context);
    public delegate int ADL2_Main_Control_DestroyDelegate(IntPtr context);
    public delegate int ADL2_OverdriveN_Temperature_GetDelegate(IntPtr context,
      int adapterIndex, ADLODNTemperatureType temperatureType,
      out int temperature);                        
    public delegate int ADL2_Overdrive6_CurrentPower_GetDelegate(IntPtr context,
      int adapterIndex, ADLODNCurrentPowerType powerType,
      out int currentValue);
    public delegate int ADL2_New_QueryPMLogData_GetDelegate(IntPtr context,
      int adapterIndex, out ADLPMLogDataOutput dataOutput);

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
    public static ADL_Overdrive5_FanSpeedToDefault_SetDelegate
      ADL_Overdrive5_FanSpeedToDefault_Set;
    public static ADL_Overdrive5_FanSpeed_SetDelegate
      ADL_Overdrive5_FanSpeed_Set;
    public static ADL_Overdrive_CapsDelegate 
      ADL_Overdrive_Caps;
    private static ADL2_Main_Control_CreateDelegate
      _ADL2_Main_Control_Create;
    public static ADL2_Main_Control_DestroyDelegate
      ADL2_Main_Control_Destroy;
    public static ADL2_OverdriveN_Temperature_GetDelegate
      ADL2_OverdriveN_Temperature_Get;
    public static ADL2_Overdrive6_CurrentPower_GetDelegate
      ADL2_Overdrive6_CurrentPower_Get;
    public static ADL2_New_QueryPMLogData_GetDelegate
      ADL2_New_QueryPMLogData_Get;

    private static string dllName;

    private static void GetDelegate<T>(string entryPoint, out T newDelegate)
      where T : class 
    {
      DllImportAttribute attribute = new DllImportAttribute(dllName);
      attribute.CallingConvention = CallingConvention.Cdecl;
      attribute.PreserveSig = true;
      attribute.EntryPoint = entryPoint;
      PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
    }

    private static void CreateDelegates(string name) {
      int p = (int)Environment.OSVersion.Platform;
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
      GetDelegate("ADL_Overdrive5_FanSpeedToDefault_Set",
        out ADL_Overdrive5_FanSpeedToDefault_Set);
      GetDelegate("ADL_Overdrive5_FanSpeed_Set",
        out ADL_Overdrive5_FanSpeed_Set);
      GetDelegate("ADL_Overdrive_Caps", 
        out ADL_Overdrive_Caps);
      GetDelegate("ADL2_Main_Control_Create",
        out _ADL2_Main_Control_Create);
      GetDelegate("ADL2_Main_Control_Destroy",
        out ADL2_Main_Control_Destroy);
      GetDelegate("ADL2_OverdriveN_Temperature_Get",
        out ADL2_OverdriveN_Temperature_Get);
      GetDelegate("ADL2_Overdrive6_CurrentPower_Get",
        out ADL2_Overdrive6_CurrentPower_Get);
      GetDelegate("ADL2_New_QueryPMLogData_Get",
        out ADL2_New_QueryPMLogData_Get);
  }

    static ADL() {
      CreateDelegates("atiadlxx");
    }

    private ADL() { }

    public static int ADL_Main_Control_Create(int enumConnectedAdapters) {
      try {
        try {
          return _ADL_Main_Control_Create(Main_Memory_Alloc,
            enumConnectedAdapters);
        } catch {
          CreateDelegates("atiadlxy");
          return _ADL_Main_Control_Create(Main_Memory_Alloc,
            enumConnectedAdapters);
        }
      } catch {
        return ADL_ERR;
      }
    }

    public static int ADL2_Main_Control_Create(int enumConnectedAdapters,
      out IntPtr context) 
    {
      try {
        var result = _ADL2_Main_Control_Create(Main_Memory_Alloc,
          enumConnectedAdapters, out context);
        if (result != ADL.ADL_OK)
          context = IntPtr.Zero;
        return result;
      } catch {
        context = IntPtr.Zero;
        return ADL_ERR;
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

      // the ADLAdapterInfo.VendorID field reported by ADL is wrong on 
      // Windows systems (parse error), so we fix this here
      for (int i = 0; i < info.Length; i++) {
        // try Windows UDID format
        Match m = Regex.Match(info[i].UDID, "PCI_VEN_([A-Fa-f0-9]{1,4})&.*");
        if (m.Success && m.Groups.Count == 2) {
          info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 16);
          continue;
        }
        // if above failed, try Unix UDID format
        m = Regex.Match(info[i].UDID, "[0-9]+:[0-9]+:([0-9]+):[0-9]+:[0-9]+");
        if (m.Success && m.Groups.Count == 2) {
          info[i].VendorID = Convert.ToInt32(m.Groups[1].Value, 10);
        }
      }

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

    // create a Main_Memory_Alloc delegate and keep it alive
    private static ADL_Main_Memory_AllocDelegate Main_Memory_Alloc =
      delegate(int size) {
        return Marshal.AllocHGlobal(size);
      };

    private static void Main_Memory_Free(IntPtr buffer) {
      if (IntPtr.Zero != buffer)
        Marshal.FreeHGlobal(buffer);
    }
  }
}
