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
using System.Drawing;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {
  public class IntelCPU : Hardware, IHardware {

    private int processorIndex;
    private CPUID[][] cpuid;
    private int coreCount;
    
    private string name;
    private Image icon;

    private uint family;
    private uint model;
    private uint stepping;

    private Sensor[] coreTemperatures;

    private Sensor totalLoad;
    private Sensor[] coreLoads;
    private Sensor[] coreClocks;
    private Sensor busClock;    
    private bool hasTSC;
    private bool invariantTSC;    
    private double estimatedMaxClock;

    private CPULoad cpuLoad;

    private ulong lastTimeStampCount;    
    private long lastTime;
    private uint maxNehalemMultiplier = 0;    
    
    private const uint IA32_THERM_STATUS_MSR = 0x019C;
    private const uint IA32_TEMPERATURE_TARGET = 0x01A2;
    private const uint IA32_PERF_STATUS = 0x0198;
    private const uint MSR_PLATFORM_INFO = 0xCE;

    private string CoreString(int i) {
      if (coreCount == 1)
        return "CPU Core";
      else
        return "CPU Core #" + (i + 1);
    }

    private float[] Floats(float f) {
      float[] result = new float[coreCount];
      for (int i = 0; i < coreCount; i++)
        result[i] = f;
      return result;
    }

    public IntelCPU(int processorIndex, CPUID[][] cpuid) {

      this.processorIndex = processorIndex;
      this.cpuid = cpuid;
      this.coreCount = cpuid.Length;
      this.name = cpuid[0][0].Name;
      this.icon = Utilities.EmbeddedResources.GetImage("cpu.png");

      this.family = cpuid[0][0].Family;
      this.model = cpuid[0][0].Model;
      this.stepping = cpuid[0][0].Stepping;

      float[] tjMax;
      switch (family) {
        case 0x06: {
            switch (model) {
              case 0x0F: // Intel Core (65nm)
                switch (stepping) {
                  case 0x06: // B2
                    switch (coreCount) {
                      case 2:
                        tjMax = Floats(80 + 10); break;
                      case 4:
                        tjMax = Floats(90 + 10); break;
                      default:
                        tjMax = Floats(85 + 10); break;
                    }
                    tjMax = Floats(80 + 10); break;
                  case 0x0B: // G0
                    tjMax = Floats(90 + 10); break;
                  case 0x0D: // M0
                    tjMax = Floats(85 + 10); break;
                  default:
                    tjMax = Floats(85 + 10); break;
                } break;
              case 0x17: // Intel Core (45nm)
                tjMax = Floats(100); break;
              case 0x1C: // Intel Atom 
                tjMax = Floats(90); break;
              case 0x1A: // Intel Core i7 LGA1366 (45nm)
              case 0x1E: // Intel Core i5, i7 LGA1156 (45nm)
              case 0x25: // Intel Core i3, i5, i7 LGA1156 (32nm)
              case 0x2C: // Intel Core i7 LGA1366 (32nm) 6 Core
                uint eax, edx;
                tjMax = new float[coreCount];
                for (int i = 0; i < coreCount; i++) {
                  if (WinRing0.RdmsrTx(IA32_TEMPERATURE_TARGET, out eax,
                    out edx, (UIntPtr)(1L << cpuid[i][0].Thread)))
                  {
                    tjMax[i] = (eax >> 16) & 0xFF;
                  } else {
                    tjMax[i] = 100;
                  }
                }
                if (WinRing0.Rdmsr(MSR_PLATFORM_INFO, out eax, out edx)) {
                  maxNehalemMultiplier = (eax >> 8) & 0xff;
                }
                break;
              default:
                tjMax = Floats(100); break;
            }
          } break;
        default: tjMax = Floats(100); break;
      }

      // check if processor supports a digital thermal sensor
      if (cpuid[0][0].Data.GetLength(0) > 6 && 
        (cpuid[0][0].Data[6, 0] & 1) != 0) 
      {
        coreTemperatures = new Sensor[coreCount];
        for (int i = 0; i < coreTemperatures.Length; i++) {
          coreTemperatures[i] = new Sensor(CoreString(i), i, tjMax[i],
            SensorType.Temperature, this, new ParameterDescription[] { 
              new ParameterDescription(
                "TjMax", "TjMax temperature of the core.\n" + 
                "Temperature = TjMax - TSlope * Value.", tjMax[i]), 
              new ParameterDescription(
                "TSlope", "Temperature slope of the digital thermal sensor.\n" + 
                "Temperature = TjMax - TSlope * Value.", 1)});
        }
      } else {
        coreTemperatures = new Sensor[0];
      }

      if (coreCount > 1)
        totalLoad = new Sensor("CPU Total", 0, SensorType.Load, this);
      else
        totalLoad = null;
      coreLoads = new Sensor[coreCount];
      for (int i = 0; i < coreLoads.Length; i++)
        coreLoads[i] = new Sensor(CoreString(i), i + 1,
          SensorType.Load, this);     
      cpuLoad = new CPULoad(cpuid);
      if (cpuLoad.IsAvailable) {
        foreach (Sensor sensor in coreLoads)
          ActivateSensor(sensor);
        if (totalLoad != null)
          ActivateSensor(totalLoad);
      }

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

      // preload the function
      EstimateMaxClock(0); 
      EstimateMaxClock(0); 

      // estimate the max clock in MHz      
      List<double> estimatedMaxClocks = new List<double>(3);
      for (int i = 0; i < 3; i++)
        estimatedMaxClocks.Add(1e-6 * EstimateMaxClock(0.025));
      estimatedMaxClocks.Sort();
      estimatedMaxClock = estimatedMaxClocks[1];

      lastTimeStampCount = 0;
      lastTime = 0;
      busClock = new Sensor("Bus Speed", 0, SensorType.Clock, this);      
      coreClocks = new Sensor[coreCount];
      for (int i = 0; i < coreClocks.Length; i++) {
        coreClocks[i] =
          new Sensor(CoreString(i), i + 1, SensorType.Clock, this);
        if (hasTSC)
          ActivateSensor(coreClocks[i]);
      }
      
      Update();                   
    }

    public override string Name {
      get { return name; }
    }

    public override Identifier Identifier {
      get { return new Identifier("intelcpu", processorIndex.ToString()); }
    }

    public override Image Icon {
      get { return icon; }
    }

    private void AppendMSRData(StringBuilder r, uint msr, int thread) {
      uint eax, edx;
      if (WinRing0.RdmsrTx(msr, out eax, out edx, (UIntPtr)(1L << thread))) {
        r.Append(" ");
        r.Append((msr).ToString("X8"));
        r.Append("  ");
        r.Append((edx).ToString("X8"));
        r.Append("  ");
        r.Append((eax).ToString("X8"));
        r.AppendLine();
      }
    }

    public override string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("Intel CPU");
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
        "Max Clock: {0} MHz", Math.Round(estimatedMaxClock * 100) * 0.01));
      r.AppendLine();

      for (int i = 0; i < cpuid.Length; i++) {
        r.AppendLine("MSR Core #" + (i + 1));
        r.AppendLine();
        r.AppendLine(" MSR       EDX       EAX");
        AppendMSRData(r, MSR_PLATFORM_INFO, cpuid[i][0].Thread);
        AppendMSRData(r, IA32_PERF_STATUS, cpuid[i][0].Thread);
        AppendMSRData(r, IA32_THERM_STATUS_MSR, cpuid[i][0].Thread);
        AppendMSRData(r, IA32_TEMPERATURE_TARGET, cpuid[i][0].Thread);
        r.AppendLine();
      }

      return r.ToString();
    }

    private double EstimateMaxClock(double timeWindow) {
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

    public override void Update() {      
      for (int i = 0; i < coreTemperatures.Length; i++) {
        uint eax, edx;
        if (WinRing0.RdmsrTx(
          IA32_THERM_STATUS_MSR, out eax, out edx, 
            (UIntPtr)(1L << cpuid[i][0].Thread))) {
          // if reading is valid
          if ((eax & 0x80000000) != 0) {
            // get the dist from tjMax from bits 22:16
            float deltaT = ((eax & 0x007F0000) >> 16);
            float tjMax = coreTemperatures[i].Parameters[0].Value;
            float tSlope = coreTemperatures[i].Parameters[1].Value;
            coreTemperatures[i].Value = tjMax - tSlope * deltaT;
            ActivateSensor(coreTemperatures[i]);
          } else {
            DeactivateSensor(coreTemperatures[i]);
          }
        }
      }

      if (cpuLoad.IsAvailable) {
        cpuLoad.Update();
        for (int i = 0; i < coreLoads.Length; i++)
          coreLoads[i].Value = cpuLoad.GetCoreLoad(i);
        if (totalLoad != null)
          totalLoad.Value = cpuLoad.GetTotalLoad();
      }

      if (hasTSC) {
        uint lsb, msb;
        WinRing0.RdtscTx(out lsb, out msb, (UIntPtr)1);
        long time = Stopwatch.GetTimestamp();
        ulong timeStampCount = ((ulong)msb << 32) | lsb;
        double delta = ((double)(time - lastTime)) / Stopwatch.Frequency;
        if (delta > 0.5) {
          double maxClock;
          if (invariantTSC)
            maxClock = (timeStampCount - lastTimeStampCount) / (1e6 * delta);
          else
            maxClock = estimatedMaxClock;

          double busClock = 0;
          uint eax, edx;
          for (int i = 0; i < coreClocks.Length; i++) {
            System.Threading.Thread.Sleep(1);
            if (WinRing0.RdmsrTx(IA32_PERF_STATUS, out eax, out edx,
              (UIntPtr)(1L << cpuid[i][0].Thread))) {
              if (maxNehalemMultiplier > 0) { // Core i3, i5, i7
                uint nehalemMultiplier = eax & 0xff;
                coreClocks[i].Value =
                  (float)(nehalemMultiplier * maxClock / maxNehalemMultiplier);
                busClock = (float)(maxClock / maxNehalemMultiplier);
              } else { // Core 2
                uint multiplier = (eax >> 8) & 0x1f;
                uint maxMultiplier = (edx >> 8) & 0x1f;
                // factor = multiplier * 2 to handle non integer multipliers 
                uint factor = (multiplier << 1) | ((eax >> 14) & 1);
                uint maxFactor = (maxMultiplier << 1) | ((edx >> 14) & 1);
                if (maxFactor > 0) {
                  coreClocks[i].Value = (float)(factor * maxClock / maxFactor);
                  busClock = (float)(2 * maxClock / maxFactor);
                }
              }
            } else { // Intel Pentium 4
              // if IA32_PERF_STATUS is not available, assume maxClock
              coreClocks[i].Value = (float)maxClock;
            }
          }
          if (busClock > 0) {
            this.busClock.Value = (float)busClock;
            ActivateSensor(this.busClock);
          }
        }
        lastTimeStampCount = timeStampCount;
        lastTime = time;
      }
    }
  }  
}
