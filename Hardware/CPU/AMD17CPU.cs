//This Source Code Form is subject to the terms of the Mozilla Public
//License, v. 2.0. If a copy of the MPL was not distributed with this
//file, You can obtain one at http://mozilla.org/MPL/2.0/. 
//Copyright (C) 2016-2017 Sebastian Grams <https://github.com/sebastian-dev>
//Copyright (C) 2016-2017 Aqua Computer <https://github.com/aquacomputer, info@aqua-computer.de>

using System;
using System.Collections.Generic;
//using System.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.CPU
{
  internal sealed class AMD17CPU : AMDCPU
  {
    //counter, to create sensor index values
    public int sensor_temperatures = 0;
    public int sensor_power = 0;
    public int sensor_current = 0;
    public int sensor_voltage = 0;
    public int sensor_clock = 0;
    public int sensor_multi = 0;

    //register index names for CPUID[]
    private const int EAX = 0;
    private const int EBX = 1;
    private const int ECX = 2;
    private const int EDX = 3;

    //zen register defninitions
    public const uint PERF_CTL_0 = 0xC0010000;
    public const uint PERF_CTR_0 = 0xC0010004;
    public const uint HWCR = 0xC0010015;

    public const uint MSR_PSTATE_L = 0xC0010061;
    public const uint MSR_PSTATE_C = 0xC0010062;
    public const uint MSR_PSTATE_S = 0xC0010063;
    public const uint MSR_PSTATE_0 = 0xC0010064;

    public const uint MSR_PWR_UNIT = 0xC0010299;
    public const uint MSR_CORE_ENERGY_STAT = 0xC001029A;
    public const uint MSR_PKG_ENERGY_STAT = 0xC001029B;
    public const uint MSR_HARDWARE_PSTATE_STATUS = 0xC0010293;
    public const uint COFVID_STATUS = 0xC0010071;
    public const uint FAMILY_17H_PCI_CONTROL_REGISTER = 0x60;
    public const uint FAMILY_17H_MODEL_01_MISC_CONTROL_DEVICE_ID = 0x1463;
    public const uint F17H_M01H_THM_TCON_CUR_TMP = 0x00059800;
    public const uint F17H_M01H_SVI = 0x0005A000;

    #region Processor
    private class Processor
    {
      private AMD17CPU hw = null;
      private DateTime last_pwr_time = new DateTime(0);
      private uint last_pwr_value = 0;

      public Sensor packagePower { get; set; }

      public Processor(Hardware _hw)
      {
        this.hw = (AMD17CPU)_hw;
        nodes = new List<NumaNode>();

        packagePower = new Sensor("Package Power", hw.sensor_power++, SensorType.Power, hw, hw.settings);
        coreTemperatureTctl = new Sensor("Core (Tctl)", hw.sensor_temperatures++, SensorType.Temperature, hw, hw.settings);
        coreTemperatureTdie = new Sensor("Core (Tdie)", hw.sensor_temperatures++, SensorType.Temperature, hw, hw.settings);
        coreVoltage = new Sensor("Core (SVI2)", hw.sensor_voltage++, SensorType.Voltage, hw, hw.settings);
        socVoltage = new Sensor("SoC (SVI2)", hw.sensor_voltage++, SensorType.Voltage, hw, hw.settings);
        //node.coreCurrent = new Sensor("Core Current", hw.sensor_current++, SensorType.Current, hw, hw.settings);
        //node.socCurrent = new Sensor("SoC Current", hw.sensor_current++, SensorType.Current, hw,hw.settings);
      }

      #region UpdateSensors
      public void UpdateSensors()
      {
        //var node = nodes.FirstOrDefault();
        var node = nodes[0];
        if (node == null)
          return;
        //var core = node.cores.FirstOrDefault();
        Core core = node.cores[0];
        if (core == null)
          return;
        //CPUID cpu = core.threads.FirstOrDefault();
        CPUID cpu = core.threads[0];
        if (cpu == null)
          return;
        uint eax, edx;

        ulong mask = Ring0.ThreadAffinitySet(1UL << cpu.Thread);

        //MSRC001_0299
        //TU [19:16]
        //ESU [12:8] -> Unit 15.3 micro Joule per increment
        //PU [3:0]
        Ring0.Rdmsr(MSR_PWR_UNIT, out eax, out edx);
        int tu = (int)((eax >> 16) & 0xf);
        int esu = (int)((eax >> 12) & 0xf);
        int pu = (int)(eax & 0xf);

        //MSRC001_029B
        //total_energy [31:0]
        DateTime sample_time = DateTime.Now;
        Ring0.Rdmsr(MSR_PKG_ENERGY_STAT, out eax, out edx);
        uint total_energy = eax;

        //THM_TCON_CUR_TMP
        //CUR_TEMP [31:21]
        uint temperature = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_THM_TCON_CUR_TMP);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out temperature);

        //SVI0_TFN_PLANE0 [0]
        //SVI0_TFN_PLANE1 [1]
        uint smusvi0_tfn = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0x8);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tfn);

        //SVI0_PLANE0_VDDCOR [24:16]
        //SVI0_PLANE0_IDDCOR [7:0]
        uint smusvi0_tel_plane0 = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0xc);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tel_plane0);

        //SVI0_PLANE1_VDDCOR [24:16]
        //SVI0_PLANE1_IDDCOR [7:0]
        uint smusvi0_tel_plane1 = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0x10);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tel_plane1);

        Ring0.ThreadAffinitySet(mask);

        //power consumption
        //power.Value = (float) ((double)pu * 0.125);
        //esu = 15.3 micro Joule per increment
        if (last_pwr_time.Ticks == 0)
        {
          last_pwr_time = sample_time;
          last_pwr_value = total_energy;
        }
        //ticks diff
        TimeSpan time = sample_time - last_pwr_time;
        long pwr;
        if (last_pwr_value <= total_energy)
          pwr = total_energy - last_pwr_value;
        else
          pwr = (0xffffffff - last_pwr_value) + total_energy;

        //update for next sample
        last_pwr_time = sample_time;
        last_pwr_value = total_energy;

        double energy = 15.3e-6 * pwr;
        energy /= time.TotalSeconds;

        packagePower.Value = (float)energy;
        hw.ActivateSensor(packagePower);

        //current temp Bit [31:21]
        temperature = (temperature >> 21) * 125;
        float offset = 0.0f;
        if (cpu.Name != null && (cpu.Name.Contains("1600X") || cpu.Name.Contains("1700X") || cpu.Name.Contains("1800X")))
          offset = -20.0f;
        else if (cpu.Name != null && (cpu.Name.Contains("1920X") || cpu.Name.Contains("1950X")))
          offset = -27.0f;
        else if (cpu.Name != null && (cpu.Name.Contains("1910") || cpu.Name.Contains("1920")))
          offset = -10.0f;

        coreTemperatureTctl.Value = (temperature * 0.001f);
        coreTemperatureTdie.Value = (temperature * 0.001f) + offset;
        hw.ActivateSensor(coreTemperatureTctl);
        hw.ActivateSensor(coreTemperatureTdie);

        //voltage
        double VIDStep = 0.00625;
        double vcc;
        uint svi0_plane_x_vddcor;
        uint svi0_plane_x_iddcor;

        //Core
        if ((smusvi0_tfn & 0x01) == 0)
        {
          svi0_plane_x_vddcor = (smusvi0_tel_plane0 >> 16) & 0xff;
          svi0_plane_x_iddcor = smusvi0_tel_plane0 & 0xff;
          vcc = 1.550 - (double)VIDStep * svi0_plane_x_vddcor;
          coreVoltage.Value = (float)vcc;
          hw.ActivateSensor(coreVoltage);
          //coreCurrent.Value = (float)(svi0_plane_x_iddcor * 1);
          //hw.ActivateSensor(coreCurrent);
        }

        //SoC
        if ((smusvi0_tfn & 0x02) == 0)
        {
          svi0_plane_x_vddcor = (smusvi0_tel_plane1 >> 16) & 0xff;
          svi0_plane_x_iddcor = smusvi0_tel_plane1 & 0xff;
          vcc = 1.550 - (double)VIDStep * svi0_plane_x_vddcor;
          socVoltage.Value = (float)vcc;
          hw.ActivateSensor(socVoltage);
          //socCurrent.Value = (float)(svi0_plane_x_iddcor * 1);
          //hw.ActivateSensor(socCurrent);
        }

      }
      #endregion

      public void appendThread(CPUID thread, int numa_id, int core_id)
      {
        NumaNode node = null;
        //node = (from x in nodes
        //        where x.nodeId == numa_id
        //        select x).FirstOrDefault();
        foreach (var n in nodes)
        {
          if (n.nodeId == numa_id)
            node = n;
        }
        if (node == null)
        {
          node = new NumaNode(hw);
          node.nodeId = numa_id;
          node.parent = this;
          nodes.Add(node);
        }

        if (thread != null)
          node.appendThread(thread, core_id);
      }

      public Sensor coreTemperatureTctl { get; set; }
      public Sensor coreTemperatureTdie { get; set; }

      public Sensor coreVoltage { get; set; }
      //public Sensor coreCurrent { get; set; }
      public Sensor socVoltage { get; set; }
      //public Sensor socCurrent { get; set; }

      public List<NumaNode> nodes { get; set; }
    }
    #endregion

    #region NumaNode
    private class NumaNode
    {
      private AMD17CPU hw = null;

      public NumaNode(Hardware _hw)
      {
        cores = new List<Core>();
        nodeId = -1;
        this.hw = (AMD17CPU)_hw;
      }

      public void appendThread(CPUID thread, int core_id)
      {
        Core core = null;
        //Core core = (from x in cores
        //             where x.coreId == core_id
        //             select x).FirstOrDefault();
        foreach (var c in cores)
        {
          if (c.coreId == core_id)
            core = c;
        }
        if (core == null)
        {
          core = new Core(hw);
          core.coreId = core_id;
          core.parent = this;

          core.clock = new Sensor("Core #" + core.coreId.ToString(), hw.sensor_clock++, SensorType.Clock, hw, hw.settings);
          core.multiplier = new Sensor("Core #" + core.coreId.ToString(), hw.sensor_multi++, SensorType.Factor, hw, hw.settings);
          core.power = new Sensor("Core #" + core.coreId.ToString() + " (SMU)", hw.sensor_power++, SensorType.Power, hw, hw.settings);
          core.vcore = new Sensor("Core #" + core.coreId.ToString() + " VID", hw.sensor_voltage++, SensorType.Voltage, hw, hw.settings);
          cores.Add(core);
        }

        if (thread != null)
          core.threads.Add(thread);
      }

      #region UpdateSensors
      public void UpdateSensors()
      {


      }
      #endregion

      public int nodeId { get; set; }
      public List<Core> cores { get; set; }

      public Processor parent { get; set; }
    }
    #endregion

    #region Core
    private class Core
    {
      private AMD17CPU hw = null;

      public Core(Hardware _hw)
      {
        threads = new List<CPUID>();
        coreId = -1;
        hw = (AMD17CPU)_hw;
      }

      DateTime last_pwr_time = new DateTime(0);
      uint last_pwr_value = 0;

      #region UpdateSensors
      public void UpdateSensors()
      {
        //CPUID cpu = threads.FirstOrDefault();
        CPUID cpu = threads[0];
        if (cpu == null)
          return;
        uint eax, edx;
        ulong mask = Ring0.ThreadAffinitySet(1UL << cpu.Thread);


        //MSRC001_0299
        //TU [19:16]
        //ESU [12:8] -> Unit 15.3 micro Joule per increment
        //PU [3:0]
        Ring0.Rdmsr(MSR_PWR_UNIT, out eax, out edx);
        int tu = (int)((eax >> 16) & 0xf);
        int esu = (int)((eax >> 12) & 0xf);
        int pu = (int)(eax & 0xf);

        //MSRC001_029A
        //total_energy [31:0]
        DateTime sample_time = DateTime.Now;
        Ring0.Rdmsr(MSR_CORE_ENERGY_STAT, out eax, out edx);
        uint total_energy = eax;

        //MSRC001_0293
        //CurHwPstate [24:22]
        //CurCpuVid [21:14]
        //CurCpuDfsId [13:8]
        //CurCpuFid [7:0]
        Ring0.Rdmsr(MSR_HARDWARE_PSTATE_STATUS, out eax, out edx);
        int CurHwPstate = (int)((eax >> 22) & 0x3);
        int CurCpuVid = (int)((eax >> 14) & 0xff);
        int CurCpuDfsId = (int)((eax >> 8) & 0x3f);
        int CurCpuFid = (int)(eax & 0xff);

        //MSRC001_0064 + x
        //IddDiv [31:30]
        //IddValue [29:22]
        //CpuVid [21:14]
        //CpuDfsId [13:8]
        //CpuFid [7:0]
        //Ring0.Rdmsr(MSR_PSTATE_0 + (uint)CurHwPstate, out eax, out edx);
        //int IddDiv = (int)((eax >> 30) & 0x03);
        //int IddValue = (int)((eax >> 22) & 0xff);
        //int CpuVid = (int)((eax >> 14) & 0xff);

        Ring0.ThreadAffinitySet(mask);

        //clock
        //CoreCOF is (Core::X86::Msr::PStateDef[CpuFid[7:0]] / Core::X86::Msr::PStateDef[CpuDfsId]) * 200
        clock.Value = (float)((double)CurCpuFid / (double)CurCpuDfsId * 200.0);
        hw.ActivateSensor(clock);

        //multiplier
        multiplier.Value = (float)((double)CurCpuFid / (double)CurCpuDfsId * 2.0);
        hw.ActivateSensor(multiplier);

        //Voltage
        double VIDStep = 0.00625;
        double vcc = 1.550 - (double)VIDStep * CurCpuVid;
        vcore.Value = (float)vcc;
        hw.ActivateSensor(vcore);

        //power consumption
        //power.Value = (float) ((double)pu * 0.125);
        //esu = 15.3 micro Joule per increment
        if (last_pwr_time.Ticks == 0)
        {
          last_pwr_time = sample_time;
          last_pwr_value = total_energy;
        }
        //ticks diff
        TimeSpan time = sample_time - last_pwr_time;
        long pwr;
        if (last_pwr_value <= total_energy)
          pwr = total_energy - last_pwr_value;
        else
          pwr = (0xffffffff - last_pwr_value) + total_energy;

        //update for next sample
        last_pwr_time = sample_time;
        last_pwr_value = total_energy;

        double energy = 15.3e-6 * pwr;
        energy /= time.TotalSeconds;

        power.Value = (float)energy;
        hw.ActivateSensor(power);

      }
      #endregion

      public Sensor clock { get; set; }
      public Sensor vcore { get; set; }
      public Sensor power { get; set; }
      public Sensor multiplier { get; set; }

      public int coreId { get; set; }
      public List<CPUID> threads { get; set; }
      public NumaNode parent { get; set; }
    }
    #endregion

    private Processor ryzen = null;

    public AMD17CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings)
    {
      //add all numa nodes
      //Register ..1E_ECX, [10:8] + 1
      ryzen = new Processor(this);
      int NodesPerProcessor = 1 + (int)((cpuid[0][0].ExtData[0x1e, ECX] >> 8) & 0x7);

      //add all numa nodes
      foreach (CPUID[] cpu in cpuid)
      {
        CPUID thread = cpu[0];

        //coreID
        //Register ..1E_EBX, [7:0]
        int core_id = (int)(thread.ExtData[0x1e, EBX] & 0xff);

        //nodeID
        //Register ..1E_ECX, [7:0]
        int node_id = (int)(thread.ExtData[0x1e, ECX] & 0xff);

        ryzen.appendThread(null, node_id, core_id);
      }

      //add all threads to numa nodes and specific core
      foreach (CPUID[] cpu in cpuid)
      {
        CPUID thread = cpu[0];

        //coreID
        //Register ..1E_EBX, [7:0]
        int core_id = (int)(thread.ExtData[0x1e, EBX] & 0xff);

        //nodeID
        //Register ..1E_ECX, [7:0]
        int node_id = (int)(thread.ExtData[0x1e, ECX] & 0xff);

        ryzen.appendThread(thread, node_id, core_id);
      }
      Update();
    }

    protected override uint[] GetMSRs()
    {
      return new uint[] { PERF_CTL_0, PERF_CTR_0, HWCR, MSR_PSTATE_0, COFVID_STATUS };
    }

    public override string GetReport()
    {
      StringBuilder r = new StringBuilder();
      r.Append(base.GetReport());
      r.Append("Ryzen");
      return r.ToString();
    }

    private string ReadFirstLine(Stream stream)
    {
      StringBuilder sb = new StringBuilder();
      try
      {
        stream.Seek(0, SeekOrigin.Begin);
        int b = stream.ReadByte();
        while (b != -1 && b != 10)
        {
          sb.Append((char)b);
          b = stream.ReadByte();
        }
      }
      catch { }
      return sb.ToString();
    }

    public override void Update()
    {
      base.Update();

      ryzen.UpdateSensors();
      foreach (NumaNode node in ryzen.nodes)
      {
        node.UpdateSensors();

        foreach (Core c in node.cores)
        {
          c.UpdateSensors();
        }
      }
    }

    public override void Close()
    {
      base.Close();
    }
  }
}
