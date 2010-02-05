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

namespace OpenHardwareMonitor.Hardware {

  public class Sensor : ISensor {

    private string defaultName;
    private string name;
    private int index;
    private SensorType sensorType;
    private IHardware hardware;
    private float? value;
    private float? min;
    private float? max;
    private float? limit;
    private float? defaultLimit;
    private Queue<ISensorEntry> entries = 
      new Queue<ISensorEntry>(MAX_MINUTES * 15);
    
    private float sum = 0;
    private int count = 0;

    private const int MAX_MINUTES = 120;

    private string GetIdentifier() {
      return hardware.Identifier + "/" + sensorType.ToString().ToLower() + 
        "/" + index;
    }

    public Sensor(string name, int index, SensorType sensorType,
      IHardware hardware)
      : this(name, index, null, sensorType, hardware) { }

    public Sensor(string name, int index, float? limit, 
      SensorType sensorType, IHardware hardware) 
    {
      this.defaultName = name;      
      this.index = index;
      this.defaultLimit = limit;
      this.sensorType = sensorType;
      this.hardware = hardware;
      string configName = 
        Utilities.Config.Settings[GetIdentifier() + "/name"];
      if (configName != null)
        this.name = configName;
      else
        this.name = name;
      string configLimit =
        Utilities.Config.Settings[GetIdentifier() + "/limit"];
      if (configLimit != null && configLimit != "")
        this.limit = float.Parse(configLimit);
      else
        this.limit = limit;
    }

    public SensorType SensorType {
      get { return sensorType; }
    }

    public string Name {
      get { 
        return name; 
      }
      set {
        if (value != "") 
          name = value;          
        else 
          name = defaultName;
        Utilities.Config.Settings[GetIdentifier() + "/name"] = name;
      }
    }

    public int Index {
      get { return index; }
    }

    public float? Value {
      get { 
        return value; 
      }
      set {
        while (entries.Count > 0 && 
          (DateTime.Now - entries.Peek().Time).TotalMinutes > MAX_MINUTES)
          entries.Dequeue();

        if (value.HasValue) {
          sum += value.Value;
          count++;
          if (count == 4) {
            entries.Enqueue(new Entry(sum / count, DateTime.Now));
            sum = 0;
            count = 0;
          }
        }

        this.value = value;
        if (min > value || !min.HasValue)
          min = value;
        if (max < value || !max.HasValue)
          max = value;
      }
    }

    public float? Min { get { return min; } }
    public float? Max { get { return max; } }

    public float? Limit {
      get {
        return limit;
      }

      set {
        if (value.HasValue) {
          limit = value;
          Utilities.Config.Settings[GetIdentifier() + "/limit"] =
            limit.ToString();
        } else {
          limit = defaultLimit;
          Utilities.Config.Settings[GetIdentifier() + "/limit"] = "";          
        }        
      }
    }

    public IEnumerable<ISensorEntry> Plot {
      get { return entries; }
    }

    public struct Entry : ISensorEntry {
      private float value;
      private DateTime time;

      public Entry(float value, DateTime time) {
        this.value = value;
        this.time = time;
      }

      public float Value { get { return value; } }
      public DateTime Time { get { return time; } }
    }
  }
}
