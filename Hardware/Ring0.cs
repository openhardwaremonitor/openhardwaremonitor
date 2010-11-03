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
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;

namespace OpenHardwareMonitor.Hardware {
  internal static class Ring0 {

    private static KernelDriver driver;
    private static Mutex isaBusMutex;

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
        IOControlCode.Access.Write);

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
      // No implementation for Unix systems
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128))
        return;  
      
      if (driver != null)
        return;
     
      driver = new KernelDriver("WinRing0_1_2_0");
      driver.Open();

      if (!driver.IsOpen) {
        string fileName = Path.GetTempFileName();
        if (ExtractDriver(fileName)) {

          driver.Install(fileName);
          File.Delete(fileName);

          driver.Open();

          if (!driver.IsOpen)
            driver.Delete();
        }
      }

      if (!driver.IsOpen) 
        driver = null;

      isaBusMutex = new Mutex(false, "Access_ISABUS.HTP.Method");
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
  }
}
