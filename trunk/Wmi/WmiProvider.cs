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
  Paul Werelds <paul@werelds.net>.
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
using System.Management.Instrumentation;
using OpenHardwareMonitor.Hardware;

[assembly: Instrumented("root/OpenHardwareMonitor")]

[System.ComponentModel.RunInstaller(true)]
public class InstanceInstaller : DefaultManagementProjectInstaller { }

namespace OpenHardwareMonitor.WMI {
  /// <summary>
  /// The WMI Provider.
  /// This class is not exposed to WMI itself.
  /// </summary>
  public class WmiProvider : IDisposable {
    private List<IWmiObject> activeInstances;

    public WmiProvider(IComputer computer) {
      activeInstances = new List<IWmiObject>();

      foreach (IHardware hardware in computer.Hardware)
        ComputerHardwareAdded(hardware);

      computer.HardwareAdded += ComputerHardwareAdded;
      computer.HardwareRemoved += ComputerHardwareRemoved;
    }

    public void Update() {
      foreach (IWmiObject instance in activeInstances)
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
        activeInstances.Add(hw);

        Instrumentation.Publish(hw);
      }

      foreach (IHardware subHardware in hardware.SubHardware)
        ComputerHardwareAdded(subHardware);
    }

    private void HardwareSensorAdded(ISensor data) {
      Sensor sensor = new Sensor(data);
      activeInstances.Add(sensor);

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
      return activeInstances.Exists(h => h.Identifier == identifier);
    }

    private void RevokeInstance(string identifier) {
      int instanceIndex = activeInstances.FindIndex(
        item => item.Identifier == identifier.ToString()
      );

      Instrumentation.Revoke(activeInstances[instanceIndex]);

      activeInstances.RemoveAt(instanceIndex);
    }

    #endregion

    public void Dispose() {
      foreach (IWmiObject instance in activeInstances)
        Instrumentation.Revoke(instance);
      activeInstances = null;
    }
  }
}
