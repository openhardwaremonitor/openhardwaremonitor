/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>
  Copyright (C) 2012-2013 Michael Möller <mmoeller@openhardwaremonitor.org>

*/

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Utilities {

  public class HttpServer {
    private HttpListener listener;
    private int listenerPort, nodeCount;
    private Thread listenerThread;
    private Node root;

    public HttpServer(Node node, int port) {
      root = node;
      listenerPort = port;

      //JSON node count. 
      nodeCount = 0;

      try {
        listener = new HttpListener();
        listener.IgnoreWriteExceptions = true;
      } catch (PlatformNotSupportedException) {
        listener = null;
      }
    }

    public bool PlatformNotSupported {
      get {
        return listener == null;
      }
    }

    public Boolean StartHTTPListener() {
      if (PlatformNotSupported)
        return false;

      try {
        if (listener.IsListening)
          return true;

        string prefix = "http://+:" + listenerPort + "/";
        listener.Prefixes.Clear();
        listener.Prefixes.Add(prefix);
        listener.Start();

        if (listenerThread == null) {
          listenerThread = new Thread(HandleRequests);
          listenerThread.Start();
        }
      } catch (Exception) {
        return false;
      }

      return true;
    }

    public Boolean StopHTTPListener() {
      if (PlatformNotSupported)
        return false;

      try {
        listenerThread.Abort();
        listener.Stop();
        listenerThread = null;
      } catch (HttpListenerException) {
      } catch (ThreadAbortException) {
      } catch (NullReferenceException) {
      } catch (Exception) {
      }
      return true;
    }

    private void HandleRequests() {

      while (listener.IsListening) {
        var context = listener.BeginGetContext(
          new AsyncCallback(ListenerCallback), listener);
        context.AsyncWaitHandle.WaitOne();
      }
    }

    private void ListenerCallback(IAsyncResult result) {
      HttpListener listener = (HttpListener)result.AsyncState;
      if (listener == null || !listener.IsListening)
        return;

      // Call EndGetContext to complete the asynchronous operation.
      HttpListenerContext context;
      try {
        context = listener.EndGetContext(result);     
      } catch (Exception) {
        return;
      }

      HttpListenerRequest request = context.Request;

      var requestedFile = request.RawUrl.Substring(1);
      if (requestedFile == "data.json") {
        SendJSON(context.Response);
        return;
      }

      if (requestedFile.Contains("images_icon")) {
        ServeResourceImage(context.Response, 
          requestedFile.Replace("images_icon/", ""));
        return;
      }

      // default file to be served
      if (string.IsNullOrEmpty(requestedFile))
        requestedFile = "index.html";

      string[] splits = requestedFile.Split('.');
      string ext = splits[splits.Length - 1];
      ServeResourceFile(context.Response, 
        "Web." + requestedFile.Replace('/', '.'), ext);
    }

    private void ServeResourceFile(HttpListenerResponse response, string name, 
      string ext) 
    {
      // resource names do not support the hyphen
      name = "OpenHardwareMonitor.Resources." + 
        name.Replace("custom-theme", "custom_theme");

      string[] names =
        Assembly.GetExecutingAssembly().GetManifestResourceNames();
      for (int i = 0; i < names.Length; i++) {
        if (names[i].Replace('\\', '.') == name) {
          using (Stream stream = Assembly.GetExecutingAssembly().
            GetManifestResourceStream(names[i])) {
            response.ContentType = GetcontentType("." + ext);
            response.ContentLength64 = stream.Length;
            byte[] buffer = new byte[512 * 1024];
            int len;
            try {
              Stream output = response.OutputStream;
              while ((len = stream.Read(buffer, 0, buffer.Length)) > 0) {
                output.Write(buffer, 0, len);
              }
              output.Flush();
              output.Close();              
              response.Close();
            } catch (HttpListenerException) { 
            } catch (InvalidOperationException) { 
            }
            return;
          }          
        }
      }

      response.StatusCode = 404;
      response.Close();
    }

    private void ServeResourceImage(HttpListenerResponse response, string name) {
      name = "OpenHardwareMonitor.Resources." + name;

      string[] names =
        Assembly.GetExecutingAssembly().GetManifestResourceNames();
      for (int i = 0; i < names.Length; i++) {
        if (names[i].Replace('\\', '.') == name) {
          using (Stream stream = Assembly.GetExecutingAssembly().
            GetManifestResourceStream(names[i])) {

            Image image = Image.FromStream(stream);
            response.ContentType = "image/png";
            try {
              Stream output = response.OutputStream;
              using (MemoryStream ms = new MemoryStream()) {
                image.Save(ms, ImageFormat.Png);
                ms.WriteTo(output);
              }
              output.Close();
            } catch (HttpListenerException) {              
            }
            image.Dispose();
            response.Close();
            return;
          }
        }
      }

      response.StatusCode = 404;
      response.Close();
    }

    private void SendJSON(HttpListenerResponse response) {

      string JSON = "{\"id\": 0, \"Text\": \"Sensor\", \"Children\": [";
      nodeCount = 1;
      JSON += GenerateJSON(root);
      JSON += "]";
      JSON += ", \"Min\": \"Min\"";
      JSON += ", \"Value\": \"Value\"";
      JSON += ", \"Max\": \"Max\"";
      JSON += ", \"ImageURL\": \"\"";
      JSON += "}";

      var responseContent = JSON;
      byte[] buffer = Encoding.UTF8.GetBytes(responseContent);

      response.AddHeader("Cache-Control", "no-cache");

      response.ContentLength64 = buffer.Length;
      response.ContentType = "application/json";

      try {
        Stream output = response.OutputStream;
        output.Write(buffer, 0, buffer.Length);
        output.Close();
      } catch (HttpListenerException) {
      }

      response.Close();
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
        JSON += ", \"ImageURL\": \"images/transparent.png\"";
      } else if (n is HardwareNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/" + 
          GetHardwareImageFile((HardwareNode)n) + "\"";
      } else if (n is TypeNode) {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/" + 
          GetTypeImageFile((TypeNode)n) + "\"";
      } else {
        JSON += ", \"Min\": \"\"";
        JSON += ", \"Value\": \"\"";
        JSON += ", \"Max\": \"\"";
        JSON += ", \"ImageURL\": \"images_icon/computer.png\"";
      }

      JSON += "}";
      return JSON;
    }

    private static void ReturnFile(HttpListenerContext context, string filePath) 
    {
      context.Response.ContentType = 
        GetcontentType(Path.GetExtension(filePath));
      const int bufferSize = 1024 * 512; //512KB
      var buffer = new byte[bufferSize];
      using (var fs = File.OpenRead(filePath)) {

        context.Response.ContentLength64 = fs.Length;
        int read;
        while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
          context.Response.OutputStream.Write(buffer, 0, read);
      }

      context.Response.OutputStream.Close();
    }

    private static string GetcontentType(string extension) {
      switch (extension) {
        case ".avi": return "video/x-msvideo";
        case ".css": return "text/css";
        case ".doc": return "application/msword";
        case ".gif": return "image/gif";
        case ".htm":
        case ".html": return "text/html";
        case ".jpg":
        case ".jpeg": return "image/jpeg";
        case ".js": return "application/x-javascript";
        case ".mp3": return "audio/mpeg";
        case ".png": return "image/png";
        case ".pdf": return "application/pdf";
        case ".ppt": return "application/vnd.ms-powerpoint";
        case ".zip": return "application/zip";
        case ".txt": return "text/plain";
        default: return "application/octet-stream";
      }
    }

    private static string GetHardwareImageFile(HardwareNode hn) {

      switch (hn.Hardware.HardwareType) {
        case HardwareType.CPU:
          return "cpu.png";
        case HardwareType.GpuNvidia:
          return "nvidia.png";
        case HardwareType.GpuAti:
          return "ati.png";
        case HardwareType.HDD:
          return "hdd.png";
        case HardwareType.Heatmaster:
          return "bigng.png";
        case HardwareType.Mainboard:
          return "mainboard.png";
        case HardwareType.SuperIO:
          return "chip.png";
        case HardwareType.TBalancer:
          return "bigng.png";
        case HardwareType.RAM:
          return "ram.png";
        default:
          return "cpu.png";
      }

    }

    private static string GetTypeImageFile(TypeNode tn) {

      switch (tn.SensorType) {
        case SensorType.Voltage:
          return "voltage.png";
        case SensorType.Clock:
          return "clock.png";
        case SensorType.Load:
          return "load.png";
        case SensorType.Temperature:
          return "temperature.png";
        case SensorType.Fan:
          return "fan.png";
        case SensorType.Flow:
          return "flow.png";
        case SensorType.Control:
          return "control.png";
        case SensorType.Level:
          return "level.png";
        case SensorType.Power:
          return "power.png";
        default:
          return "power.png";
      }

    }

    public int ListenerPort {
      get { return listenerPort; }
      set { listenerPort = value; }
    }

    ~HttpServer() {
      if (PlatformNotSupported)
        return;

      StopHTTPListener();
      listener.Abort();
    }

    public void Quit() {
      if (PlatformNotSupported)
        return;

      StopHTTPListener();
      listener.Abort();
    }
  }
}
