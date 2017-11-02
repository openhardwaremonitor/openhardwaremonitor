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
    // counter, to create sensor index values 
    public int _sensorTemperatures = 0;
    public int _sensorPower = 0;
    public int _sensorCurrent = 0;
    public int _sensorVoltage = 0;
    public int _sensorClock = 0;
    public int _sensorMulti = 0;

    // register index names for CPUID[] 
    private const int EAX = 0;
    private const int EBX = 1;
    private const int ECX = 2;
    private const int EDX = 3;

    // zen register defninitions 
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
      private AMD17CPU _hw = null;
      private DateTime _lastPwrTime = new DateTime(0);
      private uint _lastPwrValue = 0;

      public Sensor PackagePower { get; set; }

      public Processor(Hardware hw)
      {
        this._hw = (AMD17CPU)hw;
        Nodes = new List<NumaNode>();

        PackagePower = new Sensor("Package Power", this._hw._sensorPower++, SensorType.Power, this._hw, this._hw.settings);
        CoreTemperatureTctl = new Sensor("Core (Tctl)", this._hw._sensorTemperatures++, SensorType.Temperature, this._hw, this._hw.settings);
        CoreTemperatureTdie = new Sensor("Core (Tdie)", this._hw._sensorTemperatures++, SensorType.Temperature, this._hw, this._hw.settings);
        CoreVoltage = new Sensor("Core (SVI2)", this._hw._sensorVoltage++, SensorType.Voltage, this._hw, this._hw.settings);
        SocVoltage = new Sensor("SoC (SVI2)", this._hw._sensorVoltage++, SensorType.Voltage, this._hw, this._hw.settings);
        // node.coreCurrent = new Sensor("Core Current", hw.sensor_current++, SensorType.Current, hw, hw.settings); 
        // node.socCurrent = new Sensor("SoC Current", hw.sensor_current++, SensorType.Current, hw,hw.settings); 
      }

      #region UpdateSensors
      public void UpdateSensors()
      {
        // var node = nodes.FirstOrDefault(); 
        var node = Nodes[0];
        if (node == null)
          return;
        // var core = node.cores.FirstOrDefault(); 
        Core core = node.Cores[0];
        if (core == null)
          return;
        // CPUID cpu = core.threads.FirstOrDefault(); 
        CPUID cpu = core.Threads[0];
        if (cpu == null)
          return;
        uint eax, edx;

        ulong mask = Ring0.ThreadAffinitySet(1UL << cpu.Thread);

        // MSRC001_0299 
        // TU [19:16] 
        // ESU [12:8] -> Unit 15.3 micro Joule per increment 
        // PU [3:0] 
        Ring0.Rdmsr(MSR_PWR_UNIT, out eax, out edx);
        int tu = (int)((eax >> 16) & 0xf);
        int esu = (int)((eax >> 12) & 0xf);
        int pu = (int)(eax & 0xf);

        // MSRC001_029B 
        // total_energy [31:0] 
        DateTime sample_time = DateTime.Now;
        Ring0.Rdmsr(MSR_PKG_ENERGY_STAT, out eax, out edx);
        uint total_energy = eax;

        // THM_TCON_CUR_TMP 
        // CUR_TEMP [31:21] 
        uint temperature = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_THM_TCON_CUR_TMP);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out temperature);

        // SVI0_TFN_PLANE0 [0] 
        // SVI0_TFN_PLANE1 [1] 
        uint smusvi0_tfn = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0x8);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tfn);

        // SVI0_PLANE0_VDDCOR [24:16] 
        // SVI0_PLANE0_IDDCOR [7:0] 
        uint smusvi0_tel_plane0 = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0xc);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tel_plane0);

        // SVI0_PLANE1_VDDCOR [24:16] 
        // SVI0_PLANE1_IDDCOR [7:0] 
        uint smusvi0_tel_plane1 = 0;
        Ring0.WritePciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER, F17H_M01H_SVI + 0x10);
        Ring0.ReadPciConfig(Ring0.GetPciAddress(0, 0, 0), FAMILY_17H_PCI_CONTROL_REGISTER + 4, out smusvi0_tel_plane1);

        Ring0.ThreadAffinitySet(mask);

        // power consumption 
        // power.Value = (float) ((double)pu * 0.125); 
        // esu = 15.3 micro Joule per increment 
        if (_lastPwrTime.Ticks == 0)
        {
          _lastPwrTime = sample_time;
          _lastPwrValue = total_energy;
        }
        // ticks diff 
        TimeSpan time = sample_time - _lastPwrTime;
        long pwr;
        if (_lastPwrValue <= total_energy)
          pwr = total_energy - _lastPwrValue;
        else
          pwr = (0xffffffff - _lastPwrValue) + total_energy;

        // update for next sample 
        _lastPwrTime = sample_time;
        _lastPwrValue = total_energy;

        double energy = 15.3e-6 * pwr;
        energy /= time.TotalSeconds;

        PackagePower.Value = (float)energy;
        _hw.ActivateSensor(PackagePower);

        // current temp Bit [31:21] 
        temperature = (temperature >> 21) * 125;
        float offset = 0.0f;
        if (cpu.Name != null && (cpu.Name.Contains("1600X") || cpu.Name.Contains("1700X") || cpu.Name.Contains("1800X")))
          offset = -20.0f;
        else if (cpu.Name != null && (cpu.Name.Contains("1920X") || cpu.Name.Contains("1950X")))
          offset = -27.0f;
        else if (cpu.Name != null && (cpu.Name.Contains("1910") || cpu.Name.Contains("1920")))
          offset = -10.0f;

        CoreTemperatureTctl.Value = (temperature * 0.001f);
        CoreTemperatureTdie.Value = (temperature * 0.001f) + offset;
        _hw.ActivateSensor(CoreTemperatureTctl);
        _hw.ActivateSensor(CoreTemperatureTdie);

        // voltage 
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
          CoreVoltage.Value = (float)vcc;
          _hw.ActivateSensor(CoreVoltage);
          // coreCurrent.Value = (float)(svi0_plane_x_iddcor * 1); 
          // hw.ActivateSensor(coreCurrent); 
        }

        // SoC 
        if ((smusvi0_tfn & 0x02) == 0)
        {
          svi0_plane_x_vddcor = (smusvi0_tel_plane1 >> 16) & 0xff;
          svi0_plane_x_iddcor = smusvi0_tel_plane1 & 0xff;
          vcc = 1.550 - (double)VIDStep * svi0_plane_x_vddcor;
          SocVoltage.Value = (float)vcc;
          _hw.ActivateSensor(SocVoltage);
          //socCurrent.Value = (float)(svi0_plane_x_iddcor * 1);
          //hw.ActivateSensor(socCurrent);
        }

      }
      #endregion

      public void AppendThread(CPUID thread, int numa_id, int core_id)
      {
        NumaNode node = null;
        // node = (from x in nodes 
        //        where x.nodeId == numa_id 
        //        select x).FirstOrDefault(); 
        foreach (var n in Nodes)
        {
          if (n.NodeId == numa_id)
            node = n;
        }
        if (node == null)
        {
          node = new NumaNode(_hw);
          node.NodeId = numa_id;
          node.Parent = this;
          Nodes.Add(node);
        }

        if (thread != null)
          node.AppendThread(thread, core_id);
      }

      public Sensor CoreTemperatureTctl { get; set; }
      public Sensor CoreTemperatureTdie { get; set; }

      public Sensor CoreVoltage { get; set; }
      // public Sensor coreCurrent { get; set; } 
      public Sensor SocVoltage { get; set; }
      // public Sensor socCurrent { get; set; } 

      public List<NumaNode> Nodes { get; set; }
    }
    #endregion

    #region NumaNode
    private class NumaNode
    {
      private AMD17CPU _hw = null;

      public NumaNode(Hardware hw)
      {
        Cores = new List<Core>();
        NodeId = -1;
        this._hw = (AMD17CPU)hw;
      }

      public void AppendThread(CPUID thread, int core_id)
      {
        Core core = null;
        // Core core = (from x in cores 
        //             where x.coreId == core_id 
        //             select x).FirstOrDefault(); 
        foreach (var c in Cores)
        {
          if (c.CoreId == core_id)
            core = c;
        }
        if (core == null)
        {
          core = new Core(_hw);
          core.CoreId = core_id;
          core.Parent = this;

          core.Clock = new Sensor("Core #" + core.CoreId.ToString(), _hw._sensorClock++, SensorType.Clock, _hw, _hw.settings);
          core.Multiplier = new Sensor("Core #" + core.CoreId.ToString(), _hw._sensorMulti++, SensorType.Factor, _hw, _hw.settings);
          core.Power = new Sensor("Core #" + core.CoreId.ToString() + " (SMU)", _hw._sensorPower++, SensorType.Power, _hw, _hw.settings);
          core.Vcore = new Sensor("Core #" + core.CoreId.ToString() + " VID", _hw._sensorVoltage++, SensorType.Voltage, _hw, _hw.settings);
          Cores.Add(core);
        }

        if (thread != null)
          core.Threads.Add(thread);
      }

      #region UpdateSensors
      public void UpdateSensors()
      {


      }
      #endregion

      public int NodeId { get; set; }
      public List<Core> Cores { get; set; }

      public Processor Parent { get; set; }
    }
    #endregion

    #region Core
    private class Core
    {
      private AMD17CPU _hw = null;

      public Core(Hardware hw)
      {
        Threads = new List<CPUID>();
        CoreId = -1;
        this._hw = (AMD17CPU)hw;
      }

      DateTime _lastPwrTime = new DateTime(0);
      uint _lastPwrValue = 0;

      #region UpdateSensors
      public void UpdateSensors()
      {
        // CPUID cpu = threads.FirstOrDefault(); 
        CPUID cpu = Threads[0];
        if (cpu == null)
          return;
        uint eax, edx;
        ulong mask = Ring0.ThreadAffinitySet(1UL << cpu.Thread);


        // MSRC001_0299 
        // TU [19:16] 
        // ESU [12:8] -> Unit 15.3 micro Joule per increment 
        // PU [3:0] 
        Ring0.Rdmsr(MSR_PWR_UNIT, out eax, out edx);
        int tu = (int)((eax >> 16) & 0xf);
        int esu = (int)((eax >> 12) & 0xf);
        int pu = (int)(eax & 0xf);

        // MSRC001_029A 
        // total_energy [31:0] 
        DateTime sample_time = DateTime.Now;
        Ring0.Rdmsr(MSR_CORE_ENERGY_STAT, out eax, out edx);
        uint total_energy = eax;

        // MSRC001_0293 
        // CurHwPstate [24:22] 
        // CurCpuVid [21:14] 
        // CurCpuDfsId [13:8] 
        // CurCpuFid [7:0] 
        Ring0.Rdmsr(MSR_HARDWARE_PSTATE_STATUS, out eax, out edx);
        int CurHwPstate = (int)((eax >> 22) & 0x3);
        int CurCpuVid = (int)((eax >> 14) & 0xff);
        int CurCpuDfsId = (int)((eax >> 8) & 0x3f);
        int CurCpuFid = (int)(eax & 0xff);

        // MSRC001_0064 + x 
        // IddDiv [31:30] 
        // IddValue [29:22] 
        // CpuVid [21:14] 
        // CpuDfsId [13:8] 
        // CpuFid [7:0] 
        // Ring0.Rdmsr(MSR_PSTATE_0 + (uint)CurHwPstate, out eax, out edx); 
        // int IddDiv = (int)((eax >> 30) & 0x03); 
        // int IddValue = (int)((eax >> 22) & 0xff); 
        // int CpuVid = (int)((eax >> 14) & 0xff); 
        Ring0.ThreadAffinitySet(mask);

        // clock 
        // CoreCOF is (Core::X86::Msr::PStateDef[CpuFid[7:0]] / Core::X86::Msr::PStateDef[CpuDfsId]) * 200 
        Clock.Value = (float)((double)CurCpuFid / (double)CurCpuDfsId * 200.0);
        _hw.ActivateSensor(Clock);

        // multiplier 
        Multiplier.Value = (float)((double)CurCpuFid / (double)CurCpuDfsId * 2.0);
        _hw.ActivateSensor(Multiplier);

        // Voltage 
        double VIDStep = 0.00625;
        double vcc = 1.550 - (double)VIDStep * CurCpuVid;
        Vcore.Value = (float)vcc;
        _hw.ActivateSensor(Vcore);

        // power consumption 
        // power.Value = (float) ((double)pu * 0.125); 
        // esu = 15.3 micro Joule per increment 
        if (_lastPwrTime.Ticks == 0)
        {
          _lastPwrTime = sample_time;
          _lastPwrValue = total_energy;
        }
        // ticks diff 
        TimeSpan time = sample_time - _lastPwrTime;
        long pwr;
        if (_lastPwrValue <= total_energy)
          pwr = total_energy - _lastPwrValue;
        else
          pwr = (0xffffffff - _lastPwrValue) + total_energy;

        // update for next sample 
        _lastPwrTime = sample_time;
        _lastPwrValue = total_energy;

        double energy = 15.3e-6 * pwr;
        energy /= time.TotalSeconds;

        Power.Value = (float)energy;
        _hw.ActivateSensor(Power);

      }
      #endregion

      public Sensor Clock { get; set; }
      public Sensor Vcore { get; set; }
      public Sensor Power { get; set; }
      public Sensor Multiplier { get; set; }

      public int CoreId { get; set; }
      public List<CPUID> Threads { get; set; }
      public NumaNode Parent { get; set; }
    }
    #endregion

    private Processor _ryzen = null;

    public AMD17CPU(int processorIndex, CPUID[][] cpuid, ISettings settings)
      : base(processorIndex, cpuid, settings)
    {
      // add all numa nodes 
      // Register ..1E_ECX, [10:8] + 1 
      _ryzen = new Processor(this);
      int NodesPerProcessor = 1 + (int)((cpuid[0][0].ExtData[0x1e, ECX] >> 8) & 0x7);

      // add all numa nodes
      foreach (CPUID[] cpu in cpuid)
      {
        CPUID thread = cpu[0];

        // coreID
        // Register ..1E_EBX, [7:0] 
        int core_id = (int)(thread.ExtData[0x1e, EBX] & 0xff);

        // nodeID
        // Register ..1E_ECX, [7:0] 
        int node_id = (int)(thread.ExtData[0x1e, ECX] & 0xff);

        _ryzen.AppendThread(null, node_id, core_id);
      }

      // add all threads to numa nodes and specific core 
      foreach (CPUID[] cpu in cpuid)
      {
        CPUID thread = cpu[0];

        // coreID 
        // Register ..1E_EBX, [7:0] 
        int core_id = (int)(thread.ExtData[0x1e, EBX] & 0xff);

        // nodeID 
        // Register ..1E_ECX, [7:0] 
        int node_id = (int)(thread.ExtData[0x1e, ECX] & 0xff);

        _ryzen.AppendThread(thread, node_id, core_id);
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

      _ryzen.UpdateSensors();
      foreach (NumaNode node in _ryzen.Nodes)
      {
        node.UpdateSensors();

        foreach (Core c in node.Cores)
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
