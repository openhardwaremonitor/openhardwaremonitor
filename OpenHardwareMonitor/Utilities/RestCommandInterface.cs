using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grapevine;

namespace OpenHardwareMonitor.Utilities
{
    [RestResource]
    public class RestCommandInterface
    {
        private readonly IGrapevineServer server;

        public RestCommandInterface(IGrapevineServer server)
        {
            this.server = server;
        }
        [RestRoute("Get", "/api/available")]
        public async Task IsAvailable(IHttpContext context)
        {
            await context.Response.SendResponseAsync("True").ConfigureAwait(false);
        }

        [RestRoute("Get", "data.json")]
        public async Task GetData(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.GetJson());
        }
    }
}
