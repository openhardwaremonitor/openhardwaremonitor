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
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.CPU {

  internal sealed class AMD10CPU : AMDCPU {

    private readonly Sensor coreTemperature;
    private readonly Sensor[] coreClocks;
    private readonly Sensor busClock;
      
    private const uint PERF_CTL_0 = 0xC0010000;
    private const uint PERF_CTR_0 = 0xC0010004;
    private const uint HWCR = 0xC0010015;
    private const uint P_STATE_0 = 0xC0010064;
    private const uint COFVID_STATUS = 0xC0010071;

    private const byte MISCELLANEOUS_CONTROL_FUNCTION = 3;
    private const ushort FAMILY_10H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1203;
    private const ushort FAMILY_11H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1303;
    private const ushort FAMILY_14H_MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1703; 
    private const uint REPORTED_TEMPERATURE_CONTROL_REGISTER = 0xA4;
    private const uint CLOCK_POWER_TIMING_CONTROL_0_REGISTER = 0xD4;

    private readonly uint miscellaneousControlAddress;
    private readonly ushort miscellaneousControlDeviceId;

    private readonly FileStream temperatureStream;

    private readonly double timeStampCounterMultiplier;
    private readonly bool corePerformanceBoostSupport;

    public AMD10CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) 
    {            
      // AMD family 10h/11h processors support only one temperature sensor
      coreTemperature = new Sensor(
        "Core" + (coreCount > 1 ? " #1 - #" + coreCount : ""), 0,
        SensorType.Temperature, this, new [] {
            new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
          }, settings);

      switch (family) {
        case 0x10: miscellaneousControlDeviceId =
          FAMILY_10H_MISCELLANEOUS_CONTROL_DEVICE_ID; break;
        case 0x11: miscellaneousControlDeviceId =
          FAMILY_11H_MISCELLANEOUS_CONTROL_DEVICE_ID; break;
        case 0x14: miscellaneousControlDeviceId = 
          FAMILY_14H_MISCELLANEOUS_CONTROL_DEVICE_ID; break;
        default: miscellaneousControlDeviceId = 0; break;
      }

      // get the pci address for the Miscellaneous Control registers 
      miscellaneousControlAddress = GetPciAddress(
        MISCELLANEOUS_CONTROL_FUNCTION, miscellaneousControlDeviceId);        

      busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this, settings);
      coreClocks = new Sensor[coreCount];
      for (int i = 0; i < coreClocks.Length; i++) {
        coreClocks[i] = new Sensor(CoreString(i), i + 1, SensorType.Clock,
          this, settings);
        if (HasTimeStampCounter)
          ActivateSensor(coreClocks[i]);
      }

      corePerformanceBoostSupport = (cpuid[0][0].ExtData[7, 3] & (1 << 9)) > 0;

      // set affinity to the first thread for all frequency estimations     
      ulong mask = ThreadAffinity.Set(1UL << cpuid[0][0].Thread);

      // disable core performance boost  
      uint hwcrEax, hwcrEdx;
      Ring0.Rdmsr(HWCR, out hwcrEax, out hwcrEdx);
      if (corePerformanceBoostSupport) 
        Ring0.Wrmsr(HWCR, hwcrEax | (1 << 25), hwcrEdx);

      uint ctlEax, ctlEdx;
      Ring0.Rdmsr(PERF_CTL_0, out ctlEax, out ctlEdx);
      uint ctrEax, ctrEdx;
      Ring0.Rdmsr(PERF_CTR_0, out ctrEax, out ctrEdx);

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
      int p = (int)Environment.OSVersion.Platform;
      if ((p == 4) || (p == 128)) {
        string[] devicePaths = Directory.GetDirectories("/sys/class/hwmon/");
        foreach (string path in devicePaths) {
          string name = null;
          try {
            using (StreamReader reader = new StreamReader(path + "/device/name"))
              name = reader.ReadLine();
          } catch (IOException) { }
          switch (name) {
            case "k10temp":
              temperatureStream = new FileStream(path + "/device/temp1_input", 
                FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
              break;
          }
        }
      }

      Update();                   
    }

    private double estimateTimeStampCounterMultiplier() {
      // preload the function
      estimateTimeStampCounterMultiplier(0);
      estimateTimeStampCounterMultiplier(0);

      // estimate the multiplier
      List<double> estimate = new List<double>(3);
      for (int i = 0; i < 3; i++)
        estimate.Add(estimateTimeStampCounterMultiplier(0.025));
      estimate.Sort();
      return estimate[1];
    }

    private double estimateTimeStampCounterMultiplier(double timeWindow) {
      uint eax, edx;
     
      // select event "076h CPU Clocks not Halted" and enable the counter
      Ring0.Wrmsr(PERF_CTL_0,
        (1 << 22) | // enable performance counter
        (1 << 17) | // count events in user mode
        (1 << 16) | // count events in operating-system mode
        0x76, 0x00000000);

      // set the counter to 0
      Ring0.Wrmsr(PERF_CTR_0, 0, 0);

      long ticks = (long)(timeWindow * Stopwatch.Frequency);
      uint lsbBegin, msbBegin, lsbEnd, msbEnd;      

      long timeBegin = Stopwatch.GetTimestamp() +
        (long)Math.Ceiling(0.001 * ticks);
      long timeEnd = timeBegin + ticks;
      while (Stopwatch.GetTimestamp() < timeBegin) { }
      Ring0.Rdmsr(PERF_CTR_0, out lsbBegin, out msbBegin);

      while (Stopwatch.GetTimestamp() < timeEnd) { }
      Ring0.Rdmsr(PERF_CTR_0, out lsbEnd, out msbEnd);
      Ring0.Rdmsr(COFVID_STATUS, out eax, out edx);

      double coreMultiplier;
      if (family == 0x14) {               
        uint divisorIdMSD = (eax >> 4) & 0x1F;
        uint divisorIdLSD = eax & 0xF;
        uint value = 0;
        Ring0.ReadPciConfig(miscellaneousControlAddress,
          CLOCK_POWER_TIMING_CONTROL_0_REGISTER, out value);
        uint frequencyId = value & 0x1F;

        coreMultiplier = 
          MultiplierFromIDs(divisorIdMSD, divisorIdLSD, frequencyId);
      } else {
        uint cpuDid = (eax >> 6) & 7;
        uint cpuFid = eax & 0x1F;
        coreMultiplier = MultiplierFromIDs(cpuDid, cpuFid);
      }
      ulong countBegin = ((ulong)msbBegin << 32) | lsbBegin;
      ulong countEnd = ((ulong)msbEnd << 32) | lsbEnd;

      double coreFrequency = 1e-6 * 
        (((double)(countEnd - countBegin)) * Stopwatch.Frequency) /
        (timeEnd - timeBegin);

      double busFrequency = coreFrequency / coreMultiplier;

      return 0.25 * Math.Round(4 * TimeStampCounterFrequency / busFrequency);
    }

    protected override uint[] GetMSRs() {
      return new uint[] { PERF_CTL_0, PERF_CTR_0, HWCR, P_STATE_0, 
        COFVID_STATUS };
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();
      r.Append(base.GetReport());

      r.Append("Miscellaneous Control Address: 0x");
      r.AppendLine((miscellaneousControlAddress).ToString("X",
        CultureInfo.InvariantCulture));
      r.Append("Time Stamp Counter Multiplier: ");
      r.AppendLine(timeStampCounterMultiplier.ToString(
        CultureInfo.InvariantCulture));
      if (family == 0x14) {
        uint value = 0;
        Ring0.ReadPciConfig(miscellaneousControlAddress,
          CLOCK_POWER_TIMING_CONTROL_0_REGISTER, out value);
        r.Append("PCI Register D18F3xD4: ");
        r.AppendLine(value.ToString("X8", CultureInfo.InvariantCulture));
      }
      r.AppendLine();

      return r.ToString();
    }

    // calculate the multiplier for family 10h based on Did and Fid
    private static double MultiplierFromIDs(uint divisorID, uint frequencyID) {
      return 0.5 * (frequencyID + 0x10) / (1 << (int)divisorID);
    }

    // calculate the multiplier for family 14h based on DidMSD, DidLSD and Fid
    private static double MultiplierFromIDs(uint divisorIdMSD, 
      uint divisorIdLSD, uint frequencyId) 
    {
      return (frequencyId + 0x10) / (divisorIdMSD + (divisorIdLSD * 0.25) + 1);
    }

    private string ReadFirstLine(Stream stream) {
      StringBuilder sb = new StringBuilder();
      try {
        stream.Seek(0, SeekOrigin.Begin);
        int b = stream.ReadByte();
        while (b != -1 && b != 10) {
          sb.Append((char)b);
          b = stream.ReadByte();
        }
      } catch { }
      return sb.ToString();
    }

    public override void Update() {
      base.Update();

      if (temperatureStream == null) {
        if (miscellaneousControlAddress != Ring0.InvalidPciAddress) {
          uint value;
          if (Ring0.ReadPciConfig(miscellaneousControlAddress,
            REPORTED_TEMPERATURE_CONTROL_REGISTER, out value)) {
            coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
              coreTemperature.Parameters[0].Value;
            ActivateSensor(coreTemperature);
          } else {
            DeactivateSensor(coreTemperature);
          }
        }
      } else {
        string s = ReadFirstLine(temperatureStream);
        try {
          coreTemperature.Value = 0.001f *
            long.Parse(s, CultureInfo.InvariantCulture);
          ActivateSensor(coreTemperature);
        } catch {
          DeactivateSensor(coreTemperature);
        }        
      }

      if (HasTimeStampCounter) {
        double newBusClock = 0;

        for (int i = 0; i < coreClocks.Length; i++) {
          Thread.Sleep(1);

          uint curEax, curEdx;
          if (Ring0.RdmsrTx(COFVID_STATUS, out curEax, out curEdx,
            1UL << cpuid[i][0].Thread)) 
          {
            double multiplier;
            if (family == 0x14) {
              uint divisorIdMSD = (curEax >> 4) & 0x1F;
              uint divisorIdLSD = curEax & 0xF;
              uint value = 0;
              Ring0.ReadPciConfig(miscellaneousControlAddress,
                CLOCK_POWER_TIMING_CONTROL_0_REGISTER, out value);
              uint frequencyId = value & 0x1F;
              multiplier =
                MultiplierFromIDs(divisorIdMSD, divisorIdLSD, frequencyId);
            } else {
              // 8:6 CpuDid: current core divisor ID
              // 5:0 CpuFid: current core frequency ID
              uint cpuDid = (curEax >> 6) & 7;
              uint cpuFid = curEax & 0x1F;
              multiplier = MultiplierFromIDs(cpuDid, cpuFid);
            }

            coreClocks[i].Value = 
              (float)(multiplier * TimeStampCounterFrequency / 
              timeStampCounterMultiplier);
            newBusClock = 
              (float)(TimeStampCounterFrequency / timeStampCounterMultiplier);
          } else {
            coreClocks[i].Value = (float)TimeStampCounterFrequency;
          }
        }

        if (newBusClock > 0) {
          this.busClock.Value = (float)newBusClock;
          ActivateSensor(this.busClock);
        }
      }
    }

    public override void Close() {
      if (temperatureStream != null) {
        temperatureStream.Close();
      }
    }
  }
}
