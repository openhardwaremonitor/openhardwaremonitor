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
  Portions created by the Initial Developer are Copyright (C) 2009-2010
  the Initial Developer. All Rights Reserved.

  Contributor(s):

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
