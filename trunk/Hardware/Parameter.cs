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
using System.Globalization;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Hardware {

  public struct ParameterDescription {
    private string name;
    private string description;
    private float defaultValue;    

    public ParameterDescription(string name, string description, 
      float defaultValue) {
      this.name = name;
      this.description = description;
      this.defaultValue = defaultValue;
    }

    public string Name { get { return name; } }

    public string Description { get { return description; } }

    public float DefaultValue { get { return defaultValue; } }
  }

  public class Parameter : IParameter {
    private ISensor sensor;
    private ParameterDescription description;
    private float value;
    private bool isDefault;
    private ISettings settings;

    public Parameter(ParameterDescription description, ISensor sensor, 
      ISettings settings) 
    {
      this.sensor = sensor;
      this.description = description;
      this.settings = settings;
      this.isDefault = !settings.Contains(Identifier.ToString());
      this.value = description.DefaultValue;
      if (!this.isDefault) {
        if (!float.TryParse(settings.GetValue(Identifier.ToString(), "0"),
          NumberStyles.Float,
          CultureInfo.InvariantCulture,
          out this.value))
          this.value = description.DefaultValue;
      }
    }

    public ISensor Sensor {
      get {
        return sensor;
      }
    }

    public Identifier Identifier {
      get {
        return new Identifier(sensor.Identifier, "parameter",
          Name.Replace(" ", "").ToLowerInvariant());
      }
    }

    public string Name { get { return description.Name; } }

    public string Description { get { return description.Description; } }

    public float Value {
      get {
        return value;
      }
      set {
        this.isDefault = false;
        this.value = value;
        this.settings.SetValue(Identifier.ToString(), value.ToString(
          CultureInfo.InvariantCulture));
      }
    }

    public float DefaultValue { 
      get { return description.DefaultValue; } 
    }

    public bool IsDefault {
      get { return isDefault; }
      set {
        this.isDefault = value;
        if (value) {
          this.value = description.DefaultValue;
          this.settings.Remove(Identifier.ToString());
        }
      }
    }

    public void Accept(IVisitor visitor) {
      if (visitor != null)
        visitor.VisitParameter(this);
    }

    public void Traverse(IVisitor visitor) { }
  }
}
