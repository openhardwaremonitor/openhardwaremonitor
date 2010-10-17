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

  Contributor(s): Paul Werelds

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

namespace OpenHardwareMonitor.Hardware.HDD {

  internal class SMART {

    [Flags]
    public enum Status : ushort {
      PreFailureWarranty = 0x01,
      OnLineCollection = 0x02,
      Performance = 0x04,
      ErrorRate = 0x08,
      EventCount = 0x10,
      SelfPreserving = 0x20
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct AttributeID {
      private byte value;

      public AttributeID(byte value) {
        this.value = value;
      }

      public override bool Equals(Object obj) {
        return obj is AttributeID && this == (AttributeID)obj;
      }
      public override int GetHashCode() {
        return value.GetHashCode() ^ value.GetHashCode();
      }
      public static bool operator ==(AttributeID a, AttributeID b) {
        return a.value == b.value;
      }
      public static bool operator !=(AttributeID a, AttributeID b) {
        return !(a == b);
      }

      public string ToString(string format) {
        return value.ToString(format);
      }

      public static readonly AttributeID None = new AttributeID(0x00);
    }

    // Common SMART attributes
    public static class CommonAttributes {      
      public static readonly AttributeID 
        ReadErrorRate = new AttributeID(0x01);
      public static readonly AttributeID 
        ThroughputPerformance = new AttributeID(0x02);
      public static readonly AttributeID 
        SpinUpTime = new AttributeID(0x03);
      public static readonly AttributeID 
        StartStopCount = new AttributeID(0x04);
      public static readonly AttributeID 
        ReallocatedSectorsCount = new AttributeID(0x05);
      public static readonly AttributeID 
        ReadChannelMargin = new AttributeID(0x06);
      public static readonly AttributeID 
        SeekErrorRate = new AttributeID(0x07);
      public static readonly AttributeID 
        SeekTimePerformance = new AttributeID(0x08);
      public static readonly AttributeID 
        PowerOnHours = new AttributeID(0x09);
      public static readonly AttributeID 
        SpinRetryCount = new AttributeID(0x0A);
      public static readonly AttributeID 
        RecalibrationRetries = new AttributeID(0x0B);
      public static readonly AttributeID 
        PowerCycleCount = new AttributeID(0x0C);
      public static readonly AttributeID 
        SoftReadErrorRate = new AttributeID(0x0D);
      public static readonly AttributeID 
        AirflowTemperature = new AttributeID(0xBE);
      public static readonly AttributeID 
        Temperature = new AttributeID(0xC2);
      public static readonly AttributeID 
        HardwareECCRecovered = new AttributeID(0xC3);
      public static readonly AttributeID 
        ReallocationEventCount = new AttributeID(0xC4);
      public static readonly AttributeID 
        CurrentPendingSectorCount = new AttributeID(0xC5);
      public static readonly AttributeID 
        UncorrectableSectorCount = new AttributeID(0xC6);
      public static readonly AttributeID 
        UltraDMACRCErrorCount = new AttributeID(0xC7);
      public static readonly AttributeID 
        WriteErrorRate = new AttributeID(0xC8);
      public static readonly AttributeID 
        DriveTemperature = new AttributeID(0xE7);
    }

    // Indilinx SSD SMART attributes
    public static class IndilinxAttributes {      
      public static readonly AttributeID RemainingLife = new AttributeID(0xD1);
    }

    // Intel SSD SMART attributes
    public static class IntelAttributes {      
      public static readonly AttributeID RemainingLife = new AttributeID(0xE8);
    }

    // Samsung SSD SMART attributes
    public static class SamsungAttributes {      
      public static readonly AttributeID RemainingLife = new AttributeID(0xB4);
    }

    // SandForce SSD SMART attributes
    public static class SandForceAttributes {      
      public static readonly AttributeID RemainingLife = new AttributeID(0xE7);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct DriveAttribute {
      public AttributeID ID;
      public Status StatusFlags;
      public byte AttrValue;
      public byte WorstValue;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
      public byte[] RawValue;
      public byte Reserved;
    };

