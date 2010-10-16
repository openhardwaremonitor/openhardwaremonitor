using System.Management.Instrumentation;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.WMIProvider {
  [InstrumentationClass(InstrumentationType.Instance)]
  public class Sensor : IWmiClass {

    private ISensor _sensor;

    public string SensorType { get; private set; }
    public string Identifier { get; private set; }
    public string Parent { get; private set; }
    public string Name { get; private set; }
    public float Value { get; private set; }
    public float Min { get; private set; }
    public float Max { get; private set; }
    public int Index { get; private set; }

    public Sensor(ISensor sensor) {
      Name = sensor.Name;
      Index = sensor.Index;

      SensorType = sensor.SensorType.ToString();
      Identifier = sensor.Identifier.ToString();
      Parent = sensor.Hardware.Identifier.ToString();

      _sensor = sensor;
    }
    
    public void Update() {
      Value = (_sensor.Value != null) ? (float)_sensor.Value : 0;

      if (_sensor.Min != null)
        Min = (float)_sensor.Min;

      if (_sensor.Max != null)
        Max = (float)_sensor.Max;
    }
  }
}
