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
using System.Configuration;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Text;

namespace OpenHardwareMonitor.Hardware.TBalancer {
  public class TBalancer : IHardware {

    private string portName;
    private Image icon;
    private SerialPort serialPort;
    private byte protocolVersion;
    private Sensor[] digitalTemperatures = new Sensor[8];
    private Sensor[] analogTemperatures = new Sensor[4];
    private Sensor[] sensorhubTemperatures = new Sensor[6];
    private Sensor[] sensorhubFlows = new Sensor[2];
    private Sensor[] fans = new Sensor[4];
    private Sensor[] miniNGTemperatures = new Sensor[4];
    private Sensor[] miniNGFans = new Sensor[4];
    private List<ISensor> active = new List<ISensor>();
    private List<ISensor> deactivating = new List<ISensor>();
    private int[] primaryData = new int[0];
    private int[] alternativeData = new int[0];

    public const byte STARTFLAG = 100;
    public const byte ENDFLAG = 254;

    private delegate void MethodDelegate();
    private MethodDelegate alternativeRequest;    

    public TBalancer(string portName, byte protocolVersion) {
      this.portName = portName;
      this.icon = Utilities.EmbeddedResources.GetImage("bigng.png");
      this.protocolVersion = protocolVersion;

      ParameterDescription[] parameter = new ParameterDescription[] {
        new ParameterDescription("Offset", "Temperature offset.", 0)
      };
      int offset = 0;
      for (int i = 0; i < digitalTemperatures.Length; i++)
        digitalTemperatures[i] = new Sensor("Digital Sensor #" + (i + 1),
          offset + i, null, SensorType.Temperature, this, parameter);
      offset += digitalTemperatures.Length;

      for (int i = 0; i < analogTemperatures.Length; i++)
        analogTemperatures[i] = new Sensor("Analog Sensor #" + (i + 1),
          offset + i, null, SensorType.Temperature, this, parameter);
      offset += analogTemperatures.Length;

      for (int i = 0; i < sensorhubTemperatures.Length; i++)
        sensorhubTemperatures[i] = new Sensor("Sensorhub Sensor #" + (i + 1),
          offset + i, null, SensorType.Temperature, this, parameter);
      offset += sensorhubTemperatures.Length;

      for (int i = 0; i < sensorhubFlows.Length; i++)
        sensorhubFlows[i] = new Sensor("Flowmeter #" + (i + 1),
          offset + i, null, SensorType.Flow, this, new ParameterDescription[] {
            new ParameterDescription("Impulse Rate", 
              "The impulse rate of the flowmeter in pulses/L", 509)
          });
      offset += sensorhubFlows.Length;

      for (int i = 0; i < miniNGTemperatures.Length; i++)
        miniNGTemperatures[i] = new Sensor("miniNG #" + (i / 2 + 1) +
          " Sensor #" + (i % 2 + 1), offset + i, null, SensorType.Temperature, 
          this, parameter);
      offset += miniNGTemperatures.Length;

      alternativeRequest = new MethodDelegate(DelayedAlternativeRequest);

      try {
        serialPort = new SerialPort(portName, 19200, Parity.None, 8,
          StopBits.One);
        serialPort.Open();
        Update();
      } catch (IOException) { }      
    }

    private void ActivateSensor(Sensor sensor) {
      deactivating.Remove(sensor);
      if (!active.Contains(sensor)) {
        active.Add(sensor);
        if (SensorAdded != null)
          SensorAdded(sensor);
      }      
    }

    private void DeactivateSensor(Sensor sensor) {
      if (deactivating.Contains(sensor)) {
        active.Remove(sensor);
        deactivating.Remove(sensor);
        if (SensorRemoved != null)
          SensorRemoved(sensor);
      } else if (active.Contains(sensor)) {
        deactivating.Add(sensor);
      }     
    }

    private void ReadminiNG(int[] data, int number) {
      int offset = 1 + number * 65;

      if (data[offset + 61] != ENDFLAG)
        return;

      for (int i = 0; i < 2; i++) {
        Sensor sensor = miniNGTemperatures[number * 2 + i];
        if (data[offset + 7 + i] > 0) {
          sensor.Value = 0.5f * data[offset + 7 + i] + 
            sensor.Parameters[0].Value;
          ActivateSensor(sensor);
        } else {
          DeactivateSensor(sensor);
        }
      }

      for (int i = 0; i < 2; i++) {
        float maxRPM = 20.0f * data[offset + 44 + 2 * i];

        if (miniNGFans[number * 2 + i] == null)
          miniNGFans[number * 2 + i] = 
            new Sensor("miniNG #" + (number + 1) + " Fan #" + (i + 1),
            4 + number * 2 + i, maxRPM, SensorType.Fan, this);
        
        Sensor sensor = miniNGFans[number * 2 + i];

        sensor.Value = 20.0f * data[offset + 43 + 2 * i];
        ActivateSensor(sensor);
      }
    }

