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
using System.Text;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI {
  public class SensorSystemTray : IDisposable {
    private Computer computer;
    private List<SensorNotifyIcon> list = new List<SensorNotifyIcon>();

    public SensorSystemTray(Computer computer) {
      this.computer = computer;
      computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
      computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);
    }

    private void HardwareRemoved(IHardware hardware) {
      hardware.SensorAdded -= new SensorEventHandler(SensorAdded);
      hardware.SensorRemoved -= new SensorEventHandler(SensorRemoved);
      foreach (ISensor sensor in hardware.Sensors) 
        SensorRemoved(sensor);
    }
   
    private void HardwareAdded(IHardware hardware) {
      foreach (ISensor sensor in hardware.Sensors)
        SensorAdded(sensor);
      hardware.SensorAdded += new SensorEventHandler(SensorAdded);
      hardware.SensorRemoved += new SensorEventHandler(SensorRemoved);
    }

    private void SensorAdded(ISensor sensor) {
      if (Config.Get(sensor.Identifier + "/tray", false)) 
        Add(sensor);   
    }

    private void SensorRemoved(ISensor sensor) {
      if (Contains(sensor)) {        
        Remove(sensor);
        Config.Set(sensor.Identifier + "/tray", true);
      }
    }

    public void Dispose() {
      foreach (SensorNotifyIcon icon in list)
        icon.Dispose();
    }

    public void Redraw() {
      foreach (SensorNotifyIcon icon in list)
        icon.Update();
    }

    public bool Contains(ISensor sensor) {
      foreach (SensorNotifyIcon icon in list)
        if (icon.Sensor == sensor)
          return true;
      return false;
    }

    public void Add(ISensor sensor) {
      if (Contains(sensor)) {
        return;
      } else {
        list.Add(new SensorNotifyIcon(this, sensor));
        Config.Set(sensor.Identifier + "/tray", true);
      }
    }

    public void Remove(ISensor sensor) {
      Config.Remove(sensor.Identifier + "/tray");
      SensorNotifyIcon instance = null;
      foreach (SensorNotifyIcon icon in list)
        if (icon.Sensor == sensor)
          instance = icon;
      if (instance != null) {
        list.Remove(instance);
        instance.Dispose();
      }
    }

  }
}
