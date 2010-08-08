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

namespace OpenHardwareMonitor.Hardware.TBalancer {

  internal enum FT_DEVICE : uint {
    FT_DEVICE_BM,
    FT_DEVICE_AM,
    FT_DEVICE_100AX,
    FT_DEVICE_UNKNOWN,
    FT_DEVICE_2232,
    FT_DEVICE_232R,
    FT_DEVICE_2232H,
    FT_DEVICE_4232H
  }

  internal enum FT_STATUS {
    FT_OK,
    FT_INVALID_HANDLE,
    FT_DEVICE_NOT_FOUND,
    FT_DEVICE_NOT_OPENED,
    FT_IO_ERROR,
    FT_INSUFFICIENT_RESOURCES,
    FT_INVALID_PARAMETER,
    FT_INVALID_BAUD_RATE,
    FT_DEVICE_NOT_OPENED_FOR_ERASE,
    FT_DEVICE_NOT_OPENED_FOR_WRITE,
    FT_FAILED_TO_WRITE_DEVICE,
    FT_EEPROM_READ_FAILED,
    FT_EEPROM_WRITE_FAILED,
    FT_EEPROM_ERASE_FAILED,
    FT_EEPROM_NOT_PRESENT,
    FT_EEPROM_NOT_PROGRAMMED,
    FT_INVALID_ARGS,
    FT_OTHER_ERROR
  }

  internal enum FT_FLOW_CONTROL : ushort {
    FT_FLOW_DTR_DSR = 512,
    FT_FLOW_NONE = 0,
    FT_FLOW_RTS_CTS = 256,
    FT_FLOW_XON_XOFF = 1024,
  }

  internal enum FT_PURGE : uint {
    FT_PURGE_RX = 1,
    FT_PURGE_TX = 2,
    FT_PURGE_ALL = 3,
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct FT_HANDLE {
    private uint handle;
  }

  [StructLayout(LayoutKind.Sequential)]
  internal struct FT_DEVICE_INFO_NODE {    
    public uint Flags;
    public FT_DEVICE Type; 
    public uint ID; 
    public uint LocId; 
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
    public string SerialNumber; 
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string Description;
    public FT_HANDLE Handle;
  }

  internal class FTD2XX {

    public delegate FT_STATUS FT_CreateDeviceInfoListDelegate(
      out uint numDevices);
    public delegate FT_STATUS FT_GetDeviceInfoListDelegate(
      [Out] FT_DEVICE_INFO_NODE[] deviceInfoNodes, ref uint length);
    public delegate FT_STATUS FT_OpenDelegate(int device, out FT_HANDLE handle);
    public delegate FT_STATUS FT_CloseDelegate(FT_HANDLE handle);
    public delegate FT_STATUS FT_SetBaudRateDelegate(FT_HANDLE handle,
      uint baudRate);
    public delegate FT_STATUS FT_SetDataCharacteristicsDelegate(
      FT_HANDLE handle, byte wordLength, byte stopBits, byte parity);
    public delegate FT_STATUS FT_SetFlowControlDelegate(FT_HANDLE handle,
      FT_FLOW_CONTROL flowControl, byte xon, byte xoff);
    public delegate FT_STATUS FT_SetTimeoutsDelegate(FT_HANDLE handle,
      uint readTimeout, uint writeTimeout);
    public delegate FT_STATUS FT_WriteDelegate(FT_HANDLE handle, byte[] buffer,
      uint bytesToWrite, out uint bytesWritten);
    public delegate FT_STATUS FT_PurgeDelegate(FT_HANDLE handle, FT_PURGE mask);
    public delegate FT_STATUS FT_GetStatusDelegate(FT_HANDLE handle,
      out uint amountInRxQueue, out uint amountInTxQueue, out uint eventStatus);
    public delegate FT_STATUS FT_ReadDelegate(FT_HANDLE handle, 
      [Out] byte[] buffer, uint bytesToRead, out uint bytesReturned);

    public static FT_CreateDeviceInfoListDelegate FT_CreateDeviceInfoList;
    public static FT_GetDeviceInfoListDelegate FT_GetDeviceInfoList;
    public static FT_OpenDelegate FT_Open;
    public static FT_CloseDelegate FT_Close;
    public static FT_SetBaudRateDelegate FT_SetBaudRate;
    public static FT_SetDataCharacteristicsDelegate FT_SetDataCharacteristics;
    public static FT_SetFlowControlDelegate FT_SetFlowControl;
    public static FT_SetTimeoutsDelegate FT_SetTimeouts;
    public static FT_WriteDelegate FT_Write;
    public static FT_PurgeDelegate FT_Purge;
    public static FT_GetStatusDelegate FT_GetStatus;
    public static FT_ReadDelegate FT_Read;

    public static FT_STATUS Write(FT_HANDLE handle, byte[] buffer) {
      uint bytesWritten;
      FT_STATUS status = FT_Write(handle, buffer, (uint)buffer.Length, 
        out bytesWritten);
      if (bytesWritten != buffer.Length)
        return FT_STATUS.FT_FAILED_TO_WRITE_DEVICE;
      else
        return status;
    }

    public static int BytesToRead(FT_HANDLE handle) {
      uint amountInRxQueue;
      uint amountInTxQueue;
      uint eventStatus;
      if (FT_GetStatus(handle, out amountInRxQueue, out amountInTxQueue,
        out eventStatus) == FT_STATUS.FT_OK) {
        return (int)amountInRxQueue;
      } else {
        return 0;
      }
    }

    public static byte ReadByte(FT_HANDLE handle) {
      byte[] buffer = new byte[1];
      uint bytesReturned;
      FT_STATUS status = FT_Read(handle, buffer, 1, out bytesReturned);
      if (status != FT_STATUS.FT_OK || bytesReturned != 1)
        throw new Exception();
      return buffer[0];
    }

    private static string dllName;

    private static void GetDelegate<T>(string entryPoint, out T newDelegate)
      where T : class {
      DllImportAttribute attribute = new DllImportAttribute(dllName);
      attribute.CallingConvention = CallingConvention.StdCall;
      attribute.PreserveSig = true;
      attribute.EntryPoint = entryPoint;
      PInvokeDelegateFactory.CreateDelegate(attribute, out newDelegate);
    }

    static FTD2XX() {
      int p = (int)System.Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        dllName = "libftd2xx.so";
      else
        dllName = "ftd2xx.dll";

      GetDelegate("FT_CreateDeviceInfoList", out FT_CreateDeviceInfoList);
      GetDelegate("FT_GetDeviceInfoList", out FT_GetDeviceInfoList);
      GetDelegate("FT_Open", out FT_Open);
      GetDelegate("FT_Close", out FT_Close);
      GetDelegate("FT_SetBaudRate", out FT_SetBaudRate);
      GetDelegate("FT_SetDataCharacteristics", out FT_SetDataCharacteristics);
      GetDelegate("FT_SetFlowControl", out FT_SetFlowControl);
      GetDelegate("FT_SetTimeouts", out FT_SetTimeouts);
      GetDelegate("FT_Write", out FT_Write);
      GetDelegate("FT_Purge", out FT_Purge);
      GetDelegate("FT_GetStatus", out FT_GetStatus);
      GetDelegate("FT_Read", out FT_Read);
    }
  }
}
