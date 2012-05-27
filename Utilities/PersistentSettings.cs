/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2010 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using System.Xml;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor {
  public class PersistentSettings : ISettings {

    private IDictionary<string, string> settings = 
      new Dictionary<string, string>();

    public void Load(string fileName) {
      XmlDocument doc = new XmlDocument();
      try {
        doc.Load(fileName);
      } catch {
        return;
      }
      XmlNodeList list = doc.GetElementsByTagName("appSettings");
      foreach (XmlNode node in list) {
        XmlNode parent = node.ParentNode;
        if (parent != null && parent.Name == "configuration" && 
          parent.ParentNode is XmlDocument) {
          foreach (XmlNode child in node.ChildNodes) {
            if (child.Name == "add") {
              XmlAttributeCollection attributes = child.Attributes;
              XmlAttribute keyAttribute = attributes["key"];
              XmlAttribute valueAttribute = attributes["value"];
              if (keyAttribute != null && valueAttribute != null && 
                keyAttribute.Value != null) {
                settings.Add(keyAttribute.Value, valueAttribute.Value);
              }
            }
          }
        }
      }      
    }

    public void Save(string fileName) {
      XmlDocument doc = new XmlDocument();
      doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
      XmlElement configuration = doc.CreateElement("configuration");
      doc.AppendChild(configuration);
      XmlElement appSettings = doc.CreateElement("appSettings");
      configuration.AppendChild(appSettings);
      foreach (KeyValuePair<string, string> keyValuePair in settings) {
        XmlElement add = doc.CreateElement("add");
        add.SetAttribute("key", keyValuePair.Key);
        add.SetAttribute("value", keyValuePair.Value);
        appSettings.AppendChild(add);
      }
      doc.Save(fileName);
    }

    public bool Contains(string name) {
      return settings.ContainsKey(name);
    }

    public void SetValue(string name, string value) {
      settings[name] = value;
    }

    public string GetValue(string name, string value) {
      string result;
      if (settings.TryGetValue(name, out result))
        return result;
      else
        return value;
    }

    public void Remove(string name) {
      settings.Remove(name);
    }

    public void SetValue(string name, int value) {
      settings[name] = value.ToString();
    }

    public int GetValue(string name, int value) {
      string str;
      if (settings.TryGetValue(name, out str)) {
        int parsedValue;
        if (int.TryParse(str, out parsedValue))
          return parsedValue;
        else
          return value;
      } else {
        return value;
      }
    }

    public void SetValue(string name, float value) {
      settings[name] = value.ToString(CultureInfo.InvariantCulture);
    }

    public float GetValue(string name, float value) {
      string str;
      if (settings.TryGetValue(name, out str)) {
        float parsedValue;
        if (float.TryParse(str, NumberStyles.Float, 
          CultureInfo.InvariantCulture, out parsedValue))
          return parsedValue;
        else
          return value;
      } else {
        return value;
      }
    }

    public void SetValue(string name, bool value) {
      settings[name] = value ? "true" : "false";
    }

    public bool GetValue(string name, bool value) {
      string str;
      if (settings.TryGetValue(name, out str)) {
        return str == "true";
      } else {
        return value;
      }
    }

    public void SetValue(string name, Color color) {
      settings[name] = color.ToArgb().ToString("X8");
    }

    public Color GetValue(string name, Color value) {
      string str;
      if (settings.TryGetValue(name, out str)) {
        int parsedValue;
        if (int.TryParse(str, NumberStyles.HexNumber,
          CultureInfo.InvariantCulture, out parsedValue))
          return Color.FromArgb(parsedValue);
        else
          return value;
      } else {
        return value;
      }
    }
  }
}
