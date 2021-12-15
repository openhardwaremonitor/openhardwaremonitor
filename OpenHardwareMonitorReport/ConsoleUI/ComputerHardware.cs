/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2018 Jochen Wezel <jwezel@compumaster.de>
	
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitorReport
{
    class ComputerHardware : IDisposable
    {
        public ComputerHardware()
        {
            settings = new OpenHardwareMonitor.PersistentSettings();
            unitManager = new OpenHardwareMonitor.GUI.UnitManager(settings);
            treeModel = new OpenHardwareMonitor.GUI.TreeModel();
            root = new OpenHardwareMonitor.GUI.Node(System.Environment.MachineName);
            root.Image = OpenHardwareMonitor.Utilities.EmbeddedResources.GetImage("computer.png");
            treeModel.Nodes.Add(root);
            treeModel.ForceVisible = true;
        }

        public Node root;
        private OpenHardwareMonitor.PersistentSettings settings;
        private OpenHardwareMonitor.GUI.TreeModel treeModel;
        private UnitManager unitManager;
        private UpdateVisitor updateVisitor = new OpenHardwareMonitor.GUI.UpdateVisitor();
        private Computer computer;

        public Computer ComputerDiagnostics(CommandLineOptions.OptionsBase options)
        {
            if (computer != null) { computer.Close(); }

            computer = new Computer();

            computer.CPUEnabled = !options.IgnoreMonitorCPU;
            computer.FanControllerEnabled = !options.IgnoreMonitorFanController;
            computer.GPUEnabled = !options.IgnoreMonitorGPU;
            computer.HDDEnabled = !options.IgnoreMonitorHDD;
            computer.MainboardEnabled = !options.IgnoreMonitorMainboard;
            computer.RAMEnabled = !options.IgnoreMonitorRAM;
            computer.NetworkEnabled = !options.IgnoreMonitorNetwork;

            computer.HardwareAdded += new HardwareEventHandler(HardwareAdded);
            computer.HardwareRemoved += new HardwareEventHandler(HardwareRemoved);

            // add platform dependent code
            var platForm = Environment.OSVersion.Platform;
            if (platForm == PlatformID.Win32NT)
            {
                // Windows
                // not sure if really required: gadget = new OpenHardwareMonitor.GUI.SensorGadget(computer, settings, unitManager);
                // wmiProvider = new OpenHardwareMonitor.WMI.WmiProvider(computer);
            }

            computer.Open();

            computer.Accept(this.updateVisitor);

            return computer;
        }

        private void InsertSorted(Collection<Node> nodes, HardwareNode node)
        {
            int i = 0;
            while (i < nodes.Count && nodes[i] is HardwareNode &&
              ((HardwareNode)nodes[i]).Hardware.HardwareType <
                node.Hardware.HardwareType)
                i++;
            nodes.Insert(i, node);
        }

        private void SubHardwareAdded(IHardware hardware, Node node)
        {
            HardwareNode hardwareNode =
              new HardwareNode(hardware, settings, unitManager);

            InsertSorted(node.Nodes, hardwareNode);

            foreach (IHardware subHardware in hardware.SubHardware)
                SubHardwareAdded(subHardware, hardwareNode);
        }

        private void HardwareAdded(IHardware hardware)
        {
            SubHardwareAdded(hardware, root);
        }

        private void HardwareRemoved(IHardware hardware)
        {
            List<HardwareNode> nodesToRemove = new List<HardwareNode>();
            foreach (Node node in root.Nodes)
            {
                HardwareNode hardwareNode = node as HardwareNode;
                if (hardwareNode != null && hardwareNode.Hardware == hardware)
                    nodesToRemove.Add(hardwareNode);
            }
            foreach (HardwareNode hardwareNode in nodesToRemove)
            {
                root.Nodes.Remove(hardwareNode);
            }
        }

        /// <summary>
        /// Refresh the data for the next web request
        /// </summary>
        public void RefreshData()
        {
            computer.Accept(updateVisitor);
            // treeView.Invalidate();

            // not sure if really required - see also constructor!
            // if (gadget != null)
            //     gadget.Redraw();
        }

        #region IDisposable Support
        private bool disposedValue = false; // Dient zur Erkennung redundanter Aufrufe.

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: verwalteten Zustand (verwaltete Objekte) entsorgen.
                }

                // TODO: nicht verwaltete Ressourcen (nicht verwaltete Objekte) freigeben und Finalizer weiter unten überschreiben.
                if (computer != null)
                {
                    computer.Close();
                    computer = null;
                }
                // TODO: große Felder auf Null setzen.

                disposedValue = true;
            }
        }

        // TODO: Finalizer nur überschreiben, wenn Dispose(bool disposing) weiter oben Code für die Freigabe nicht verwalteter Ressourcen enthält.
         ~ComputerHardware() {
           // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
           Dispose(false);
         }

        // Dieser Code wird hinzugefügt, um das Dispose-Muster richtig zu implementieren.
        public void Dispose()
        {
            // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in Dispose(bool disposing) weiter oben ein.
            Dispose(true);
            // TODO: Auskommentierung der folgenden Zeile aufheben, wenn der Finalizer weiter oben überschrieben wird.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
