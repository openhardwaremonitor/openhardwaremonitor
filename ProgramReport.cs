/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2012 Michael Möller <mmoeller@openhardwaremonitor.org>
 
*/

using System;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitorReport {

  class Program {
    static void Main(string[] args) {

      Computer computer = new Computer();
     
      computer.CPUEnabled = true;
      computer.FanControllerEnabled = true;
      computer.GPUEnabled = true;
      computer.HDDEnabled = true;
      computer.MainboardEnabled = true;
      computer.RAMEnabled = true;

      computer.Open();

      computer.Accept(new UpdateVisitor());

      Console.Out.Write(computer.GetReport());

      computer.Close();
    }
  }

  public class UpdateVisitor : IVisitor {
    public void VisitComputer(IComputer computer) {
      computer.Traverse(this);
    }

    public void VisitHardware(IHardware hardware) {
      hardware.Update();
      foreach (IHardware subHardware in hardware.SubHardware)
        subHardware.Accept(this);
    }

    public void VisitSensor(ISensor sensor) { }

    public void VisitParameter(IParameter parameter) { }
  }
}
