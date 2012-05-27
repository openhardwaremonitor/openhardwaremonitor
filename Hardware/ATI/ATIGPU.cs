/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2011 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware.ATI {
  internal sealed class ATIGPU : Hardware {

    private readonly int adapterIndex;
    private readonly int busNumber;
    private readonly int deviceNumber;
    private readonly Sensor temperature;
    private readonly Sensor fan;
    private readonly Sensor coreClock;
    private readonly Sensor memoryClock;
    private readonly Sensor coreVoltage;
    private readonly Sensor coreLoad;
    private readonly Sensor controlSensor;
    private readonly Control fanControl;

    private bool restoreDefaultFanSpeedRequired = false;
    private ADLFanSpeedValue initialFanSpeedValue;    

    public ATIGPU(string name, int adapterIndex, int busNumber, 
      int deviceNumber, ISettings settings) 
      : base(name, new Identifier("atigpu", 
        adapterIndex.ToString(CultureInfo.InvariantCulture)), settings)
    {
      this.adapterIndex = adapterIndex;
      this.busNumber = busNumber;
      this.deviceNumber = deviceNumber;

      this.temperature = new Sensor("GPU Core", 0, SensorType.Temperature, this, settings);
      this.fan = new Sensor("GPU Fan", 0, SensorType.Fan, this, settings);
      this.coreClock = new Sensor("GPU Core", 0, SensorType.Clock, this, settings);
      this.memoryClock = new Sensor("GPU Memory", 1, SensorType.Clock, this, settings);
      this.coreVoltage = new Sensor("GPU Core", 0, SensorType.Voltage, this, settings);
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

    private void SaveDefaultFanSpeed() {
      if (!restoreDefaultFanSpeedRequired) {        
        initialFanSpeedValue = new ADLFanSpeedValue();
        initialFanSpeedValue.SpeedType =
          ADL.ADL_DL_FANCTRL_SPEED_TYPE_PERCENT;
        restoreDefaultFanSpeedRequired = 
          ADL.ADL_Overdrive5_FanSpeed_Get(adapterIndex, 0,
          ref initialFanSpeedValue) == ADL.ADL_OK;
      }
    }

    private void RestoreDefaultFanSpeed() {
      if (restoreDefaultFanSpeedRequired) {        
        ADL.ADL_Overdrive5_FanSpeed_Set(adapterIndex, 0,
          ref this.initialFanSpeedValue);
        if ((initialFanSpeedValue.Flags &
          ADL.ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED) == 0)
          ADL.ADL_Overdrive5_FanSpeedToDefault_Set(adapterIndex, 0);
        restoreDefaultFanSpeedRequired = false;
      }
    }

    private void SoftwareControlValueChanged(IControl control) {
      if (control.ControlMode == ControlMode.Software) {
        SaveDefaultFanSpeed();
        ADLFanSpeedValue adlf = new ADLFanSpeedValue();
        adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_PERCENT;
        adlf.Flags = ADL.ADL_DL_FANCTRL_FLAG_USER_DEFINED_SPEED;
        adlf.FanSpeed = (int)control.SoftwareValue;
        ADL.ADL_Overdrive5_FanSpeed_Set(adapterIndex, 0, ref adlf);
      }
    }

    private void ControlModeChanged(IControl control) {
      if (control.ControlMode == ControlMode.Default) {
        RestoreDefaultFanSpeed();     
      } else {
        SoftwareControlValueChanged(control);
      }
    }

    public int BusNumber { get { return busNumber; } }

    public int DeviceNumber { get { return deviceNumber; } }


    public override HardwareType HardwareType {
      get { return HardwareType.GpuAti; }
    }

    public override void Update() {
      ADLTemperature adlt = new ADLTemperature();
      if (ADL.ADL_Overdrive5_Temperature_Get(adapterIndex, 0, ref adlt)
        == ADL.ADL_OK) 
      {
        temperature.Value = 0.001f * adlt.Temperature;
        ActivateSensor(temperature);
      } else {
        temperature.Value = null;
      }

      ADLFanSpeedValue adlf = new ADLFanSpeedValue();
      adlf.SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM;
      if (ADL.ADL_Overdrive5_FanSpeed_Get(adapterIndex, 0, ref adlf)
        == ADL.ADL_OK) 
      {
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
        == ADL.ADL_OK) 
      {
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

    public override void Close() {
      this.fanControl.ControlModeChanged -= ControlModeChanged;
      this.fanControl.SoftwareControlValueChanged -=
        SoftwareControlValueChanged;

      RestoreDefaultFanSpeed();
      base.Close();
    }
  }
}