    [Flags]
    protected enum AccessMode : uint {     
      Read = 0x80000000,    
      Write = 0x40000000,     
      Execute = 0x20000000,     
      All = 0x10000000
    }

    [Flags]
    protected enum ShareMode : uint {
      None = 0,     
      Read = 1,     
      Write = 2,    
      Delete = 4
    }

    protected enum CreationMode : uint {
      New = 1,
      CreateAlways = 2,    
      OpenExisting = 3,    
      OpenAlways = 4,    
      TruncateExisting = 5
    }

    [Flags]
    protected enum FileAttribute : uint {
      Readonly = 0x00000001,
      Hidden = 0x00000002,
      System = 0x00000004,
      Directory = 0x00000010,
      Archive = 0x00000020,
      Device = 0x00000040,
      Normal = 0x00000080,
      Temporary = 0x00000100,
      SparseFile = 0x00000200,
      ReparsePoint = 0x00000400,
      Compressed = 0x00000800,
      Offline = 0x00001000,
      NotContentIndexed = 0x00002000,
      Encrypted = 0x00004000,
    }

    protected enum DriveCommand : uint {
      GetVersion = 0x00074080,
      SendDriveCommand = 0x0007c084,
      ReceiveDriveData = 0x0007c088
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct CommandBlockRegisters {
      public byte Features;         
      public byte SectorCount;      
      public byte LBALow;       
      public byte LBAMid;           
      public byte LBAHigh;        
      public byte Device;       
      public byte Command;           
      public byte Reserved;                  
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct DriveCommandParameter {
      public uint BufferSize;           
      public CommandBlockRegisters Registers;           
      public byte DriveNumber;   
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
      public byte[] Reserved;                                
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct DriverStatus {
      public byte DriverError;   
      public byte IDEError;             
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
      public byte[] Reserved;               
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct DriveCommandResult {
      public uint BufferSize;
      public DriverStatus DriverStatus;
    } 

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct DriveSmartReadResult {
      public uint BufferSize;           
      public DriverStatus DriverStatus;
      public byte Version;
      public byte Reserved;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_DRIVE_ATTRIBUTES)]
      public DriveAttribute[] Attributes;                                                                                       
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct Identify {
      public ushort GeneralConfiguration;
      public ushort NumberOfCylinders;
      public ushort Reserved;
      public ushort NumberOfHeads;
      public ushort UnformattedBytesPerTrack;
      public ushort UnformattedBytesPerSector;
      public ushort SectorsPerTrack;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public ushort[] VendorUnique;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
      public byte[] SerialNumber;
      public ushort BufferType;
      public ushort BufferSectorSize;
      public ushort NumberOfEccBytes;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] FirmwareRevision;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 40)]
      public byte[] ModelNumber;
      public ushort MoreVendorUnique;
      public ushort DoubleWordIo;
      public ushort Capabilities;
      public ushort MoreReserved;
      public ushort PioCycleTimingMode;
      public ushort DmaCycleTimingMode;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 406)]
      public byte[] More;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    protected struct DriveIdentifyResult {
      public uint BufferSize;
      public DriverStatus DriverStatus;
      public Identify Identify;
    } 

    public static readonly IntPtr INVALID_HANDLE_VALUE = (IntPtr)(-1);

    private const byte SMART_CMD = 0xB0;
    private const byte ID_CMD = 0xEC;
    
    private const byte SMART_READ_DATA = 0xD0;
    private const byte SMART_ENABLE_OPERATIONS = 0xD8;
    
    private const byte SMART_LBA_MID = 0x4F;
    private const byte SMART_LBA_HI = 0xC2;

    private const int MAX_DRIVE_ATTRIBUTES = 512;

    private SMART() { }

    public static IntPtr OpenPhysicalDrive(int driveNumber) {
      return NativeMethods.CreateFile(@"\\.\PhysicalDrive" + driveNumber,
        AccessMode.Read | AccessMode.Write, ShareMode.Read | ShareMode.Write,
        IntPtr.Zero, CreationMode.OpenExisting, FileAttribute.Device,
        IntPtr.Zero);
    }

