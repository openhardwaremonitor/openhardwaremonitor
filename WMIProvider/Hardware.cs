using System.Management.Instrumentation;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.WMIProvider {
  [InstrumentationClass(InstrumentationType.Instance)]
  public class Hardware : IWmiClass {
    public string HardwareType { get; private set; }
    public string Identifier { get; private set; }
    public string Name { get; private set; }

    public Hardware(IHardware hardware) {
      Name = hardware.Name;
      Identifier = hardware.Identifier.ToString();
      HardwareType = hardware.HardwareType.ToString();
    }

    public void Update() { }
  }
}
