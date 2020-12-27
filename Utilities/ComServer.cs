/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>
  Copyright (C) 2012-2013 Michael Möller <mmoeller@openhardwaremonitor.org>

*/

using System.IO.Ports;
using System.Threading;
using System.IO;
using System.Net;
using System.Text;
using OpenHardwareMonitor.GUI;
using System;

namespace OpenHardwareMonitor.Utilities {

  public class ComServer {

    private Boolean isRunning;
    private int nodeCount, waitPeriod;
    private SerialPort serialPort;
    private Thread serverThread;
    private Node root;

    public int SendInterval {
      get { return waitPeriod; }
      set { waitPeriod = value; }
    }
    public string PortName {
      get { return serialPort.PortName; }
      set { serialPort.PortName = value; }
    }
    public int BaudRate {
      get { return serialPort.BaudRate; }
      set { serialPort.BaudRate = value; }
    }
    public int Parity {
      get { return (int)serialPort.Parity;  }
      set { serialPort.Parity = (Parity)value; }
    }
    public int DataBits {
      get { return serialPort.DataBits; }
      set { serialPort.DataBits = value; }
    }
    public int StopBits {
      get { return (int)serialPort.StopBits; }
      set { serialPort.StopBits = (StopBits)value; }
    }

    public ComServer(Node node, int sendInterval, String portName, int baudRate, int parity, int dataBits, int stopBits) {
      nodeCount = 0;
      isRunning = false;
      root = node;
      waitPeriod = sendInterval;
      serialPort = new SerialPort(portName, baudRate, (Parity)parity, dataBits, (StopBits)stopBits);
    }

    ~ComServer()
    {
      StopServer();
    }

    public Boolean StartServer() {
      // If already running, just return.
      if (isRunning)
        return true;

      // Try to open the specified COM port
      try {
        serialPort.Open();
        serverThread = new Thread(Serve);
        serverThread.Start();
        isRunning = true;
      } catch (Exception) {
        isRunning = false;
      }

      return isRunning;
    }

    public Boolean StopServer() {
      if (!isRunning)
        return false;

      try {
        serverThread.Abort();
      } catch (Exception) { }

      try {
        serialPort.Close();
      } catch (Exception) { }

      isRunning = false;

      return true;
    }

    private void Serve() {
      while (isRunning) {
        SendJSON();
        Thread.Sleep(waitPeriod);
      }
    }

    private void SendJSON() {

      string JSON = "{\"id\": 0, \"Text\": \"Sensor\", \"Children\": [";
      nodeCount = 1;
      JSON += GenerateJSON(root);
      JSON += "]";
      JSON += ", \"Min\": \"Min\"";
      JSON += ", \"Value\": \"Value\"";
      JSON += ", \"Max\": \"Max\"";
      JSON += "}";

      byte[] buffer = Encoding.UTF8.GetBytes(JSON);

      try {
        serialPort.Write(buffer, 0, buffer.Length);
      } catch (Exception) {}
    }

    private string GenerateJSON(Node n) {
      string JSON = "{\"id\": " + nodeCount + ", \"Text\": \"" + n.Text 
        + "\", \"Children\": [";
      nodeCount++;

      foreach (Node child in n.Nodes)
        JSON += GenerateJSON(child) + ", ";
      if (JSON.EndsWith(", "))
        JSON = JSON.Remove(JSON.LastIndexOf(","));
      JSON += "]";

      if (n is SensorNode) {
        JSON += ", \"Min\": \"" + ((SensorNode)n).Min + "\"";
        JSON += ", \"Value\": \"" + ((SensorNode)n).Value + "\"";
        JSON += ", \"Max\": \"" + ((SensorNode)n).Max + "\"";
      } else if (n is HardwareNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
      } else if (n is TypeNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
      } else {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
      }

      JSON += "}";
      return JSON;
    }
  }
}
