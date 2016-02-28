/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.GUI
{
    public class HardwareNode : Node
    {
        private readonly PersistentSettings settings;

        private readonly List<TypeNode> typeNodes = new List<TypeNode>();
        private readonly UnitManager unitManager;

        public HardwareNode(IHardware hardware, PersistentSettings settings,
            UnitManager unitManager)
        {
            this.settings = settings;
            this.unitManager = unitManager;
            Hardware = hardware;
            Image = HardwareTypeImage.Instance.GetImage(hardware.HardwareType);

            foreach (SensorType sensorType in Enum.GetValues(typeof (SensorType)))
                typeNodes.Add(new TypeNode(sensorType));

            foreach (var sensor in hardware.Sensors)
                SensorAdded(sensor);

            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;
        }

        public override string Text
        {
            get { return Hardware.Name; }
            set { Hardware.Name = value; }
        }

        public IHardware Hardware { get; }

        private void UpdateNode(TypeNode node)
        {
            if (node.Nodes.Count > 0)
            {
                if (!Nodes.Contains(node))
                {
                    var i = 0;
                    while (i < Nodes.Count &&
                           ((TypeNode) Nodes[i]).SensorType < node.SensorType)
                        i++;
                    Nodes.Insert(i, node);
                }
            }
            else
            {
                if (Nodes.Contains(node))
                    Nodes.Remove(node);
            }
        }

        private void SensorRemoved(ISensor sensor)
        {
            foreach (var typeNode in typeNodes)
                if (typeNode.SensorType == sensor.SensorType)
                {
                    SensorNode sensorNode = null;
                    foreach (var node in typeNode.Nodes)
                    {
                        var n = node as SensorNode;
                        if (n != null && n.Sensor == sensor)
                            sensorNode = n;
                    }
                    if (sensorNode != null)
                    {
                        sensorNode.PlotSelectionChanged -= SensorPlotSelectionChanged;
                        typeNode.Nodes.Remove(sensorNode);
                        UpdateNode(typeNode);
                    }
                }
            if (PlotSelectionChanged != null)
                PlotSelectionChanged(this, null);
        }

        private void InsertSorted(Node node, ISensor sensor)
        {
            var i = 0;
            while (i < node.Nodes.Count &&
                   ((SensorNode) node.Nodes[i]).Sensor.Index < sensor.Index)
                i++;
            var sensorNode = new SensorNode(sensor, settings, unitManager);
            sensorNode.PlotSelectionChanged += SensorPlotSelectionChanged;
            node.Nodes.Insert(i, sensorNode);
        }

        private void SensorPlotSelectionChanged(object sender, EventArgs e)
        {
            if (PlotSelectionChanged != null)
                PlotSelectionChanged(this, null);
        }

        private void SensorAdded(ISensor sensor)
        {
            foreach (var typeNode in typeNodes)
                if (typeNode.SensorType == sensor.SensorType)
                {
                    InsertSorted(typeNode, sensor);
                    UpdateNode(typeNode);
                }
            if (PlotSelectionChanged != null)
                PlotSelectionChanged(this, null);
        }

        public event EventHandler PlotSelectionChanged;
    }
}