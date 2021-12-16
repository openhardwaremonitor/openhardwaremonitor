using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grapevine;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Utilities
{
    public sealed class GrapevineServer : IDisposable, IGrapevineServer
    {
        private readonly int port;
        private IRestServer server;
        private Node root;

        public GrapevineServer(Node node, int port)
        {
            this.port = port;
            root = node;
        }

        public int ListenerPort => port;
        public bool PlatformNotSupported
        {
            get
            {
                return false;
            }
        }

        public bool Start()
        {
            try
            {
                ServerConfig.ServerPort = ListenerPort;
                ServerConfig.ActiveServer = this;
                var builder = RestServerBuilder.From<ServerConfig>();
                server = builder.Build();
                server.Start();
                return server.IsListening;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void Stop()
        {
            if (server != null)
            {
                server.Stop();
                server.Dispose();
                server = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }

        public String GetJson()
        {

            string JSON = "{\"id\": 0, \"Text\": \"Sensor\", \"Children\": [";
            int nodeCount = 1;
            JSON += GenerateJSON(root, ref nodeCount);
            JSON += "]";
            JSON += ", \"Min\": \"Min\"";
            JSON += ", \"Value\": \"Value\"";
            JSON += ", \"Max\": \"Max\"";
            JSON += ", \"ImageURL\": \"\"";
            JSON += ", \"NodeId\": \"NodeId\"";
            JSON += "}";

            return JSON;
        }

        private string GenerateJSON(Node n, ref int nodeCount)
        {
            string JSON = "{\"id\": " + nodeCount + ", \"Text\": \"" + n.Text
              + "\", \"Children\": [";
            nodeCount++;

            foreach (Node child in n.Nodes)
                JSON += GenerateJSON(child, ref nodeCount) + ", ";
            if (JSON.EndsWith(", "))
                JSON = JSON.Remove(JSON.LastIndexOf(","));
            JSON += "]";

            if (n is SensorNode sn)
            {
                JSON += ", \"Min\": \"" + sn.Min + "\"";
                JSON += ", \"Value\": \"" + sn.Value + "\"";
                JSON += ", \"Max\": \"" + sn.Max + "\"";
                JSON += ", \"ImageURL\": \"images/transparent.png\"";
                JSON += ", \"NodeId\": \"" + sn.Sensor.Identifier + "\"";
            }
            else if (n is HardwareNode hn)
            {
                JSON += ", \"Min\": \"\"";
                JSON += ", \"Value\": \"\"";
                JSON += ", \"Max\": \"\"";
                JSON += ", \"ImageURL\": \"images_icon/" +
                  GetHardwareImageFile(hn) + "\"";
                JSON += ", \"NodeId\": \"" + hn.Hardware.Identifier + "\"";
            }
            else if (n is TypeNode tn)
            {
                JSON += ", \"Min\": \"\"";
                JSON += ", \"Value\": \"\"";
                JSON += ", \"Max\": \"\"";
                JSON += ", \"ImageURL\": \"images_icon/" +
                  GetTypeImageFile(tn) + "\"";
                JSON += ", \"NodeId\": \"" + tn.Text + "\"";
            }
            else
            {
                JSON += ", \"Min\": \"\"";
                JSON += ", \"Value\": \"\"";
                JSON += ", \"Max\": \"\"";
                JSON += ", \"ImageURL\": \"images_icon/computer.png\"";
                JSON += ", \"NodeId\": \"/\"";
            }

            JSON += "}";
            return JSON;
        }

        private static string GetHardwareImageFile(HardwareNode hn)
        {

            switch (hn.Hardware.HardwareType)
            {
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

        private static string GetTypeImageFile(TypeNode tn)
        {

            switch (tn.SensorType)
            {
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


    }
}
