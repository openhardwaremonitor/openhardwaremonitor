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
using System.Diagnostics;
using System.Globalization;
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
    private const uint P_STATE_0 = 0xC0010064;
    private const uint COFVID_STATUS = 0xC0010071;

    private const byte MISCELLANEOUS_CONTROL_FUNCTION = 3;
    private const ushort MISCELLANEOUS_CONTROL_DEVICE_ID = 0x1203;
    private const uint REPORTED_TEMPERATURE_CONTROL_REGISTER = 0xA4;

    private readonly uint miscellaneousControlAddress;

    private double timeStampCounterMultiplier;

    public AMD10CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings) 
    {            
      // AMD family 10h processors support only one temperature sensor
      coreTemperature = new Sensor(
        "Core" + (coreCount > 1 ? " #1 - #" + coreCount : ""), 0,
        SensorType.Temperature, this, new [] {
            new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
          }, settings);

      // get the pci address for the Miscellaneous Control registers 
      miscellaneousControlAddress = GetPciAddress(
        MISCELLANEOUS_CONTROL_FUNCTION, MISCELLANEOUS_CONTROL_DEVICE_ID);

      busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this, settings);
      coreClocks = new Sensor[coreCount];
      for (int i = 0; i < coreClocks.Length; i++) {
        coreClocks[i] = new Sensor(CoreString(i), i + 1, SensorType.Clock,
          this, settings);
        if (HasTimeStampCounter)
          ActivateSensor(coreClocks[i]);
      }

      // set affinity to the first thread for all frequency estimations
      IntPtr thread = NativeMethods.GetCurrentThread();
      UIntPtr mask = NativeMethods.SetThreadAffinityMask(thread,
        (UIntPtr)(1L << cpuid[0][0].Thread));

      uint ctlEax, ctlEdx;
      WinRing0.Rdmsr(PERF_CTL_0, out ctlEax, out ctlEdx);
      uint ctrEax, ctrEdx;
      WinRing0.Rdmsr(PERF_CTR_0, out ctrEax, out ctrEdx);

      timeStampCounterMultiplier = estimateTimeStampCounterMultiplier();

      // restore the performance counter registers
      WinRing0.Wrmsr(PERF_CTL_0, ctlEax, ctlEdx);
      WinRing0.Wrmsr(PERF_CTR_0, ctrEax, ctrEdx);

      // restore the thread affinity.
      NativeMethods.SetThreadAffinityMask(thread, mask);

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
      WinRing0.Wrmsr(PERF_CTL_0,
        (1 << 22) | // enable performance counter
        (1 << 17) | // count events in user mode
        (1 << 16) | // count events in operating-system mode
        0x76, 0x00000000);

      // set the counter to 0
      WinRing0.Wrmsr(PERF_CTR_0, 0, 0);

      long ticks = (long)(timeWindow * Stopwatch.Frequency);
      uint lsbBegin, msbBegin, lsbEnd, msbEnd;

      long timeBegin = Stopwatch.GetTimestamp() +
        (long)Math.Ceiling(0.001 * ticks);
      long timeEnd = timeBegin + ticks;
      while (Stopwatch.GetTimestamp() < timeBegin) { }
      WinRing0.Rdmsr(PERF_CTR_0, out lsbBegin, out msbBegin);
      while (Stopwatch.GetTimestamp() < timeEnd) { }
      WinRing0.Rdmsr(PERF_CTR_0, out lsbEnd, out msbEnd);

      WinRing0.Rdmsr(COFVID_STATUS, out eax, out edx);
      uint cpuDid = (eax >> 6) & 7;
      uint cpuFid = eax & 0x1F;
      double coreMultiplier = MultiplierFromIDs(cpuDid, cpuFid);

      ulong countBegin = ((ulong)msbBegin << 32) | lsbBegin;
      ulong countEnd = ((ulong)msbEnd << 32) | lsbEnd;

      double coreFrequency = 1e-6 * 
        (((double)(countEnd - countBegin)) * Stopwatch.Frequency) /
        (timeEnd - timeBegin);

      double busFrequency = coreFrequency / coreMultiplier;
      return 0.5 * Math.Round(2 * TimeStampCounterFrequency / busFrequency);
    }

    protected override uint[] GetMSRs() {
      return new uint[] { PERF_CTL_0, PERF_CTR_0, P_STATE_0, COFVID_STATUS };
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
      r.AppendLine();

      return r.ToString();
    }

    private static double MultiplierFromIDs(uint divisorID, uint frequencyID) {
      return 0.5 * (frequencyID + 0x10) / (1 << (int)divisorID);
    }

    public override void Update() {
      base.Update();

      if (miscellaneousControlAddress != WinRing0.InvalidPciAddress) {
        uint value;
        if (WinRing0.ReadPciConfigDwordEx(miscellaneousControlAddress,
          REPORTED_TEMPERATURE_CONTROL_REGISTER, out value)) {
          coreTemperature.Value = ((value >> 21) & 0x7FF) / 8.0f +
            coreTemperature.Parameters[0].Value;
          ActivateSensor(coreTemperature);
        } else {
          DeactivateSensor(coreTemperature);
        }
      }

      if (HasTimeStampCounter) {
        double newBusClock = 0;

        for (int i = 0; i < coreClocks.Length; i++) {
          Thread.Sleep(1);

          uint curEax, curEdx;
          if (WinRing0.RdmsrTx(COFVID_STATUS, out curEax, out curEdx,
            (UIntPtr)(1L << cpuid[i][0].Thread))) 
          {
            // 8:6 CpuDid: current core divisor ID
            // 5:0 CpuFid: current core frequency ID
            uint cpuDid = (curEax >> 6) & 7;
            uint cpuFid = curEax & 0x1F;
            double multiplier = MultiplierFromIDs(cpuDid, cpuFid);

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

    private static class NativeMethods {
      private const string KERNEL = "kernel32.dll";

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern UIntPtr
        SetThreadAffinityMask(IntPtr handle, UIntPtr mask);

      [DllImport(KERNEL, CallingConvention = CallingConvention.Winapi)]
      public static extern IntPtr GetCurrentThread();
    }
  }
}