    private void ReadData() {
      int[] data = new int[285];
      for (int i = 0; i < data.Length; i++)
        data[i] = serialPort.ReadByte();

      if (data[0] != STARTFLAG) {
        serialPort.DiscardInBuffer();   
        return;
      }

      if (data[1] == 255) { // bigNG

        if (data[274] != protocolVersion) 
          return;

        this.primaryData = data;

        for (int i = 0; i < digitalTemperatures.Length; i++)
          if (data[238 + i] > 0) {
            digitalTemperatures[i].Value = 0.5f * data[238 + i] + 
              digitalTemperatures[i].Parameters[0].Value;
            ActivateSensor(digitalTemperatures[i]);
          } else {
            DeactivateSensor(digitalTemperatures[i]);
          }

        for (int i = 0; i < analogTemperatures.Length; i++)
          if (data[260 + i] > 0) {
            analogTemperatures[i].Value = 0.5f * data[260 + i] +
              analogTemperatures[i].Parameters[0].Value;
            ActivateSensor(analogTemperatures[i]);
          } else {
            DeactivateSensor(analogTemperatures[i]);
          }

        for (int i = 0; i < sensorhubTemperatures.Length; i++)
          if (data[246 + i] > 0) {
            sensorhubTemperatures[i].Value = 0.5f * data[246 + i] +
              sensorhubTemperatures[i].Parameters[0].Value;
            ActivateSensor(sensorhubTemperatures[i]);
          } else {
            DeactivateSensor(sensorhubTemperatures[i]);
          }

        for (int i = 0; i < sensorhubFlows.Length; i++)
          if (data[231 + i] > 0 && data[234] > 0) {
            float pulsesPerSecond = ((float)data[231 + i]) / data[234];
            float pulsesPerLiter = sensorhubFlows[i].Parameters[0].Value;
            sensorhubFlows[i].Value = pulsesPerSecond * 3600 / pulsesPerLiter;
            ActivateSensor(sensorhubFlows[i]);
          } else {
            DeactivateSensor(sensorhubFlows[i]);
          }

        for (int i = 0; i < fans.Length; i++) {
          float maxRPM = 11.5f * ((data[149 + 2 * i] << 8) | data[148 + 2 * i]);

          if (fans[i] == null)
            fans[i] = new Sensor("Fan #" + (i + 1), i, maxRPM, SensorType.Fan,
              this, new ParameterDescription[] {
                new ParameterDescription("MaxRPM", 
                  "Maximum revolutions per minute (RPM) of the fan.", maxRPM)
              });

          if ((data[136] & (1 << i)) == 0) // pwm mode
            fans[i].Value = fans[i].Parameters[0].Value * 0.01f * data[156 + i];
          else // analog mode
            fans[i].Value = fans[i].Parameters[0].Value * 0.01f * data[141 + i]; 
          ActivateSensor(fans[i]);
        }

      } else if (data[1] == 253) { // miniNG #1
        this.alternativeData = data;

        ReadminiNG(data, 0);        
              
        if (data[66] == 252)  // miniNG #2
          ReadminiNG(data, 1);
      } 
    }

    public Image Icon {
      get { return icon; }
    }

    public string Name {
      get { return "T-Balancer bigNG"; }
    }

    public string Identifier {
      get { return "/bigng/" + 
        this.portName.TrimStart(new char[]{'/'}).ToLower(); }
    }

    public ISensor[] Sensors {
      get { return active.ToArray(); }
    }

    public string GetReport() {
      StringBuilder r = new StringBuilder();

      r.AppendLine("T-Balancer bigNG");
      r.AppendLine();
      r.Append("Port Name: "); r.AppendLine(serialPort.PortName);
      r.AppendLine();

      r.AppendLine("Primary System Information Answer");
      r.AppendLine();
      r.AppendLine("       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
      r.AppendLine();
      for (int i = 0; i <= 0x11; i++) {
        r.Append(" "); r.Append((i << 4).ToString("X3")); r.Append("  ");
        for (int j = 0; j <= 0xF; j++) {
          int index = ((i << 4) | j);
          if (index < primaryData.Length) {
            r.Append(" ");
            r.Append(primaryData[index].ToString("X2"));
          }          
        }
        r.AppendLine();
      }
      r.AppendLine();

      if (alternativeData.Length > 0) {
        r.AppendLine("Alternative System Information Answer");
        r.AppendLine();
        r.AppendLine("       00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F");
        r.AppendLine();
        for (int i = 0; i <= 0x11; i++) {
          r.Append(" "); r.Append((i << 4).ToString("X3")); r.Append("  ");
          for (int j = 0; j <= 0xF; j++) {
            int index = ((i << 4) | j);
            if (index < alternativeData.Length) {
              r.Append(" ");
              r.Append(alternativeData[index].ToString("X2"));
            }
          }
          r.AppendLine();
        }
        r.AppendLine();
      }

      return r.ToString();
    }

    private void DelayedAlternativeRequest() {
      System.Threading.Thread.Sleep(500);
      try {
        if (serialPort.IsOpen)
          serialPort.Write(new byte[] { 0x37 }, 0, 1);
      } catch (Exception) { }
    }

    public void Update() {
      while (serialPort.BytesToRead >= 285)
        ReadData();
      if (serialPort.BytesToRead == 1)
        serialPort.ReadByte();

      serialPort.Write(new byte[] { 0x38 }, 0, 1);
      alternativeRequest.BeginInvoke(null, null);
    }

    public void Close() {
      serialPort.Close();
    }

    public event SensorEventHandler SensorAdded;
    public event SensorEventHandler SensorRemoved;
  }
}
