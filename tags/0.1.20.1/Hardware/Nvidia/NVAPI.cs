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
using System.Text;

namespace OpenHardwareMonitor.Hardware.Nvidia {

  public enum NvStatus {
    OK = 0,
    ERROR = -1,
    LIBRARY_NOT_FOUND = -2,
    NO_IMPLEMENTATION = -3,
    API_NOT_INTIALIZED = -4,
    INVALID_ARGUMENT = -5,
    NVIDIA_DEVICE_NOT_FOUND = -6,
    END_ENUMERATION = -7,
    INVALID_HANDLE = -8,
    INCOMPATIBLE_STRUCT_VERSION = -9,
    HANDLE_INVALIDATED = -10,
    OPENGL_CONTEXT_NOT_CURRENT = -11,
    NO_GL_EXPERT = -12,
    INSTRUMENTATION_DISABLED = -13,
    EXPECTED_LOGICAL_GPU_HANDLE = -100,
    EXPECTED_PHYSICAL_GPU_HANDLE = -101,
    EXPECTED_DISPLAY_HANDLE = -102,
    INVALID_COMBINATION = -103,
    NOT_SUPPORTED = -104,
    PORTID_NOT_FOUND = -105,
    EXPECTED_UNATTACHED_DISPLAY_HANDLE = -106,
    INVALID_PERF_LEVEL = -107,
    DEVICE_BUSY = -108,
    NV_PERSIST_FILE_NOT_FOUND = -109,
    PERSIST_DATA_NOT_FOUND = -110,
    EXPECTED_TV_DISPLAY = -111,
    EXPECTED_TV_DISPLAY_ON_DCONNECTOR = -112,
    NO_ACTIVE_SLI_TOPOLOGY = -113,
    SLI_RENDERING_MODE_NOTALLOWED = -114,
    EXPECTED_DIGITAL_FLAT_PANEL = -115,
    ARGUMENT_EXCEED_MAX_SIZE = -116,
    DEVICE_SWITCHING_NOT_ALLOWED = -117,
    TESTING_CLOCKS_NOT_SUPPORTED = -118,
    UNKNOWN_UNDERSCAN_CONFIG = -119,
    TIMEOUT_RECONFIGURING_GPU_TOPO = -120,
    DATA_NOT_FOUND = -121,
    EXPECTED_ANALOG_DISPLAY = -122,
    NO_VIDLINK = -123,
    REQUIRES_REBOOT = -124,
    INVALID_HYBRID_MODE = -125,
    MIXED_TARGET_TYPES = -126,
    SYSWOW64_NOT_SUPPORTED = -127,
    IMPLICIT_SET_GPU_TOPOLOGY_CHANGE_NOT_ALLOWED = -128,
    REQUEST_USER_TO_CLOSE_NON_MIGRATABLE_APPS = -129,
    OUT_OF_MEMORY = -130,
    WAS_STILL_DRAWING = -131,
    FILE_NOT_FOUND = -132,
    TOO_MANY_UNIQUE_STATE_OBJECTS = -133,
    INVALID_CALL = -134,
    D3D10_1_LIBRARY_NOT_FOUND = -135,
    FUNCTION_NOT_FOUND = -136
  }  

  public enum NvThermalController {
    NONE = 0,
    GPU_INTERNAL,  
    ADM1032,
    MAX6649,       
    MAX1617,      
    LM99,      
    LM89,         
    LM64,         
    ADT7473,
    SBMAX6649,
    VBIOSEVT,  
    OS,    
    UNKNOWN = -1,
  } 

  public enum NvThermalTarget {
    NONE = 0,
    GPU = 1,
    MEMORY = 2,
    POWER_SUPPLY = 4,
    BOARD = 8,
    ALL = 15,
    UNKNOWN = -1
  };

