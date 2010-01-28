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
    private ushort address;

    public W83627DHG(byte revision, ushort address) {      
      this.revision = revision;
      this.address = address;

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
      r.Append("Base Adress: 0x"); r.AppendLine(address.ToString("X4"));
      r.AppendLine();

      return r.ToString();
    }

    public void Update() { }

    #pragma warning disable 67
    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
    #pragma warning restore 67
  }
}
