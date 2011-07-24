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
using System.Runtime.InteropServices;

namespace OpenHardwareMonitor.Hardware.CPU {
  internal class CPULoad {

    [StructLayout(LayoutKind.Sequential)]
    protected struct SystemProcessorPerformanceInformation {
      public long IdleTime;
      public long KernelTime;
      public long UserTime;
      public long Reserved0;
      public long Reserved1;
      public ulong Reserved2;
    }

    protected enum SystemInformationClass {
      SystemBasicInformation = 0,
      SystemCpuInformation = 1,
      SystemPerformanceInformation = 2,
      SystemTimeOfDayInformation = 3,
      SystemProcessInformation = 5,
      SystemProcessorPerformanceInformation = 8
    }

    private readonly CPUID[][] cpuid;

    private long[] idleTimes;
    private long[] totalTimes;

    private float totalLoad;
    private readonly float[] coreLoads;

    private readonly bool available;

    private static bool GetTimes(out long[] idle, out long[] total) {      
      SystemProcessorPerformanceInformation[] informations = new
        SystemProcessorPerformanceInformation[64];

      int size = Marshal.SizeOf(typeof(SystemProcessorPerformanceInformation));

      idle = null;
      total = null;

      IntPtr returnLength;
      if (NativeMethods.NtQuerySystemInformation(
        SystemInformationClass.SystemProcessorPerformanceInformation,
        informations, informations.Length * size, out returnLength) != 0)
        return false;

      idle = new long[(int)returnLength / size];
      total = new long[(int)returnLength / size];

      for (int i = 0; i < idle.Length; i++) {
        idle[i] = informations[i].IdleTime;
        total[i] = informations[i].KernelTime + informations[i].UserTime;
      }

      return true;
    }

    public CPULoad(CPUID[][] cpuid) {
      this.cpuid = cpuid;
      this.coreLoads = new float[cpuid.Length];         
      this.totalLoad = 0;
      try {
        GetTimes(out idleTimes, out totalTimes);
      } catch (Exception) {
        this.idleTimes = null;
        this.totalTimes = null;
      }
      if (idleTimes != null)
        available = true;
    }

    public bool IsAvailable {
      get { return available; }
    }

    public float GetTotalLoad() {
      return totalLoad;
    }

    public float GetCoreLoad(int core) {
      return coreLoads[core];
    }

    public void Update() {
      if (this.idleTimes == null)
        return;

      long[] newIdleTimes;
      long[] newTotalTimes;

      if (!GetTimes(out newIdleTimes, out newTotalTimes))
        return;

      for (int i = 0; i < Math.Min(newTotalTimes.Length, totalTimes.Length); i++) 
        if (newTotalTimes[i] - this.totalTimes[i] < 100000)
          return;

      if (newIdleTimes == null || newTotalTimes == null)
        return;

      float total = 0;
      int count = 0;
      for (int i = 0; i < cpuid.Length; i++) {
        float value = 0;
        for (int j = 0; j < cpuid[i].Length; j++) {
          long index = cpuid[i][j].Thread;
          if (index < newIdleTimes.Length && index < totalTimes.Length) {
            float idle = 
              (float)(newIdleTimes[index] - this.idleTimes[index]) /
              (float)(newTotalTimes[index] - this.totalTimes[index]);
            value += idle;
            total += idle;
            count++;
          }
        }
        value = 1.0f - value / cpuid[i].Length;
        value = value < 0 ? 0 : value;
        coreLoads[i] = value * 100;
      }
      if (count > 0) {
        total = 1.0f - total / count;
        total = total < 0 ? 0 : total;
      } else {
        total = 0;
      }
      this.totalLoad = total * 100;

      this.totalTimes = newTotalTimes;
      this.idleTimes = newIdleTimes;
    }

    protected static class NativeMethods {

      [DllImport("ntdll.dll")]
      public static extern int NtQuerySystemInformation(
        SystemInformationClass informationClass,
        [Out] SystemProcessorPerformanceInformation[] informations,
        int structSize, out IntPtr returnLength);
    }
  }
}
