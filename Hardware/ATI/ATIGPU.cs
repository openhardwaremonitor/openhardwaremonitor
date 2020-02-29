/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2020 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;
using System.Text;

namespace OpenHardwareMonitor.Hardware.ATI {
  internal sealed class ATIGPU : Hardware {

    private readonly int adapterIndex;
    private readonly int busNumber;
    private readonly int deviceNumber;
    private readonly Sensor temperatureCore;
    private readonly Sensor temperatureMemory;
    private readonly Sensor temperatureVrmCore;
    private readonly Sensor temperatureVrmMemory;
    private readonly Sensor temperatureLiquid;
    private readonly Sensor temperaturePlx;
    private readonly Sensor temperatureHotSpot;
    private readonly Sensor powerCore;
    private readonly Sensor powerPpt;
    private readonly Sensor powerSocket;
    private readonly Sensor powerTotal;
    private readonly Sensor fan;
    private readonly Sensor coreClock;
    private readonly Sensor memoryClock;
    private readonly Sensor coreVoltage;
    private readonly Sensor memoryVoltage;
    private readonly Sensor coreLoad;
    private readonly Sensor controlSensor;
    private readonly Control fanControl;

    private IntPtr context;
    private readonly int overdriveVersion;

    public ATIGPU(string name, int adapterIndex, int busNumber, 
      int deviceNumber, IntPtr context, ISettings settings) 
      : base(name, new Identifier("atigpu", 
        adapterIndex.ToString(CultureInfo.InvariantCulture)), settings)
    {
      this.adapterIndex = adapterIndex;
      this.busNumber = busNumber;
      this.deviceNumber = deviceNumber;

      this.context = context;

      if (ADL.ADL_Overdrive_Caps(adapterIndex, out _, out _,
        out overdriveVersion) != ADL.ADL_OK)
      {
        overdriveVersion = -1;
      }

      this.temperatureCore = 
        new Sensor("GPU Core", 0, SensorType.Temperature, this, settings);
      this.temperatureMemory = 
        new Sensor("GPU Memory", 1, SensorType.Temperature, this, settings);
      this.temperatureVrmCore = 
        new Sensor("GPU VRM Core", 2, SensorType.Temperature, this, settings);
      this.temperatureVrmMemory = 
        new Sensor("GPU VRM Memory", 3, SensorType.Temperature, this, settings);
      this.temperatureLiquid = 
        new Sensor("GPU Liquid", 4, SensorType.Temperature, this, settings);
      this.temperaturePlx = 
        new Sensor("GPU PLX", 5, SensorType.Temperature, this, settings);
      this.temperatureHotSpot = 
        new Sensor("GPU Hot Spot", 6, SensorType.Temperature, this, settings);

      this.powerTotal = new Sensor("GPU Total", 0, SensorType.Power, this, settings);
      this.powerCore = new Sensor("GPU Core", 1, SensorType.Power, this, settings);
      this.powerPpt = new Sensor("GPU PPT", 2, SensorType.Power, this, settings);
      this.powerSocket = new Sensor("GPU Socket", 3, SensorType.Power, this, settings);

      this.fan = new Sensor("GPU Fan", 0, SensorType.Fan, this, settings);
      this.coreClock = new Sensor("GPU Core", 0, SensorType.Clock, this, settings);
      this.memoryClock = new Sensor("GPU Memory", 1, SensorType.Clock, this, settings);
      this.coreVoltage = new Sensor("GPU Core", 0, SensorType.Voltage, this, settings);
      this.memoryVoltage = new Sensor("GPU Memory", 1, SensorType.Voltage, this, settings);
      this.coreLoad = new Sensor("GPU Core", 0, SensorType.Load, this, settings);
      this.controlSensor = new Sensor("GPU Fan", 0, SensorType.Control, this, settings);

      ADLFanSpeedInfo afsi = new ADLFanSpeedInfo();
      if (ADL.ADL_Overdrive5_FanSpeedInfo_Get(adapterIndex, 0, ref afsi)
        != ADL.ADL_OK) 
      {
        afsi.MaxPercent = 100;
        afsi.MinPercent = 0;
      }

      this.fanControl = new Control(controlSensor, settings, afsi.MinPercent, 
        afsi.MaxPercent);
      this.fanControl.ControlModeChanged += ControlModeChanged;
      this.fanControl.SoftwareControlValueChanged += 
        SoftwareControlValueChanged;
      ControlModeChanged(fanControl);
      this.controlSensor.Control = fanControl;
      Update();                   
    }

