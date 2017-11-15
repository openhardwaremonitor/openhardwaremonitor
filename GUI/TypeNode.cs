/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI
{
    public class TypeNode : Node
    {
        public TypeNode(SensorType sensorType)
        {
            SensorType = sensorType;

            switch (sensorType)
            {
                case SensorType.Voltage:
                    Image = EmbeddedResources.GetImage("voltage.png");
                    Text = "Voltages";
                    break;
                case SensorType.Clock:
                    Image = EmbeddedResources.GetImage("clock.png");
                    Text = "Clocks";
                    break;
                case SensorType.Load:
                    Image = EmbeddedResources.GetImage("load.png");
                    Text = "Load";
                    break;
                case SensorType.Temperature:
                    Image = EmbeddedResources.GetImage("temperature.png");
                    Text = "Temperatures";
                    break;
                case SensorType.Fan:
                    Image = EmbeddedResources.GetImage("fan.png");
                    Text = "Fans";
                    break;
                case SensorType.Flow:
                    Image = EmbeddedResources.GetImage("flow.png");
                    Text = "Flows";
                    break;
                case SensorType.Control:
                    Image = EmbeddedResources.GetImage("control.png");
                    Text = "Controls";
                    break;
                case SensorType.Level:
                    Image = EmbeddedResources.GetImage("level.png");
                    Text = "Levels";
                    break;
                case SensorType.Power:
                    Image = EmbeddedResources.GetImage("power.png");
                    Text = "Powers";
                    break;
                case SensorType.Data:
                    Image = EmbeddedResources.GetImage("data.png");
                    Text = "Data";
                    break;
                case SensorType.SmallData:
                    Image = EmbeddedResources.GetImage("data.png");
                    Text = "Data";
                    break;
                case SensorType.Factor:
                    Image = EmbeddedResources.GetImage("factor.png");
                    Text = "Factors";
                    break;
            }

            NodeAdded += TypeNode_NodeAdded;
            NodeRemoved += TypeNode_NodeRemoved;
        }

        public SensorType SensorType { get; }

        private void TypeNode_NodeRemoved(Node node)
        {
            node.IsVisibleChanged -= node_IsVisibleChanged;
            node_IsVisibleChanged(null);
        }

        private void TypeNode_NodeAdded(Node node)
        {
            node.IsVisibleChanged += node_IsVisibleChanged;
            node_IsVisibleChanged(null);
        }

        private void node_IsVisibleChanged(Node node)
        {
            foreach (var n in Nodes)
                if (n.IsVisible)
                {
                    IsVisible = true;
                    return;
                }
            IsVisible = false;
        }
    }
}