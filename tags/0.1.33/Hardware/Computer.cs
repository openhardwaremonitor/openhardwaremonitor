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
using System.IO;
using System.Globalization;
using System.Text;
using System.Threading;

namespace OpenHardwareMonitor.Hardware {

  public class Computer : IComputer {

    private List<IGroup> groups = new List<IGroup>();

    private bool open = false;
    private bool hddEnabled = false;

    public Computer() { }

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

    public void Open() {
      if (open)
        return;

      Add(new Mainboard.MainboardGroup());
      Add(new CPU.CPUGroup());
      Add(new ATI.ATIGroup());
      Add(new Nvidia.NvidiaGroup());
      Add(new TBalancer.TBalancerGroup());

      if (hddEnabled)
        Add(new HDD.HDDGroup());

      open = true;
    }

    private void SubHardwareUpdate(IHardware hardware) {
      foreach (IHardware subHardware in hardware.SubHardware) {
        subHardware.Update();
        SubHardwareUpdate(subHardware);
      }
    }

    public void Update() {
      foreach (IGroup group in groups)
        foreach (IHardware hardware in group.Hardware) {
          hardware.Update();
          SubHardwareUpdate(hardware);
        }
    }

    public bool HDDEnabled {
      get { return hddEnabled; }
      set {
        if (open && value && !hddEnabled) {
          Add(new HDD.HDDGroup());
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

    private void NewSection(TextWriter writer) {
      for (int i = 0; i < 8; i++)
        writer.Write("----------");
      writer.WriteLine();
      writer.WriteLine();
    }

    private void ReportHardwareTree(IHardware hardware, TextWriter w,
      string space) {
      w.WriteLine("{0}|", space);
      w.WriteLine("{0}+-+ {1} ({2})",
        space, hardware.Name, hardware.Identifier);
      foreach (ISensor sensor in hardware.Sensors) {
        w.WriteLine("{0}|   +- {1} : {2} : {3} : {4}",
          space, sensor.SensorType, sensor.Index, sensor.Name,
            string.Format(CultureInfo.InvariantCulture, "{0} : {1} : {2}",
            sensor.Value, sensor.Min, sensor.Max));
        foreach (IParameter parameter in sensor.Parameters) {
          w.WriteLine("{0}|      +- {1} : {2} : {3}",
            space, parameter.Name, parameter.IsDefault,
            string.Format(CultureInfo.InvariantCulture, "{0} : {1}",
              parameter.DefaultValue, parameter.Value));
        }
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardwareTree(subHardware, w, "|   ");
    }

    private void ReportHardware(IHardware hardware, TextWriter w) {
      string hardwareReport = hardware.GetReport();
      if (hardwareReport != null && hardwareReport != "") {
        NewSection(w);
        w.Write(hardwareReport);
      }
      foreach (IHardware subHardware in hardware.SubHardware)
        ReportHardware(subHardware, w);
    }

    public string GetReport() {

      using (StringWriter w = new StringWriter()) {

        w.WriteLine();
        w.WriteLine("Open Hardware Monitor Report");
        w.WriteLine();

        Version version = typeof(Computer).Assembly.GetName().Version;

        NewSection(w);
        w.Write("Version: "); w.WriteLine(version.ToString());
        w.WriteLine();

        NewSection(w);
        foreach (IGroup group in groups) {
          foreach (IHardware hardware in group.Hardware)
            ReportHardwareTree(hardware, w, "");
        }
        w.WriteLine();

        foreach (IGroup group in groups) {
          string report = group.GetReport();
          if (report != null && report != "") {
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

      foreach (IGroup group in groups)
        group.Close();
      groups.Clear();

      open = false;
    }

    public event HardwareEventHandler HardwareAdded;
    public event HardwareEventHandler HardwareRemoved;
  }
}
