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
using System.Runtime.InteropServices;
using System.Text;

namespace OpenHardwareMonitor.Hardware.CPU {
  public class CPULoad {

    [StructLayout(LayoutKind.Sequential)]
    private struct SystemProcessorPerformanceInformation {
      public long IdleTime;
      public long KernelTime;
      public long UserTime;
      public long Reserved0;
      public long Reserved1;
      public ulong Reserved2;
    }

    private enum SystemInformationClass : int {
      SystemBasicInformation = 0,
      SystemCpuInformation = 1,
      SystemPerformanceInformation = 2,
      SystemTimeOfDayInformation = 3,
      SystemProcessInformation = 5,
      SystemProcessorPerformanceInformation = 8
    }

    [DllImport("ntdll.dll")]
    private static extern int NtQuerySystemInformation(
      SystemInformationClass informationClass,
      [Out] SystemProcessorPerformanceInformation[] informations, 
      int structSize, out IntPtr returnLength);

    private uint coreCount;
    private uint logicalProcessorsPerCore;

    private long systemTime;
    private long[] idleTimes;

    private float totalLoad;
    private float[] coreLoads;

    private bool available = false;

    private long[] GetIdleTimes() {
      long[] result = new long[coreCount * logicalProcessorsPerCore];
      SystemProcessorPerformanceInformation[] informations = new
       SystemProcessorPerformanceInformation[result.Length];

      IntPtr returnLength;
      NtQuerySystemInformation(
        SystemInformationClass.SystemProcessorPerformanceInformation,
        informations, informations.Length * 
        Marshal.SizeOf(typeof(SystemProcessorPerformanceInformation)),
        out returnLength);

      for (int i = 0; i < result.Length; i++)
        result[i] = informations[i].IdleTime;

      return result;
    }

    public CPULoad(uint coreCount, uint logicalProcessorsPerCore) {
      this.coreCount = coreCount;
      this.logicalProcessorsPerCore = logicalProcessorsPerCore;
      this.coreLoads = new float[coreCount];         
      this.systemTime = DateTime.Now.Ticks;
      this.totalLoad = 0;
      try {
        this.idleTimes = GetIdleTimes();
      } catch (Exception) {
        this.idleTimes = null;
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

      long systemTime = DateTime.Now.Ticks;
      long[] idleTimes = GetIdleTimes();

      if (systemTime - this.systemTime < 10000)
        return;

      float total = 0;
      for (int i = 0; i < coreCount; i++) {
        float value = 0;
        for (int j = 0; j < logicalProcessorsPerCore; j++) {
          long index = i * logicalProcessorsPerCore + j;
          long delta = idleTimes[index] - this.idleTimes[index];
          value += delta;
          total += delta;
        }
        value = 1.0f - value / (logicalProcessorsPerCore * 
          (systemTime - this.systemTime));
        value = value < 0 ? 0 : value;
        coreLoads[i] = value * 100;
      }
      total = 1.0f - total / (coreCount * logicalProcessorsPerCore *
        (systemTime - this.systemTime));
      total = total < 0 ? 0 : total;
      this.totalLoad = total * 100;

      this.systemTime = systemTime;
      this.idleTimes = idleTimes;
    }

  }
}
