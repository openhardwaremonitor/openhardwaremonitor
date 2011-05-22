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
  Portions created by the Initial Developer are Copyright (C) 2010-2011
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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Text;

namespace OpenHardwareMonitor.Hardware {
  internal static class Ring0 {

    private static KernelDriver driver;
    private static string fileName;
    private static Mutex isaBusMutex;
    private static readonly StringBuilder report = new StringBuilder();

    private const uint OLS_TYPE = 40000;
    private static IOControlCode
      IOCTL_OLS_GET_REFCOUNT = new IOControlCode(OLS_TYPE, 0x801,
        IOControlCode.Access.Any),
      IOCTL_OLS_GET_DRIVER_VERSION = new IOControlCode(OLS_TYPE, 0x800,
        IOControlCode.Access.Any),
      IOCTL_OLS_READ_MSR = new IOControlCode(OLS_TYPE, 0x821,
        IOControlCode.Access.Any),
      IOCTL_OLS_WRITE_MSR = new IOControlCode(OLS_TYPE, 0x822, 
        IOControlCode.Access.Any),
      IOCTL_OLS_READ_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x833,
        IOControlCode.Access.Read),
      IOCTL_OLS_WRITE_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x836, 
        IOControlCode.Access.Write),
      IOCTL_OLS_READ_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x851, 
        IOControlCode.Access.Read),
      IOCTL_OLS_WRITE_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x852,
        IOControlCode.Access.Write),
      IOCTL_OLS_READ_MEMORY = new IOControlCode(OLS_TYPE, 0x841,
        IOControlCode.Access.Read);

    private static string GetTempFileName() {

      // try to get a file in the temporary folder
      try {
        return Path.GetTempFileName();        
      } catch (IOException) { 
          // some I/O exception
        } 
        catch (UnauthorizedAccessException) { 
          // we do not have the right to create a file in the temp folder
        }
        catch (NotSupportedException) {
          // invalid path format of the TMP system environment variable
        }

      // if this failed, we try to create one in the application folder
      string fileName = Path.ChangeExtension(
        Assembly.GetExecutingAssembly().Location, ".sys");
      try {
        using (FileStream stream = File.Create(fileName)) {
          return fileName;
        }        
      } catch (IOException) { } 
        catch (UnauthorizedAccessException) { }
     
      return null;
    }

    private static bool ExtractDriver(string fileName) {
      string resourceName = "OpenHardwareMonitor.Hardware." +
        (IntPtr.Size == 4 ? "WinRing0.sys" : "WinRing0x64.sys");

      string[] names =
        Assembly.GetExecutingAssembly().GetManifestResourceNames();
      byte[] buffer = null;
      for (int i = 0; i < names.Length; i++) {
        if (names[i].Replace('\\', '.') == resourceName) {
          using (Stream stream = Assembly.GetExecutingAssembly().
            GetManifestResourceStream(names[i])) 
          {
              buffer = new byte[stream.Length];
              stream.Read(buffer, 0, buffer.Length);
          }
        }
      }

      if (buffer == null)
        return false;

      using (FileStream target = new FileStream(fileName, FileMode.Create)) {
        target.Write(buffer, 0, buffer.Length);
      }

      return true;
    }

    public static void Open() {
      // no implementation for unix systems
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;  
      
      if (driver != null)
        return;

      // clear the current report
      report.Length = 0;
     
      driver = new KernelDriver("WinRing0_1_2_0");
      driver.Open();

      if (!driver.IsOpen) {
        // driver is not loaded, try to reinstall and open

        driver.Delete();
        fileName = GetTempFileName();
        if (fileName != null && ExtractDriver(fileName)) {
          if (driver.Install(fileName)) {
            driver.Open();

            if (!driver.IsOpen) {
              driver.Delete();
              report.AppendLine("Status: Opening driver failed");
            }
          } else {
            report.AppendLine("Status: Installing driver \"" +
              fileName + "\" failed" +
              (File.Exists(fileName) ? " and file exists" : ""));
            report.AppendLine();
            report.Append("Exception: " + Marshal.GetExceptionForHR(
              Marshal.GetHRForLastWin32Error()).Message);
          }
        } else {
          report.AppendLine("Status: Extracting driver failed");
        }

        try {
          // try to delte the driver file
          if (File.Exists(fileName))
            File.Delete(fileName);
          fileName = null;
        } catch (IOException) { } 
          catch (UnauthorizedAccessException) { }
      }

      if (!driver.IsOpen) 
        driver = null;

      string mutexName = "Global\\Access_ISABUS.HTP.Method";
      try {
        isaBusMutex = new Mutex(false, mutexName);
      } catch (UnauthorizedAccessException) {
        try {
          isaBusMutex = Mutex.OpenExisting(mutexName, MutexRights.Synchronize);
        } catch { }
      }
    }

    public static bool IsOpen {
      get { return driver != null; }
    }

    public static void Close() {
      if (driver == null)
        return;

      uint refCount = 0;
      driver.DeviceIOControl(IOCTL_OLS_GET_REFCOUNT, null, ref refCount);

      driver.Close();

      if (refCount <= 1)
        driver.Delete();

      driver = null;

      if (isaBusMutex != null) {
        isaBusMutex.Close();
        isaBusMutex = null;
      }

      // try to delete temporary driver file again if failed during open
      if (fileName != null && File.Exists(fileName)) {
        try {
          File.Delete(fileName);
          fileName = null;
        } catch (IOException) { } 
          catch (UnauthorizedAccessException) { }
      }
    }

    public static string GetReport() {
      if (report.Length > 0) {
        StringBuilder r = new StringBuilder();
        r.AppendLine("Ring0");
        r.AppendLine();
        r.Append(report);
        r.AppendLine();
        return r.ToString();
      } else
        return null;
    }

    public static bool WaitIsaBusMutex(int millisecondsTimeout) {
      if (isaBusMutex == null)
        return true;
      try {
        return isaBusMutex.WaitOne(millisecondsTimeout, false);
      } catch (AbandonedMutexException) { return false; } 
        catch (InvalidOperationException) { return false; }
    }

    public static void ReleaseIsaBusMutex() {
      if (isaBusMutex == null)
        return;
      isaBusMutex.ReleaseMutex();
    }

    public static bool Rdmsr(uint index, out uint eax, out uint edx) {
      if (driver == null) {
        eax = 0;
        edx = 0;
        return false;
      }

      ulong buffer = 0;
      bool result = driver.DeviceIOControl(IOCTL_OLS_READ_MSR, index,
        ref buffer);

      edx = (uint)((buffer >> 32) & 0xFFFFFFFF);
      eax = (uint)(buffer & 0xFFFFFFFF);
      return result;
    }

    public static bool RdmsrTx(uint index, out uint eax, out uint edx,
      ulong threadAffinityMask) 
    {
      ulong mask = ThreadAffinity.Set(threadAffinityMask);

      bool result = Rdmsr(index, out eax, out edx);

      ThreadAffinity.Set(mask);
      return result;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WrmsrInput {
      public uint Register;
      public ulong Value;
    }

    public static bool Wrmsr(uint index, uint eax, uint edx) {
      if (driver == null)
        return false;

      WrmsrInput input = new WrmsrInput();
      input.Register = index;
      input.Value = ((ulong)edx << 32) | eax;

      return driver.DeviceIOControl(IOCTL_OLS_WRITE_MSR, input);
    }

    public static byte ReadIoPort(uint port) {
      if (driver == null)
        return 0;

      uint value = 0;
      driver.DeviceIOControl(IOCTL_OLS_READ_IO_PORT_BYTE, port, ref value);

      return (byte)(value & 0xFF);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WriteIoPortInput {
      public uint PortNumber;
      public byte Value;
    }

    public static void WriteIoPort(uint port, byte value) {
      if (driver == null)
        return;

      WriteIoPortInput input = new WriteIoPortInput();
      input.PortNumber = port;
      input.Value = value;

      driver.DeviceIOControl(IOCTL_OLS_WRITE_IO_PORT_BYTE, input);
    }

    public const uint InvalidPciAddress = 0xFFFFFFFF;

    public static uint GetPciAddress(byte bus, byte device, byte function) {
      return
        (uint)(((bus & 0xFF) << 8) | ((device & 0x1F) << 3) | (function & 7));
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ReadPciConfigInput {
      public uint PciAddress;
      public uint RegAddress;
    }

    public static bool ReadPciConfig(uint pciAddress, uint regAddress, 
      out uint value) 
    {
      if (driver == null || (regAddress & 3) != 0) {
        value = 0;
        return false;
      }

      ReadPciConfigInput input = new ReadPciConfigInput();
      input.PciAddress = pciAddress;
      input.RegAddress = regAddress;

      value = 0;
      return driver.DeviceIOControl(IOCTL_OLS_READ_PCI_CONFIG, input, 
        ref value);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct WritePciConfigInput {
      public uint PciAddress;
      public uint RegAddress;
      public uint Value;
    }

    public static bool WritePciConfig(uint pciAddress, uint regAddress, 
      uint value) 
    {
      if (driver == null || (regAddress & 3) != 0)
        return false;

      WritePciConfigInput input = new WritePciConfigInput();
      input.PciAddress = pciAddress;
      input.RegAddress = regAddress;
      input.Value = value;

      return driver.DeviceIOControl(IOCTL_OLS_WRITE_PCI_CONFIG, input);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct ReadMemoryInput {
      public ulong address;
      public uint unitSize;
      public uint count;
    }

    public static bool ReadMemory<T>(ulong address, ref T buffer) {
      if (driver == null) {
        return false;
      }

      ReadMemoryInput input = new ReadMemoryInput();
      input.address = address;
      input.unitSize = 1;
      input.count = (uint)Marshal.SizeOf(buffer);

      return driver.DeviceIOControl(IOCTL_OLS_READ_MEMORY, input,
        ref buffer);
    }
  }
}