    private void SoftwareControlValueChanged(IControl control) {
      if (control.ControlMode == ControlMode.Software) {        
        ADLFanSpeedValue adlf = new ADLFanSpeedValue();
        adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_PERCENT;
        adlf.Flags = ADL.ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED;
        adlf.FanSpeed = (int)control.SoftwareValue;
        ADL.ADL_Overdrive5_FanSpeed_Set(adapterIndex, 0, ref adlf);
      }
    }

    private void ControlModeChanged(IControl control) {
      switch (control.ControlMode) {
        case ControlMode.Undefined:
          return;
        case ControlMode.Default:
          SetDefaultFanSpeed();
          break;
        case ControlMode.Software:
          SoftwareControlValueChanged(control);
          break;
        default:
          return;
      }
    }

    private void SetDefaultFanSpeed() {
      ADL.ADL_Overdrive5_FanSpeedToDefault_Set(adapterIndex, 0);
    }

    public int BusNumber { get { return busNumber; } }

    public int DeviceNumber { get { return deviceNumber; } }


    public override HardwareType HardwareType {
      get { return HardwareType.GpuAti; }
    }

    private void GetODNTemperature(ADLODNTemperatureType type, 
      Sensor sensor) 
    {
      if (ADL.ADL2_OverdriveN_Temperature_Get(context, adapterIndex,
          type, out int temperature) == ADL.ADL_OK) 
      {
        sensor.Value = 0.001f * temperature;
        ActivateSensor(sensor);
      } else {
        sensor.Value = null;
      }
    }

    private void GetOD6Power(ADLODNCurrentPowerType type, Sensor sensor) 
    {
      if (ADL.ADL2_Overdrive6_CurrentPower_Get(context, adapterIndex, type, 
        out int power) == ADL.ADL_OK) 
      {
        sensor.Value = power * (1.0f / 0xFF);
        ActivateSensor(sensor);
      } else {
        sensor.Value = null;
      }

    }

