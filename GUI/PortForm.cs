/*
 
  This Source Code Form is subject to the terms of the Mozilla Public
  License, v. 2.0. If a copy of the MPL was not distributed with this
  file, You can obtain one at http://mozilla.org/MPL/2.0/.
 
	Copyright (C) 2012 Prince Samuel <prince.samuel@gmail.com>

*/

using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;

namespace OpenHardwareMonitor.GUI
{
    public partial class PortForm : Form
    {
        private readonly string localIP;
        private readonly MainForm parent;

        public PortForm(MainForm m)
        {
            InitializeComponent();
            parent = m;

            localIP = Dns.GetHostEntry(Dns.GetHostName())
                          .AddressList
                          .FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork)
                          ?.ToString() ?? "?";
        }

        private void portNumericUpDn_ValueChanged(object sender, EventArgs e)
        {
            var url = $"http://{localIP}:{portNumericUpDn.Value}/";
            webServerLinkLabel.Text = url;
            webServerLinkLabel.Links.Remove(webServerLinkLabel.Links[0]);
            webServerLinkLabel.Links.Add(0, webServerLinkLabel.Text.Length, url);
        }

        private void portOKButton_Click(object sender, EventArgs e)
        {
            parent.Server.ListenerPort = (int) portNumericUpDn.Value;
            Close();
        }

        private void portCancelButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void PortForm_Load(object sender, EventArgs e)
        {
            portNumericUpDn.Value = parent.Server.ListenerPort;
            portNumericUpDn_ValueChanged(null, null);
        }

        private void webServerLinkLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo(e.Link.LinkData.ToString()));
            }
            catch
            {
            }
        }
    }
}