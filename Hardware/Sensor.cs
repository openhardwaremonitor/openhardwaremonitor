/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2012 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using OpenHardwareMonitor.Collections;

namespace OpenHardwareMonitor.Hardware
{
    internal class Sensor : ISensor
    {
        private readonly string defaultName;
        private readonly Hardware hardware;
        private readonly ReadOnlyArray<IParameter> parameters;
        private readonly ISettings settings;

        private readonly RingCollection<SensorValue>
            values = new RingCollection<SensorValue>();

        private int count;
        private float? currentValue;
        private string name;

        private float sum;

        public Sensor(string name, int index, SensorType sensorType,
            Hardware hardware, ISettings settings) :
            this(name, index, sensorType, hardware, null, settings)
        {
        }

        public Sensor(string name, int index, SensorType sensorType,
            Hardware hardware, ParameterDescription[] parameterDescriptions,
            ISettings settings) :
            this(name, index, false, sensorType, hardware,
                parameterDescriptions, settings)
        {
        }

        public Sensor(string name, int index, bool defaultHidden,
            SensorType sensorType, Hardware hardware,
            ParameterDescription[] parameterDescriptions, ISettings settings)
        {
            Index = index;
            IsDefaultHidden = defaultHidden;
            SensorType = sensorType;
            this.hardware = hardware;
            var parameters = new Parameter[parameterDescriptions == null ? 0 : parameterDescriptions.Length];
            for (var i = 0; i < parameters.Length; i++)
                parameters[i] = new Parameter(parameterDescriptions[i], this, settings);
            this.parameters = parameters;

            this.settings = settings;
            defaultName = name;
            this.name = settings.GetValue(
                new Identifier(Identifier, "name").ToString(), name);

            GetSensorValuesFromSettings();

            hardware.Closing += delegate { SetSensorValuesToSettings(); };
        }

        public IHardware Hardware => hardware;

        public SensorType SensorType { get; }

        public Identifier Identifier => new Identifier(hardware.Identifier,
            SensorType.ToString().ToLowerInvariant(),
            Index.ToString(CultureInfo.InvariantCulture));

        public string Name
        {
            get => name;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    name = value;
                else
                    name = defaultName;
                settings.SetValue(new Identifier(Identifier, "name").ToString(), name);
            }
        }

        public int Index { get; }

        public bool IsDefaultHidden { get; }

        public IReadOnlyArray<IParameter> Parameters => parameters;

        public float? Value
        {
            get => currentValue;
            set
            {
                var now = DateTime.UtcNow;
                while (values.Count > 0 && (now - values.First.Time).TotalDays > 1)
                    values.Remove();

                if (value.HasValue)
                {
                    sum += value.Value;
                    count++;
                    if (count == 4)
                    {
                        AppendValue(sum / count, now);
                        sum = 0;
                        count = 0;
                    }
                }

                currentValue = value;
                if (Min > value || !Min.HasValue)
                    Min = value;
                if (Max < value || !Max.HasValue)
                    Max = value;
            }
        }

        public float? Min { get; private set; }

        public float? Max { get; private set; }

        public void ResetMin()
        {
            Min = null;
        }

        public void ResetMax()
        {
            Max = null;
        }

        public IEnumerable<SensorValue> Values => values;

        public void Accept(IVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));
            visitor.VisitSensor(this);
        }

        public void Traverse(IVisitor visitor)
        {
            foreach (var parameter in parameters)
                parameter.Accept(visitor);
        }

        public IControl Control { get; internal set; }

        private void SetSensorValuesToSettings()
        {
            using (var m = new MemoryStream())
            {
                using (var c = new GZipStream(m, CompressionMode.Compress))
                using (var b = new BufferedStream(c, 65536))
                using (var writer = new BinaryWriter(b))
                {
                    long t = 0;
                    foreach (var sensorValue in values)
                    {
                        var v = sensorValue.Time.ToBinary();
                        writer.Write(v - t);
                        t = v;
                        writer.Write(sensorValue.Value);
                    }
                    writer.Flush();
                }
                settings.SetValue(new Identifier(Identifier, "values").ToString(),
                    Convert.ToBase64String(m.ToArray()));
            }
        }

        private void GetSensorValuesFromSettings()
        {
            var name = new Identifier(Identifier, "values").ToString();
            var s = settings.GetValue(name, null);

            try
            {
                var array = Convert.FromBase64String(s);
                s = null;
                var now = DateTime.UtcNow;
                using (var m = new MemoryStream(array))
                using (var c = new GZipStream(m, CompressionMode.Decompress))
                using (var reader = new BinaryReader(c))
                {
                    try
                    {
                        long t = 0;
                        while (true)
                        {
                            t += reader.ReadInt64();
                            var time = DateTime.FromBinary(t);
                            if (time > now)
                                break;
                            var value = reader.ReadSingle();
                            AppendValue(value, time);
                        }
                    }
                    catch (EndOfStreamException)
                    {
                    }
                }
            }
            catch
            {
            }
            if (values.Count > 0)
                AppendValue(float.NaN, DateTime.UtcNow);

            // remove the value string from the settings to reduce memory usage
            settings.Remove(name);
        }

        private void AppendValue(float value, DateTime time)
        {
            if (values.Count >= 2 && values.Last.Value == value &&
                values[values.Count - 2].Value == value)
            {
                values.Last = new SensorValue(value, time);
                return;
            }

            values.Append(new SensorValue(value, time));
        }
    }
}