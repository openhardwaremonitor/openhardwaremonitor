/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using OpenHardwareMonitor.Hardware.ATI;
using OpenHardwareMonitor.Hardware.CPU;
using OpenHardwareMonitor.Hardware.HDD;
using OpenHardwareMonitor.Hardware.Heatmaster;
using OpenHardwareMonitor.Hardware.Mainboard;
using OpenHardwareMonitor.Hardware.Nvidia;
using OpenHardwareMonitor.Hardware.RAM;
using OpenHardwareMonitor.Hardware.TBalancer;

namespace OpenHardwareMonitor.Hardware
{
    public class Computer : IComputer
    {
        private readonly List<IGroup> groups = new List<IGroup>();
        private readonly ISettings settings;
        private bool cpuEnabled;
        private bool fanControllerEnabled;
        private bool gpuEnabled;
        private bool hddEnabled;

        private bool mainboardEnabled;

        private bool open;
        private bool ramEnabled;

        private SMBIOS smbios;

        public Computer()
        {
            settings = new Settings();
        }

        public Computer(ISettings settings)
        {
            this.settings = settings ?? new Settings();
        }

        public bool MainboardEnabled
        {
            get { return mainboardEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != mainboardEnabled)
                {
                    if (value)
                        Add(new MainboardGroup(smbios, settings));
                    else
                        RemoveType<MainboardGroup>();
                }
                mainboardEnabled = value;
            }
        }

