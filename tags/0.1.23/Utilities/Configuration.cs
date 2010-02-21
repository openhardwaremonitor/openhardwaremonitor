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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace OpenHardwareMonitor.Utilities {

  public sealed class Config {
    private static readonly Config instance = new Config();

    private string fileName;

    private System.Configuration.Configuration config;

    private Config() {
      this.fileName = Path.ChangeExtension(
        System.Windows.Forms.Application.ExecutablePath, ".config");
      System.Configuration.ExeConfigurationFileMap fileMap = 
        new System.Configuration.ExeConfigurationFileMap();
      fileMap.ExeConfigFilename = fileName;        
      config = System.Configuration.ConfigurationManager.
        OpenMappedExeConfiguration(fileMap, 
        System.Configuration.ConfigurationUserLevel.None);
    }

    ~Config() {
      string tempName = Path.ChangeExtension(fileName, ".tmp");

      if (File.Exists(tempName))
        File.Delete(tempName);
      try {
        config.SaveAs(tempName);
        if (File.Exists(fileName) && File.Exists(tempName))
          File.Delete(fileName);
        File.Move(tempName, fileName);
      } catch (System.Configuration.ConfigurationErrorsException) { }
    }

    public static Config Settings {
      get {
        return instance;
      }
    }

    public string this[string name] {
      get {
        System.Configuration.KeyValueConfigurationElement element =
          config.AppSettings.Settings[name];
        if (element != null)
          return element.Value;
        else
          return null;
      }
      set {
        config.AppSettings.Settings.Remove(name);
        config.AppSettings.Settings.Add(name, value);
      }
    }

    public static bool Contains(string name) {
      System.Configuration.KeyValueConfigurationElement element =
        instance.config.AppSettings.Settings[name];
      return element != null;
    }

    public static void Remove(string name) {
      instance.config.AppSettings.Settings.Remove(name);
    }

    public static void Set(string name, bool value) {
      instance[name] = value ? "true" : "false";
    }

    public static bool Get(string name, bool value) {
      System.Configuration.KeyValueConfigurationElement element =
        instance.config.AppSettings.Settings[name];
      if (element == null)
        return value;
      else
        return element.Value == "true"; 
    }

    public static void Set(string name, int value) {
      instance[name] = value.ToString();
    }

    public static int Get(string name, int value) {
      System.Configuration.KeyValueConfigurationElement element =
        instance.config.AppSettings.Settings[name];
      if (element == null)
        return value;
      else {
        int parsedValue;
        if (int.TryParse(element.Value, out parsedValue))
          return parsedValue;
        else
          return value;
      }
    }

    public static void Set(string name, Color color) {
      instance[name] = color.ToArgb().ToString("X8");
    }

    public static Color Get(string name, Color value) {
      System.Configuration.KeyValueConfigurationElement element =
        instance.config.AppSettings.Settings[name];
      if (element == null)
        return value;
      else {
        int parsedValue;
        if (int.TryParse(element.Value, 
          System.Globalization.NumberStyles.HexNumber, 
          System.Globalization.CultureInfo.InvariantCulture, out parsedValue))
          return Color.FromArgb(parsedValue);
        else
          return value;
      }
    }
  }
}
