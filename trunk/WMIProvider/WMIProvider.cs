using System;
using System.Collections.Generic;
using System.Management.Instrumentation;
using OpenHardwareMonitor.Hardware;

[assembly: Instrumented("root/OpenHardwareMonitor")]

[System.ComponentModel.RunInstaller(true)]
public class InstanceInstaller : DefaultManagementProjectInstaller { }

namespace OpenHardwareMonitor.WMIProvider {
  public class WmiProvider : IDisposable {
    private List<IWmiClass> _activeInstances;

    public WmiProvider(IComputer computer) {
      _activeInstances = new List<IWmiClass>();

      foreach (IHardware hardware in computer.Hardware)
        ComputerHardwareAdded(hardware);

      computer.HardwareAdded += ComputerHardwareAdded;
      computer.HardwareRemoved += ComputerHardwareRemoved;
    }

    public void Update() {
      foreach (IWmiClass instance in _activeInstances)
        instance.Update();
    }

    #region Eventhandlers
    
    private void ComputerHardwareAdded(IHardware hardware) {
      if (!Exists(hardware.Identifier.ToString())) {
        foreach (ISensor sensor in hardware.Sensors)
          HardwareSensorAdded(sensor);

        hardware.SensorAdded += HardwareSensorAdded;
        hardware.SensorRemoved += HardwareSensorRemoved;

        Hardware hw = new Hardware(hardware);
        _activeInstances.Add(hw);

        Instrumentation.Publish(hw);
      }

      foreach (IHardware subHardware in hardware.SubHardware)
        ComputerHardwareAdded(subHardware);
    }

    private void HardwareSensorAdded(ISensor data) {
      Sensor sensor = new Sensor(data);
      _activeInstances.Add(sensor);

      Instrumentation.Publish(sensor);
    }

    private void ComputerHardwareRemoved(IHardware hardware) {
      hardware.SensorAdded -= HardwareSensorAdded;
      hardware.SensorRemoved -= HardwareSensorRemoved;
      
      foreach (ISensor sensor in hardware.Sensors) 
        HardwareSensorRemoved(sensor);
      
      foreach (IHardware subHardware in hardware.SubHardware)
        ComputerHardwareRemoved(subHardware);

      RevokeInstance(hardware.Identifier.ToString());
    }

    private void HardwareSensorRemoved(ISensor sensor) {
      RevokeInstance(sensor.Identifier.ToString());
    }

    #endregion

    #region Helpers
    
    private bool Exists(string identifier) {
      return _activeInstances.Exists(h => h.Identifier == identifier);
    }

    private void RevokeInstance(string identifier) {
      int instanceIndex = _activeInstances.FindIndex(
        item => item.Identifier == identifier.ToString()
      );

      Instrumentation.Revoke(_activeInstances[instanceIndex]);

      _activeInstances.RemoveAt(instanceIndex);
    }

    #endregion

    public void Dispose() {
      foreach (IWmiClass instance in _activeInstances)
        Instrumentation.Revoke(instance);
      _activeInstances = null;
    }
  }
}
