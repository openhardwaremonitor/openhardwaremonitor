using OpenHardwareMonitor.Hardware;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibDemoConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Computer c = new Computer()
            {
                CPUEnabled = true,
                FanControllerEnabled = true,
                GPUEnabled = true,
                HDDEnabled = true,
                MainboardEnabled = true,
                RAMEnabled = true,
            };
            try
            {
                c.Open();
                var visitor = new HWVisitor();
                c.Accept(visitor);
            }
            finally
            {
                c.Close();
            }
        }
    }
}
