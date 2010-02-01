using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenHardwareMonitor.Hardware {
  public class ReportWriter {

    private static void NewSection(TextWriter writer) {      
      for (int i = 0; i < 8; i++)
        writer.Write("----------");
      writer.WriteLine();
      writer.WriteLine();
    }

    public static void Save(List<IGroup> groupList, Version version) {

      using (TextWriter w =
        new StreamWriter("OpenHardwareMonitor.Report.txt")) {

        w.WriteLine();
        w.WriteLine("Open Hardware Monitor Report");
        w.WriteLine();

        NewSection(w);
        w.Write("Version: "); w.WriteLine(version.ToString());
        w.WriteLine();

        NewSection(w);
        foreach (IGroup group in groupList) {          
          foreach (IHardware hardware in group.Hardware) {
            w.WriteLine("|");
            w.WriteLine("+-+ {0} ({1})",
              new object[] { hardware.Name, hardware.Identifier });
            foreach (ISensor sensor in hardware.Sensors) {
              w.WriteLine("|   +- {0} : {1} : {2} : {3}",
                new object[] { sensor.SensorType, sensor.Index, sensor.Name, 
                  sensor.Value });
            }
          }
        }
        w.WriteLine();

        foreach (IGroup group in groupList) {
          string report = group.GetReport();
          if (report != null) {
            NewSection(w);
            w.Write(report);            
          }

          IHardware[] hardwareArray = group.Hardware;
          foreach (IHardware hardware in hardwareArray) {
            string hardwareReport = hardware.GetReport();
            if (hardwareReport != null) {
              NewSection(w);
              w.Write(hardwareReport);
            }
          }
        }
      }
    }
  }
}