    public override string GetReport() {
      var r = new StringBuilder();

      r.AppendLine("AMD GPU");
      r.AppendLine();

      r.Append("AdapterIndex: ");
      r.AppendLine(adapterIndex.ToString(CultureInfo.InvariantCulture));
      r.AppendLine();

      r.AppendLine("ADL Overdrive");
      r.AppendLine();
      int status = ADL.ADL_Overdrive_Caps(adapterIndex,
        out int supported, out int enabled, out int version);
      
      r.Append(" Status: ");
      r.AppendLine(status == ADL.ADL_OK ? "OK" :
          status.ToString(CultureInfo.InvariantCulture));
      r.Append(" Supported: ");
      r.AppendLine(supported.ToString(CultureInfo.InvariantCulture));
      r.Append(" Enabled: ");
      r.AppendLine(enabled.ToString(CultureInfo.InvariantCulture));
      r.Append(" Version: ");
      r.AppendLine(version.ToString(CultureInfo.InvariantCulture));
      r.AppendLine();

      if (context != IntPtr.Zero && overdriveVersion >= 6) {
        r.AppendLine("Overdrive6 CurrentPower:");
        r.AppendLine();
        for (int i = 0; i < 4; i++) {
          var pt = ((ADLODNCurrentPowerType)i).ToString();
          var ps = ADL.ADL2_Overdrive6_CurrentPower_Get(
            context, adapterIndex, (ADLODNCurrentPowerType)i,
            out int power);
          if (ps == ADL.ADL_OK) {
            r.AppendFormat(" Power[{0}].Value: {1}{2}", pt,
              power * (1.0f / 0xFF), Environment.NewLine);
          } else {
            r.AppendFormat(" Power[{0}].Status: {1}{2}", pt,
              ps, Environment.NewLine);
          }
        }
        r.AppendLine();
      }

      if (context != IntPtr.Zero && overdriveVersion >= 7) {
        r.AppendLine("OverdriveN Temperature:");
        r.AppendLine();
        for (int i = 1; i < 8; i++) {
          var tt = ((ADLODNTemperatureType)i).ToString();
          var ts = ADL.ADL2_OverdriveN_Temperature_Get(
            context, adapterIndex, (ADLODNTemperatureType)i, 
            out int temperature);
          if (ts == ADL.ADL_OK) {
            r.AppendFormat(" Temperature[{0}].Value: {1}{2}", tt,
              0.001f * temperature, Environment.NewLine);
          } else {
            r.AppendFormat(" Temperature[{0}].Status: {1}{2}", tt,
              ts, Environment.NewLine);
          }
        }
        r.AppendLine();
      }

      if (context != IntPtr.Zero && overdriveVersion >= 8) {
        r.AppendLine("Performance Metrics:");
        r.AppendLine();
        var ps = ADL.ADL2_New_QueryPMLogData_Get(context, adapterIndex, 
          out var data);

        if (ps == ADL.ADL_OK) {
          for (int i = 0; i < data.Sensors.Length; i++) {
            if (data.Sensors[i].Supported) {
              var st = ((ADLSensorType)i).ToString();
              r.AppendFormat(" Sensor[{0}].Value: {1}{2}", st,
                data.Sensors[i].Value, Environment.NewLine);
            }
          }
        } else {
          r.Append(" Status: ");
          r.AppendLine(ps.ToString(CultureInfo.InvariantCulture));
        }       
        r.AppendLine();
      }

      return r.ToString();
    }

    private void GetPMLog(ADLPMLogDataOutput data, 
      ADLSensorType sensorType, Sensor sensor, float factor = 1.0f) 
    {
      int i = (int)sensorType;
      if (i < data.Sensors.Length && data.Sensors[i].Supported) {
        sensor.Value = data.Sensors[i].Value * factor;
        ActivateSensor(sensor);
      }
    }

