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
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.CPU {
  internal class GenericCPU : Hardware {

    protected readonly CPUID[][] cpuid;
   
    protected readonly uint family;
    protected readonly uint model;
    protected readonly uint stepping;

    protected readonly int processorIndex;
    protected readonly int coreCount;
    protected readonly string name;

    protected readonly bool hasTSC;
    protected readonly bool invariantTSC;
    private readonly double estimatedMaxClock;

    private ulong lastTimeStampCount;
    private long lastTime;
    private double maxClock;    

    private readonly Vendor vendor;

    private readonly CPULoad cpuLoad;
    private readonly Sensor totalLoad;
    private readonly Sensor[] coreLoads;

    protected string CoreString(int i) {
      if (coreCount == 1)
        return "CPU Core";
      else
        return "CPU Core #" + (i + 1);
    }

    public GenericCPU(int processorIndex, CPUID[][] cpuid, ISettings settings) {
      this.cpuid = cpuid;

      this.vendor = cpuid[0][0].Vendor;

      this.family = cpuid[0][0].Family;
      this.model = cpuid[0][0].Model;
      this.stepping = cpuid[0][0].Stepping;

      this.processorIndex = processorIndex;
      this.coreCount = cpuid.Length;
      this.name = cpuid[0][0].Name;      

      // check if processor has TSC
      if (cpuid[0][0].Data.GetLength(0) > 1
        && (cpuid[0][0].Data[1, 3] & 0x10) != 0)
        hasTSC = true;
      else
        hasTSC = false;

      // check if processor supports invariant TSC 
      if (cpuid[0][0].ExtData.GetLength(0) > 7
        && (cpuid[0][0].ExtData[7, 3] & 0x100) != 0)
        invariantTSC = true;
      else
        invariantTSC = false;

      if (coreCount > 1)
        totalLoad = new Sensor("CPU Total", 0, SensorType.Load, this, settings);
      else
        totalLoad = null;
      coreLoads = new Sensor[coreCount];
      for (int i = 0; i < coreLoads.Length; i++)
        coreLoads[i] = new Sensor(CoreString(i), i + 1,
          SensorType.Load, this, settings);
      cpuLoad = new CPULoad(cpuid);
      if (cpuLoad.IsAvailable) {
        foreach (Sensor sensor in coreLoads)
          ActivateSensor(sensor);
        if (totalLoad != null)
          ActivateSensor(totalLoad);
      }

      if (hasTSC)
        estimatedMaxClock = EstimateMaxClock();
      else
        estimatedMaxClock = 0;
      maxClock = estimatedMaxClock;

      lastTimeStampCount = 0;
      lastTime = 0;
    }

    private static double EstimateMaxClock() {
      // preload the function
      EstimateMaxClock(0);
      EstimateMaxClock(0);

      // estimate the max clock in MHz      
      List<double> estimatedMaxClocks = new List<double>(3);
      for (int i = 0; i < 3; i++)
        estimatedMaxClocks.Add(1e-6 * EstimateMaxClock(0.025));
      estimatedMaxClocks.Sort();
      return estimatedMaxClocks[1];
    }

    private static double EstimateMaxClock(double timeWindow) {
      long ticks = (long)(timeWindow * Stopwatch.Frequency);
      uint lsbBegin, msbBegin, lsbEnd, msbEnd;

      Thread.BeginThreadAffinity();
      long timeBegin = Stopwatch.GetTimestamp() +
        (long)Math.Ceiling(0.001 * ticks);
      long timeEnd = timeBegin + ticks;
      while (Stopwatch.GetTimestamp() < timeBegin) { }
      WinRing0.Rdtsc(out lsbBegin, out msbBegin);
      while (Stopwatch.GetTimestamp() < timeEnd) { }
      WinRing0.Rdtsc(out lsbEnd, out msbEnd);
      Thread.EndThreadAffinity();

      ulong countBegin = ((ulong)msbBegin << 32) | lsbBegin;
      ulong countEnd = ((ulong)msbEnd << 32) | lsbEnd;

      return (((double)(countEnd - countBegin)) * Stopwatch.Frequency) /
        (timeEnd - timeBegin);
    }

    private static void AppendMSRData(StringBuilder r, uint msr, int thread) {
      uint eax, edx;
      if (WinRing0.RdmsrTx(msr, out eax, out edx, (UIntPtr)(1L << thread))) {
        r.Append(" ");
        r.Append((msr).ToString("X8", CultureInfo.InvariantCulture));
        r.Append("  ");
        r.Append((edx).ToString("X8", CultureInfo.InvariantCulture));
        r.Append("  ");
        r.Append((eax).ToString("X8", CultureInfo.InvariantCulture));
        r.AppendLine();
      }
    }

    protected virtual uint[] GetMSRs() {
      return null;
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      switch (vendor) {
        case Vendor.AMD: r.AppendLine("AMD CPU"); break;
        case Vendor.Intel: r.AppendLine("Intel CPU"); break;
        default: r.AppendLine("Generic CPU"); break;
      }

      r.AppendLine();
      r.AppendFormat("Name: {0}{1}", name, Environment.NewLine);
      r.AppendFormat("Number of Cores: {0}{1}", coreCount,
        Environment.NewLine);
      r.AppendFormat("Threads per Core: {0}{1}", cpuid[0].Length,
        Environment.NewLine);
      r.AppendLine("TSC: " +
        (hasTSC ? (invariantTSC ? "Invariant" : "Not Invariant") : "None"));
      r.AppendLine(string.Format(CultureInfo.InvariantCulture,
        "Timer Frequency: {0} MHz", Stopwatch.Frequency * 1e-6));
      r.AppendLine(string.Format(CultureInfo.InvariantCulture,
        "Max Clock: {0} MHz", Math.Round(maxClock * 100) * 0.01));
      r.AppendLine();

      uint[] msrArray = GetMSRs();
      if (msrArray != null && msrArray.Length > 0) {
        for (int i = 0; i < cpuid.Length; i++) {
          r.AppendLine("MSR Core #" + (i + 1));
          r.AppendLine();
          r.AppendLine(" MSR       EDX       EAX");
          foreach (uint msr in msrArray)
            AppendMSRData(r, msr, cpuid[i][0].Thread);
          r.AppendLine();
        }
      }

      return r.ToString();
    }

    public override Identifier Identifier {
      get {
        string s;
        switch (vendor) {
          case Vendor.AMD: s = "amdcpu"; break;
          case Vendor.Intel: s = "intelcpu"; break;
          default: s = "genericcpu"; break;
        }
        return new Identifier(s, 
          processorIndex.ToString(CultureInfo.InvariantCulture));
      }
    }

    public override string Name {
      get { return name; }
    }

    public override HardwareType HardwareType {
      get { return HardwareType.CPU; }
    }

    protected double MaxClock {
      get { return maxClock; }
    }

    public override void Update() {
      if (hasTSC) {
        uint lsb, msb;
        WinRing0.RdtscTx(out lsb, out msb, (UIntPtr)1);
        long time = Stopwatch.GetTimestamp();
        ulong timeStampCount = ((ulong)msb << 32) | lsb;
        double delta = ((double)(time - lastTime)) / Stopwatch.Frequency;
        if (delta > 0.5) {
          if (invariantTSC)
            maxClock = (timeStampCount - lastTimeStampCount) / (1e6 * delta);
          else
            maxClock = estimatedMaxClock;

          lastTimeStampCount = timeStampCount;
          lastTime = time;
        }        
      }

      if (cpuLoad.IsAvailable) {
        cpuLoad.Update();
        for (int i = 0; i < coreLoads.Length; i++)
          coreLoads[i].Value = cpuLoad.GetCoreLoad(i);
        if (totalLoad != null)
          totalLoad.Value = cpuLoad.GetTotalLoad();
      }
    }
  }
}
