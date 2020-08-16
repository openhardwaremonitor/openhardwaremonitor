using OpenHardwareMonitor.GUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace OpenHardwareMonitor.Utilities
{
    class HttpAgent
    {
    private Node root;
    private string _actionUrl;
    private HttpClient client;
    private Timer aTimer;
 

    public bool isRunAgent
    {
      get { return aTimer.Enabled; }

    }

    public string ActionUrl
    {
      set { _actionUrl = value; }
      get { return _actionUrl; }

    }

    public int IntervalSecond
    {
      set { aTimer.Interval = value*1000; }
      get { return (int)aTimer.Interval / 1000; }
    }

    public HttpAgent(Node node)
    {
      root = node;
      client = new HttpClient();
      aTimer = new Timer();
      aTimer.Elapsed += OnTimedEvent;
      aTimer.AutoReset = true;
      aTimer.Enabled = false;
    }

    public void StartAgent()
    {
      aTimer.Start();

    }

    private void OnTimedEvent(Object source, ElapsedEventArgs e)
    {
      Upload();
    }


    public void StopAgent()
    {

      aTimer.Stop();
    }

    private void Upload()
    {
      try
      {
        JsonGenerator responseJSON = new JsonGenerator();

        var content = new StringContent(responseJSON.GetJSON(root), Encoding.UTF8, "application/json");
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        client.PostAsync(_actionUrl, content);

      }
      catch
      {
        return;
      }
    }

    ~HttpAgent()
    {
      aTimer.Stop();
      aTimer.Close();

            if (client != null)
            {
              client.Dispose();
              client = null;

            }    
    }

  }
}