        public bool CPUEnabled
        {
            get { return cpuEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != cpuEnabled)
                {
                    if (value)
                        Add(new CPUGroup(settings));
                    else
                        RemoveType<CPUGroup>();
                }
                cpuEnabled = value;
            }
        }

        public bool RAMEnabled
        {
            get { return ramEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != ramEnabled)
                {
                    if (value)
                        Add(new RAMGroup(smbios, settings));
                    else
                        RemoveType<RAMGroup>();
                }
                ramEnabled = value;
            }
        }

        public bool GPUEnabled
        {
            get { return gpuEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != gpuEnabled)
                {
                    if (value)
                    {
                        Add(new ATIGroup(settings));
                        Add(new NvidiaGroup(settings));
                    }
                    else
                    {
                        RemoveType<ATIGroup>();
                        RemoveType<NvidiaGroup>();
                    }
                }
                gpuEnabled = value;
            }
        }

        public bool FanControllerEnabled
        {
            get { return fanControllerEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != fanControllerEnabled)
                {
                    if (value)
                    {
                        Add(new TBalancerGroup(settings));
                        Add(new HeatmasterGroup(settings));
                    }
                    else
                    {
                        RemoveType<TBalancerGroup>();
                        RemoveType<HeatmasterGroup>();
                    }
                }
                fanControllerEnabled = value;
            }
        }

        public bool HDDEnabled
        {
            get { return hddEnabled; }

            [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
            set
            {
                if (open && value != hddEnabled)
                {
                    if (value)
                        Add(new HarddriveGroup(settings));
                    else
                        RemoveType<HarddriveGroup>();
                }
                hddEnabled = value;
            }
        }

        public IHardware[] Hardware
        {
            get
            {
                var list = new List<IHardware>();
                foreach (var group in groups)
                    foreach (var hardware in group.Hardware)
                        list.Add(hardware);
                return list.ToArray();
            }
        }

        public string GetReport()
        {
            using (var w = new StringWriter(CultureInfo.InvariantCulture))
            {
                w.WriteLine();
                w.WriteLine("Open Hardware Monitor Report");
                w.WriteLine();

                var version = typeof (Computer).Assembly.GetName().Version;

                NewSection(w);
                w.Write("Version: ");
                w.WriteLine(version.ToString());
                w.WriteLine();

                NewSection(w);
                w.Write("Common Language Runtime: ");
                w.WriteLine(Environment.Version.ToString());
                w.Write("Operating System: ");
                w.WriteLine(Environment.OSVersion.ToString());
                w.Write("Process Type: ");
                w.WriteLine(IntPtr.Size == 4 ? "32-Bit" : "64-Bit");
                w.WriteLine();

                var r = Ring0.GetReport();
                if (r != null)
                {
                    NewSection(w);
                    w.Write(r);
                    w.WriteLine();
                }

                NewSection(w);
                w.WriteLine("Sensors");
                w.WriteLine();
                foreach (var group in groups)
                {
                    foreach (var hardware in group.Hardware)
                        ReportHardwareSensorTree(hardware, w, "");
                }
                w.WriteLine();

                NewSection(w);
                w.WriteLine("Parameters");
                w.WriteLine();
                foreach (var group in groups)
                {
                    foreach (var hardware in group.Hardware)
                        ReportHardwareParameterTree(hardware, w, "");
                }
                w.WriteLine();

                foreach (var group in groups)
                {
                    var report = group.GetReport();
                    if (!string.IsNullOrEmpty(report))
                    {
                        NewSection(w);
                        w.Write(report);
                    }

                    var hardwareArray = group.Hardware;
                    foreach (var hardware in hardwareArray)
                        ReportHardware(hardware, w);
                }
                return w.ToString();
            }
        }

        public event HardwareEventHandler HardwareAdded;
        public event HardwareEventHandler HardwareRemoved;

        public void Accept(IVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));
            visitor.VisitComputer(this);
        }

        public void Traverse(IVisitor visitor)
        {
            foreach (var group in groups)
                foreach (var hardware in group.Hardware)
                    hardware.Accept(visitor);
        }

        private void Add(IGroup group)
        {
            if (groups.Contains(group))
                return;

            groups.Add(group);

            if (HardwareAdded == null) return;
            foreach (var hardware in @group.Hardware)
                HardwareAdded(hardware);
        }

        private void Remove(IGroup group)
        {
            if (!groups.Contains(group))
                return;

            groups.Remove(group);

            if (HardwareRemoved != null)
                foreach (var hardware in group.Hardware)
                    HardwareRemoved(hardware);

            group.Close();
        }

        private void RemoveType<T>() where T : IGroup
        {
            var list = new List<IGroup>();
            foreach (var group in groups)
                if (group is T)
                    list.Add(group);
            foreach (var group in list)
                Remove(group);
        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
        public void Open()
        {
            if (open)
                return;

            smbios = new SMBIOS();

            Ring0.Open();
            Opcode.Open();

            if (mainboardEnabled)
                Add(new MainboardGroup(smbios, settings));

            if (cpuEnabled)
                Add(new CPUGroup(settings));

            if (ramEnabled)
                Add(new RAMGroup(smbios, settings));

            if (gpuEnabled)
            {
                Add(new ATIGroup(settings));
                Add(new NvidiaGroup(settings));
            }

            if (fanControllerEnabled)
            {
                Add(new TBalancerGroup(settings));
                Add(new HeatmasterGroup(settings));
            }

            if (hddEnabled)
                Add(new HarddriveGroup(settings));

            open = true;
        }

        private static void NewSection(TextWriter writer)
        {
            for (var i = 0; i < 8; i++)
                writer.Write("----------");
            writer.WriteLine();
            writer.WriteLine();
        }

        private static int CompareSensor(ISensor a, ISensor b)
        {
            var c = a.SensorType.CompareTo(b.SensorType);
            if (c == 0)
                return a.Index.CompareTo(b.Index);
            return c;
        }

        private static void ReportHardwareSensorTree(
            IHardware hardware, TextWriter w, string space)
        {
            w.WriteLine("{0}|", space);
            w.WriteLine("{0}+- {1} ({2})",
                space, hardware.Name, hardware.Identifier);
            var sensors = hardware.Sensors;
            Array.Sort(sensors, CompareSensor);
            foreach (var sensor in sensors)
            {
                w.WriteLine("{0}|  +- {1,-14} : {2,8:G6} {3,8:G6} {4,8:G6} ({5})",
                    space, sensor.Name, sensor.Value, sensor.Min, sensor.Max,
                    sensor.Identifier);
            }
            foreach (var subHardware in hardware.SubHardware)
                ReportHardwareSensorTree(subHardware, w, "|  ");
        }

        private static void ReportHardwareParameterTree(
            IHardware hardware, TextWriter w, string space)
        {
            w.WriteLine("{0}|", space);
            w.WriteLine("{0}+- {1} ({2})",
                space, hardware.Name, hardware.Identifier);
            var sensors = hardware.Sensors;
            Array.Sort(sensors, CompareSensor);
            foreach (var sensor in sensors)
            {
                var innerSpace = space + "|  ";
                if (sensor.Parameters.Length > 0)
                {
                    w.WriteLine("{0}|", innerSpace);
                    w.WriteLine("{0}+- {1} ({2})",
                        innerSpace, sensor.Name, sensor.Identifier);
                    foreach (var parameter in sensor.Parameters)
                    {
                        var innerInnerSpace = innerSpace + "|  ";
                        w.WriteLine("{0}+- {1} : {2}",
                            innerInnerSpace, parameter.Name,
                            string.Format(CultureInfo.InvariantCulture, "{0} : {1}",
                                parameter.DefaultValue, parameter.Value));
                    }
                }
            }
            foreach (var subHardware in hardware.SubHardware)
                ReportHardwareParameterTree(subHardware, w, "|  ");
        }

        private static void ReportHardware(IHardware hardware, TextWriter w)
        {
            var hardwareReport = hardware.GetReport();
            if (!string.IsNullOrEmpty(hardwareReport))
            {
                NewSection(w);
                w.Write(hardwareReport);
            }
            foreach (var subHardware in hardware.SubHardware)
                ReportHardware(subHardware, w);
        }

        public void Close()
        {
            if (!open)
                return;

            while (groups.Count > 0)
            {
                var group = groups[groups.Count - 1];
                Remove(group);
            }

            Opcode.Close();
            Ring0.Close();

            smbios = null;

            open = false;
        }

        private class Settings : ISettings
        {
            public bool Contains(string name)
            {
                return false;
            }

            public void SetValue(string name, string value)
            {
            }

            public string GetValue(string name, string value)
            {
                return value;
            }

            public void Remove(string name)
            {
            }
        }
    }
}