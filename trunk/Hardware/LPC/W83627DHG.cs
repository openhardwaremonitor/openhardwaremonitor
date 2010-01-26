using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace OpenHardwareMonitor.Hardware.LPC {
  public class W83627DHG : IHardware {

    private byte revision;

    private string name;
    private Image icon;

    private bool available = false;

    public W83627DHG(byte revision) {      
      this.revision = revision;

      this.name = "Winbond W83627DHG";
      this.icon = Utilities.EmbeddedResources.GetImage("chip.png");
    }

    public bool IsAvailable {
      get { return available; }
    }

    public string Name {
      get { return name; }
    }

    public string Identifier {
      get { return "/lpc/w83627dhg"; }
    }

    public Image Icon {
      get { return icon; }
    }

    public ISensor[] Sensors {
      get { return new ISensor[0]; }
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("LPC W83627DHG");
      r.AppendLine();
      r.Append("Chip revision: 0x"); r.AppendLine(revision.ToString("X"));     

      return r.ToString();
    }

    public void Update() { }

    #pragma warning disable 67
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    #pragma warning restore 67
  }
}
