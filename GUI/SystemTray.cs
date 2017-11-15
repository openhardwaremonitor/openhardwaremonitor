/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using OpenHardwareMonitor.Hardware;
using OpenHardwareMonitor.Utilities;

namespace OpenHardwareMonitor.GUI
{
    public class SystemTray : IDisposable
    {
        private readonly List<SensorNotifyIcon> list = new List<SensorNotifyIcon>();
        private readonly NotifyIconAdv mainIcon;
        private bool mainIconEnabled;
        private readonly PersistentSettings settings;
        private readonly UnitManager unitManager;

        public SystemTray(IComputer computer, PersistentSettings settings,
            UnitManager unitManager)
        {
            this.settings = settings;
            this.unitManager = unitManager;
            computer.HardwareAdded += HardwareAdded;
            computer.HardwareRemoved += HardwareRemoved;

            mainIcon = new NotifyIconAdv();

            var contextMenu = new ContextMenu();
            var hideShowItem = new MenuItem("Hide/Show");
            hideShowItem.Click += delegate { SendHideShowCommand(); };
            contextMenu.MenuItems.Add(hideShowItem);
            contextMenu.MenuItems.Add(new MenuItem("-"));
            var exitItem = new MenuItem("Exit");
            exitItem.Click += delegate { SendExitCommand(); };
            contextMenu.MenuItems.Add(exitItem);
            mainIcon.ContextMenu = contextMenu;
            mainIcon.DoubleClick += delegate { SendHideShowCommand(); };
            mainIcon.Icon = EmbeddedResources.GetIcon("smallicon.ico");
            mainIcon.Text = "Open Hardware Monitor";
        }

        public bool IsMainIconEnabled
        {
            get => mainIconEnabled;
            set
            {
                if (mainIconEnabled != value)
                {
                    mainIconEnabled = value;
                    UpdateMainIconVisibilty();
                }
            }
        }

        public void Dispose()
        {
            foreach (var icon in list)
                icon.Dispose();
            mainIcon.Dispose();
        }

        private void HardwareRemoved(IHardware hardware)
        {
            hardware.SensorAdded -= SensorAdded;
            hardware.SensorRemoved -= SensorRemoved;
            foreach (var sensor in hardware.Sensors)
                SensorRemoved(sensor);
            foreach (var subHardware in hardware.SubHardware)
                HardwareRemoved(subHardware);
        }

        private void HardwareAdded(IHardware hardware)
        {
            foreach (var sensor in hardware.Sensors)
                SensorAdded(sensor);
            hardware.SensorAdded += SensorAdded;
            hardware.SensorRemoved += SensorRemoved;
            foreach (var subHardware in hardware.SubHardware)
                HardwareAdded(subHardware);
        }

        private void SensorAdded(ISensor sensor)
        {
            if (settings.GetValue(new Identifier(sensor.Identifier,
                "tray").ToString(), false))
                Add(sensor, false);
        }

        private void SensorRemoved(ISensor sensor)
        {
            if (Contains(sensor))
                Remove(sensor, false);
        }

        public void Redraw()
        {
            foreach (var icon in list)
                icon.Update();
        }

        public bool Contains(ISensor sensor)
        {
            foreach (var icon in list)
                if (icon.Sensor == sensor)
                    return true;
            return false;
        }

        public void Add(ISensor sensor, bool balloonTip)
        {
            if (Contains(sensor))
            {
            }
            else
            {
                list.Add(new SensorNotifyIcon(this, sensor, balloonTip, settings, unitManager));
                UpdateMainIconVisibilty();
                settings.SetValue(new Identifier(sensor.Identifier, "tray").ToString(), true);
            }
        }

        public void Remove(ISensor sensor)
        {
            Remove(sensor, true);
        }

        private void Remove(ISensor sensor, bool deleteConfig)
        {
            if (deleteConfig)
            {
                settings.Remove(
                    new Identifier(sensor.Identifier, "tray").ToString());
                settings.Remove(
                    new Identifier(sensor.Identifier, "traycolor").ToString());
            }
            SensorNotifyIcon instance = null;
            foreach (var icon in list)
                if (icon.Sensor == sensor)
                    instance = icon;
            if (instance != null)
            {
                list.Remove(instance);
                UpdateMainIconVisibilty();
                instance.Dispose();
            }
        }

        public event EventHandler HideShowCommand;

        public void SendHideShowCommand()
        {
            HideShowCommand?.Invoke(this, null);
        }

        public event EventHandler ExitCommand;

        public void SendExitCommand()
        {
            ExitCommand?.Invoke(this, null);
        }

        private void UpdateMainIconVisibilty()
        {
            if (mainIconEnabled) mainIcon.Visible = list.Count == 0;
            else mainIcon.Visible = false;
        }
    }
}