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

  Contributor(s): Paul Werelds

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
using System.Globalization;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal class HDD : IHardware {

    private const int UPDATE_DIVIDER = 30; // update only every 30s

    private readonly string name;
    private readonly IntPtr handle;
    private readonly int drive;
    private int count;

    private readonly SMART.AttributeID temperatureID = 0x00;    
    private readonly SMART.SSDLifeID lifeID = 0x00;

    private readonly Sensor temperatureSensor;
    private readonly Sensor lifeSensor;

    public HDD(string name, IntPtr handle, int drive,
      SMART.AttributeID temperatureID, ISettings settings)
    {
      this.name = name;
      this.handle = handle;
      this.drive = drive;
      this.count = 0;
      this.temperatureID = temperatureID;
      this.temperatureSensor = new Sensor("HDD", 0, SensorType.Temperature,
        this, settings);

      Update();
    }

    public HDD(string name, IntPtr handle, int drive, SMART.SSDLifeID lifeID,
      ISettings settings)
    {
      this.name = name;
      this.handle = handle;
      this.drive = drive;
      this.count = 0;
      this.lifeID = lifeID;
      this.lifeSensor = new Sensor("HDD", 0, SensorType.Level, this, settings);

      Update();
    }

    public string Name {
      get { return name; }
    }

    public Identifier Identifier {
      get { 
        return new Identifier("hdd", 
          drive.ToString(CultureInfo.InvariantCulture)); 
      }
    }

    public HardwareType HardwareType {
      get { return HardwareType.HDD; }
    }

    public IHardware[] SubHardware {
      get { return new IHardware[0]; }
    }

    public virtual IHardware Parent {
      get { return null; }
    }

    public ISensor[] Sensors {
      get {
        if (lifeID != SMART.SSDLifeID.None)
          return new ISensor[] { lifeSensor };

        if (temperatureID != 0x00)
          return new ISensor[] { temperatureSensor };

        return new ISensor[] {};
      }
    }

    public string GetReport() {
      return null;
    }

    public void Update() {
      if (count == 0) {
        List<SMART.DriveAttribute> attributes = SMART.ReadSmart(handle, drive);
        if (temperatureID != 0x00 &&
          attributes.Exists(attr => (int)attr.ID == (int)temperatureID))
        {
          temperatureSensor.Value = attributes
            .Find(attr => (int)attr.ID == (int)temperatureID)
            .RawValue[0];
        }

        if (lifeID != 0x00 &&
          attributes.Exists(attr => (int)attr.ID == (int)lifeID))
        {
          lifeSensor.Value = attributes
            .Find(attr => (int)attr.ID == (int)temperatureID)
            .AttrValue;
        }
      } else {
        if (temperatureID != 0x00) {
          temperatureSensor.Value = temperatureSensor.Value;
        }

        if (lifeID != 0x00) {
          lifeSensor.Value = lifeSensor.Value;
        }
      }

      count++; count %= UPDATE_DIVIDER; 
    }

    public void Close() {
      SMART.CloseHandle(handle);
    }

    #pragma warning disable 67
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    #pragma warning restore 67    

    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitHardware(this);
    }

    public void Traverse(IVisitor visitor) { }
  }
}
