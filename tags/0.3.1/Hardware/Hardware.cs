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
  Portions created by the Initial Developer are Copyright (C) 2009-2011
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
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware {
  internal abstract class Hardware : IHardware {

    private readonly Identifier identifier;
    protected readonly string name;
    private string customName;
    protected readonly ISettings settings;
    protected readonly ListSet<ISensor> active = new ListSet<ISensor>();

    public Hardware(string name, Identifier identifier, ISettings settings) {
      this.settings = settings;
      this.identifier = identifier;
      this.name = name;
      this.customName = settings.GetValue(
        new Identifier(Identifier, "name").ToString(), name);
    }

    public IHardware[] SubHardware {
      get { return new IHardware[0]; }
    }

    public virtual IHardware Parent {
      get { return null; }
    }

    public virtual ISensor[] Sensors {
      get { return active.ToArray(); }
    }

    protected virtual void ActivateSensor(ISensor sensor) {
      if (active.Add(sensor)) 
        if (SensorAdded != null)
          SensorAdded(sensor);
    }

    protected virtual void DeactivateSensor(ISensor sensor) {
      if (active.Remove(sensor))
        if (SensorRemoved != null)
          SensorRemoved(sensor);     
    }

    public string Name {
      get {
        return customName;
      }
      set {
        if (!string.IsNullOrEmpty(value))
          customName = value;
        else
          customName = name;
        settings.SetValue(new Identifier(Identifier, "name").ToString(), 
          customName);
      }
    }

    public Identifier Identifier {
      get {
        return identifier;
      }
    }

    #pragma warning disable 67
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    #pragma warning restore 67
  
    
    public abstract HardwareType HardwareType { get; }

    public virtual string GetReport() {
      return null;
    }

    public abstract void Update();

    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitHardware(this);
    }

    public virtual void Traverse(IVisitor visitor) {
      foreach (ISensor sensor in active)
        sensor.Accept(visitor);
    }
  }
}
