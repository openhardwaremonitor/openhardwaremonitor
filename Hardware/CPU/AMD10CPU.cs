/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2013 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.CPU
{
    internal sealed class AMD10CPU : AMDCPU
    {
        private const uint PERF_CTL_0 = 0xC0010000;
        private const uint PERF_CTR_0 = 0xC0010004;
        private const uint HWCR = 0xC0010015;
        private const uint P_STATE_0 = 0xC0010064;
        private const uint COFVID_STATUS = 0xC0010071;

        private const byte MISCELLANEOUS_CONTROL_FUNCTION = 3;
        private const ushort FAMILY_10H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1203;
        private const ushort FAMILY_11H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1303;
        private const ushort FAMILY_12H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1703;
        private const ushort FAMILY_14H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1703;
        private const ushort FAMILY_15H_MODEL_00_MISC_CONTROL_DEVICE_ID = 0x1603;
        private const ushort FAMILY_15H_MODEL_10_MISC_CONTROL_DEVICE_ID = 0x1403;
        private const ushort FAMILY_15H_MODEL_30_MISC_CONTROL_DEVICE_ID = 0x141D;
        private const ushort FAMILY_16H_MODEL_00_MISC_CONTROL_DEVICE_ID = 0x1533;
        private const ushort FAMILY_16H_MODEL_30_MISC_CONTROL_DEVICE_ID = 0x1583;

        private const uint REPORTED_TEMPERATURE_CONTROL_REGISTER = 0xA4;
        private const uint CLOCK_POWER_TIMING_CONTROL_0_REGISTER = 0xD4;
        private readonly Sensor busClock;
        private readonly Sensor[] coreClocks;
        private readonly bool corePerformanceBoostSupport;

        private readonly Sensor coreTemperature;

        private readonly uint miscellaneousControlAddress;
        private readonly ushort miscellaneousControlDeviceId;

        private readonly FileStream temperatureStream;

        private readonly double timeStampCounterMultiplier;

        public AMD10CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
            : base(processorIndex, cpuid, settings)
        {
            // AMD family 1Xh processors support only one temperature sensor
            coreTemperature = new Sensor(
                "Core" + (coreCount > 1 ? " #1 - #" + coreCount : ""), 0,
                SensorType.Temperature, this, new[]
                {
                    new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
                }, settings);

            switch (family)
            {
                case 0x10:
                    miscellaneousControlDeviceId =
                        FAMILY_10H_MISCELLANEOUS_CONTROL_DEVICE_ID;
                    break;
                case 0x11:
                    miscellaneousControlDeviceId =
                        FAMILY_11H_MISCELLANEOUS_CONTROL_DEVICE_ID;
                    break;
                case 0x12:
                    miscellaneousControlDeviceId =
                        FAMILY_12H_MISCELLANEOUS_CONTROL_DEVICE_ID;
                    break;
                case 0x14:
                    miscellaneousControlDeviceId =
                        FAMILY_14H_MISCELLANEOUS_CONTROL_DEVICE_ID;
                    break;
                case 0x15:
                    switch (model & 0xF0)
                    {
                        case 0x00:
                            miscellaneousControlDeviceId =
                                FAMILY_15H_MODEL_00_MISC_CONTROL_DEVICE_ID;
                            break;
                        case 0x10:
                            miscellaneousControlDeviceId =
                                FAMILY_15H_MODEL_10_MISC_CONTROL_DEVICE_ID;
                            break;
                        case 0x30:
                            miscellaneousControlDeviceId =
                                FAMILY_15H_MODEL_30_MISC_CONTROL_DEVICE_ID;
                            break;
                        default:
                            miscellaneousControlDeviceId = 0;
                            break;
                    }
                    break;
                case 0x16:
                    switch (model & 0xF0)
                    {
                        case 0x00:
                            miscellaneousControlDeviceId =
                                FAMILY_16H_MODEL_00_MISC_CONTROL_DEVICE_ID;
                            break;
                        case 0x30:
                            miscellaneousControlDeviceId =
                                FAMILY_16H_MODEL_30_MISC_CONTROL_DEVICE_ID;
                            break;
                        default:
                            miscellaneousControlDeviceId = 0;
                            break;
                    }
                    break;
                default:
                    miscellaneousControlDeviceId = 0;
                    break;
            }

            // get the pci address for the Miscellaneous Control registers 
            miscellaneousControlAddress = GetPciAddress(
                MISCELLANEOUS_CONTROL_FUNCTION, miscellaneousControlDeviceId);

            busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this, settings);
            coreClocks = new Sensor[coreCount];
            for (var i = 0; i < coreClocks.Length; i++)
            {
                coreClocks[i] = new Sensor(CoreString(i), i + 1, SensorType.Clock,
                    this, settings);
                if (HasTimeStampCounter)
                    ActivateSensor(coreClocks[i]);
            }

            corePerformanceBoostSupport = (cpuid[0][0].ExtData[7, 3] & (1 << 9)) > 0;

            // set affinity to the first thread for all frequency estimations     
            var mask = ThreadAffinity.Set(1UL << cpuid[0][0].Thread);
            Ring0.Rdmsr(HWCR, out uint hwcrEax, out uint hwcrEdx);
            if (corePerformanceBoostSupport)
                Ring0.Wrmsr(HWCR, hwcrEax | (1 << 25), hwcrEdx);
            Ring0.Rdmsr(PERF_CTL_0, out uint ctlEax, out uint ctlEdx);
            Ring0.Rdmsr(PERF_CTR_0, out uint ctrEax, out uint ctrEdx);

            timeStampCounterMultiplier = estimateTimeStampCounterMultiplier();

            // restore the performance counter registers
            Ring0.Wrmsr(PERF_CTL_0, ctlEax, ctlEdx);
            Ring0.Wrmsr(PERF_CTR_0, ctrEax, ctrEdx);

            // restore core performance boost
            if (corePerformanceBoostSupport)
                Ring0.Wrmsr(HWCR, hwcrEax, hwcrEdx);

            // restore the thread affinity.
            ThreadAffinity.Set(mask);

            // the file reader for lm-sensors support on Linux
            temperatureStream = null;
            var p = (int) Environment.OSVersion.Platform;
            if (p == 4 || p == 128)
            {
                var devicePaths = Directory.GetDirectories("/sys/class/hwmon/");
                foreach (var path in devicePaths)
                {
                    string name = null;
                    try
                    {
                        using (var reader = new StreamReader(path + "/device/name"))
                        {
                            name = reader.ReadLine();
                        }
                    }
                    catch (IOException)
                    {
                    }
                    switch (name)
                    {
                        case "k10temp":
                            temperatureStream = new FileStream(path + "/device/temp1_input",
                                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                            break;
                    }
                }
            }

            Update();
        }

        private double estimateTimeStampCounterMultiplier()
        {
            // preload the function
            estimateTimeStampCounterMultiplier(0);
            estimateTimeStampCounterMultiplier(0);

            // estimate the multiplier
            var estimate = new List<double>(3);
            for (var i = 0; i < 3; i++)
                estimate.Add(estimateTimeStampCounterMultiplier(0.025));
            estimate.Sort();
            return estimate[1];
        }

        private double estimateTimeStampCounterMultiplier(double timeWindow)
        {

            // select event "076h CPU Clocks not Halted" and enable the counter
            Ring0.Wrmsr(PERF_CTL_0,
                (1 << 22) | // enable performance counter
                (1 << 17) | // count events in user mode
                (1 << 16) | // count events in operating-system mode
                0x76, 0x00000000);

            // set the counter to 0
            Ring0.Wrmsr(PERF_CTR_0, 0, 0);

            var ticks = (long) (timeWindow * Stopwatch.Frequency);
            var timeBegin = Stopwatch.GetTimestamp() +
                            (long) Math.Ceiling(0.001 * ticks);
            var timeEnd = timeBegin + ticks;
            while (Stopwatch.GetTimestamp() < timeBegin)
            {
            }
            Ring0.Rdmsr(PERF_CTR_0, out uint lsbBegin, out uint msbBegin);

            while (Stopwatch.GetTimestamp() < timeEnd)
            {
            }
            Ring0.Rdmsr(PERF_CTR_0, out uint lsbEnd, out uint msbEnd);
            Ring0.Rdmsr(COFVID_STATUS, out uint eax, out uint edx);
            var coreMultiplier = GetCoreMultiplier(eax);

            var countBegin = ((ulong) msbBegin << 32) | lsbBegin;
            var countEnd = ((ulong) msbEnd << 32) | lsbEnd;

            var coreFrequency = 1e-6 *
                                ((double) (countEnd - countBegin) * Stopwatch.Frequency) /
                                (timeEnd - timeBegin);

            var busFrequency = coreFrequency / coreMultiplier;

            return 0.25 * Math.Round(4 * TimeStampCounterFrequency / busFrequency);
        }

        protected override uint[] GetMSRs()
        {
            return new[]
            {
                PERF_CTL_0, PERF_CTR_0, HWCR, P_STATE_0,
                COFVID_STATUS
            };
        }

        public override string GetReport()
        {
            var r = new StringBuilder();
            r.Append(base.GetReport());

            r.Append("Miscellaneous Control Address: 0x");
            r.AppendLine(miscellaneousControlAddress.ToString("X",
                CultureInfo.InvariantCulture));
            r.Append("Time Stamp Counter Multiplier: ");
            r.AppendLine(timeStampCounterMultiplier.ToString(
                CultureInfo.InvariantCulture));
            if (family == 0x14)
            {
                Ring0.ReadPciConfig(miscellaneousControlAddress,
                    CLOCK_POWER_TIMING_CONTROL_0_REGISTER, out uint value);
                r.Append("PCI Register D18F3xD4: ");
                r.AppendLine(value.ToString("X8", CultureInfo.InvariantCulture));
            }
            r.AppendLine();

            return r.ToString();
        }

        private double GetCoreMultiplier(uint cofvidEax)
        {
            switch (family)
            {
                case 0x10:
                case 0x11:
                case 0x15:
                case 0x16:
                {
                    // 8:6 CpuDid: current core divisor ID
                    // 5:0 CpuFid: current core frequency ID
                    var cpuDid = (cofvidEax >> 6) & 7;
                    var cpuFid = cofvidEax & 0x1F;
                    return 0.5 * (cpuFid + 0x10) / (1 << (int) cpuDid);
                }
                case 0x12:
                {
                    // 8:4 CpuFid: current CPU core frequency ID
                    // 3:0 CpuDid: current CPU core divisor ID
                    var cpuFid = (cofvidEax >> 4) & 0x1F;
                    var cpuDid = cofvidEax & 0xF;
                    double divisor;
                    switch (cpuDid)
                    {
                        case 0:
                            divisor = 1;
                            break;
                        case 1:
                            divisor = 1.5;
                            break;
                        case 2:
                            divisor = 2;
                            break;
                        case 3:
                            divisor = 3;
                            break;
                        case 4:
                            divisor = 4;
                            break;
                        case 5:
                            divisor = 6;
                            break;
                        case 6:
                            divisor = 8;
                            break;
                        case 7:
                            divisor = 12;
                            break;
                        case 8:
                            divisor = 16;
                            break;
                        default:
                            divisor = 1;
                            break;
                    }
                    return (cpuFid + 0x10) / divisor;
                }
                case 0x14:
                {
                    // 8:4: current CPU core divisor ID most significant digit
                    // 3:0: current CPU core divisor ID least significant digit
                    var divisorIdMSD = (cofvidEax >> 4) & 0x1F;
                    var divisorIdLSD = cofvidEax & 0xF;
                        Ring0.ReadPciConfig(miscellaneousControlAddress,
                            CLOCK_POWER_TIMING_CONTROL_0_REGISTER, out uint value);
                        var frequencyId = value & 0x1F;
                    return (frequencyId + 0x10) /
                           (divisorIdMSD + divisorIdLSD * 0.25 + 1);
                }
                default:
                    return 1;
            }
        }

        private string ReadFirstLine(Stream stream)
        {
            var sb = new StringBuilder();
            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var b = stream.ReadByte();
                while (b != -1 && b != 10)
                {
                    sb.Append((char) b);
                    b = stream.ReadByte();
                }
            }
            catch
            {
            }
            return sb.ToString();
        }

        public override void Update()
        {
            base.Update();

            if (temperatureStream == null)
            {
                if (miscellaneousControlAddress != Ring0.InvalidPciAddress)
                {
                    if (Ring0.ReadPciConfig(miscellaneousControlAddress,
                        REPORTED_TEMPERATURE_CONTROL_REGISTER, out uint value))
                    {
                        if (family == 0x15 && (value & 0x30000) == 0x30000)
                            if ((model & 0xF0) == 0x00)
                                coreTemperature.Value = ((value >> 21) & 0x7FC) / 8.0f +
                                                        coreTemperature.Parameters[0].Value - 49;
                            else
                                coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
                                                        coreTemperature.Parameters[0].Value - 49;
                        else if (family == 0x16 &&
                                 ((value & 0x30000) == 0x30000 || (value & 0x80000) == 0x80000))
                            coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
                                                    coreTemperature.Parameters[0].Value - 49;
                        else
                            coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
                                                    coreTemperature.Parameters[0].Value;
                        ActivateSensor(coreTemperature);
                    }
                    else
                    {
                        DeactivateSensor(coreTemperature);
                    }
                }
            }
            else
            {
                var s = ReadFirstLine(temperatureStream);
                try
                {
                    coreTemperature.Value = 0.001f *
                                            long.Parse(s, CultureInfo.InvariantCulture);
                    ActivateSensor(coreTemperature);
                }
                catch
                {
                    DeactivateSensor(coreTemperature);
                }
            }

            if (HasTimeStampCounter)
            {
                double newBusClock = 0;

                for (var i = 0; i < coreClocks.Length; i++)
                {
                    Thread.Sleep(1);
                    if (Ring0.RdmsrTx(COFVID_STATUS, out uint curEax, out uint curEdx,
                        1UL << cpuid[i][0].Thread))
                    {
                        double multiplier;
                        multiplier = GetCoreMultiplier(curEax);

                        coreClocks[i].Value =
                            (float) (multiplier * TimeStampCounterFrequency /
                                     timeStampCounterMultiplier);
                        newBusClock =
                            (float) (TimeStampCounterFrequency / timeStampCounterMultiplier);
                    }
                    else
                    {
                        coreClocks[i].Value = (float) TimeStampCounterFrequency;
                    }
                }

                if (newBusClock > 0)
                {
                    busClock.Value = (float) newBusClock;
                    ActivateSensor(busClock);
                }
            }
        }

        public override void Close()
        {
            if (temperatureStream != null) temperatureStream.Close();
            base.Close();
        }
    }
}