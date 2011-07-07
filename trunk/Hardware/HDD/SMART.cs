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
  Portions created by the Initial Developer are Copyright (C) 2009-2011
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

    // These are the more-or-less standard S.M.A.R.T attributes
    // TODO: Filter out unused/obscure ones; some are interpreted differently
    // between manufacturers
    public static class CommonAttributes {
      public static readonly AttributeID
        ReadErrorRate = new AttributeID(0x01),
        ThroughputPerformance = new AttributeID(0x02),
        SpinUpTime = new AttributeID(0x03),
        StartStopCount = new AttributeID(0x04),
        ReallocatedSectorsCount = new AttributeID(0x05),
        ReadChannelMargin = new AttributeID(0x06),
        SeekErrorRate = new AttributeID(0x07),
        SeekTimePerformance = new AttributeID(0x08),
        PowerOnHours = new AttributeID(0x09),
        SpinRetryCount = new AttributeID(0x0A),
        RecalibrationRetries = new AttributeID(0x0B),
        PowerCycleCount = new AttributeID(0x0C),
        SoftReadErrorRate = new AttributeID(0x0D),
        SataDownshiftErrorCount = new AttributeID(0xB7),
        EndToEndError = new AttributeID(0xB8),
        HeadStability = new AttributeID(0xB9),
        InducedOpVibrationDetection = new AttributeID(0xBA),
        ReportedUncorrectableErrors = new AttributeID(0xBB),
        CommandTimeout = new AttributeID(0xBC),
        HighFlyWrites = new AttributeID(0xBD),
        AirflowTemperature = new AttributeID(0xBE),
        GSenseErrorRate = new AttributeID(0xBF),
        PowerOffRetractCount = new AttributeID(0xC0),
        LoadCycleCount = new AttributeID(0xC1),
        Temperature = new AttributeID(0xC2),
        HardwareEccRecovered = new AttributeID(0xC3),
        ReallocationEventCount = new AttributeID(0xC4),
        CurrentPendingSectorCount = new AttributeID(0xC5),
        UncorrectableSectorCount = new AttributeID(0xC6),
        UltraDmaCrcErrorCount = new AttributeID(0xC7),
        WriteErrorRate = new AttributeID(0xC8),
        DataAddressMarkerrors = new AttributeID(0xCA),
        RunOutCancel = new AttributeID(0xCB),
        SoftEccCorrection = new AttributeID(0xCC),
        ThermalAsperityRate = new AttributeID(0xCD),
        FlyingHeight = new AttributeID(0xCE),
        SpinHighCurrent = new AttributeID(0xCF),
        SpinBuzz = new AttributeID(0xD0),
        OfflineSeekPerformance = new AttributeID(0xD1),
        VibrationDuringWrite = new AttributeID(0xD3),
        ShockDuringWrite = new AttributeID(0xD4),
        DiskShift = new AttributeID(0xDC),
        GSenseErrorRateAlt = new AttributeID(0xDD), // Alternative to 0xBF
        LoadedHours = new AttributeID(0xDE),
        LoadUnloadRetryCount = new AttributeID(0xDF),
        LoadFriction = new AttributeID(0xE0),
        LoadUnloadCycleCount = new AttributeID(0xE1),
        LoadInTime = new AttributeID(0xE2),
        TorqueAmplificationCount = new AttributeID(0xE3),
        PowerOffRetractCycle = new AttributeID(0xE4),
        GMRHeadAmplitude = new AttributeID(0xE6),
        DriveTemperature = new AttributeID(0xE7),
        HeadFlyingHours = new AttributeID(0xF0),
        LBAsWrittenTotal = new AttributeID(0xF1),
        LBAsReadTotal = new AttributeID(0xF2),
        ReadErrorRetryRate = new AttributeID(0xFA),
        FreeFallProtection = new AttributeID(0xFE)
      ;
    }

    // Indilinx SSD SMART attributes
    // TODO: Find out the purpose of attribute 0xD2
    // Seems to be unique to Indilinx drives, hence its name of UnknownUnique.
    public static class IndilinxAttributes {
      public static readonly AttributeID
        ReadErrorRate = CommonAttributes.ReadErrorRate,
        PowerOnHours = CommonAttributes.PowerOnHours,
        PowerCycleCount = CommonAttributes.PowerCycleCount,
        InitialBadBlockCount = new AttributeID(0xB8),
        RemainingLife = new AttributeID(0xD1),
        ProgramFailure = new AttributeID(0xC3),
        EraseFailure = new AttributeID(0xC4),
        ReadFailure = new AttributeID(0xC5),
        SectorsRead = new AttributeID(0xC6),
        SectorsWritten = new AttributeID(0xC7),
        ReadCommands = new AttributeID(0xC8),
        WriteCommands = new AttributeID(0xC9),
        BitErrors = new AttributeID(0xCA),
        CorrectedErrors = new AttributeID(0xCB),
        BadBlockFullFlag = new AttributeID(0xCC),
        MaxCellcycles = new AttributeID(0xCD),
        MinErase = new AttributeID(0xCE),
        MaxErase = new AttributeID(0xCF),
        AverageEraseCount = new AttributeID(0xD0),
        UnknownUnique = new AttributeID(0xD2),
        SataErrorCountCRC = new AttributeID(0xD3),
        SataErrorCountHandshake = new AttributeID(0xD4)
      ;
    }

    // Intel SSD SMART attributes
    // TODO: Find out the meaning behind 0xE2, 0xE3 and 0xE4
    public static class IntelAttributes {
      public static readonly AttributeID
        ReadErrorRate = CommonAttributes.ReadErrorRate,
        SpinUpTime = CommonAttributes.SpinUpTime,
        StartStopCount = CommonAttributes.StartStopCount,
        ReallocatedSectorsCount = CommonAttributes.ReallocatedSectorsCount,
        PowerOnHours = CommonAttributes.PowerOnHours,
        PowerCycleCount = CommonAttributes.PowerCycleCount,
        EndToEndError = CommonAttributes.EndToEndError, // Only on G2 drives!

        // Different from the common attribute PowerOffRetractCount, same ID
        UnsafeShutdownCount = new AttributeID(0xC0),
        HostWrites = new AttributeID(0xE1),
        RemainingLife = new AttributeID(0xE8),
        MediaWearOutIndicator = new AttributeID(0xE9)
      ;
    }

    // Samsung SSD SMART attributes
    // TODO: AF, B0, B1, B5, B6, BB, C3, C6, C7, E8, E9
    public static class SamsungAttributes {
      public static readonly AttributeID
        PowerOnHours = CommonAttributes.PowerOnHours,
        PowerCycleCount = CommonAttributes.PowerCycleCount,
        UsedReservedBlockCountChip = new AttributeID(0xB2), // Unique
        UsedReservedBlockCountTotal = new AttributeID(0xB3), // Unique
        RemainingLife = new AttributeID(0xB4), // Unique
        RuntimeBadBlockTotal = new AttributeID(0xB7)
      ;
    }

    // SandForce SSD SMART attributes
    // Note: 0xE9 and 0xEA are reserved attributes and unique
    public static class SandForceAttributes {
      public static readonly AttributeID
        ReadErrorRate = CommonAttributes.ReadErrorRate,
        RetiredBlockCount = new AttributeID(0x05),
        PowerOnHours = CommonAttributes.PowerOnHours,
        PowerCycleCount = CommonAttributes.PowerCycleCount,
        ProgramFailCount = new AttributeID(0xAB), // Unique
        EraseFailCount = new AttributeID(0xAC), // Unique
        UnexpectedPowerLossCount = new AttributeID(0xAE), // Unique
        WearRangeDelta = new AttributeID(0xB1), // Unique
        ProgramFailCountAlt = new AttributeID(0xB5), // Same as 0xAB
        EraseFailCountAlt = new AttributeID(0xB6), // Same as 0xAC
        ReportedUncorrectableErrors =
          CommonAttributes.ReportedUncorrectableErrors,
        
        Temperature = CommonAttributes.Temperature, // SF-1500 only!
        
        // Opposite of the common attribute HardwareECCRecovered
        UnrecoverableECC = new AttributeID(0xC3),
        ReallocationEventCount = new AttributeID(0xC4),
        RemainingLife = new AttributeID(0xE7),
        LifetimeWrites = new AttributeID(0xF1),
        LifetimeReads = new AttributeID(0xF2)
      ;
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

    public static DriveAttribute[] ReadSmart(IntPtr handle,
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

      return (isValid) ? result.Attributes : new DriveAttribute[0];
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

        return new string(chars).Trim(new char[] {' ', '\0'});
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
