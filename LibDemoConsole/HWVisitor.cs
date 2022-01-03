using OpenHardwareMonitor.Hardware;

namespace LibDemoConsole {
    internal class HWVisitor : IVisitor
    {
        int depth = 0;
        public void VisitComputer(IComputer computer)
        {
            Serilog.Log.Information("{computer}", computer);
            computer.Traverse(this);
        }

        public void VisitHardware(IHardware hardware)
        {
            Serilog.Log.Information(new string('\t', depth) + "{type} {name} {identifier}", hardware.HardwareType, hardware.Name, hardware.Identifier);
            hardware.Update();
            depth++;
            hardware.Traverse(this);
            depth--;
        }

        public void VisitParameter(IParameter parameter)
        {
            Serilog.Log.Information(new string('\t', depth) + "{type} {value} {name} {identifier}", parameter.Sensor.SensorType, parameter.Value , parameter.Name, parameter.Identifier);
        }

        public void VisitSensor(ISensor sensor)
        {
            Serilog.Log.Information(new string('\t', depth) + "{type} {value} {name} {identifier}", sensor.SensorType, sensor.Value, sensor.Name, sensor.Identifier);
            depth++;
            sensor.Traverse(this);
            depth--;
        }
    }
}
