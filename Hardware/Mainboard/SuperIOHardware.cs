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
using System.Globalization;
using OpenHardwareMonitor.Hardware.LPC;

namespace OpenHardwareMonitor.Hardware.Mainboard {
  internal class SuperIOHardware : Hardware {

    private ISuperIO superIO;
    private string name;

    private List<Sensor> voltages = new List<Sensor>();
    private List<Sensor> temperatures = new List<Sensor>();
    private List<Sensor> fans = new List<Sensor>();


    public SuperIOHardware(ISuperIO superIO, Manufacturer manufacturer,
      Model model, ISettings settings) 
    {
      this.superIO = superIO;
      this.name = ChipName.GetName(superIO.Chip);

      List<Voltage> v = new List<Voltage>();
      List<Temperature> t = new List<Temperature>();
      List<Fan> f = new List<Fan>();

      switch (superIO.Chip) {
        case Chip.IT8712F:
        case Chip.IT8716F:
        case Chip.IT8718F:
        case Chip.IT8720F:
        case Chip.IT8726F:
          switch (manufacturer) {
            case Manufacturer.ASUS:
              switch (model) {
                case Model.Crosshair_III_Formula: // IT8720F
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("CPU", 0));
                  for (int i = 0; i < superIO.Fans.Length; i++)
                    f.Add(new Fan("Fan #" + (i + 1), i));
                  break;
                case Model.M2N_SLI_DELUXE:                
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("+3.3V", 1));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 4, 30, 10, 0));
                  v.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Motherboard", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("Chassis Fan #1", 1));
                  f.Add(new Fan("Power Fan", 2));
                  break;
                case Model.M4A79XTD_EVO: // IT8720F           
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Motherboard", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("Chassis Fan #1", 1));
                  f.Add(new Fan("Chassis Fan #2", 2));
                  break;
                default:
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("Voltage #2", 1, true));
                  v.Add(new Voltage("Voltage #3", 2, true));
                  v.Add(new Voltage("Voltage #4", 3, true));
                  v.Add(new Voltage("Voltage #5", 4, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("Voltage #8", 7, true));
                  v.Add(new Voltage("VBat", 8));
                  for (int i = 0; i < superIO.Temperatures.Length; i++)
                    t.Add(new Temperature("Temperature #" + (i + 1), i));
                  for (int i = 0; i < superIO.Fans.Length; i++)
                    f.Add(new Fan("Fan #" + (i + 1), i));
                  break;
              }
              break;
            case Manufacturer.DFI:
              switch (model) {
                case Model.LP_BI_P45_T2RS_Elite: // IT8718F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("FSB VTT", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 4, 30, 10, 0));
                  v.Add(new Voltage("NB Core", 5));
                  v.Add(new Voltage("VDIMM", 6));
                  v.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("System", 1));
                  t.Add(new Temperature("Chipset", 2));
                  f.Add(new Fan("Fan #1", 0));
                  f.Add(new Fan("Fan #2", 1));
                  f.Add(new Fan("Fan #3", 2));
                  break;
                case Model.LP_DK_P55_T3eH9: // IT8720F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("VTT", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 4, 30, 10, 0));
                  v.Add(new Voltage("CPU PLL", 5));
                  v.Add(new Voltage("DRAM", 6));
                  v.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("Chipset", 0));
                  t.Add(new Temperature("CPU PWM", 1));
                  t.Add(new Temperature("CPU", 2));
                  f.Add(new Fan("Fan #1", 0));
                  f.Add(new Fan("Fan #2", 1));
                  f.Add(new Fan("Fan #3", 2));
                  break;
                default:
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("VTT", 1, true));
                  v.Add(new Voltage("+3.3V", 2, true));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0, true));
                  v.Add(new Voltage("+12V", 4, 30, 10, 0, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("DRAM", 6, true));
                  v.Add(new Voltage("+5VSB", 7, 6.8f, 10, 0, true));
                  v.Add(new Voltage("VBat", 8));
                  for (int i = 0; i < superIO.Temperatures.Length; i++)
                    t.Add(new Temperature("Temperature #" + (i + 1), i));
                  for (int i = 0; i < superIO.Fans.Length; i++)
                    f.Add(new Fan("Fan #" + (i + 1), i));
                  break;
              }
              break;

            case Manufacturer.Gigabyte:
              switch (model) {
                case Model._965P_S3: // IT8718F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 7, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan", 1));
                  break;
                case Model.EP45_DS3R: // IT8718F
                case Model.EP45_UD3R: 
                case Model.X38_DS5:    
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 7, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #2", 1));
                  f.Add(new Fan("Power Fan", 2));
                  f.Add(new Fan("System Fan #1", 3));
                  break;
                case Model.EX58_EXTREME: // IT8720F 
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  t.Add(new Temperature("Northbridge", 2));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #2", 1));
                  f.Add(new Fan("Power Fan", 2));
                  f.Add(new Fan("System Fan #1", 3));
                  break;
                case Model.P35_DS3: // IT8718F 
                case Model.P35_DS3L: // IT8718F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 7, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #1", 1));
                  f.Add(new Fan("System Fan #2", 2));
                  f.Add(new Fan("Power Fan", 3));
                  break;
                case Model.P55_UD4: // IT8720F
                case Model.P55M_UD4: // IT8720F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 5, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 2));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #2", 1));
                  f.Add(new Fan("Power Fan", 2));
                  f.Add(new Fan("System Fan #1", 3));
                  break;
                case Model.GA_MA770T_UD3: // IT8720F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 4, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #1", 1));
                  f.Add(new Fan("System Fan #2", 2));
                  f.Add(new Fan("Power Fan", 3));
                  break;
                case Model.GA_MA785GMT_UD2H: // IT8718F
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 4, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan", 1));
                  f.Add(new Fan("NB Fan", 2));
                  break;
                case Model.X58A_UD3R: // IT8720F 
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1));
                  v.Add(new Voltage("+3.3V", 2));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0));
                  v.Add(new Voltage("+12V", 5, 27, 9.1f, 0));
                  v.Add(new Voltage("VBat", 8));
                  t.Add(new Temperature("System", 0));
                  t.Add(new Temperature("CPU", 1));
                  t.Add(new Temperature("Northbridge", 2));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("System Fan #2", 1));
                  f.Add(new Fan("Power Fan", 2));
                  f.Add(new Fan("System Fan #1", 3));
                  break;
                default:
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("DRAM", 1, true));
                  v.Add(new Voltage("+3.3V", 2, true));
                  v.Add(new Voltage("+5V", 3, 6.8f, 10, 0, true));
                  v.Add(new Voltage("Voltage #5", 4, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("+12V", 7, 27, 9.1f, 0, true));
                  v.Add(new Voltage("VBat", 8));
                  for (int i = 0; i < superIO.Temperatures.Length; i++)
                    t.Add(new Temperature("Temperature #" + (i + 1), i));
                  for (int i = 0; i < superIO.Fans.Length; i++)
                    f.Add(new Fan("Fan #" + (i + 1), i));
                  break;
              }
              break;

            default:
              v.Add(new Voltage("CPU VCore", 0));
              v.Add(new Voltage("Voltage #2", 1, true));
              v.Add(new Voltage("Voltage #3", 2, true));
              v.Add(new Voltage("Voltage #4", 3, true));
              v.Add(new Voltage("Voltage #5", 4, true));
              v.Add(new Voltage("Voltage #6", 5, true));
              v.Add(new Voltage("Voltage #7", 6, true));
              v.Add(new Voltage("Voltage #8", 7, true));
              v.Add(new Voltage("VBat", 8));
              for (int i = 0; i < superIO.Temperatures.Length; i++)
                t.Add(new Temperature("Temperature #" + (i + 1), i));
              for (int i = 0; i < superIO.Fans.Length; i++)
                f.Add(new Fan("Fan #" + (i + 1), i));
              break;
          }
          break;
          
        case Chip.F71858:
          v.Add(new Voltage("VCC3V", 0, 150, 150, 0));
          v.Add(new Voltage("VSB3V", 1, 150, 150, 0));
          v.Add(new Voltage("Battery", 2, 150, 150, 0));
          for (int i = 0; i < superIO.Temperatures.Length; i++)
            t.Add(new Temperature("Temperature #" + (i + 1), i));
          for (int i = 0; i < superIO.Fans.Length; i++)
            f.Add(new Fan("Fan #" + (i + 1), i));
          break;
        case Chip.F71862: 
        case Chip.F71869: 
        case Chip.F71882:
        case Chip.F71889ED: 
        case Chip.F71889F:
          switch (manufacturer) {
            case Manufacturer.EVGA:
              switch (model) {
                case Model.X58_SLI_Classified: // F71882 
                  v.Add(new Voltage("VCC3V", 0, 150, 150, 0));
                  v.Add(new Voltage("CPU VCore", 1, 47, 100, 0));
                  v.Add(new Voltage("DIMM", 2, 47, 100, 0));
                  v.Add(new Voltage("CPU VTT", 3, 24, 100, 0));
                  v.Add(new Voltage("IOH Vcore", 4, 24, 100, 0));
                  v.Add(new Voltage("+5V", 5, 51, 12, 0));
                  v.Add(new Voltage("+12V", 6, 56, 6.8f, 0));
                  v.Add(new Voltage("3VSB", 7, 150, 150, 0));
                  v.Add(new Voltage("VBat", 8, 150, 150, 0));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("VREG", 1));
                  t.Add(new Temperature("System", 2));
                  f.Add(new Fan("CPU Fan", 0));
                  f.Add(new Fan("Power Fan", 1));
                  f.Add(new Fan("Chassis Fan", 2));
                  break;
                default:
                  v.Add(new Voltage("VCC3V", 0, 150, 150, 0));
                  v.Add(new Voltage("CPU VCore", 1));
                  v.Add(new Voltage("Voltage #3", 2, true));
                  v.Add(new Voltage("Voltage #4", 3, true));
                  v.Add(new Voltage("Voltage #5", 4, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("VSB3V", 7, 150, 150, 0));
                  v.Add(new Voltage("VBat", 8, 150, 150, 0));
                  for (int i = 0; i < superIO.Temperatures.Length; i++)
                    t.Add(new Temperature("Temperature #" + (i + 1), i));
                  for (int i = 0; i < superIO.Fans.Length; i++)
                    f.Add(new Fan("Fan #" + (i + 1), i));
                  break;
              }
              break;
            default:
              v.Add(new Voltage("VCC3V", 0, 150, 150, 0));
              v.Add(new Voltage("CPU VCore", 1));
              v.Add(new Voltage("Voltage #3", 2, true));
              v.Add(new Voltage("Voltage #4", 3, true));
              v.Add(new Voltage("Voltage #5", 4, true));
              v.Add(new Voltage("Voltage #6", 5, true));
              v.Add(new Voltage("Voltage #7", 6, true));
              v.Add(new Voltage("VSB3V", 7, 150, 150, 0));
              v.Add(new Voltage("VBat", 8, 150, 150, 0));
              for (int i = 0; i < superIO.Temperatures.Length; i++)
                t.Add(new Temperature("Temperature #" + (i + 1), i));
              for (int i = 0; i < superIO.Fans.Length; i++)
                f.Add(new Fan("Fan #" + (i + 1), i));
              break;
          }
          break;

        case Chip.W83627EHF:          
          v.Add(new Voltage("CPU VCore", 0));
          v.Add(new Voltage("Voltage #2", 1, true));
          v.Add(new Voltage("AVCC", 2, 34, 34, 0));
          v.Add(new Voltage("3VCC", 3, 34, 34, 0));
          v.Add(new Voltage("Voltage #5", 4, true));
          v.Add(new Voltage("Voltage #6", 5, true));
          v.Add(new Voltage("Voltage #7", 6, true));
          v.Add(new Voltage("3VSB", 7, 34, 34, 0));
          v.Add(new Voltage("VBAT", 8, 34, 34, 0));
          v.Add(new Voltage("Voltage #10", 9, true));
          t.Add(new Temperature("CPU", 0));
          t.Add(new Temperature("Auxiliary", 1));
          t.Add(new Temperature("System", 2));
          f.Add(new Fan("System Fan", 0));
          f.Add(new Fan("CPU Fan", 1));
          f.Add(new Fan("Auxiliary Fan", 2));
          f.Add(new Fan("CPU Fan #2", 3));
          f.Add(new Fan("Auxiliary Fan #2", 4));
          break;
        case Chip.W83627DHG: 
        case Chip.W83627DHGP:                      
        case Chip.W83667HG:
        case Chip.W83667HGB:
          switch (manufacturer) {
            case Manufacturer.ASRock:
              switch (model) {
                case Model._880GMH_USB3: // W83627DHG-P
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("+3.3V", 3, 34, 34, 0));
                  v.Add(new Voltage("+5V", 5, 15, 7.5f, 0));
                  v.Add(new Voltage("+12V", 6, 56, 10, 0));
                  v.Add(new Voltage("Standby +3.3V", 7, 34, 34, 0));
                  v.Add(new Voltage("VBAT", 8, 34, 34, 0));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Motherboard", 2));
                  f.Add(new Fan("Chassis Fan", 0));
                  f.Add(new Fan("CPU Fan", 1));
                  f.Add(new Fan("Power Fan", 2));
                  break;
                default:
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("Voltage #2", 1, true));
                  v.Add(new Voltage("AVCC", 2, 34, 34, 0));
                  v.Add(new Voltage("3VCC", 3, 34, 34, 0));
                  v.Add(new Voltage("Voltage #5", 4, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("3VSB", 7, 34, 34, 0));
                  v.Add(new Voltage("VBAT", 8, 34, 34, 0));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Auxiliary", 1));
                  t.Add(new Temperature("System", 2));
                  f.Add(new Fan("System Fan", 0));
                  f.Add(new Fan("CPU Fan", 1));
                  f.Add(new Fan("Auxiliary Fan", 2));
                  f.Add(new Fan("CPU Fan #2", 3));
                  f.Add(new Fan("Auxiliary Fan #2", 4));
                  break;
              }
              break;
            case Manufacturer.ASUS:
              switch (model) {
                case Model.P6X58D_E: // W83667HG 
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("+12V", 1, 11.5f, 1.91f, 0));
                  v.Add(new Voltage("Analog +3.3V", 2, 34, 34, 0));
                  v.Add(new Voltage("+3.3V", 3, 34, 34, 0));
                  v.Add(new Voltage("+5V", 4, 15, 7.5f, 0));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("Standby +3.3V", 7, 34, 34, 0));
                  v.Add(new Voltage("VBAT", 8, 34, 34, 0));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Motherboard", 2));
                  f.Add(new Fan("Chassis Fan #1", 0));
                  f.Add(new Fan("CPU Fan", 1));
                  f.Add(new Fan("Power Fan", 2));
                  f.Add(new Fan("Chassis Fan #2", 3));
                  f.Add(new Fan("Chassis Fan #3", 4));
                  break;
                default:
                  v.Add(new Voltage("CPU VCore", 0));
                  v.Add(new Voltage("Voltage #2", 1, true));
                  v.Add(new Voltage("AVCC", 2, 34, 34, 0));
                  v.Add(new Voltage("3VCC", 3, 34, 34, 0));
                  v.Add(new Voltage("Voltage #5", 4, true));
                  v.Add(new Voltage("Voltage #6", 5, true));
                  v.Add(new Voltage("Voltage #7", 6, true));
                  v.Add(new Voltage("3VSB", 7, 34, 34, 0));
                  v.Add(new Voltage("VBAT", 8, 34, 34, 0));
                  t.Add(new Temperature("CPU", 0));
                  t.Add(new Temperature("Auxiliary", 1));
                  t.Add(new Temperature("System", 2));
                  f.Add(new Fan("System Fan", 0));
                  f.Add(new Fan("CPU Fan", 1));
                  f.Add(new Fan("Auxiliary Fan", 2));
                  f.Add(new Fan("CPU Fan #2", 3));
                  f.Add(new Fan("Auxiliary Fan #2", 4));
                  break;
              }
              break;
            default:
              v.Add(new Voltage("CPU VCore", 0));
              v.Add(new Voltage("Voltage #2", 1, true));
              v.Add(new Voltage("AVCC", 2, 34, 34, 0));
              v.Add(new Voltage("3VCC", 3, 34, 34, 0));
              v.Add(new Voltage("Voltage #5", 4, true));
              v.Add(new Voltage("Voltage #6", 5, true));
              v.Add(new Voltage("Voltage #7", 6, true));
              v.Add(new Voltage("3VSB", 7, 34, 34, 0));
              v.Add(new Voltage("VBAT", 8, 34, 34, 0));
              t.Add(new Temperature("CPU", 0));
              t.Add(new Temperature("Auxiliary", 1));
              t.Add(new Temperature("System", 2));
              f.Add(new Fan("System Fan", 0));
              f.Add(new Fan("CPU Fan", 1));
              f.Add(new Fan("Auxiliary Fan", 2));
              f.Add(new Fan("CPU Fan #2", 3));
              f.Add(new Fan("Auxiliary Fan #2", 4));
              break;
          } 
          break;
        case Chip.W83627HF: 
        case Chip.W83627THF: 
        case Chip.W83687THF:
          v.Add(new Voltage("CPU VCore", 0));
          v.Add(new Voltage("Voltage #2", 1, true));
          v.Add(new Voltage("Voltage #3", 2, true));
          v.Add(new Voltage("AVCC", 3, 34, 51, 0));
          v.Add(new Voltage("Voltage #5", 4, true));
          v.Add(new Voltage("5VSB", 5, 34, 51, 0));
          v.Add(new Voltage("VBAT", 6));
          t.Add(new Temperature("CPU", 0));
          t.Add(new Temperature("Auxiliary", 1));
          t.Add(new Temperature("System", 2));
          f.Add(new Fan("System Fan", 0));
          f.Add(new Fan("CPU Fan", 1));
          f.Add(new Fan("Auxiliary Fan", 2));
          break;
        default:
          for (int i = 0; i < superIO.Voltages.Length; i++)
            v.Add(new Voltage("Voltage #" + (i + 1), i, true));
          for (int i = 0; i < superIO.Temperatures.Length; i++)
            t.Add(new Temperature("Temperature #" + (i + 1), i));
          for (int i = 0; i < superIO.Fans.Length; i++)
            f.Add(new Fan("Fan #" + (i + 1), i));
          break;
      }

      string formula = "Voltage = value + (value - Vf) * Ri / Rf.";
      foreach (Voltage voltage in v) 
        if (voltage.Index < superIO.Voltages.Length) {
          Sensor sensor = new Sensor(voltage.Name, voltage.Index, 
            voltage.Hidden, SensorType.Voltage, this, 
            new ParameterDescription[] {
            new ParameterDescription("Ri [kΩ]", "Input resistance.\n" + 
              formula, voltage.Ri),
            new ParameterDescription("Rf [kΩ]", "Reference resistance.\n" + 
              formula, voltage.Rf),
            new ParameterDescription("Vf [V]", "Reference voltage.\n" + 
              formula, voltage.Vf)
            }, settings);
          voltages.Add(sensor);
      }

      foreach (Temperature temperature in t) 
        if (temperature.Index < superIO.Temperatures.Length) {
        Sensor sensor = new Sensor(temperature.Name, temperature.Index,
          SensorType.Temperature, this, new ParameterDescription[] {
          new ParameterDescription("Offset [°C]", "Temperature offset.", 0)
        }, settings);
        temperatures.Add(sensor);
      }

      foreach (Fan fan in f)
        if (fan.Index < superIO.Fans.Length) {
          Sensor sensor = new Sensor(fan.Name, fan.Index, SensorType.Fan,
            this, settings);
          fans.Add(sensor);
        }
    }

    public override Identifier Identifier {
      get { 
        return new Identifier("lpc", 
          superIO.Chip.ToString().ToLower(CultureInfo.InvariantCulture)); 
      }
    }

    public override HardwareType HardwareType {
      get { return HardwareType.SuperIO; }
    }

    public override string Name {
      get { return name; }
    }

    public override string GetReport() {
      return superIO.GetReport();
    }

    public override void Update() {
      superIO.Update();

      foreach (Sensor sensor in voltages) {
        float? value = superIO.Voltages[sensor.Index];
        if (value.HasValue) {
          sensor.Value = value + (value - sensor.Parameters[2].Value) *
            sensor.Parameters[0].Value / sensor.Parameters[1].Value;
          ActivateSensor(sensor);
        }
      }

      foreach (Sensor sensor in temperatures) {
        float? value = superIO.Temperatures[sensor.Index];
        if (value.HasValue) {
          sensor.Value = value + sensor.Parameters[0].Value;
          ActivateSensor(sensor);
        }
      }

      foreach (Sensor sensor in fans) {
        float? value = superIO.Fans[sensor.Index];
        if (value.HasValue) {
          sensor.Value = value;
          if (value.Value > 0)
            ActivateSensor(sensor);
        }
      }
    }

    private class Voltage {
      public readonly string Name;
      public readonly int Index;
      public readonly float Ri;
      public readonly float Rf;
      public readonly float Vf;
      public readonly bool Hidden;

      public Voltage(string name, int index) :
        this(name, index, 0, 1, 0, false) { }

      public Voltage(string name, int index, bool hidden) :
        this(name, index, 0, 1, 0, hidden) { }

      public Voltage(string name, int index, float ri, float rf, float vf) :
        this(name, index, ri, rf, vf, false) { }

      public Voltage(string name, int index, float ri, float rf, float vf,
        bool hidden) {
        this.Name = name;
        this.Index = index;
        this.Ri = ri;
        this.Rf = rf;
        this.Vf = vf;
        this.Hidden = hidden;
      }
    }

    private class Temperature {
      public readonly string Name;
      public readonly int Index;

      public Temperature(string name, int index) {
        this.Name = name;
        this.Index = index;
      }
    }

    private class Fan {
      public readonly string Name;
      public readonly int Index;

      public Fan(string name, int index) {
        this.Name = name;
        this.Index = index;
      }
    }
  }
}
