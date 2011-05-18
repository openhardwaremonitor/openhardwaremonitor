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
using System.Globalization;
using System.IO;
using System.Security.Permissions;
using System.Reflection;

namespace OpenHardwareMonitor.Hardware {

  public class Computer : IComputer {

    private readonly List<IGroup> groups = new List<IGroup>();
    private readonly ISettings settings;

    private bool open;
    private bool hddEnabled;    

    public Computer() {
      this.settings = new Settings();
    }

    public Computer(ISettings settings) {
      this.settings = settings ?? new Settings();
    }

    private void Add(IGroup group) {
      if (groups.Contains(group))
        return;

      groups.Add(group);

      if (HardwareAdded != null)
        foreach (IHardware hardware in group.Hardware)
          HardwareAdded(hardware);
    }

    private void Remove(IGroup group) {
      if (!groups.Contains(group))
        return;

      groups.Remove(group);

      if (HardwareRemoved != null)
        foreach (IHardware hardware in group.Hardware)
          HardwareRemoved(hardware);
    }

    [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
    public void Open() {
      if (open)
        return;

      Ring0.Open();
      Opcode.Open();

      Add(new Mainboard.MainboardGroup(settings));
      Add(new CPU.CPUGroup(settings));
      Add(new ATI.ATIGroup(settings));
      Add(new Nvidia.NvidiaGroup(settings));      
      Add(new TBalancer.TBalancerGroup(settings));
      Add(new Heatmaster.HeatmasterGroup(settings));

      if (hddEnabled)
        Add(new HDD.HDDGroup(settings));

      open = true;
    }
    
    public bool HDDEnabled {
      get { return hddEnabled; }

      [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
      set {
        if (open && value && !hddEnabled) {
          Add(new HDD.HDDGroup(settings));
        } else if (open && !value && hddEnabled) {
          List<IGroup> list = new List<IGroup>();
          foreach (IGroup group in groups)
            if (group is HDD.HDDGroup)
              list.Add(group);
          foreach (IGroup group in list)
            Remove(group);
        }
        hddEnabled = value;
      }
    }

    public IHardware[] Hardware {
      get {
        List<IHardware> list = new List<IHardware>();
        foreach (IGroup group in groups)
          foreach (IHardware hardware in group.Hardware)
            list.Add(hardware);
        return list.ToArray();
      }
    }

    private static void NewSection(TextWriter writer) {
      for (int i = 0; i < 8; i++)
        writer.Write("----------");
      writer.WriteLine();
      writer.WriteLine();
    }

    private static int CompareSensor(ISensor a, ISensor b) {
      int c = a.SensorType.CompareTo(b.SensorType);
      if (c == 0)
        return a.Index.CompareTo(b.Index);
      else
        return c;
    }

    private static void ReportHardwareSensorTree(
      IHardware hardware, TextWriter w, string space) 
    {
      w.WriteLine("{0}|", space);
      w.WriteLine("{0}+- {1} ({2})",
        space, hardware.Name, hardware.Identifier);
      ISensor[] sensors = hardware.Sensors;
      Array.Sort(sensors, CompareSensor);
      foreach (ISensor sensor in sensors) {
        w.WriteLine("{0}|  +- {1,-14} : {2,8:G6} {3,8:G6} {4,8:G6} ({5})", 
          space, sensor.Name, sensor.Value, sensor.Min, sensor.Max, 
          sensor.Identifier);
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardwareSensorTree(subHardware, w, "|  ");
    }

    private static void ReportHardwareParameterTree(
      IHardware hardware, TextWriter w, string space) {
      w.WriteLine("{0}|", space);
      w.WriteLine("{0}+- {1} ({2})",
        space, hardware.Name, hardware.Identifier);
      ISensor[] sensors = hardware.Sensors;
      Array.Sort(sensors, CompareSensor);
      foreach (ISensor sensor in sensors) {
        string innerSpace = space + "|  ";
        if (sensor.Parameters.Length > 0) {
          w.WriteLine("{0}|", innerSpace);
          w.WriteLine("{0}+- {1} ({2})",
            innerSpace, sensor.Name, sensor.Identifier);
          foreach (IParameter parameter in sensor.Parameters) {
            string innerInnerSpace = innerSpace + "|  ";
            w.WriteLine("{0}+- {1} : {2}",
              innerInnerSpace, parameter.Name,
              string.Format(CultureInfo.InvariantCulture, "{0} : {1}",
                parameter.DefaultValue, parameter.Value));
          }
        }
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardwareParameterTree(subHardware, w, "|  ");
    }

    private static void ReportHardware(IHardware hardware, TextWriter w) {
      string hardwareReport = hardware.GetReport();
      if (!string.IsNullOrEmpty(hardwareReport)) {
        NewSection(w);
        w.Write(hardwareReport);
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardware(subHardware, w);
    }

    public string GetReport() {

      using (StringWriter w = new StringWriter(CultureInfo.InvariantCulture)) {

        w.WriteLine();
        w.WriteLine("Open Hardware Monitor Report");
        w.WriteLine();

        Version version = typeof(Computer).Assembly.GetName().Version;

        NewSection(w);
        w.Write("Version: "); w.WriteLine(version.ToString());
        w.WriteLine();

        NewSection(w);
        w.Write("Common Language Runtime: ");
        w.WriteLine(Environment.Version.ToString());
        w.Write("Operating System: ");
        w.WriteLine(Environment.OSVersion.ToString());
        w.Write("Process Type: ");
        w.WriteLine(IntPtr.Size == 4 ? "32-Bit" : "64-Bit");
        w.WriteLine();

        string r = Ring0.GetReport();
        if (r != null) {
          NewSection(w);
          w.Write(r);
          w.WriteLine();
        }

        NewSection(w);
        w.WriteLine("Sensors");
        w.WriteLine();
        foreach (IGroup group in groups) {
          foreach (IHardware hardware in group.Hardware)
            ReportHardwareSensorTree(hardware, w, "");
        }
        w.WriteLine();

        NewSection(w);
        w.WriteLine("Parameters");
        w.WriteLine();
        foreach (IGroup group in groups) {
          foreach (IHardware hardware in group.Hardware)
            ReportHardwareParameterTree(hardware, w, "");
        }
        w.WriteLine();

        foreach (IGroup group in groups) {
          string report = group.GetReport();
          if (!string.IsNullOrEmpty(report)) {
            NewSection(w);
            w.Write(report);
          }

          IHardware[] hardwareArray = group.Hardware;
          foreach (IHardware hardware in hardwareArray)
            ReportHardware(hardware, w);

        }
        return w.ToString();
      }
    }

    public void Close() {      
      if (!open)
        return;

      while (groups.Count > 0) {
        IGroup group = groups[groups.Count - 1];
        Remove(group);
        group.Close(); 
      } 

      Opcode.Close();
      Ring0.Close();

      open = false;
    }

    public event HardwareEventHandler HardwareAdded;
    public event HardwareEventHandler HardwareRemoved;

    public void Accept(IVisitor visitor) {
      if (visitor == null)
        throw new ArgumentNullException("visitor");
      visitor.VisitComputer(this);
    }

    public void Traverse(IVisitor visitor) {
      foreach (IGroup group in groups)
        foreach (IHardware hardware in group.Hardware) 
          hardware.Accept(visitor);
    }

    private class Settings : ISettings {

      public bool Contains(string name) {
        return false;
      }

      public void SetValue(string name, string value) { }

      public string GetValue(string name, string value) {
        return value;
      }

      public void Remove(string name) { }
    }
  }
}