  [StructLayout(LayoutKind.Sequential)]
  public struct NvSensor {
    public NvThermalController Controller;
    public int DefaultMinTemp;
    public int DefaultMaxTemp;
    public int CurrentTemp;
    public NvThermalTarget Target;     
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NvGPUThermalSettings {
    public int Version;
    public int Count;
    [MarshalAs(UnmanagedType.ByValArray, 
      SizeConst = NVAPI.MAX_THERMAL_SENSORS_PER_GPU)]
    public NvSensor[] Sensor;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NvDisplayHandle {
    private int handle;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct NvPhysicalGpuHandle {
    private int handle;
  }

  public class NVAPI {
          
    private const int SHORT_STRING_MAX = 64;

    public const int MAX_THERMAL_SENSORS_PER_GPU = 3;
    public const int MAX_PHYSICAL_GPUS = 64;
    public static readonly int GPU_THERMAL_SETTINGS_VER =
      Marshal.SizeOf(typeof(NvGPUThermalSettings)) | 0x10000;
            
    private delegate IntPtr nvapi_QueryInterfaceDelegate(uint id);
    private delegate NvStatus NvAPI_InitializeDelegate();
    private delegate NvStatus NvAPI_GPU_GetFullNameDelegate(
      NvPhysicalGpuHandle gpuHandle, StringBuilder name);

    public delegate NvStatus NvAPI_GPU_GetThermalSettingsDelegate(
      NvPhysicalGpuHandle gpuHandle, int sensorIndex, 
      ref NvGPUThermalSettings nvGPUThermalSettings);
    public delegate NvStatus NvAPI_EnumNvidiaDisplayHandleDelegate(int thisEnum,
      ref NvDisplayHandle displayHandle);
    public delegate NvStatus NvAPI_GetPhysicalGPUsFromDisplayDelegate(
      NvDisplayHandle displayHandle, [Out] NvPhysicalGpuHandle[] gpuHandles, 
      out int gpuCount);
    public delegate NvStatus NvAPI_EnumPhysicalGPUsDelegate(
      [Out] NvPhysicalGpuHandle[] gpuHandles, out int gpuCount);
    public delegate NvStatus NvAPI_GPU_GetTachReadingDelegate(
      NvPhysicalGpuHandle gpuHandle, out int value);

    private static bool available = false;
    private static nvapi_QueryInterfaceDelegate nvapi_QueryInterface;
    private static NvAPI_InitializeDelegate NvAPI_Initialize;
    private static NvAPI_GPU_GetFullNameDelegate _NvAPI_GPU_GetFullName;

    public static NvAPI_GPU_GetThermalSettingsDelegate 
      NvAPI_GPU_GetThermalSettings;
    public static NvAPI_EnumNvidiaDisplayHandleDelegate
      NvAPI_EnumNvidiaDisplayHandle;
    public static NvAPI_GetPhysicalGPUsFromDisplayDelegate
      NvAPI_GetPhysicalGPUsFromDisplay;
    public static NvAPI_EnumPhysicalGPUsDelegate
      NvAPI_EnumPhysicalGPUs;
    public static NvAPI_GPU_GetTachReadingDelegate
      NvAPI_GPU_GetTachReading;

    public static NvStatus NvAPI_GPU_GetFullName(NvPhysicalGpuHandle gpuHandle,
      out string name) {
      StringBuilder builder = new StringBuilder(SHORT_STRING_MAX);
      NvStatus status = _NvAPI_GPU_GetFullName(gpuHandle, builder);
      name = builder.ToString();
      return status;
    }

    private static string GetDllName() {
      if (IntPtr.Size == 4) {
        return "nvapi.dll";
      } else {
        return "nvapi64.dll";
      }
    }

    private static void GetDelegate<T>(uint id, out T newDelegate) 
      where T : class 
    {
      IntPtr ptr = nvapi_QueryInterface(id);
      if (ptr != IntPtr.Zero) {
        newDelegate =
          Marshal.GetDelegateForFunctionPointer(ptr, typeof(T)) as T;
      } else {
        newDelegate = null;
      }
    }

    static NVAPI() { 
      DllImportAttribute attribute = new DllImportAttribute(GetDllName());
      attribute.CallingConvention = CallingConvention.Cdecl;
      attribute.PreserveSig = true;
      attribute.EntryPoint = "nvapi_QueryInterface";
      PInvokeDelegateFactory.CreateDelegate(attribute,
        out nvapi_QueryInterface);

      try {
        GetDelegate(0x0150E828, out NvAPI_Initialize);
      } catch (DllNotFoundException) { return; } 
        catch (ArgumentNullException) { return; }

      if (NvAPI_Initialize() == NvStatus.OK) {
        GetDelegate(0xE3640A56, out NvAPI_GPU_GetThermalSettings);
        GetDelegate(0xCEEE8E9F, out _NvAPI_GPU_GetFullName);
        GetDelegate(0x9ABDD40D, out NvAPI_EnumNvidiaDisplayHandle);
        GetDelegate(0x34EF9506, out NvAPI_GetPhysicalGPUsFromDisplay);
        GetDelegate(0xE5AC921F, out NvAPI_EnumPhysicalGPUs);
        GetDelegate(0x5F608315, out NvAPI_GPU_GetTachReading);        
        available = true;
      }
    }

    public static bool IsAvailable {
      get { return available; }
    }

  }
}
