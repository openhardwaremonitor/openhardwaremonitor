using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grapevine;
using OpenHardwareMonitor.GUI;
using OpenHardwareMonitor.Hardware;

namespace OpenHardwareMonitor.Utilities
{
    public sealed class GrapevineServer : IDisposable, IGrapevineServer
    {
        private readonly int port;
        private readonly Computer computer;
        private IRestServer server;
        private Node root;

        public GrapevineServer(Node node, Computer computer, int port, bool allowRemoteAccess)
        {
            this.port = port;
            this.computer = computer;
            root = node;
            AllowRemoteAccess = allowRemoteAccess;
        }

        public int ListenerPort => port;

        public bool AllowRemoteAccess { get; set; }

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

        private IList<SensorNode> GetSensors(HardwareNode node)
        {
            var ret = new List<SensorNode>();
            foreach (var n in node.Nodes)
            {
                foreach (var sensor in n.Nodes)
                {
                    if (sensor is SensorNode sn)
                    {
                        ret.Add(sn);
                    }
                    // a hardware node may contain a type node, then we need to go one level deeper still
                    else if (sensor is TypeNode tn)
                    {
                        foreach (var sensor2 in tn.Nodes)
                        {
                            if (sensor2 is SensorNode sn2)
                            {
                                ret.Add(sn2);
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public String GetNode(IHttpContext context)
        {
            StringBuilder json = new StringBuilder();

            json.Append("{");
            Uri uri = context.Request.Url;
            string myNode = uri.LocalPath;
            // The remainder of the URI is our node.
            // It may consist of two or four parts. In the former case, it's a hardware node, otherwise it's a sensor node.
            myNode = myNode.Replace("/api/nodes", string.Empty);
            var node = root.FindNode(myNode);
            if (node is HardwareNode hn)
            {
                JsonForHardware(json, hn);
            }
            else if (node is SensorNode sn)
            {
                JsonForSensor(json, sn);
            }
            json.Append("}");

            return json.ToString();
        }

        public void JsonForType(StringBuilder json, TypeNode tn)
        {
            json.AppendLine("\"NodeId\": \"" + tn.NodeId + "\",");
            json.AppendLine("\"Type\": \"" + tn.SensorType.ToString() + "\",");
            json.AppendLine("\"Name\": \"" + tn.Text + "\",");
        }

        public IList<HardwareNode> GetHardwareNodes(Node rootNode)
        {
            var ret = new List<HardwareNode>();
            foreach (var n in rootNode.Nodes)
            {
                if (n is HardwareNode hn1)
                {
                    ret.Add(hn1);
                }
                foreach (var node in n.Nodes)
                {
                    if (node is HardwareNode hn)
                    {
                        ret.Add(hn);
                        ret.AddRange(GetHardwareNodes(hn));
                    }
                }
            }

            return ret;
        }

        public string GetVersion()
        {
            return "OpenHardwareMonitor " + Application.ProductVersion;
        }

        public string Report(IHttpContext context)
        {
            return computer.GetReport();
        }

        public string RootNode(IHttpContext context)
        {
            StringBuilder json = new StringBuilder();

            json.Append("{");
            json.AppendLine("\"ComputerName\": \"" + root.Text + "\",");
            json.AppendLine("\"LogicalProcessorCount\": " + Environment.ProcessorCount + ",");
            json.AppendLine("\"Units\": [");
            var units = UnitDefinition.CommonUnits;
            for (int i = 0; i < units.Count; i++)
            {
                var u = units[i];
                json.AppendLine("{\"Abbreviation\": \"" + u.Abbreviation + "\", ");
                json.AppendLine("\"Name\": \"" + u.Fullname + "\", ");
                json.AppendLine("\"Dimension\": \"" + u.Dimension + "\"}");

                if (i != units.Count - 1)
                {
                    json.AppendLine(", ");
                }
            }
            json.AppendLine("],");
            json.AppendLine("\"Hardware\": [");
            var hardwareNodes = GetHardwareNodes(root);
            for (int index = 0; index < hardwareNodes.Count; index++)
            {
                var s = hardwareNodes[index];

                json.Append("{");
                JsonForHardware(json, s);
                json.Append("}");

                if (index != hardwareNodes.Count - 1)
                {
                    json.Append(", ");
                }
            }
            json.AppendLine("]");
            json.AppendLine("}");

            return json.ToString();
        }

        private void JsonForHardware(StringBuilder json, HardwareNode hardwareNode)
        {
            json.AppendLine("\"NodeId\": \"" + hardwareNode.Hardware.Identifier + "\", ");
            json.AppendLine("\"Name\": \"" + hardwareNode.Text + "\", ");
            json.AppendLine("\"Parent\": \"" + hardwareNode.Parent.NodeId + "\", ");
            json.AppendLine("\"HardwareType\": \"" + hardwareNode.Hardware.HardwareType.ToString() + "\", ");
            json.AppendLine("\"Sensors\": [");
            if (hardwareNode.Nodes.All(x => x is HardwareNode))
            {
                // If this node has only further hardware nodes as children, we write an empty entry and flatten the structure by one level
                json.AppendLine("]");
                return;
            }
            var sensors = GetSensors(hardwareNode);
            for (var index = 0; index < sensors.Count; index++)
            {
                var s = sensors[index];

                json.Append("{");
                JsonForSensor(json, s);
                json.Append("}");

                if (index != sensors.Count - 1)
                {
                    json.Append(", ");
                }
            }

            json.Append("]");
        }

        private static void JsonForSensor(StringBuilder json, SensorNode sensorNode)
        {
            json.AppendLine("\"NodeId\": \"" + sensorNode.NodeId + "\", ");
            json.AppendLine("\"Name\": \"" + sensorNode.Text + "\", ");
            // We need the hardware node that is the parent, not the type node ("Voltage")
            string parenNodeId = sensorNode.Parent.NodeId;
            if (sensorNode.Parent is TypeNode && sensorNode.Parent.Parent != null)
            {
                parenNodeId = sensorNode.Parent.Parent.NodeId;
            }
            json.AppendLine("\"Parent\": \"" + parenNodeId + "\", ");
            json.AppendLine("\"Type\": \"" + sensorNode.Sensor.SensorType.ToString() + "\", ");
            json.AppendLine("\"Unit\": \"" + sensorNode.Unit() + "\", ");
            var value = sensorNode.Sensor.Value;
            if (value.HasValue)
            {
                json.AppendLine("\"Value\": " + value.Value.ToString("R", CultureInfo.InvariantCulture)); // Not in quotes
            }
            else
            {
                json.AppendLine("\"Value\": 0"); // TODO: Pass null, but requires a corresponding change on the client parser
            }
        }

        public String GetJson()
        {

            StringBuilder json = new StringBuilder("{\"id\": 0, \"Text\": \"Sensor\", \"Children\": [");
            int nodeCount = 1;
            GenerateJSON(json, root, ref nodeCount);
            json.Append("]");
            json.Append(", \"Min\": \"Min\"");
            json.Append(", \"Value\": \"Value\"");
            json.Append(", \"Max\": \"Max\"");
            json.Append(", \"ImageURL\": \"\"");
            json.Append(", \"NodeId\": \"NodeId\"");
            json.Append("}");

            return json.ToString();
        }

        private void GenerateJSON(StringBuilder json, Node n, ref int nodeCount)
        {
            json.Append("{\"id\": " + nodeCount + ", \"Text\": \"" + n.Text
              + "\", \"Children\": [");
            nodeCount++;

            for (var index = 0; index < n.Nodes.Count; index++)
            {
                Node child = n.Nodes[index];
                GenerateJSON(json, child, ref nodeCount);
                if (index != n.Nodes.Count - 1)
                {
                    json.Append(", ");
                }
            }

            json.Append("]");

            if (n is SensorNode sn)
            {
                json.Append(", \"Min\": \"" + sn.Min + "\"");
                json.Append(", \"Value\": \"" + sn.Value + "\"");
                json.Append(", \"Max\": \"" + sn.Max + "\"");
                json.Append(", \"ImageURL\": \"images/transparent.png\"");
                json.Append(", \"NodeId\": \"" + sn.Sensor.Identifier + "\"");
            }
            else if (n is HardwareNode hn)
            {
                json.Append(", \"Min\": \"\"");
                json.Append(", \"Value\": \"\"");
                json.Append(", \"Max\": \"\"");
                json.Append(", \"ImageURL\": \"images_icon/" + GetHardwareImageFile(hn) + "\"");
                json.Append(", \"NodeId\": \"" + hn.Hardware.Identifier + "\"");
            }
            else if (n is TypeNode tn)
            {
                json.Append(", \"Min\": \"\"");
                json.Append(", \"Value\": \"\"");
                json.Append(", \"Max\": \"\"");
                json.Append(", \"ImageURL\": \"images_icon/" + GetTypeImageFile(tn) + "\"");
                json.Append(", \"NodeId\": \"" + tn.Text + "\"");
            }
            else
            {
                json.Append(", \"Min\": \"\"");
                json.Append(", \"Value\": \"\"");
                json.Append(", \"Max\": \"\"");
                json.Append(", \"ImageURL\": \"images_icon/computer.png\"");
                json.Append(", \"NodeId\": \"/\"");
            }

            json.Append("}");
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