    public override void Update() {
      if (context != IntPtr.Zero && overdriveVersion >= 8 && 
        ADL.ADL2_New_QueryPMLogData_Get(context, adapterIndex, 
        out var data) == ADL.ADL_OK) 
      {
        GetPMLog(data, ADLSensorType.TEMPERATURE_EDGE, temperatureCore);
        GetPMLog(data, ADLSensorType.TEMPERATURE_MEM, temperatureMemory);
        GetPMLog(data, ADLSensorType.TEMPERATURE_VRVDDC, temperatureVrmCore);
        GetPMLog(data, ADLSensorType.TEMPERATURE_VRMVDD, temperatureVrmMemory);
        GetPMLog(data, ADLSensorType.TEMPERATURE_LIQUID, temperatureLiquid);
        GetPMLog(data, ADLSensorType.TEMPERATURE_PLX, temperaturePlx);
        GetPMLog(data, ADLSensorType.TEMPERATURE_HOTSPOT, temperatureHotSpot);
        GetPMLog(data, ADLSensorType.GFX_POWER, powerCore);
        GetPMLog(data, ADLSensorType.ASIC_POWER, powerTotal);
        GetPMLog(data, ADLSensorType.FAN_RPM, fan);
        GetPMLog(data, ADLSensorType.CLK_GFXCLK, coreClock);
        GetPMLog(data, ADLSensorType.CLK_MEMCLK, memoryClock);        
        GetPMLog(data, ADLSensorType.GFX_VOLTAGE, coreVoltage, 0.001f);
        GetPMLog(data, ADLSensorType.MEM_VOLTAGE, memoryVoltage, 0.001f);
        GetPMLog(data, ADLSensorType.INFO_ACTIVITY_GFX, coreLoad);
        GetPMLog(data, ADLSensorType.FAN_PERCENTAGE, controlSensor);
      } else {
        if (context != IntPtr.Zero && overdriveVersion >= 7) {
          GetODNTemperature(ADLODNTemperatureType.CORE, temperatureCore);
          GetODNTemperature(ADLODNTemperatureType.MEMORY, temperatureMemory);
          GetODNTemperature(ADLODNTemperatureType.VRM_CORE, temperatureVrmCore);
          GetODNTemperature(ADLODNTemperatureType.VRM_MEMORY, temperatureVrmMemory);
          GetODNTemperature(ADLODNTemperatureType.LIQUID, temperatureLiquid);
          GetODNTemperature(ADLODNTemperatureType.PLX, temperaturePlx);
          GetODNTemperature(ADLODNTemperatureType.HOTSPOT, temperatureHotSpot);
        } else {
          ADLTemperature adlt = new ADLTemperature();
          if (ADL.ADL_Overdrive5_Temperature_Get(adapterIndex, 0, ref adlt)
            == ADL.ADL_OK) {
            temperatureCore.Value = 0.001f * adlt.Temperature;
            ActivateSensor(temperatureCore);
          } else {
            temperatureCore.Value = null;
          }
        }

        if (context != IntPtr.Zero && overdriveVersion >= 6) {
          GetOD6Power(ADLODNCurrentPowerType.TOTAL_POWER, powerTotal);
          GetOD6Power(ADLODNCurrentPowerType.CHIP_POWER, powerCore);
          GetOD6Power(ADLODNCurrentPowerType.PPT_POWER, powerPpt);
          GetOD6Power(ADLODNCurrentPowerType.SOCKET_POWER, powerSocket);
        }

        ADLFanSpeedValue adlf = new ADLFanSpeedValue();
        adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM;
        if (ADL.ADL_Overdrive5_FanSpeed_Get(adapterIndex, 0, ref adlf)
          == ADL.ADL_OK) {
          fan.Value = adlf.FanSpeed;
          ActivateSensor(fan);
        } else {
          fan.Value = null;
        }

        adlf = new ADLFanSpeedValue();
        adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_PERCENT;
        if (ADL.ADL_Overdrive5_FanSpeed_Get(adapterIndex, 0, ref adlf)
          == ADL.ADL_OK) {
          controlSensor.Value = adlf.FanSpeed;
          ActivateSensor(controlSensor);
        } else {
          controlSensor.Value = null;
        }

        ADLPMActivity adlp = new ADLPMActivity();
        if (ADL.ADL_Overdrive5_CurrentActivity_Get(adapterIndex, ref adlp)
          == ADL.ADL_OK) {
          if (adlp.EngineClock > 0) {
            coreClock.Value = 0.01f * adlp.EngineClock;
            ActivateSensor(coreClock);
          } else {
            coreClock.Value = null;
          }

          if (adlp.MemoryClock > 0) {
            memoryClock.Value = 0.01f * adlp.MemoryClock;
            ActivateSensor(memoryClock);
          } else {
            memoryClock.Value = null;
          }

          if (adlp.Vddc > 0) {
            coreVoltage.Value = 0.001f * adlp.Vddc;
            ActivateSensor(coreVoltage);
          } else {
            coreVoltage.Value = null;
          }

          coreLoad.Value = Math.Min(adlp.ActivityPercent, 100);
          ActivateSensor(coreLoad);
        } else {
          coreClock.Value = null;
          memoryClock.Value = null;
          coreVoltage.Value = null;
          coreLoad.Value = null;
        }
      }
    }

    public override void Close() {
      this.fanControl.ControlModeChanged -= ControlModeChanged;
      this.fanControl.SoftwareControlValueChanged -=
        SoftwareControlValueChanged;

      if (this.fanControl.ControlMode != ControlMode.Undefined)
        SetDefaultFanSpeed();
      base.Close();
    }
  }
}
