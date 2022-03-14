using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Grapevine;

namespace OpenHardwareMonitor.Utilities
{
    public interface IGrapevineServer
    {
        String GetJson();
        string GetNode(IHttpContext context);
        string RootNode(IHttpContext context);
        string GetVersion();
        string Report(IHttpContext context);
    }
}
