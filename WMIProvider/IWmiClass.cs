namespace OpenHardwareMonitor.WMIProvider {
  interface IWmiClass {
    string Name { get; }
    string Identifier { get; }

    void Update();
  }
}
