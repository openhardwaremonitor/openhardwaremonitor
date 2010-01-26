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
using System.IO.Ports;
using System.Threading;

namespace OpenHardwareMonitor.Hardware.TBalancer {
  public class TBalancerGroup : IGroup {

    private List<TBalancer> hardware = new List<TBalancer>();

    public TBalancerGroup() {

      string[] portNames = SerialPort.GetPortNames();
      for (int i = portNames.Length - 1; i >= 0; i--) {
        try {
          SerialPort serialPort =
            new SerialPort(portNames[i], 19200, Parity.None, 8, StopBits.One);
          serialPort.Open();
          bool isValid = false;
          if (serialPort.IsOpen && serialPort.CDHolding &&
            serialPort.CtsHolding) {
            serialPort.DiscardInBuffer();
            serialPort.DiscardOutBuffer();
            serialPort.Write(new byte[] { 0x38 }, 0, 1);
            int j = 0;
            while (serialPort.BytesToRead == 0 && j < 2) {
              Thread.Sleep(100);
              j++;
            }
            if (serialPort.BytesToRead > 0 && 
              serialPort.ReadByte() == TBalancer.STARTFLAG) 
            {
              while (serialPort.BytesToRead < 284 && j < 5) {
                Thread.Sleep(100);
                j++;
              }
              if (serialPort.BytesToRead == 284) {
                int[] data = new int[285];
                data[0] = TBalancer.STARTFLAG;
                for (int k = 1; k < data.Length; k++)
                  data[k] = serialPort.ReadByte();
                
                // check protocol version
                isValid = (data[274] == TBalancer.PROTOCOL_VERSION); 
              }
            }
          }
          serialPort.DiscardInBuffer();
          serialPort.Close();
          if (isValid) {
            hardware.Add(new TBalancer(portNames[i]));
            return;
          }
        } catch (IOException) { } catch (UnauthorizedAccessException) { } 
          catch (NullReferenceException) { }
      }
    }

    public IHardware[] Hardware {
      get {
        return hardware.ToArray();
      }
    }

    public string GetReport() {
      return null;
    }

    public void Close() {
      foreach (TBalancer tbalancer in hardware)
        tbalancer.Close();
    }
  }
}