    public static bool EnableSmart(IntPtr handle, int driveNumber) {
      DriveCommandParameter parameter = new DriveCommandParameter();
      DriveCommandResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Features = SMART_ENABLE_OPERATIONS;
      parameter.Registers.LBAMid = SMART_LBA_MID;
      parameter.Registers.LBAHigh = SMART_LBA_HI;
      parameter.Registers.Command = SMART_CMD;

      return NativeMethods.DeviceIoControl(handle, DriveCommand.SendDriveCommand, 
        ref parameter, Marshal.SizeOf(typeof(DriveCommandParameter)), out result,
        Marshal.SizeOf(typeof(DriveCommandResult)), out bytesReturned, 
        IntPtr.Zero);
    }

    public static List<DriveAttribute> ReadSmart(IntPtr handle,
      int driveNumber)
    {
      DriveCommandParameter parameter = new DriveCommandParameter();
      DriveSmartReadResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Features = SMART_READ_DATA;
      parameter.Registers.LBAMid = SMART_LBA_MID;
      parameter.Registers.LBAHigh = SMART_LBA_HI;
      parameter.Registers.Command = SMART_CMD;

      bool isValid = NativeMethods.DeviceIoControl(handle, 
        DriveCommand.ReceiveDriveData, ref parameter, Marshal.SizeOf(parameter), 
        out result, Marshal.SizeOf(typeof(DriveSmartReadResult)), 
        out bytesReturned, IntPtr.Zero);

      return (isValid)
        ? new List<DriveAttribute>(result.Attributes)
        : new List<DriveAttribute>();
    }

    public static string ReadName(IntPtr handle, int driveNumber) {
      DriveCommandParameter parameter = new DriveCommandParameter();
      DriveIdentifyResult result;
      uint bytesReturned;

      parameter.DriveNumber = (byte)driveNumber;
      parameter.Registers.Command = ID_CMD;

      bool valid = NativeMethods.DeviceIoControl(handle, 
        DriveCommand.ReceiveDriveData, ref parameter, Marshal.SizeOf(parameter), 
        out result, Marshal.SizeOf(typeof(DriveIdentifyResult)), 
        out bytesReturned, IntPtr.Zero);

      if (!valid)
        return null;
      else {

        byte[] bytes = result.Identify.ModelNumber;
        char[] chars = new char[bytes.Length];
        for (int i = 0; i < bytes.Length; i += 2) {
          chars[i] = (char)bytes[i + 1];
          chars[i + 1] = (char)bytes[i];
        }

        return new string(chars).Trim();
      }
    }

    public static int CloseHandle(IntPtr handle) {
      return NativeMethods.CloseHandle(handle);
    }

    protected static class NativeMethods {
      private const string KERNEL = "kernel32.dll";

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi,
        CharSet = CharSet.Unicode)]
      public static extern IntPtr CreateFile(string fileName,
        AccessMode desiredAccess, ShareMode shareMode, IntPtr securityAttributes,
        CreationMode creationDisposition, FileAttribute flagsAndAttributes,
        IntPtr templateFilehandle);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern int CloseHandle(IntPtr handle);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAsAttribute(UnmanagedType.Bool)]
      public static extern bool DeviceIoControl(IntPtr handle,
        DriveCommand command, ref DriveCommandParameter parameter,
        int parameterSize, out DriveSmartReadResult result, int resultSize,
        out uint bytesReturned, IntPtr overlapped);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAsAttribute(UnmanagedType.Bool)]
      public static extern bool DeviceIoControl(IntPtr handle,
        DriveCommand command, ref DriveCommandParameter parameter,
        int parameterSize, out DriveCommandResult result, int resultSize,
        out uint bytesReturned, IntPtr overlapped);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      [return: MarshalAsAttribute(UnmanagedType.Bool)]
      public static extern bool DeviceIoControl(IntPtr handle,
        DriveCommand command, ref DriveCommandParameter parameter,
        int parameterSize, out DriveIdentifyResult result, int resultSize,
        out uint bytesReturned, IntPtr overlapped);
    }    
  }
}
