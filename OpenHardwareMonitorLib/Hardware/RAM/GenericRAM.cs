/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware.RAM
{
    internal class GenericRAM : Hardware
    {

        private const double OneGigabyte = 1024 * 1024 * 1024;
        private Sensor loadSensor;
        private Sensor usedMemory;
        private Sensor availableMemory;
        private Sensor totalPhysicalMemory;
        private Sensor commitLimit; // Virtual memory commit limit (may increase if page file is allowed to grow)
        private Sensor currentCommit; // Currently commited virtual memory
        private Sensor kernelSize;
        private Sensor processCount;
        private Sensor threadCount;
        private Sensor handleCount;

        public GenericRAM(string name, ISettings settings)
            : base(name, new Identifier("ram"), settings)
        {
            loadSensor = new Sensor("Memory", 0, SensorType.Load, this, settings);
            ActivateSensor(loadSensor);

            usedMemory = new Sensor("Used Memory", 0, SensorType.Data, this,
                settings);
            ActivateSensor(usedMemory);

            availableMemory = new Sensor("Available Memory", 1, SensorType.Data, this,
                settings);
            ActivateSensor(availableMemory);

            totalPhysicalMemory = new Sensor("Total Physical Memory", 2, SensorType.Data, this, settings);
            ActivateSensor(totalPhysicalMemory);

            commitLimit = new Sensor("Virtual memory commit limit", 3, SensorType.Data, this, settings);
            ActivateSensor(commitLimit);

            currentCommit = new Sensor("Virtual memory in use", 4, SensorType.Data, this, settings);
            ActivateSensor(currentCommit);

            kernelSize = new Sensor("Kernel memory usage", 5, SensorType.Data, this, settings);
            ActivateSensor(kernelSize);

            processCount = new Sensor("Processes", 0, SensorType.RawValue, this, settings);
            ActivateSensor(processCount);

            threadCount = new Sensor("Threads", 1, SensorType.RawValue, this, settings);
            ActivateSensor(threadCount);

            handleCount = new Sensor("Handles", 2, SensorType.RawValue, this, settings);
            ActivateSensor(handleCount);
        }

        public override HardwareType HardwareType
        {
            get { return HardwareType.RAM; }
        }

        public override void Update()
        {
            NativeMethods.MemoryStatusEx status = new NativeMethods.MemoryStatusEx();
            status.Length = checked((uint)Marshal.SizeOf(
                typeof(NativeMethods.MemoryStatusEx)));

            if (!NativeMethods.GlobalMemoryStatusEx(ref status))
                return;

            loadSensor.Value = 100.0f -
                               (100.0f * status.AvailablePhysicalMemory) /
                               status.TotalPhysicalMemory;

            usedMemory.Value = (status.TotalPhysicalMemory
                                - status.AvailablePhysicalMemory) / OneGigabyte;

            availableMemory.Value = status.AvailablePhysicalMemory / OneGigabyte;

            totalPhysicalMemory.Value = status.TotalPhysicalMemory / OneGigabyte;

            NativeMethods.PERFORMANCE_INFORMATION performanceInfo = new NativeMethods.PERFORMANCE_INFORMATION();
            performanceInfo.cb = (uint)Marshal.SizeOf<NativeMethods.PERFORMANCE_INFORMATION>();

            try
            {
                // The function is only available in the kernel as of Windows 7
                if (!NativeMethods.GetPerformanceInfo(ref performanceInfo, performanceInfo.cb))
                {
                    return;
                }
            }
            catch (EntryPointNotFoundException)
            {
                return;
            }

            commitLimit.Value = performanceInfo.CommitLimit * performanceInfo.PageSize / OneGigabyte;
            currentCommit.Value = performanceInfo.CommitTotal * performanceInfo.PageSize/ OneGigabyte;

            kernelSize.Value = performanceInfo.KernelNonpaged * performanceInfo.PageSize/ OneGigabyte;

            processCount.Value = performanceInfo.ProcessCount;
            threadCount.Value = performanceInfo.ThreadCount;
            handleCount.Value = performanceInfo.HandleCount;
        }

        private class NativeMethods
        {
            [StructLayout(LayoutKind.Sequential)]
            public struct MemoryStatusEx
            {
                public uint Length;
                public uint MemoryLoad;
                public ulong TotalPhysicalMemory;
                public ulong AvailablePhysicalMemory;
                public ulong TotalPageFile;
                public ulong AvailPageFile;
                public ulong TotalVirtual;
                public ulong AvailVirtual;
                public ulong AvailExtendedVirtual;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct PERFORMANCE_INFORMATION
            {
                public uint cb;
                public nint CommitTotal;
                public nint CommitLimit;
                public nint CommitPeak;
                public nint PhysicalTotal;
                public nint PhysicalAvailable;
                public nint SystemCache;
                public nint KernelTotal;
                public nint KernelPaged;
                public nint KernelNonpaged;
                public nint PageSize;
                public uint HandleCount;
                public uint ProcessCount;
                public uint ThreadCount;
            }

            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GlobalMemoryStatusEx(
                ref NativeMethods.MemoryStatusEx buffer);


            [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true, EntryPoint = "K32GetPerformanceInfo")]
            [return: MarshalAs(UnmanagedType.Bool)]
            internal static extern bool GetPerformanceInfo(
                ref NativeMethods.PERFORMANCE_INFORMATION pPerformanceInformation,
                uint cb
            );
        }
    }
}
