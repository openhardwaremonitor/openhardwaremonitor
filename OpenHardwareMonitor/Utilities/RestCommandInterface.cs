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

        [RestRoute("Get", "/api/version")]
        public async Task GetVersion(IHttpContext context)
        {
            await context.Response.SendResponseAsync(server.GetVersion());
        }

        /// <summary>
        /// Returns the json content that is rendered into the main page when visiting localhost:8086
        /// </summary>
        /// <param name="context">Standard argument</param>
        /// <returns>A json describing all current sensors</returns>
        [RestRoute("Get", "data.json")]
        public async Task GetWebContentData(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.GetJson());
        }

        // These functions are all equivalent, but there seems to be no way of specifying a route that contains a "rest of the path" match
        [RestRoute("Get", "/api/nodes/{hardware}/{id}")]
        public async Task HardwareNode(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.GetNode(context));
        }

        [RestRoute("Get", "/api/nodes/{hardware}/{subcomponent}/{sensorId}")]
        public async Task SensorNode1(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.GetNode(context));
        }

        [RestRoute("Get", "/api/nodes/{hardware}/{id}/{sensor}/{sensorId}")]
        public async Task SensorNode2(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.GetNode(context));
        }

        [RestRoute("Get", "/api/rootnode")]
        public async Task RootNode(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "application/json";
            await context.Response.SendResponseAsync(server.RootNode(context));
        }

        [RestRoute("Get", "/api/report")]
        public async Task Report(IHttpContext context)
        {
            context.Response.AddHeader("Cache-Control", "no-cache");
            context.Response.ContentType = "text/plain";
            await context.Response.SendResponseAsync(server.Report(context));
        }
    }
}
