/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2010-2016 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware
{
    internal static class Ring0
    {
        private const uint OLS_TYPE = 40000;

        public const uint InvalidPciAddress = 0xFFFFFFFF;

        private static KernelDriver driver;
        private static string fileName;
        private static Mutex isaBusMutex;
        private static readonly StringBuilder report = new StringBuilder();

        private static readonly IOControlCode
            IOCTL_OLS_GET_REFCOUNT = new IOControlCode(OLS_TYPE, 0x801,
                IOControlCode.Access.Any);

        private static IOControlCode
            IOCTL_OLS_GET_DRIVER_VERSION = new IOControlCode(OLS_TYPE, 0x800,
                IOControlCode.Access.Any);

        private static readonly IOControlCode
            IOCTL_OLS_READ_MSR = new IOControlCode(OLS_TYPE, 0x821,
                IOControlCode.Access.Any);

        private static readonly IOControlCode
            IOCTL_OLS_WRITE_MSR = new IOControlCode(OLS_TYPE, 0x822,
                IOControlCode.Access.Any);

        private static readonly IOControlCode
            IOCTL_OLS_READ_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x833,
                IOControlCode.Access.Read);

        private static readonly IOControlCode
            IOCTL_OLS_WRITE_IO_PORT_BYTE = new IOControlCode(OLS_TYPE, 0x836,
                IOControlCode.Access.Write);

        private static readonly IOControlCode
            IOCTL_OLS_READ_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x851,
                IOControlCode.Access.Read);

        private static readonly IOControlCode
            IOCTL_OLS_WRITE_PCI_CONFIG = new IOControlCode(OLS_TYPE, 0x852,
                IOControlCode.Access.Write);

        private static readonly IOControlCode
            IOCTL_OLS_READ_MEMORY = new IOControlCode(OLS_TYPE, 0x841,
                IOControlCode.Access.Read);

        public static bool IsOpen => driver != null;

        private static Assembly GetAssembly()
        {
            return typeof(Ring0).Assembly;
        }

        private static string GetTempFileName()
        {
            // try to create one in the application folder
            var location = GetAssembly().Location;
            if (!string.IsNullOrEmpty(location))
                try
                {
                    var fileName = Path.ChangeExtension(location, ".sys");
                    using (var stream = File.Create(fileName))
                    {
                        return fileName;
                    }
                }
                catch (Exception)
                {
                }

            // if this failed, try to get a file in the temporary folder
            try
            {
                return Path.GetTempFileName();
            }
            catch (IOException)
            {
                // some I/O exception
            }
            catch (UnauthorizedAccessException)
            {
                // we do not have the right to create a file in the temp folder
            }
            catch (NotSupportedException)
            {
                // invalid path format of the TMP system environment variable
            }

            return null;
        }

        private static bool ExtractDriver(string fileName)
        {
            var resourceName = "OpenHardwareMonitor.Hardware." +
                               (OperatingSystem.Is64BitOperatingSystem() ? "WinRing0x64.sys" : "WinRing0.sys");

            var names = GetAssembly().GetManifestResourceNames();
            byte[] buffer = null;
            for (var i = 0; i < names.Length; i++)
                if (names[i].Replace('\\', '.') == resourceName)
                    using (var stream = GetAssembly().GetManifestResourceStream(names[i]))
                    {
                        buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, buffer.Length);
                    }

            if (buffer == null)
                return false;

            try
            {
                using (var target = new FileStream(fileName, FileMode.Create))
                {
                    target.Write(buffer, 0, buffer.Length);
                    target.Flush();
                }
            }
            catch (IOException)
            {
                // for example there is not enough space on the disk
                return false;
            }

            // make sure the file is actually writen to the file system
            for (var i = 0; i < 20; i++)
                try
                {
                    if (File.Exists(fileName) &&
                        new FileInfo(fileName).Length == buffer.Length)
                        return true;
                    Thread.Sleep(100);
                }
                catch (IOException)
                {
                    Thread.Sleep(10);
                }

            // file still has not the right size, something is wrong
            return false;
        }

        public static void Open()
        {
            // no implementation for unix systems
            var p = (int) Environment.OSVersion.Platform;
            if (p == 4 || p == 128)
                return;

            if (driver != null)
                return;

            // clear the current report
            report.Length = 0;

            driver = new KernelDriver("WinRing0_1_2_0");
            driver.Open();

            if (!driver.IsOpen)
            {
                // driver is not loaded, try to install and open

                fileName = GetTempFileName();
                if (fileName != null && ExtractDriver(fileName))
                {
                    if (driver.Install(fileName, out string installError))
                    {
                        driver.Open();

                        if (!driver.IsOpen)
                        {
                            driver.Delete();
                            report.AppendLine("Status: Opening driver failed after install");
                        }
                    }
                    else
                    {
                        var errorFirstInstall = installError;

                        // install failed, try to delete and reinstall
                        driver.Delete();

                        // wait a short moment to give the OS a chance to remove the driver
                        Thread.Sleep(2000);

                        if (driver.Install(fileName, out string errorSecondInstall))
                        {
                            driver.Open();

                            if (!driver.IsOpen)
                            {
                                driver.Delete();
                                report.AppendLine(
                                    "Status: Opening driver failed after reinstall");
                            }
                        }
                        else
                        {
                            report.AppendLine("Status: Installing driver \"" +
                                              fileName + "\" failed" +
                                              (File.Exists(fileName) ? " and file exists" : ""));
                            report.AppendLine("First Exception: " + errorFirstInstall);
                            report.AppendLine("Second Exception: " + errorSecondInstall);
                        }
                    }
                }
                else
                {
                    report.AppendLine("Status: Extracting driver failed");
                }

                try
                {
                    // try to delte the driver file
                    if (File.Exists(fileName))
                        File.Delete(fileName);
                    fileName = null;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }

            if (!driver.IsOpen)
                driver = null;

            var mutexName = "Global\\Access_ISABUS.HTP.Method";
            try
            {
                isaBusMutex = new Mutex(false, mutexName);
            }
            catch (UnauthorizedAccessException)
            {
                try
                {
                    isaBusMutex = Mutex.OpenExisting(mutexName, MutexRights.Synchronize);
                }
                catch
                {
                }
            }
        }

        public static void Close()
        {
            if (driver == null)
                return;

            uint refCount = 0;
            driver.DeviceIOControl(IOCTL_OLS_GET_REFCOUNT, null, ref refCount);

            driver.Close();

            if (refCount <= 1)
                driver.Delete();

            driver = null;

            if (isaBusMutex != null)
            {
                isaBusMutex.Close();
                isaBusMutex = null;
            }

            // try to delete temporary driver file again if failed during open
            if (fileName != null && File.Exists(fileName))
                try
                {
                    File.Delete(fileName);
                    fileName = null;
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
        }

        public static string GetReport()
        {
            if (report.Length > 0)
            {
                var r = new StringBuilder();
                r.AppendLine("Ring0");
                r.AppendLine();
                r.Append(report);
                r.AppendLine();
                return r.ToString();
            }
            return null;
        }

        public static bool WaitIsaBusMutex(int millisecondsTimeout)
        {
            if (isaBusMutex == null)
                return true;
            try
            {
                return isaBusMutex.WaitOne(millisecondsTimeout, false);
            }
            catch (AbandonedMutexException)
            {
                return false;
            }
            catch (InvalidOperationException)
            {
                return false;
            }
        }

        public static void ReleaseIsaBusMutex()
        {
            isaBusMutex?.ReleaseMutex();
        }

        public static bool Rdmsr(uint index, out uint eax, out uint edx)
        {
            if (driver == null)
            {
                eax = 0;
                edx = 0;
                return false;
            }

            ulong buffer = 0;
            var result = driver.DeviceIOControl(IOCTL_OLS_READ_MSR, index,
                ref buffer);

            edx = (uint) ((buffer >> 32) & 0xFFFFFFFF);
            eax = (uint) (buffer & 0xFFFFFFFF);
            return result;
        }

        public static bool RdmsrTx(uint index, out uint eax, out uint edx,
            ulong threadAffinityMask)
        {
            var mask = ThreadAffinity.Set(threadAffinityMask);

            var result = Rdmsr(index, out eax, out edx);

            ThreadAffinity.Set(mask);
            return result;
        }

        public static bool Wrmsr(uint index, uint eax, uint edx)
        {
            if (driver == null)
                return false;

            var input = new WrmsrInput();
            input.Register = index;
            input.Value = ((ulong) edx << 32) | eax;

            return driver.DeviceIOControl(IOCTL_OLS_WRITE_MSR, input);
        }

        public static byte ReadIoPort(uint port)
        {
            if (driver == null)
                return 0;

            uint value = 0;
            driver.DeviceIOControl(IOCTL_OLS_READ_IO_PORT_BYTE, port, ref value);

            return (byte) (value & 0xFF);
        }

        public static void WriteIoPort(uint port, byte value)
        {
            if (driver == null)
                return;

            var input = new WriteIoPortInput();
            input.PortNumber = port;
            input.Value = value;

            driver.DeviceIOControl(IOCTL_OLS_WRITE_IO_PORT_BYTE, input);
        }

        public static uint GetPciAddress(byte bus, byte device, byte function)
        {
            return
                (uint) (((bus & 0xFF) << 8) | ((device & 0x1F) << 3) | (function & 7));
        }

        public static bool ReadPciConfig(uint pciAddress, uint regAddress,
            out uint value)
        {
            if (driver == null || (regAddress & 3) != 0)
            {
                value = 0;
                return false;
            }

            var input = new ReadPciConfigInput();
            input.PciAddress = pciAddress;
            input.RegAddress = regAddress;

            value = 0;
            return driver.DeviceIOControl(IOCTL_OLS_READ_PCI_CONFIG, input,
                ref value);
        }

        public static bool WritePciConfig(uint pciAddress, uint regAddress,
            uint value)
        {
            if (driver == null || (regAddress & 3) != 0)
                return false;

            var input = new WritePciConfigInput();
            input.PciAddress = pciAddress;
            input.RegAddress = regAddress;
            input.Value = value;

            return driver.DeviceIOControl(IOCTL_OLS_WRITE_PCI_CONFIG, input);
        }

        public static bool ReadMemory<T>(ulong address, ref T buffer)
        {
            if (driver == null) return false;

            var input = new ReadMemoryInput();
            input.address = address;
            input.unitSize = 1;
            input.count = (uint) Marshal.SizeOf(buffer);

            return driver.DeviceIOControl(IOCTL_OLS_READ_MEMORY, input,
                ref buffer);
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WrmsrInput
        {
            public uint Register;
            public ulong Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WriteIoPortInput
        {
            public uint PortNumber;
            public byte Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadPciConfigInput
        {
            public uint PciAddress;
            public uint RegAddress;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct WritePciConfigInput
        {
            public uint PciAddress;
            public uint RegAddress;
            public uint Value;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct ReadMemoryInput
        {
            public ulong address;
            public uint unitSize;
            public uint count;
        }
    }
}