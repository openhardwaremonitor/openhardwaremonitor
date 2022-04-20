using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenHardwareMonitor.Utilities
{
    internal sealed class InterprocessCommunicationFactory
    {
        public static NamedPipeServerStream GetServerPipe()
        {
            return new NamedPipeServerStream(Process.GetCurrentProcess().ProcessName, PipeDirection.InOut);
        }

        public static NamedPipeClientStream GetClientPipe()
        {
            return new NamedPipeClientStream(".", Process.GetCurrentProcess().ProcessName, PipeDirection.InOut);
        }
    }
}
