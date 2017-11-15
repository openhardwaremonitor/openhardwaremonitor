/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
  Copyright (C) 2009-2014 Michael Möller <mmoeller@openhardwaremonitor.org>
	
*/

using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor
{
    public class PersistentSettings : ISettings
    {
        private readonly IDictionary<string, string> settings =
            new Dictionary<string, string>();

        public bool Contains(string name)
        {
            return settings.ContainsKey(name);
        }

        public void SetValue(string name, string value)
        {
            settings[name] = value;
        }

        public string GetValue(string name, string value)
        {
            string result;
            if (settings.TryGetValue(name, out result))
                return result;
            return value;
        }

        public void Remove(string name)
        {
            settings.Remove(name);
        }

        public void Load(string fileName)
        {
            var doc = new XmlDocument();
            try
            {
                doc.Load(fileName);
            }
            catch
            {
                try
                {
                    File.Delete(fileName);
                }
                catch
                {
                }

                var backupFileName = fileName + ".backup";
                try
                {
                    doc.Load(backupFileName);
                }
                catch
                {
                    try
                    {
                        File.Delete(backupFileName);
                    }
                    catch
                    {
                    }

                    return;
                }
            }

            var list = doc.GetElementsByTagName("appSettings");
            foreach (XmlNode node in list)
            {
                var parent = node.ParentNode;
                if (parent != null && parent.Name == "configuration" &&
                    parent.ParentNode is XmlDocument)
                    foreach (XmlNode child in node.ChildNodes)
                        if (child.Name == "add")
                        {
                            var attributes = child.Attributes;
                            var keyAttribute = attributes["key"];
                            var valueAttribute = attributes["value"];
                            if (keyAttribute != null && valueAttribute != null &&
                                keyAttribute.Value != null) settings.Add(keyAttribute.Value, valueAttribute.Value);
                        }
            }
        }

        public void Save(string fileName)
        {
            var doc = new XmlDocument();
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "utf-8", null));
            var configuration = doc.CreateElement("configuration");
            doc.AppendChild(configuration);
            var appSettings = doc.CreateElement("appSettings");
            configuration.AppendChild(appSettings);
            foreach (var keyValuePair in settings)
            {
                var add = doc.CreateElement("add");
                add.SetAttribute("key", keyValuePair.Key);
                add.SetAttribute("value", keyValuePair.Value);
                appSettings.AppendChild(add);
            }

            byte[] file;
            using (var memory = new MemoryStream())
            {
                using (var writer = new StreamWriter(memory, Encoding.UTF8))
                {
                    doc.Save(writer);
                }
                file = memory.ToArray();
            }

            var backupFileName = fileName + ".backup";
            if (File.Exists(fileName))
            {
                try
                {
                    File.Delete(backupFileName);
                }
                catch
                {
                }
                try
                {
                    File.Move(fileName, backupFileName);
                }
                catch
                {
                }
            }

            using (var stream = new FileStream(fileName,
                FileMode.Create, FileAccess.Write))
            {
                stream.Write(file, 0, file.Length);
            }

            try
            {
                File.Delete(backupFileName);
            }
            catch
            {
            }
        }

        public void SetValue(string name, int value)
        {
            settings[name] = value.ToString();
        }

        public int GetValue(string name, int value)
        {
            string str;
            if (settings.TryGetValue(name, out str))
            {
                int parsedValue;
                if (int.TryParse(str, out parsedValue))
                    return parsedValue;
                return value;
            }
            return value;
        }

        public void SetValue(string name, float value)
        {
            settings[name] = value.ToString(CultureInfo.InvariantCulture);
        }

        public float GetValue(string name, float value)
        {
            string str;
            if (settings.TryGetValue(name, out str))
            {
                float parsedValue;
                if (float.TryParse(str, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out parsedValue))
                    return parsedValue;
                return value;
            }
            return value;
        }

        public void SetValue(string name, bool value)
        {
            settings[name] = value ? "true" : "false";
        }

        public bool GetValue(string name, bool value)
        {
            string str;
            if (settings.TryGetValue(name, out str)) return str == "true";
            return value;
        }

        public void SetValue(string name, Color color)
        {
            settings[name] = color.ToArgb().ToString("X8");
        }

        public Color GetValue(string name, Color value)
        {
            string str;
            if (settings.TryGetValue(name, out str))
            {
                int parsedValue;
                if (int.TryParse(str, NumberStyles.HexNumber,
                    CultureInfo.InvariantCulture, out parsedValue))
                    return Color.FromArgb(parsedValue);
                return value;
            }
            return value;
        }
    }
}