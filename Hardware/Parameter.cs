/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System;
using System.Globalization;

namespace OpenHardwareMonitor.Hardware
{
    internal struct ParameterDescription
    {
        public ParameterDescription(string name, string description,
            float defaultValue)
        {
            Name = name;
            Description = description;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        public string Description { get; }

        public float DefaultValue { get; }
    }

    internal class Parameter : IParameter
    {
        private readonly ISettings settings;
        private ParameterDescription description;
        private bool isDefault;
        private float value;

        public Parameter(ParameterDescription description, ISensor sensor,
            ISettings settings)
        {
            Sensor = sensor;
            this.description = description;
            this.settings = settings;
            isDefault = !settings.Contains(Identifier.ToString());
            value = description.DefaultValue;
            if (!isDefault)
                if (!float.TryParse(settings.GetValue(Identifier.ToString(), "0"),
                    NumberStyles.Float,
                    CultureInfo.InvariantCulture,
                    out value))
                    value = description.DefaultValue;
        }

        public ISensor Sensor { get; }

        public Identifier Identifier => new Identifier(Sensor.Identifier, "parameter",
            Name.Replace(" ", "").ToLowerInvariant());

        public string Name => description.Name;

        public string Description => description.Description;

        public float Value
        {
            get => value;
            set
            {
                isDefault = false;
                this.value = value;
                settings.SetValue(Identifier.ToString(), value.ToString(
                    CultureInfo.InvariantCulture));
            }
        }

        public float DefaultValue => description.DefaultValue;

        public bool IsDefault
        {
            get => isDefault;
            set
            {
                isDefault = value;
                if (value)
                {
                    this.value = description.DefaultValue;
                    settings.Remove(Identifier.ToString());
                }
            }
        }

        public void Accept(IVisitor visitor)
        {
            if (visitor == null)
                throw new ArgumentNullException(nameof(visitor));
            visitor.VisitParameter(this);
        }

        public void Traverse(IVisitor visitor)
        {
        }
    }
}