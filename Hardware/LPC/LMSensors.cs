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
using System.IO;

namespace OpenHardwareMonitor.Hardware.LPC {

  public class LMSensors {

    private List<LMChip> lmChips = new List<LMChip>();
    
    public LMSensors () {
      string[] devicePaths = Directory.GetDirectories("/sys/class/hwmon/");
      foreach (string path in devicePaths) {
        string name = null;
        try {
          StreamReader reader = new StreamReader(path + "/device/name");
          name = reader.ReadLine();
          reader.Close();
        } catch (IOException) { }
        switch (name) {
          case "f71858fg": 
            lmChips.Add(new LMChip(Chip.F71858, path + "/device")); break; 
          case "f71862fg": 
            lmChips.Add(new LMChip(Chip.F71862, path + "/device")); break; 
          case "f71882fg": 
            lmChips.Add(new LMChip(Chip.F71882, path + "/device")); break; 
          case "f71889fg": 
            lmChips.Add(new LMChip(Chip.F71889F, path + "/device")); break; 
        
          case "it8712": 
            lmChips.Add(new LMChip(Chip.IT8712F, path + "/device")); break; 
          case "it8716": 
            lmChips.Add(new LMChip(Chip.IT8716F, path + "/device")); break; 
          case "it8718": 
            lmChips.Add(new LMChip(Chip.IT8718F, path + "/device")); break; 
          case "it8720": 
            lmChips.Add(new LMChip(Chip.IT8720F, path + "/device")); break; 
          
          case "w83627ehf": 
            lmChips.Add(new LMChip(Chip.W83627EHF, path + "/device")); break;                               
          case "w83627dhg": 
            lmChips.Add(new LMChip(Chip.W83627DHG, path + "/device")); break;     
          case "w83667hg": 
            lmChips.Add(new LMChip(Chip.W83667HG, path + "/device")); break;               
          case "w83627hf": 
            lmChips.Add(new LMChip(Chip.W83627HF, path + "/device")); break;
          case "w83627thf": 
            lmChips.Add(new LMChip(Chip.W83627THF, path + "/device")); break;   
          case "w83687thf": 
            lmChips.Add(new LMChip(Chip.W83687THF, path + "/device")); break;             
        }
      }
    }
    
    public void Close() {
      foreach (LMChip lmChip in lmChips)
        lmChip.Close();
    }
    
    public ISuperIO[] SuperIO {
      get {
        return lmChips.ToArray();
      }
    }
    
    private class LMChip : ISuperIO {
      
      private string path;
      private Chip chip;
      
      private float?[] voltages;
      private float?[] temperatures ;
      private float?[] fans;
      
      private StreamReader[] voltageReaders;
      private StreamReader[] temperatureReaders;
      private StreamReader[] fanReaders;
      
      public Chip Chip { get { return chip; } }
      public float?[] Voltages { get { return voltages; } }
      public float?[] Temperatures { get { return temperatures; } }
      public float?[] Fans { get { return fans; } }
      
      
      public LMChip(Chip chip, string path) {
        this.path = path;
        this.chip = chip;
        
        string[] voltagePaths = Directory.GetFiles(path, "in*_input");        
        this.voltages = new float?[voltagePaths.Length];
        this.voltageReaders = new StreamReader[voltagePaths.Length];
        for (int i = 0; i < voltagePaths.Length; i++) 
         voltageReaders[i] = new StreamReader(voltagePaths[i]); 
        
        string[] temperaturePaths = Directory.GetFiles(path, "temp*_input");        
        this.temperatures = new float?[temperaturePaths.Length];
        this.temperatureReaders = new StreamReader[temperaturePaths.Length];
        for (int i = 0; i < temperaturePaths.Length; i++) 
         temperatureReaders[i] = new StreamReader(temperaturePaths[i]);    
        
        string[] fanPaths = Directory.GetFiles(path, "fan*_input");        
        this.fans = new float?[fanPaths.Length];
        this.fanReaders = new StreamReader[fanPaths.Length];
        for (int i = 0; i < fanPaths.Length; i++) 
         fanReaders[i] = new StreamReader(fanPaths[i]);    
      }
      
      public string GetReport() {
        return null;
      }
      
      public void Update() {
        for (int i = 0; i < voltages.Length; i++) {
          voltageReaders[i].BaseStream.Seek(0, SeekOrigin.Begin);
          string s = voltageReaders[i].ReadLine();  
          try {
            voltages[i] = 0.001f * long.Parse(s);
          } catch {
            voltages[i] = null;
          }
        }
        
        for (int i = 0; i < temperatures.Length; i++) {
          temperatureReaders[i].BaseStream.Seek(0, SeekOrigin.Begin);
          string s = temperatureReaders[i].ReadLine();  
          try {
            temperatures[i] = 0.001f * long.Parse(s);           
          } catch {
            temperatures[i] = null; 
          }
        }
        
        for (int i = 0; i < fans.Length; i++) {
          fanReaders[i].BaseStream.Seek(0, SeekOrigin.Begin);
          string s = fanReaders[i].ReadLine();  
          try {
            fans[i] = long.Parse(s);           
          } catch {
            fans[i] = null; 
          }
        }
      }
      
      public void Close() {
        foreach (StreamReader reader in voltageReaders)
          reader.Close();
        foreach (StreamReader reader in temperatureReaders)
          reader.Close();        
        foreach (StreamReader reader in fanReaders)
          reader.Close();
      }      
    }
  }
}
