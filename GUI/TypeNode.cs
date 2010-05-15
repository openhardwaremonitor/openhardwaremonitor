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
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI {
  public class TypeNode : Node {

    private SensorType sensorType;

    public TypeNode(SensorType sensorType) : base() {
      this.sensorType = sensorType;

      switch (sensorType) {
        case SensorType.Voltage: 
          this.Image = Utilities.EmbeddedResources.GetImage("voltage.png");
          this.Text = "Voltages";
          break;
        case SensorType.Clock:
          this.Image = Utilities.EmbeddedResources.GetImage("clock.png");
          this.Text = "Clocks";
          break;
        case SensorType.Load:
          this.Image = Utilities.EmbeddedResources.GetImage("load.png");
          this.Text = "Load";
          break;
        case SensorType.Temperature:
          this.Image = Utilities.EmbeddedResources.GetImage("temperature.png");
          this.Text = "Temperatures";
          break;
        case SensorType.Fan:
          this.Image = Utilities.EmbeddedResources.GetImage("fan.png");
          this.Text = "Fans";
          break;
        case SensorType.Flow:
          this.Image = Utilities.EmbeddedResources.GetImage("flow.png");
          this.Text = "Flows";
          break;
        case SensorType.Control:
          this.Image = Utilities.EmbeddedResources.GetImage("control.png");
          this.Text = "Controls";
          break;
      }

      NodeAdded += new NodeEventHandler(TypeNode_NodeAdded);
      NodeRemoved += new NodeEventHandler(TypeNode_NodeRemoved);
    }

    private void TypeNode_NodeRemoved(Node node) {
      node.IsVisibleChanged -= new NodeEventHandler(node_IsVisibleChanged);
      node_IsVisibleChanged(null);
    }    

    private void TypeNode_NodeAdded(Node node) {
      node.IsVisibleChanged += new NodeEventHandler(node_IsVisibleChanged);
      node_IsVisibleChanged(null);
    }

    private void node_IsVisibleChanged(Node node) {      
      foreach (Node n in Nodes)
        if (n.IsVisible) {
          this.IsVisible = true;
          return;
        }
      this.IsVisible = false;
    }

    public SensorType SensorType {
      get { return sensorType; }
    }
  }
}
