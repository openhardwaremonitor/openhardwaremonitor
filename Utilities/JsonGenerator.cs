using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Utilities {
  class JsonGenerator {

    private int nodeCount=0;

    public string GetJSON(Node root) {

      string JSON = "{\"id\": 0, \"Text\": \"Sensor\", \"Children\": [";
      nodeCount = 1;
      JSON += GenerateJSON(root);
      JSON += "]";
      JSON += ", \"Min\": \"Min\"";
      JSON += ", \"Value\": \"Value\"";
      JSON += ", \"Max\": \"Max\"";
      JSON += ", \"ImageURL\": \"\"";
      JSON += "}";

      return JSON;
    }

    private string GenerateJSON(Node n) {
      string JSON = "{\"id\": " + nodeCount + ", \"Text\": \"" + n.Text
        + "\", \"Children\": [";
      nodeCount++;

      foreach (Node child in n.Nodes)
        JSON += GenerateJSON(child) + ", ";
      if (JSON.EndsWith(", "))
        JSON = JSON.Remove(JSON.LastIndexOf(","));
      JSON += "]";

      if (n is SensorNode) {
        JSON += ", \"Min\": \"" + ((SensorNode)n).Min + "\"";
        JSON += ", \"Value\": \"" + ((SensorNode)n).Value + "\"";
        JSON += ", \"Max\": \"" + ((SensorNode)n).Max + "\"";
        JSON += ", \"ImageURL\": \"images/transparent.png\"";
      } else if (n is HardwareNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/" +
          GetHardwareImageFile((HardwareNode)n) + "\"";
      } else if (n is TypeNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/" +
          GetTypeImageFile((TypeNode)n) + "\"";
      } else {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/computer.png\"";
      }

      JSON += "}";
      return JSON;
    }

    private static string GetHardwareImageFile(HardwareNode hn) {

      switch (hn.Hardware.HardwareType) {
        case HardwareType.CPU:
          return "cpu.png";
        case HardwareType.GpuNvidia:
          return "nvidia.png";
        case HardwareType.GpuAti:
          return "ati.png";
        case HardwareType.HDD:
          return "hdd.png";
        case HardwareType.Heatmaster:
          return "bigng.png";
        case HardwareType.Mainboard:
          return "mainboard.png";
        case HardwareType.SuperIO:
          return "chip.png";
        case HardwareType.TBalancer:
          return "bigng.png";
        case HardwareType.RAM:
          return "ram.png";
        default:
          return "cpu.png";
      }

    }

    private static string GetTypeImageFile(TypeNode tn) {

      switch (tn.SensorType) {
        case SensorType.Voltage:
          return "voltage.png";
        case SensorType.Clock:
          return "clock.png";
        case SensorType.Load:
          return "load.png";
        case SensorType.Temperature:
          return "temperature.png";
        case SensorType.Fan:
          return "fan.png";
        case SensorType.Flow:
          return "flow.png";
        case SensorType.Control:
          return "control.png";
        case SensorType.Level:
          return "level.png";
        case SensorType.Power:
          return "power.png";
        default:
          return "power.png";
      }

    }
  }
}
