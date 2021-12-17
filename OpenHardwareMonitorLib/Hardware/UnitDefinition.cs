using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenHardwareMonitor.Hardware
{
    public sealed class UnitDefinition
    {
        private static List<UnitDefinition> _siUnits;

        static UnitDefinition()
        {
            _siUnits = new List<UnitDefinition>();
            _siUnits.Add(new UnitDefinition("V", "Volts", "ElectricPotential"));
            _siUnits.Add(new UnitDefinition("C", "Degrees Celsius", "Temperature"));
            _siUnits.Add(new UnitDefinition("°C", "Degrees Celsius", "Temperature"));
            _siUnits.Add(new UnitDefinition("°F", "Degrees Fahrenheit", "Temperature"));
            _siUnits.Add(new UnitDefinition("F", "Degrees Fahrenheit", "Temperature"));
            _siUnits.Add(new UnitDefinition("MHz", "Megahertz", "Frequency"));
            _siUnits.Add(new UnitDefinition("KHz", "Kilohertz", "Frequency"));
            _siUnits.Add(new UnitDefinition("Hz", "Hertz", "Frequency"));
            _siUnits.Add(new UnitDefinition("%", "Percent", "Ratio"));
            _siUnits.Add(new UnitDefinition("RPM", "Rotations Per Minute", "Rotation Speed"));
            _siUnits.Add(new UnitDefinition("L/h", "Liters per Hour", "Flow Rate"));
            _siUnits.Add(new UnitDefinition("W", "Watts", "Power"));
            _siUnits.Add(new UnitDefinition("GB", "Gigabytes", "Data"));
            _siUnits.Add(new UnitDefinition("GB/s", "Gigabytes per Second", "Data Rate"));
            _siUnits.Add(new UnitDefinition("MB/s", "Megabytes per Second", "Data Rate"));
            _siUnits.Add(new UnitDefinition("KB/s", "Kilobytes per Second", "Data Rate"));
            _siUnits.Add(new UnitDefinition(string.Empty, string.Empty, "1")); // Elements without an unit have the dimension "1"
        }

        private UnitDefinition(string abbreviation, string fullname, string dimension)
        {
            Abbreviation = abbreviation;
            Fullname = fullname;
            Dimension = dimension;
        }

        /// <summary>
        /// Returns an advanced description for units used
        /// </summary>
        public static IList<UnitDefinition> CommonUnits => new List<UnitDefinition>(_siUnits);

        public string Abbreviation { get; }
        public string Fullname { get; }
        public string Dimension { get; }

    }
}
