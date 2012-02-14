/*
  
  Version: MPL 1.1/GPL 2.0/LGPL 2.1

  The contents of this file are subject to the Mozilla Public License Version
  1.1 (the "License"); you may not use this file except in compliance with
  the License. You may obtain a copy of the License at
 
  http://www.mozilla.org/MPL/

  Software distributed under the License is distributed on an "AS IS" basis,
  WITHOUT WARRANTY OF ANY KIND, either express or implied. See the License
  for the specific language governing rights and limitations under the License.

  The Original Code is the Open Hardware Monitor code.

  The Initial Developer of the Original Code is 
  Michael Möller <m.moeller@gmx.ch>.
  Portions created by the Initial Developer are Copyright (C) 2011-2012
  the Initial Developer. All Rights Reserved.

  Contributor(s):
    Roland Reinl <roland-reinl@gmx.de>
 
  Alternatively, the contents of this file may be used under the terms of
  either the GNU General Public License Version 2 or later (the "GPL"), or
  the GNU Lesser General Public License Version 2.1 or later (the "LGPL"),
  in which case the provisions of the GPL or the LGPL are applicable instead
  of those above. If you wish to allow use of your version of this file only
  under the terms of either the GPL or the LGPL, and not to allow others to
  use your version of this file under the terms of the MPL, indicate your
  decision by deleting the provisions above and replace them with the notice
  and other provisions required by the GPL or the LGPL. If you do not delete
  the provisions above, a recipient may use your version of this file under
  the terms of any one of the MPL, the GPL or the LGPL.
 
*/

using System;
using System.Collections.Generic;

namespace OpenHardwareMonitor.Hardware.HDD {
  internal class SmartAttribute {

    private RawValueConversion rawValueConversion;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartAttribute"/> class.
    /// </summary>
    /// <param name="identifier">The SMART identifier of the attribute.</param>
    /// <param name="name">The name of the attribute.</param>
    public SmartAttribute(byte identifier, string name) : 
      this(identifier, name, null, null, 0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartAttribute"/> class.
    /// </summary>
    /// <param name="identifier">The SMART identifier of the attribute.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="rawValueConversion">A delegate for converting the raw byte 
    /// array into a value (or null to use the attribute value).</param>
    public SmartAttribute(byte identifier, string name,
      RawValueConversion rawValueConversion) :
      this(identifier, name, rawValueConversion, null, 0) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmartAttribute"/> class.
    /// </summary>
    /// <param name="identifier">The SMART identifier of the attribute.</param>
    /// <param name="name">The name of the attribute.</param>
    /// <param name="rawValueConversion">A delegate for converting the raw byte 
    /// array into a value (or null to use the attribute value).</param>
    /// <param name="sensorType">Type of the sensor or null if no sensor is to 
    /// be created.</param>
    /// <param name="sensorChannel">If there exists more than one attribute with 
    /// the same sensor channel and type, then a sensor is created only for the  
    /// first attribute.</param>
    /// <param name="defaultHiddenSensor">True to hide the sensor initially.</param>
    public SmartAttribute(byte identifier, string name,
      RawValueConversion rawValueConversion, SensorType? sensorType, 
      int sensorChannel, bool defaultHiddenSensor = false) 
    {
      this.Identifier = identifier;
      this.Name = name;
      this.rawValueConversion = rawValueConversion;
      this.SensorType = sensorType;
      this.SensorChannel = sensorChannel;
      this.DefaultHiddenSensor = defaultHiddenSensor;
    }

    /// <summary>
    /// Gets the SMART identifier.
    /// </summary>
    public byte Identifier { get; private set; }

    public string Name { get; private set; }

    public SensorType? SensorType { get; private set; }

    public int SensorChannel { get; private set; }

    public bool DefaultHiddenSensor { get; private set; }

    public bool HasRawValueConversion {
      get {
        return rawValueConversion != null;
      }
    }

    public float ConvertValue(DriveAttributeValue value) {
      if (rawValueConversion == null) {
        return value.AttrValue;
      } else {
        return rawValueConversion(value.RawValue, value.AttrValue);
      }
    }

    public delegate float RawValueConversion(byte[] rawValue, byte value);
  }
}
