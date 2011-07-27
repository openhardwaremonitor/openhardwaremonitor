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
using System.Collections.Generic;
using System.Drawing;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI {
  public class HardwareNode : Node {

    private PersistentSettings settings;
    private UnitManager unitManager;
    private IHardware hardware;

    private List<TypeNode> typeNodes = new List<TypeNode>();

    public HardwareNode(IHardware hardware, PersistentSettings settings, 
      UnitManager unitManager) : base() 
    {
      this.settings = settings;
      this.unitManager = unitManager;
      this.hardware = hardware;
      this.Image = HardwareTypeImage.Instance.GetImage(hardware.HardwareType);

      typeNodes.Add(new TypeNode(SensorType.Voltage));
      typeNodes.Add(new TypeNode(SensorType.Clock));      
      typeNodes.Add(new TypeNode(SensorType.Temperature));
      typeNodes.Add(new TypeNode(SensorType.Load));
      typeNodes.Add(new TypeNode(SensorType.Fan));
      typeNodes.Add(new TypeNode(SensorType.Flow));
      typeNodes.Add(new TypeNode(SensorType.Control));
      typeNodes.Add(new TypeNode(SensorType.Level));
      typeNodes.Add(new TypeNode(SensorType.Power));

      foreach (ISensor sensor in hardware.Sensors)
        SensorAdded(sensor);

      hardware.SensorAdded +=new SensorEventHandler(SensorAdded);
      hardware.SensorRemoved += new SensorEventHandler(SensorRemoved);
    }

    public override string Text {
      get { return hardware.Name; }
      set { hardware.Name = value; }
    }

    public IHardware Hardware {
      get { return hardware; }
    }

    private void UpdateNode(TypeNode node) {  
      if (node.Nodes.Count > 0) {
        if (!Nodes.Contains(node)) {
          int i = 0;
          while (i < Nodes.Count &&
            ((TypeNode)Nodes[i]).SensorType < node.SensorType)
            i++;
          Nodes.Insert(i, node);  
        }
      } else {
        if (Nodes.Contains(node))
          Nodes.Remove(node);
      }
    }

    private void SensorRemoved(ISensor sensor) {
      foreach (TypeNode typeNode in typeNodes)
        if (typeNode.SensorType == sensor.SensorType) { 
          SensorNode sensorNode = null;
          foreach (Node node in typeNode.Nodes) {
            SensorNode n = node as SensorNode;
            if (n != null && n.Sensor == sensor)
              sensorNode = n;
          }
          typeNode.Nodes.Remove(sensorNode);
          UpdateNode(typeNode);
        }
    }

    private void InsertSorted(Node node, ISensor sensor) {
      int i = 0;
      while (i < node.Nodes.Count &&
        ((SensorNode)node.Nodes[i]).Sensor.Index < sensor.Index)
        i++;
      SensorNode sensorNode = new SensorNode(sensor, settings, unitManager);
      node.Nodes.Insert(i, sensorNode);
    }

    private void SensorAdded(ISensor sensor) {
      foreach (TypeNode typeNode in typeNodes)
        if (typeNode.SensorType == sensor.SensorType) {
          InsertSorted(typeNode, sensor);
          UpdateNode(typeNode);          
        }
    }    
  }
}
